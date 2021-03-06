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
        [SetUp]
        public void BeforeEach()
        {
            _projects = new ProjectPrototypes();
        }

        private Sln _solution;
        private SlnFileRenderer _renderer;
        private ProjectPrototypes _projects;

        private void MakeSolution(string name, params CsProj[] csProjs)
        {
            _solution = new Sln(name, "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
            {
                ProjectsRootDirectory = @"C:\Code\src"
            };

            _solution.AddProjects(csProjs);
        }

        private void TestRender()
        {
            _renderer = new SlnFileRenderer(_solution);

            var actualContents = _renderer.Render().Replace("\r\n", "\n").Replace("\n\n", "\n");
            var expectedContents = SampleFileHelper.GetSlnFileContents(_solution.Name).Replace("\r\n", "\n")
                .Replace("\n\n", "\n");

            Assert.That(actualContents, Is.EqualTo(expectedContents));
        }

        [Test]
        public void EmptySolution()
        {
            MakeSolution("BlankSolution");

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
    }
}