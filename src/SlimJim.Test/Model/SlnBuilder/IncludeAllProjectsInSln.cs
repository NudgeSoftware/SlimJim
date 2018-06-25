using NUnit.Framework;

namespace SlimJim.Test.Model.SlnBuilder
{
    [TestFixture]
    public class IncludeAllProjectsInSln : SlnBuilderTestFixture
    {
        [Test]
        public void WithNoTargetsInOptionsAllProjectsAreIncluded()
        {
            GeneratePartialGraphSolution(new string[0], Projects.MyProject, Projects.OurProject1,
                Projects.TheirProject1, Projects.Unrelated1);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject, Projects.OurProject1,
                Projects.TheirProject1, Projects.Unrelated1
            }));
        }
    }
}