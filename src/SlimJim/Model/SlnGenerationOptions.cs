using System.Collections.Generic;
using System.IO;
using log4net.Core;

namespace SlimJim.Model
{
    public class SlnGenerationOptions
    {
        private const string DefaultSolutionName = "SlimJim";
        private string _projectsRootDirectory;
        private string _solutionName;
        private string _slnOutputPath;
        private readonly List<string> _additionalSearchPaths;
        private readonly List<string> _ignoreDirectoryPatterns;

        public SlnGenerationOptions(string workingDirectory)
        {
            ProjectsRootDirectory = workingDirectory;
            _additionalSearchPaths = new List<string>();
            _ignoreDirectoryPatterns = new List<string>();
            TargetProjectNames = new List<string>();
            VisualStudioVersion = VisualStudioVersion.VS2015;
            LoggingThreshold = Level.Info;
        }

        public List<string> TargetProjectNames { get; private set; }

        public string ProjectsRootDirectory
        {
            get { return _projectsRootDirectory; }
            set
            {
                _projectsRootDirectory = ResolvePath(value);
            }
        }

        public VisualStudioVersion VisualStudioVersion { get; set; }
        public bool SkipAfferentAssemblyReferences { get; set; }
        public bool IncludeEfferentAssemblyReferences { get; set; }
        public bool ShowHelp { get; set; }
        public bool ConvertReferences { get; set; }
        public bool RestoreReferences { get; set; }
        public bool FixHintPaths { get; set; }
        public bool RestoreHintPaths { get; set; }
        public bool OpenInVisualStudio { get; set; }
        public Level LoggingThreshold { get; set; }

        public List<string> AdditionalSearchPaths => _additionalSearchPaths.ConvertAll(ResolvePath);

        private string ResolvePath(string p)
        {
            return !Path.IsPathRooted(p) ? Path.Combine(ProjectsRootDirectory, p) : p;
        }

        public string SlnOutputPath
        {
            get { return _slnOutputPath != null ? ResolvePath(_slnOutputPath) : ProjectsRootDirectory; }
            set { _slnOutputPath = value; }
        }

        public string SolutionName
        {
            get
            {
                if (string.IsNullOrEmpty(_solutionName))
                {
                    if (TargetProjectNames.Count > 0)
                    {
                        return string.Join("_", TargetProjectNames);
                    }

                    if (!string.IsNullOrEmpty(ProjectsRootDirectory))
                    {
                        return GetLastSegmentNameOfProjectsRootDirectory();
                    }

                    return DefaultSolutionName;
                }

                return _solutionName;
            }
            set { _solutionName = value; }
        }

        private string GetLastSegmentNameOfProjectsRootDirectory()
        {
            var dir = new DirectoryInfo(ProjectsRootDirectory);
            
            if (string.IsNullOrEmpty(dir.Name) || dir.FullName == dir.Root.FullName)
            {
                return DefaultSolutionName;
            }
            return dir.Name;
        }

        public SlnGenerationMode Mode => TargetProjectNames.Count == 0
            ? SlnGenerationMode.FullGraph
            : SlnGenerationMode.PartialGraph;

        public List<string> IgnoreDirectoryPatterns => _ignoreDirectoryPatterns;

        public bool LoadAndSaveSolutionWithVS { get; set; }

        public void AddAdditionalSearchPaths(params string[] searchPaths)
        {
            _additionalSearchPaths.AddRange(searchPaths);
        }

        public void AddTargetProjectNames(params string[] targetProjectNames)
        {
            TargetProjectNames.AddRange(targetProjectNames);
        }

        public void AddIgnoreDirectoryPatterns(params string [] patterns)
        {
            _ignoreDirectoryPatterns.AddRange(patterns);
        }
    }
}
