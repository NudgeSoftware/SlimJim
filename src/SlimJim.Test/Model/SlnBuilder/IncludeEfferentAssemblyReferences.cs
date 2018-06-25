using NUnit.Framework;

namespace SlimJim.Test.Model.SlnBuilder
{
    [TestFixture]
    public class IncludeEfferentAssemblyReferences : SlnBuilderTestFixture
    {
        [Test]
        public void EfferentAssemblyReferencesAreIncludedForAllProjectsInSln()
        {
            Options.IncludeEfferentAssemblyReferences = true;
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1, Projects.TheirProject2);
            Projects.TheirProject2.ReferencesProjects(Projects.TheirProject3);
            Projects.OurProject1.ReferencesAssemblies(Projects.MyProject, Projects.OurProject2);
            GeneratePartialGraphSolution(new[] {TargetProjectName},
                Projects.MyProject,
                Projects.TheirProject1,
                Projects.TheirProject2,
                Projects.TheirProject3,
                Projects.OurProject1,
                Projects.OurProject2);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject,
                Projects.TheirProject1,
                Projects.TheirProject2,
                Projects.TheirProject3,
                Projects.OurProject1,
                Projects.OurProject2
            }));
        }
    }
}