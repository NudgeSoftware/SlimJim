using log4net.Core;
using NUnit.Framework;
using SlimJim.Infrastructure;
using SlimJim.Model;

namespace SlimJim.Test.Infrastructure
{
    [TestFixture]
    public class ArgsOptionsBuilderTests : TestBase
    {
        private SlnGenerationOptions _options;

        [Test]
        public void DefaultThresholdIsInfo()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new string[0], WorkingDirectory);

            Assert.That(_options.LoggingThreshold, Is.EqualTo(Level.Info));
        }

        [Test]
        public void ExtraVerbose()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"-vv"}, WorkingDirectory);

            Assert.That(_options.LoggingThreshold, Is.EqualTo(Level.Trace));
        }

        [Test]
        public void IgnoresFolderNames()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--ignore", "Folder1", "--ignore", "Folder2"},
                WorkingDirectory);

            Assert.That(_options.IgnoreDirectoryPatterns, Is.EqualTo(new[] {"Folder1", "Folder2"}));
        }

        [Test]
        public void IncludeEfferentAssemblyReferences()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {@"--all"}, WorkingDirectory);

            Assert.That(_options.IncludeEfferentAssemblyReferences, Is.True, "IncludeEfferentAssemblyReferences");
        }

        [Test]
        public void InvalidVisualStudioVersionNumber()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--version", "dumb"}, WorkingDirectory);

            Assert.That(_options.VisualStudioVersion, Is.EqualTo(VisualStudioVersion.VS2017));
        }

        [Test]
        public void Quiet()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"-q"}, WorkingDirectory);

            Assert.That(_options.LoggingThreshold, Is.EqualTo(Level.Warn));
        }

        [Test]
        [Explicit]
        public void ShowHelp()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--help"}, WorkingDirectory);
            // check console output
        }

        [Test]
        public void ShowHelpIsSetOnOptions()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--help"}, WorkingDirectory);

            Assert.That(_options.ShowHelp, Is.True, "ShowHelp");
        }

        [Test]
        public void SpecifiedAdditionalSearchPaths()
        {
            var otherDir = GetSamplePath("OtherProjects");
            var moreProjects = GetSamplePath("MoreProjects");
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--search", otherDir, "--search", moreProjects},
                WorkingDirectory);

            Assert.That(_options.AdditionalSearchPaths, Is.EqualTo(new[] {otherDir, moreProjects}));
        }

        [Test]
        public void SpecifiedMultipleTargetProjects()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--target", "MyProject", "--target", "YourProject"},
                WorkingDirectory);

            Assert.That(_options.TargetProjectNames, Is.EqualTo(new[] {"MyProject", "YourProject"}));
            Assert.That(_options.SolutionName, Does.Match("MyProject_YourProject"));
        }

        [Test]
        public void SpecifiedProjectsRootDirectory()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--root", WorkingDirectory}, WorkingDirectory);

            Assert.That(_options.ProjectsRootDirectory, Is.EqualTo(WorkingDirectory));
        }

        [Test]
        public void SpecifiedSlnOuputPath()
        {
            var slnDir = GetSamplePath(WorkingDirectory, "Sln");
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--out", slnDir}, WorkingDirectory);

            Assert.That(_options.SlnOutputPath, Is.EqualTo(slnDir));
        }

        [Test]
        public void SpecifiedSolutionName()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--name", "MyProjects"}, WorkingDirectory);

            Assert.That(_options.SolutionName, Is.EqualTo("MyProjects"));
        }

        [Test]
        public void SpecifiedTargetProject()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--target", "MyProject"}, WorkingDirectory);

            Assert.That(_options.TargetProjectNames, Is.EqualTo(new[] {"MyProject"}));
            Assert.That(_options.SolutionName, Is.EqualTo("MyProject"));
        }

        [Test]
        public void SpecifiedVisualStudioVersion2010()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--version", "2010"}, WorkingDirectory);

            Assert.That(_options.VisualStudioVersion, Is.EqualTo(VisualStudioVersion.VS2010));
        }

        [Test]
        public void SpecifyOpenInVisualStudio()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--open"}, WorkingDirectory);

            Assert.That(_options.OpenInVisualStudio, Is.True, "OpenInVisualStudio");
        }

        [Test]
        public void TestDefaults()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new string[0], WorkingDirectory);

            Assert.That(_options.ProjectsRootDirectory, Is.EqualTo(WorkingDirectory), "ProjectsRootDirectory");
            Assert.That(_options.TargetProjectNames, Is.Empty, "TargetProjectNames");
            Assert.That(_options.Mode, Is.EqualTo(SlnGenerationMode.FullGraph), "Mode");
            Assert.That(_options.AdditionalSearchPaths, Is.Empty, "AdditionalSearchPaths");
            Assert.That(_options.IncludeEfferentAssemblyReferences, Is.False, "IncludeEfferentAssemblyReferences");
            Assert.That(_options.ShowHelp, Is.False, "ShowHelp");
            Assert.That(_options.OpenInVisualStudio, Is.False, "OpenInVisualStudio");
        }

        [Test]
        public void UnspecifiedSolutionNameWithMultipleTargetProjectsUsesFirstProjectNamePlusSuffix()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--target", "MyProject", "--target", "YourProject"},
                WorkingDirectory);
        }

        [Test]
        public void UnspecifiedSolutionNameWithSingleTargetProject()
        {
            _options = ArgsOptionsBuilder.BuildOptions(new[] {"--target", "MyProject"}, WorkingDirectory);
        }
    }
}