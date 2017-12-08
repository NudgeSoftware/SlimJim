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

        public virtual List<CsProj> LookupCsProjsFromDirectory(SlnGenerationOptions options)
        {
            IgnoreConfiguredDirectoryPatterns(options);

            List<FileInfo> files = FindAllProjectFiles(options);
            List<CsProj> projects = ReadProjectFilesIntoCsProjObjects(files);

            return projects;
        }

        private void IgnoreConfiguredDirectoryPatterns(SlnGenerationOptions options)
        {
            if (options.IgnoreDirectoryPatterns.Count > 0)
            {
                Finder.IgnorePatterns(options.IgnoreDirectoryPatterns.ToArray());
            }
        }

        private List<FileInfo> FindAllProjectFiles(SlnGenerationOptions options)
        {
            List<FileInfo> files = Finder.FindAllProjectFiles(options.ProjectsRootDirectory);

            foreach (string path in options.AdditionalSearchPaths)
            {
                files.AddRange(Finder.FindAllProjectFiles(path));
            }

            return files;
        }

        private List<CsProj> ReadProjectFilesIntoCsProjObjects(List<FileInfo> files)
        {
            List<CsProj> projects = files.ConvertAll(f => Reader.Read(f));
            projects.RemoveAll(p => p == null);

            foreach (var project in projects)
            {
                if (project.Guid == Guid.Empty.ToString())
                {
                    var projectWithReference = projects.Find(proj =>
                        proj.ReferencedProjects.Select(tuple => tuple.assemblyName)
                            .FirstOrDefault(s => s.Equals(project.AssemblyName)) != null);

                    if (projectWithReference != null)
                    {
                        project.Guid = projectWithReference.ReferencedProjects.Find(tuple => tuple.assemblyName == project.AssemblyName).guid;
                    }
                }
            }

            return projects;
        }

        public ProjectFileFinder Finder { get; set; }
        public CsProjReader Reader { get; set; }
    }
}