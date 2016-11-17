using NUnit.Framework;
using SlimJim.Model;
using System.IO;

namespace SlimJim.Test.Model
{
	[TestFixture]
	public class SlnGenerationOptionsTests : TestBase
	{
		private SlnGenerationOptions _options;

		[Test]
		public void SolutionOutputPathDefaultsToProjectsRootPath()
		{
			_options = new SlnGenerationOptions(GetSamplePath("Projects"));

			Assert.That(_options.SlnOutputPath, Is.EqualTo(_options.ProjectsRootDirectory));
		}

		[Test]
		public void SolutionOutputPathUsesGivenValueIfSet()
		{
			string slnOutputPath = GetSamplePath("Projects", "Solutions");
			_options = new SlnGenerationOptions(GetSamplePath("Projects")) {SlnOutputPath = slnOutputPath};

			Assert.That(_options.SlnOutputPath, Is.EqualTo(slnOutputPath));
		}

		[Test]
		public void UnspecifiedSolutionNameWithNoTargetProjectsUsesFolderName()
		{
			_options = new SlnGenerationOptions(WorkingDirectory);
			Assert.That(_options.SolutionName, Is.EqualTo("WorkingDir"));

			_options = new SlnGenerationOptions(Path.Combine(WorkingDirectory, "SlumJim"));
			Assert.That(_options.SolutionName, Is.EqualTo("SlumJim"));

			_options = new SlnGenerationOptions(Path.Combine(WorkingDirectory, "SlumJim") + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
			Assert.That(_options.SolutionName, Is.EqualTo("SlumJim"));

			_options = new SlnGenerationOptions(Path.DirectorySeparatorChar.ToString());
			Assert.That(_options.SolutionName, Is.EqualTo("SlimJim"));
		}

		[Test]
		public void AdditionalSearchPathsRootedAtProjectRoot()
		{
			var root = GetSamplePath ("Proj", "Root");
			_options = new SlnGenerationOptions(root);
			var path1 = Path.Combine("..", "SearchPath");
			var path2 = Path.Combine("..", "..", "OtherPath", "Pork");
			_options.AddAdditionalSearchPaths (path1, path2);

			Assert.That(_options.AdditionalSearchPaths, Is.EqualTo(new[] {Path.Combine(root, path1), Path.Combine(root, path2)}));
		}

		[Test]
		public void RelativeSlnOutputPathRootedAtProjectsRoot()
		{
			var root = GetSamplePath ("Proj", "Root");
			_options = new SlnGenerationOptions (root);
			_options.SlnOutputPath = "Solutions";

			Assert.That(_options.SlnOutputPath, Is.EqualTo(Path.Combine(root, "Solutions")));
		}

		[Test]
		public void RelativeProjectsRootDirIsRootedAtWorkingDir()
		{
			_options = new SlnGenerationOptions(WorkingDirectory);
			_options.ProjectsRootDirectory = Path.Combine("Proj", "Root");

			Assert.That(_options.SlnOutputPath, Is.EqualTo (Path.Combine (WorkingDirectory, "Proj", "Root")));
		}
	}
}