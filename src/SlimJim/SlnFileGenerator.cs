using System.IO;
using log4net;
using SlimJim.Infrastructure;
using SlimJim.Model;

namespace SlimJim
{
    public class SlnFileGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SlnFileGenerator));

        public SlnFileGenerator()
        {
            ProjectRepository = new CsProjRepository();
            SlnWriter = new SlnFileWriter();
        }

        public CsProjRepository ProjectRepository { get; set; }
        public SlnFileWriter SlnWriter { get; set; }

        public string GenerateSolutionFile(SlnGenerationOptions options)
        {
            LogSummary(options);

            var projects = ProjectRepository.LookupCsProjsFromDirectory(options);
            var solution = SlnBuilder.GetSlnBuilder(projects).BuildSln(options);

            if (options.FixHintPaths) new HintPathConverter().ConvertHintPaths(solution, options);

            if (options.ConvertReferences)
                new ReferenceConverter().ConvertToProjectReferences(solution);
            else if (options.RestoreReferences) new ReferenceConverter().RestoreAssemblyReferences(solution);

            if (options.RestoreHintPaths) new HintPathConverter().RestoreHintPaths(solution, options);

            return SlnWriter.WriteSlnFile(solution, options.SlnOutputPath).FullName;
        }

        private void LogSummary(SlnGenerationOptions options)
        {
            Log.InfoFormat("SlimJim solution file generator.");
            Log.InfoFormat("");
            Log.InfoFormat("----------------------------------------");
            Log.InfoFormat("Target projects:\t{0}", SummarizeTargetProjects(options));
            Log.InfoFormat("Destination:\t\t{0}", Path.Combine(options.SlnOutputPath, options.SolutionName + ".sln"));
            Log.InfoFormat("Project References:\t{0}",
                options.ConvertReferences ? "Convert" : options.RestoreReferences ? "Restore" : "Do Nothing");
            Log.InfoFormat("Hint Paths:\t\t{0}",
                options.FixHintPaths ? "Adjust" : options.RestoreReferences ? "Restore" : "Do Nothing");
            Log.InfoFormat("Visual Studio Version:\t{0}", options.VisualStudioVersion);
            Log.InfoFormat("----------------------------------------");
            Log.InfoFormat("");
        }

        private string SummarizeTargetProjects(SlnGenerationOptions options)
        {
            var targets = string.Join(", ", options.TargetProjectNames);

            return string.IsNullOrEmpty(targets) ? "<none>" : targets;
        }
    }
}