using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using SlimJim.Infrastructure;
using SlimJim.Model;
using SlimJim.Test.SampleFiles;

namespace SlimJim.Test.Infrastructure
{
    [TestFixture]
    public class CsProjRepositoryTests : TestBase
    {
        [SetUp]
        public void BeforeEach()
        {
            _options = new SlnGenerationOptions(WorkingDirectory);
            _finder = MockRepository.GenerateStrictMock<ProjectFileFinder>();
            _reader = MockRepository.GenerateStrictMock<CsProjReader>();
            _repository = new CsProjRepository
            {
                Finder = _finder,
                Reader = _reader
            };
        }

        [TearDown]
        public void AfterEach()
        {
            _finder.VerifyAllExpectations();
            _reader.VerifyAllExpectations();
        }

        private string SearchPath1 => GetSamplePath("OtherProjects");

        private string SearchPath2 => GetSamplePath("MoreProjects");

        private readonly FileInfo _file1 = SampleFileHelper.GetCsProjFile("Simple");
        private readonly FileInfo _file2 = SampleFileHelper.GetCsProjFile("Simple");
        private readonly CsProj _proj1 = new CsProj {AssemblyName = "Proj1"};
        private readonly CsProj _proj2 = new CsProj {AssemblyName = "Proj1"};
        private ProjectFileFinder _finder;
        private CsProjReader _reader;
        private CsProjRepository _repository;
        private SlnGenerationOptions _options;

        [Test]
        public void CreatesOwnInstancesOfFinderAndReader()
        {
            _repository = new CsProjRepository();
            Assert.That(_repository.Finder, Is.Not.Null, "Should have created instance of CsProjFinder.");
            Assert.That(_repository.Reader, Is.Not.Null, "Should have created instance of CsProjReader.");
        }

        [Test]
        public void GetsFilesFromFinderAndProcessesThemWithCsProjReader()
        {
            _finder.Expect(f => f.FindAllProjectFiles(WorkingDirectory)).Return(new List<FileInfo> {_file1, _file2});
            _reader.Expect(r => r.Read(_file1)).Return(_proj1);
            _reader.Expect(r => r.Read(_file2)).Return(_proj2);

            var projects = _repository.LookupCsProjsFromDirectory(_options);

            Assert.That(projects, Is.EqualTo(new[] {_proj1, _proj2}));
        }

        [Test]
        public void GracefullyHandlesNullsFromReader()
        {
            _finder.Expect(f => f.FindAllProjectFiles(WorkingDirectory)).Return(new List<FileInfo> {_file1, _file2});
            _reader.Expect(r => r.Read(_file1)).Return(_proj1);
            _reader.Expect(r => r.Read(_file2)).Return(null);

            var projects = _repository.LookupCsProjsFromDirectory(_options);

            Assert.That(projects, Is.EqualTo(new[] {_proj1}));
        }

        [Test]
        public void IngoresDirectoryPatternsInOptions()
        {
            _options.AddIgnoreDirectoryPatterns("Folder1", "Folder2");
            _finder.Expect(f => f.IgnorePatterns("Folder1", "Folder2"));
            _finder.Expect(f => f.FindAllProjectFiles(WorkingDirectory)).Return(new List<FileInfo>());

            _repository.LookupCsProjsFromDirectory(_options);
        }

        [Test]
        public void ReadsFilesFromAdditionalSearchPathsAsWell()
        {
            _options.AddAdditionalSearchPaths(SearchPath1, SearchPath2);
            _finder.Expect(f => f.FindAllProjectFiles(WorkingDirectory)).Return(new List<FileInfo>());
            _finder.Expect(f => f.FindAllProjectFiles(SearchPath1)).Return(new List<FileInfo>());
            _finder.Expect(f => f.FindAllProjectFiles(SearchPath2)).Return(new List<FileInfo>());

            _repository.LookupCsProjsFromDirectory(_options);
        }
    }
}