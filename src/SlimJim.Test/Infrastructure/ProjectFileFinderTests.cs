using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SlimJim.Infrastructure;
using SlimJim.Test.SampleFiles;

namespace SlimJim.Test.Infrastructure
{
    [TestFixture]
    public class ProjectFileFinderTests : TestBase
    {
        [SetUp]
        public void BeforeEach()
        {
            _finder = new ProjectFileFinder();
        }

        private static readonly string SampleFileSystemPath = SampleFileHelper.GetSampleFileSystemPath();
        private ProjectFileFinder _finder;
        private List<FileInfo> _projectFiles;

        private void AssertFilesMatching(string[] expectedPaths)
        {
            expectedPaths = expectedPaths.Select(p => p.Replace('\\', Path.DirectorySeparatorChar)).ToArray();
            Assert.That(_projectFiles.ConvertAll(file => file.FullName.Replace(SampleFileSystemPath, "")),
                Is.EqualTo(expectedPaths));
        }

        [Test]
        public void FindsOneProjectInFolderWithCsproj()
        {
            _projectFiles = _finder.FindAllProjectFiles(Path.Combine(SampleFileSystemPath, @"MyProject"));

            AssertFilesMatching(new[]
            {
                @"MyProject\MyProject.csproj"
            });
        }

        [Test]
        public void IgnoresCertainFoldersByDefault()
        {
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, ".svn"))), Is.True,
                ".svn folders ignored");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "doo.svn.wop"))),
                Is.False, "don't ignore folders with .svn in the name");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, ".hg"))), Is.True,
                ".hg folders ignored");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "doo.hg.wop"))),
                Is.False, "don't ignore folders with .hg in the name");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, ".git"))), Is.True,
                ".git folders ignored");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "doo.git.wop"))),
                Is.False, "don't ignore folders with .git in the name");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "bin"))), Is.True,
                "bin folders ignored");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "obing"))), Is.False,
                "don't ignore folders with bin in the name");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "obj"))), Is.True,
                "obj folders ignored");
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "blobjee"))), Is.False,
                "don't ignore folders with obj in the name");
        }

        [Test]
        public void IgnoresFileName()
        {
            _finder.IgnorePatterns("TheirProject3.csproj");
            _projectFiles = _finder.FindAllProjectFiles(SampleFileSystemPath);

            AssertFilesMatching(new[]
            {
                @"MyProject\MyProject.csproj",
                @"Ours\OurProject1\OurProject1.csproj",
                @"Ours\OurProject2\OurProject2.csproj",
                @"Theirs\TheirProject1\TheirProject1.csproj",
                @"Theirs\TheirProject2\TheirProject2.csproj"
            });
        }

        [Test]
        public void IgnoresRelativePath()
        {
            _finder.IgnorePatterns("Theirs");
            _projectFiles = _finder.FindAllProjectFiles(SampleFileSystemPath);

            AssertFilesMatching(new[]
            {
                @"MyProject\MyProject.csproj",
                @"Ours\OurProject1\OurProject1.csproj",
                @"Ours\OurProject2\OurProject2.csproj"
            });
        }

        [Test]
        public void IgnoresRelativePathWithDifferentCase()
        {
            _finder.IgnorePatterns("ThEiRs");
            _projectFiles = _finder.FindAllProjectFiles(SampleFileSystemPath);

            AssertFilesMatching(new[]
            {
                @"MyProject\MyProject.csproj",
                @"Ours\OurProject1\OurProject1.csproj",
                @"Ours\OurProject2\OurProject2.csproj"
            });
        }

        [Test]
        public void IgnoresReSharperFolders()
        {
            Assert.That(
                _finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "_ReSharper.Something"))),
                Is.True);
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "ReSharper"))), Is.True);
            Assert.That(_finder.PathIsIgnored(new DirectoryInfo(Path.Combine(WorkingDirectory, "___ReSharper___"))),
                Is.True);
        }

        [Test]
        public void ReturnsFileInfosForEachProjectInFileSystem()
        {
            _projectFiles = _finder.FindAllProjectFiles(SampleFileSystemPath);

            AssertFilesMatching(new[]
            {
                @"MyProject\MyProject.csproj",
                @"Ours\OurProject1\OurProject1.csproj",
                @"Ours\OurProject2\OurProject2.csproj",
                @"Theirs\TheirProject1\TheirProject1.csproj",
                @"Theirs\TheirProject2\TheirProject2.csproj",
                @"Theirs\TheirProject3\TheirProject3.csproj"
            });
        }
    }
}