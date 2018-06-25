using NUnit.Framework;
using SlimJim.Model;

namespace SlimJim.Test.Model.SlnBuilder
{
    [TestFixture]
    public class SlnBuilderTests : SlnBuilderTestFixture
    {
        [Test]
        public void AfferentAssemblyReferenceReferencingOtherProjects()
        {
            Projects.OurProject1.ReferencesAssemblies(Projects.MyProject);
            Projects.OurProject1.ReferencesAssemblies(Projects.Unrelated1);
            Projects.OurProject1.ReferencesAssemblies(Projects.Unrelated2);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.OurProject1,
                Projects.Unrelated1, Projects.Unrelated2);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject, Projects.OurProject1
            }));
        }

        [Test]
        public void AfferentAssemblyReferencesSelectsNearestTargetFramework()
        {
            Projects.MyProject.TargetFrameworkVersion = "v4.5";
            Projects.MyProject.ReferencesAssemblies(Projects.MyMultiFrameworkProject40);
            Options.IncludeEfferentAssemblyReferences = true;
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject,
                Projects.MyMultiFrameworkProject35, Projects.MyMultiFrameworkProject40);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject, Projects.MyMultiFrameworkProject40}));
        }

        [Test]
        public void AfferentAssemblyReferencesSelectsTargetFramework()
        {
            Projects.MyProject.ReferencesAssemblies(Projects.MyMultiFrameworkProject35);
            Options.IncludeEfferentAssemblyReferences = true;
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject,
                Projects.MyMultiFrameworkProject35, Projects.MyMultiFrameworkProject40);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject, Projects.MyMultiFrameworkProject35}));
        }

        [Test]
        public void EmptySln()
        {
            GeneratePartialGraphSolution(new[] {TargetProjectName});
            Assert.That(Solution.Projects, Is.Empty);
        }

        [Test]
        public void MixedAfferentAndEfferentReferences()
        {
            Projects.OurProject1.ReferencesAssemblies(Projects.MyProject);
            Projects.OurProject1.ReferencesProjects(Projects.TheirProject1);
            Projects.OurProject2.ReferencesProjects(Projects.MyProject);
            Projects.OurProject2.ReferencesAssemblies(Projects.TheirProject2);
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1, Projects.TheirProject2);
            Projects.MyProject.ReferencesProjects(Projects.TheirProject3);
            Projects.TheirProject3.ReferencesAssemblies(Projects.Unrelated1);
            Projects.TheirProject3.ReferencesProjects(Projects.Unrelated2);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1,
                Projects.TheirProject2, Projects.TheirProject3, Projects.OurProject1, Projects.OurProject2,
                Projects.Unrelated1, Projects.Unrelated2);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject, Projects.TheirProject3, Projects.Unrelated2,
                Projects.OurProject1, Projects.TheirProject1, Projects.OurProject2
            }));
        }

        [Test]
        public void MultipleAfferentAssemblyReferences()
        {
            Projects.OurProject1.ReferencesAssemblies(Projects.MyProject);
            Projects.OurProject2.ReferencesAssemblies(Projects.MyProject);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.OurProject1,
                Projects.OurProject2);
            Assert.That(Solution.Projects,
                Is.EqualTo(new[] {Projects.MyProject, Projects.OurProject1, Projects.OurProject2}));
        }

        [Test]
        public void MultipleAfferentProjectReferences()
        {
            Projects.OurProject1.ReferencesProjects(Projects.MyProject);
            Projects.OurProject2.ReferencesProjects(Projects.MyProject);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.OurProject1,
                Projects.OurProject2);
            Assert.That(Solution.Projects,
                Is.EqualTo(new[] {Projects.MyProject, Projects.OurProject1, Projects.OurProject2}));
        }

        [Test]
        public void MultipleTargetProjects()
        {
            Projects.OurProject2.ReferencesAssemblies(Projects.MyProject, Projects.OurProject1);
            Projects.MyProject.ReferencesProjects(Projects.TheirProject1);
            Projects.OurProject1.ReferencesProjects(Projects.TheirProject2);
            GeneratePartialGraphSolution(new[] {Projects.MyProject.AssemblyName, Projects.OurProject1.AssemblyName},
                Projects.MyProject, Projects.OurProject1, Projects.OurProject2, Projects.TheirProject1,
                Projects.TheirProject2);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject, Projects.TheirProject1, Projects.OurProject2,
                Projects.OurProject1, Projects.TheirProject2
            }));
        }

        [Test]
        public void ProjectListDoesNotContainTargetProject()
        {
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.Unrelated1);
            Assert.That(Solution.Projects, Is.Empty);
        }

        [Test]
        public void ReferencesAssemblyNotInProjectsList()
        {
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }

        [Test]
        public void ReferencesProjectNotInProjectsList()
        {
            Projects.MyProject.ReferencesProjects(Projects.TheirProject1);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }

        [Test]
        public void SingleEfferentAssemblyReference()
        {
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }

        [Test]
        public void SingleEfferentAssemblyReferenceAndUnRelatedProjectInList()
        {
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1,
                Projects.Unrelated1);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }

        [Test]
        public void SingleEfferentAssemblyReferenceToSubtree()
        {
            Projects.MyProject.ReferencesAssemblies(Projects.TheirProject1);
            Projects.TheirProject1.ReferencesAssemblies(Projects.TheirProject2, Projects.TheirProject3);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1,
                Projects.TheirProject2, Projects.TheirProject3);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject
            }));
        }

        [Test]
        public void SingleEfferentProjectReference()
        {
            Projects.MyProject.ReferencesProjects(Projects.TheirProject1);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject, Projects.TheirProject1}));
        }

        [Test]
        public void SingleEfferentProjectReferenceToSubtree()
        {
            Projects.MyProject.ReferencesProjects(Projects.TheirProject1);
            Projects.TheirProject1.ReferencesProjects(Projects.TheirProject2, Projects.TheirProject3);
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.TheirProject1,
                Projects.TheirProject2, Projects.TheirProject3);
            Assert.That(Solution.Projects, Is.EqualTo(new[]
            {
                Projects.MyProject, Projects.TheirProject1, Projects.TheirProject2, Projects.TheirProject3
            }));
        }

        [Test]
        public void SingleProjectSln()
        {
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }

        [Test]
        public void SlnNameIsEqualToRootProjectName()
        {
            GeneratePartialGraphSolution(new[] {TargetProjectName});
            Assert.That(Solution.Name, Is.EqualTo(TargetProjectName));
        }

        [Test]
        public void SlnVersionEqualToVersionFromOptions()
        {
            Options.VisualStudioVersion = VisualStudioVersion.VS2010;
            GeneratePartialGraphSolution(new string[0]);
            Assert.That(Solution.Version, Is.EqualTo(VisualStudioVersion.VS2010));
        }

        [Test]
        public void UnrelatedProjectListProducesSingleProjectGraph()
        {
            GeneratePartialGraphSolution(new[] {TargetProjectName}, Projects.MyProject, Projects.Unrelated1,
                Projects.Unrelated2);
            Assert.That(Solution.Projects, Is.EqualTo(new[] {Projects.MyProject}));
        }
    }
}