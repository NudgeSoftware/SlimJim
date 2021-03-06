﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace SlimJim.Model
{
    public class SlnBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SlnFileGenerator));
        private static SlnBuilder _overriddenBuilder;

        private readonly List<CsProj> _projectsList;
        private Sln _builtSln;
        private SlnGenerationOptions _options;

        public SlnBuilder(List<CsProj> projectsList)
        {
            _projectsList = projectsList;
        }

        public virtual Sln BuildSln(SlnGenerationOptions options)
        {
            _options = options;

            _builtSln = new Sln(options.SolutionName)
            {
                Version = options.VisualStudioVersion,
                ProjectsRootDirectory = options.ProjectsRootDirectory
            };

            AddProjectsToSln(options);

            return _builtSln;
        }

        private void AddProjectsToSln(SlnGenerationOptions options)
        {
            if (options.Mode == SlnGenerationMode.PartialGraph)
                AddPartialProjectGraphToSln(options);
            else
                AddAllProjectsToSln();
        }

        private void AddPartialProjectGraphToSln(SlnGenerationOptions options)
        {
            Log.Info("Building partial graph solution for target projects: " +
                     string.Join(", ", options.TargetProjectNames));

            foreach (var targetProjectName in options.TargetProjectNames)
            {
                var rootProject = AddAssemblySubtree(targetProjectName);

                if (rootProject == null) Log.WarnFormat("Project {0} not found.", targetProjectName);

                if (options.SkipAfferentAssemblyReferences == false) AddAfferentReferencesToProject(rootProject);
            }
        }

        private void AddAllProjectsToSln()
        {
            Log.Info("Building full graph solution.");

            _projectsList.ForEach(AddProject);
        }

        private CsProj AddAssemblySubtree(string assemblyName, string targetFrameworkVersion = "")
        {
            var project = FindProjectByAssemblyName(assemblyName, targetFrameworkVersion);

            AddProjectAndReferences(project);

            return project;
        }

        private CsProj FindProjectByAssemblyName(string assemblyName, string targetFrameworkVersion)
        {
            var matches = _projectsList.Where(p => p.AssemblyName == assemblyName).ToList();


            if (matches.Count <= 1)
            {
                var single = matches.SingleOrDefault();
                if (single != null)
                    Log.InfoFormat("Found projects with AssemblyName {0}: {1}", assemblyName, single.Path);
                return single;
            }

            //TODO: filter projects that don't specify version
            if (string.IsNullOrEmpty(targetFrameworkVersion))
            {
                Log.WarnFormat(
                    "Found multiple projects with AssemblyName {0} and no target framework version is specified: {1}",
                    assemblyName, string.Join(", ", matches.Select(m => m.Path)));
                return matches.First();
            }

            var myVersion = new Version(targetFrameworkVersion.Substring(1));
            var versions = matches
                .Where(m => m.TargetFrameworkVersion != null && m.TargetFrameworkVersion.StartsWith("v"))
                .ToDictionary(m => new Version(m.TargetFrameworkVersion.Substring(1)));

            var closest = versions.Where(v => v.Key <= myVersion).OrderByDescending(v => v.Key).FirstOrDefault();

            if (closest.Value != null)
            {
                Log.InfoFormat("Found multiple projects with AssemblyName {0}: {1} and chose {2}", assemblyName,
                    string.Join(", ", matches.Select(m => m.Path)), closest.Value.Path);
                return closest.Value;
            }

            Log.WarnFormat(
                "Found multiple projects with AssemblyName {0}: {1} and none have compatible TargetFrameworkVersion property. Choosing {2}",
                assemblyName, string.Join(", ", matches.Select(m => m.Path)), matches.First());
            return matches.First();
        }

        private void AddProjectAndReferences(CsProj project)
        {
            if (project != null)
            {
                AddProject(project);

                IncludeEfferentProjectReferences(project);

                if (_options.IncludeEfferentAssemblyReferences) IncludeEfferentAssemblyReferences(project);
            }
        }

        private void AddProject(CsProj project)
        {
            _builtSln.AddProjects(project);
        }

        private void IncludeEfferentProjectReferences(CsProj project)
        {
            foreach (var referencedProj in project.ReferencedProjects) AddProjectSubtree(referencedProj.Value);
        }

        private void IncludeEfferentAssemblyReferences(CsProj project)
        {
            foreach (var assemblyName in project.ReferencedAssemblyNames)
                AddAssemblySubtree(assemblyName, project.TargetFrameworkVersion);
        }

        private void AddProjectSubtree(string projectGuid)
        {
            var referencedProject = FindProjectByProjectGuid(projectGuid);

            AddProjectAndReferences(referencedProject);
        }

        private void AddAfferentReferencesToProject(CsProj project)
        {
            if (project != null)
            {
                var afferentAssemblyReferences = _projectsList.FindAll(
                    csp => csp.ReferencedAssemblyNames.Contains(project.AssemblyName));

                AddAfferentReferences(afferentAssemblyReferences);

                var afferentProjectReferences =
                    _projectsList.FindAll(csp =>
                        csp.ReferencedProjects.Select(tuple => tuple.Value).Contains(project.Guid));

                AddAfferentReferences(afferentProjectReferences);
            }
        }

        private void AddAfferentReferences(List<CsProj> afferentReferences)
        {
            foreach (var assemblyReference in afferentReferences) AddProjectAndReferences(assemblyReference);
        }

        private CsProj FindProjectByProjectGuid(string projectGuid)
        {
            return _projectsList.Find(csp => csp.Guid == projectGuid);
        }

        public static SlnBuilder GetSlnBuilder(List<CsProj> projects)
        {
            return _overriddenBuilder ?? new SlnBuilder(projects);
        }

        public static void OverrideDefaultBuilder(SlnBuilder slnBuilder)
        {
            _overriddenBuilder = slnBuilder;
        }
    }
}