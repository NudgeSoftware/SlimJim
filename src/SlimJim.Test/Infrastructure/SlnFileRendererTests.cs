using NUnit.Framework;
using SlimJim.Infrastructure;
using SlimJim.Model;
using SlimJim.Test.Model;
using SlimJim.Test.SampleFiles;

namespace SlimJim.Test.Infrastructure
{
	[TestFixture]
	public class SlnFileRendererTests
	{
		private Sln _solution;
		private SlnFileRenderer _renderer;
		private ProjectPrototypes _projects;

		[SetUp]
		public void BeforeEach()
		{
			_projects = new ProjectPrototypes();
		}

		[Test]
		public void EmptySolution()
		{
			MakeSolution("BlankSolution");

			TestRender();
		}

		[Test]
		public void SingleProjectSolution()
		{
			MakeSolution("SingleProject", _projects.MyProject);

			TestRender();
		}

		[Test]
		public void ThreeProjectSolution()
		{
			MakeSolution("ThreeProjects", _projects.MyProject, _projects.OurProject1, _projects.OurProject2);

			TestRender();
		}

		[Test]
		public void ManyProjectSolution()
		{
			MakeSolution("ManyProjects", _projects.MyProject, _projects.OurProject1, _projects.OurProject2,
				_projects.TheirProject1, _projects.TheirProject2, _projects.TheirProject3);

			TestRender();
		}

		[Test]
		public void VisualStudio2008Solution()
		{
			MakeSolution("VS2008");
			_solution.Version = VisualStudioVersion.VS2008;

			TestRender();
		}

		private void MakeSolution(string name, params CsProj[] csProjs)
		{
			_solution = new Sln(name, "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
			_solution.AddProjects(csProjs);
		}

		private void TestRender()
		{
			_renderer = new SlnFileRenderer(_solution);

			string actualContents = _renderer.Render().Replace("\r\n", "\n").Replace("\n\n", "\n");
			string expectedContents = SampleFileHelper.GetSlnFileContents(_solution.Name).Replace("\r\n", "\n").Replace("\n\n", "\n");

			Assert.That(actualContents, Is.EqualTo(expectedContents));
		}
	}
}