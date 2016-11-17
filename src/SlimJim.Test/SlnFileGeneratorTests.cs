using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SlimJim.Infrastructure;
using SlimJim.Model;

namespace SlimJim.Test
{
	[TestFixture]
	public class SlnFileGeneratorTests : TestBase
	{
		private const string TargetProject = "MyProject";
		private SlnFileGenerator _gen;
		private SlnFileWriter _slnWriter;
		private CsProjRepository _repo;
		private SlnBuilder _slnBuilder;
		private SlnGenerationOptions _options;
		private readonly List<CsProj> _projects = new List<CsProj>();
		private readonly Sln _createdSlnObject = new Sln("Sln");

		private string ProjectsDir => GetSamplePath ("Projects");

	    [SetUp]
		public void BeforeEach()
		{
			_repo = MockRepository.GenerateStrictMock<CsProjRepository>();
			_slnWriter = MockRepository.GenerateStrictMock<SlnFileWriter>();
			_slnBuilder = MockRepository.GenerateStrictMock<SlnBuilder>(new List<CsProj>());

			_gen = new SlnFileGenerator()
			{
				ProjectRepository = _repo,
				SlnWriter = _slnWriter
			};

			SlnBuilder.OverrideDefaultBuilder(_slnBuilder);
			_options = new SlnGenerationOptions(ProjectsDir);
		}

		[Test]
		public void CreatesOwnInstancesOfRepositoryAndWriter()
		{
			_gen = new SlnFileGenerator();
			Assert.That(_gen.ProjectRepository, Is.Not.Null, "Should have created instance of CsProjRepository.");
			Assert.That(_gen.SlnWriter, Is.Not.Null, "Should have created instance of SlnFileWriter.");
		}

		[Test]
		public void GeneratesSlnFileForGivenOptions()
		{
			_options.TargetProjectNames.Add(TargetProject);

			_repo.Expect(r => r.LookupCsProjsFromDirectory(_options)).Return(_projects);
			_slnBuilder.Expect(bld => bld.BuildSln(_options)).Return(_createdSlnObject);
			_slnWriter.Expect(wr => wr.WriteSlnFile(_createdSlnObject, ProjectsDir));

			_gen.GenerateSolutionFile(_options);
		}

		[TearDown]
		public void AfterEach()
		{
			_repo.VerifyAllExpectations();
			_slnWriter.VerifyAllExpectations();
			_slnBuilder.VerifyAllExpectations();
		}
	}
}