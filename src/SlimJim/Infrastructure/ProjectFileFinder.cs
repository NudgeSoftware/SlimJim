﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;

namespace SlimJim.Infrastructure
{
    public class ProjectFileFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<Regex> _ignorePatterns;

        public ProjectFileFinder()
        {
            _ignorePatterns = new List<Regex>();
            IgnorePatterns(@"^\.svn$", @"^\.hg$", @"^\.git$", "^bin$", "^obj$", "ReSharper");
        }

        public virtual List<FileInfo> FindAllProjectFiles(string startPath)
        {
            Log.InfoFormat("Searching for .c*proj files at {0}", startPath);

            var root = new DirectoryInfo(startPath);
            var projectFiles = GetProjectFiles(root);

            return projectFiles;
        }

        private List<FileInfo> GetProjectFiles(DirectoryInfo directory)
        {
            var files = new List<FileInfo>();

            if (!PathIsIgnored(directory.Name)) SearchDirectoryForProjects(directory, files);

            return files;
        }

        private void SearchDirectoryForProjects(DirectoryInfo directory, List<FileInfo> files)
        {
            var projects = directory
                .GetFiles("*.c*proj")
                .Where(f => !PathIsIgnored(f.Name))
                .ToArray();

            if (projects.Length > 0)
                AddProjectFile(projects, files);
            else
                RecurseChildDirectories(directory, files);
        }

        private void RecurseChildDirectories(DirectoryInfo directory, List<FileInfo> files)
        {
            foreach (var dir in directory.EnumerateDirectories()) files.AddRange(GetProjectFiles(dir));
        }

        private void AddProjectFile(IEnumerable<FileInfo> projects, List<FileInfo> files)
        {
            foreach (var project in projects)
            {
                files.Add(project);
                Log.Debug(project);
            }
        }

        public bool PathIsIgnored(DirectoryInfo dir)
        {
            return PathIsIgnored(dir.Name);
        }

        public bool PathIsIgnored(string name)
        {
            return _ignorePatterns.Exists(p => p.IsMatch(name));
        }

        public virtual void IgnorePatterns(params string[] patterns)
        {
            var regexes = new List<string>(patterns).ConvertAll(p => new Regex(p, RegexOptions.IgnoreCase));
            _ignorePatterns.AddRange(regexes);
        }
    }
}