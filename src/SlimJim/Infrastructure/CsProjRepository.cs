using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
    public class CsProjRepository : ICsProjRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CsProjRepository));

        public CsProjRepository()
        {
            Finder = new ProjectFileFinder();
            Reader = new CsProjReader();
        }

        public ProjectFileFinder Finder { get; set; }
        public CsProjReader Reader { get; set; }

        public virtual List<CsProj> LookupCsProjsFromDirectory(SlnGenerationOptions options)
        {
            IgnoreConfiguredDirectoryPatterns(options);

            var files = FindAllProjectFiles(options);
            var projects = PopulateTopLevelProjectGuids(files);
            projects = PopulateProjectReferenceGuids(projects);
            projects = PopulateRemainingProjectGuids(projects);

            return projects;
        }

        private void IgnoreConfiguredDirectoryPatterns(SlnGenerationOptions options)
        {
            if (options.IgnoreDirectoryPatterns.Count > 0)
                Finder.IgnorePatterns(options.IgnoreDirectoryPatterns.ToArray());
        }

        private List<FileInfo> FindAllProjectFiles(SlnGenerationOptions options)
        {
            var files = Finder.FindAllProjectFiles(options.ProjectsRootDirectory);

            foreach (var path in options.AdditionalSearchPaths) files.AddRange(Finder.FindAllProjectFiles(path));

            return files;
        }

        private List<CsProj> PopulateTopLevelProjectGuids(List<FileInfo> files)
        {
            var projects = files.ConvertAll(f => Reader.Read(f));
            projects.RemoveAll(p => p == null);

            foreach (var project in projects)
                if (project.Guid == Guid.Empty.ToString())
                {
                    var projectWithReference = projects.Find(proj =>
                        proj.ReferencedProjects.Where(tuple => tuple.Value != Guid.Empty.ToString())
                            .Select(tuple => tuple.Key)
                            .FirstOrDefault(s => s.Equals(project.AssemblyName)) != null);

                    if (projectWithReference != null)
                        project.Guid = projectWithReference.ReferencedProjects[project.AssemblyName];
                }

            return projects;
        }

        private List<CsProj> PopulateProjectReferenceGuids(List<CsProj> projects)
        {
            foreach (var project in projects)
            {
                var copiedReferences = new Dictionary<string, string>(project.ReferencedProjects);

                foreach (var reference in project.ReferencedProjects)
                    if (reference.Value == Guid.Empty.ToString())
                        copiedReferences[reference.Key] =
                            projects.Find(proj => proj.AssemblyName == reference.Key).Guid;

                project.ReferencedProjects = copiedReferences;
            }

            return projects;
        }

        /// <summary>
        /// At this point, we would have filled out project guids for projects that
        /// were still referenced in an old style CsProj, thus having a project Guid.
        /// That means the remaining ones are only referenced in new style projects,
        /// which means we can give them whatever guid we want here.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        private List<CsProj> PopulateRemainingProjectGuids(List<CsProj> projects)
        {
            var projectGuidMap = new Dictionary<string, string>();

            foreach (var project in projects)
            {
                if (project.Guid == Guid.Empty.ToString())
                {
                    var guid = Guid.NewGuid().ToString("B").ToUpperInvariant();
                    project.Guid = guid;
                    projectGuidMap.Add(project.AssemblyName, guid);
                }
            }

            foreach (var project in projects)
            {
                var copiedReferences = new Dictionary<string, string>(project.ReferencedProjects);

                foreach (var reference in project.ReferencedProjects)
                    if (reference.Value == Guid.Empty.ToString())
                        copiedReferences[reference.Key] =
                            projectGuidMap[reference.Key];

                project.ReferencedProjects = copiedReferences;
            }

            return projects;
        }
    }
}