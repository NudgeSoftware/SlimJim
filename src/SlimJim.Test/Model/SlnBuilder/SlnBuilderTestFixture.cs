using System.Collections.Generic;
using NUnit.Framework;
using SlimJim.Model;

namespace SlimJim.Test.Model.SlnBuilder
{
    public class SlnBuilderTestFixture : TestBase
    {
        protected SlnGenerationOptions Options;
        protected ProjectPrototypes Projects;
        protected Sln Solution;
        protected string TargetProjectName;

        [SetUp]
        public void BeforeEach()
        {
            Projects = new ProjectPrototypes();
            TargetProjectName = Projects.MyProject.AssemblyName;
            Options = new SlnGenerationOptions(GetSamplePath("Projects"));
        }

        protected void GeneratePartialGraphSolution(string[] targetProjectNames, params CsProj[] projectsList)
        {
            var generator = new SlimJim.Model.SlnBuilder(new List<CsProj>(projectsList));
            Options.AddTargetProjectNames(targetProjectNames);
            Solution = generator.BuildSln(Options);
        }
    }
}