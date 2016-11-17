using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using NDesk.Options;
using SlimJim.Model;
using SlimJim.Util;

namespace SlimJim.Infrastructure
{
    public class ArgsOptionsBuilder
    {
        private static ILog _log = LogManager.GetLogger(typeof (ArgsOptionsBuilder));
        private SlnGenerationOptions _options;
        private OptionSet _optionSet;

        public static SlnGenerationOptions BuildOptions(string[] args, string workingDirectory)
        {
            var builder = new ArgsOptionsBuilder();
            return builder.Build(args, workingDirectory);
        }

        public SlnGenerationOptions Build(string[] args, string workingDirectory)
        {
            _options = new SlnGenerationOptions(workingDirectory);

            ProcessArguments(args);

            return _options;
        }

        private void ProcessArguments(string[] args)
        {
            _optionSet = new OptionSet
                            {
                                { "t|target=", "{NAME} of a target project (repeat for multiple targets)", 
                                    v => _options.TargetProjectNames.Add(v) },
                                { "r|root=", "{PATH} to the root directory where your projects reside (optional, defaults to working directory)", 
                                    v => _options.ProjectsRootDirectory = v },
                                { "s|search=", "additional {PATH}(s) to search for projects to include outside of the root directory (repeat for multiple paths)",
                                    v => _options.AddAdditionalSearchPaths(v) },
                                { "o|out=", "directory {PATH} where you want the .sln file written", 
                                    v => _options.SlnOutputPath = v },
                                { "version=", "Visual Studio {VERSION} compatibility (2008, 2010, 2012, 2013, 2015 default)", 
                                    v => _options.VisualStudioVersion = TryParseVersionNumber(v) },
                                { "n|name=", "alternate {NAME} for solution file", 
                                    v => _options.SolutionName = v},
                                { "m|minimal", "skip all afferent assembly references (included by default)",
                                    v => _options.SkipAfferentAssemblyReferences = true },
                                { "a|all", "include all efferent assembly references (omitted by default)", 
                                    v => _options.IncludeEfferentAssemblyReferences = true },
                                { "h|help", "display the help screen", 
                                    v => _options.ShowHelp = true },
                                { "i|ignore=", "ignore directories whose name matches the given {REGEX_PATTERN} (repeat for multiple ignores)", 
                                    v => _options.AddIgnoreDirectoryPatterns(v) },
                                { "c|convert", "convert assembly references in csproj files to project references", 
                                    v => _options.ConvertReferences = true },
                                { "u|unconvert", "unconvert (restore) assembly references that were previously converted", 
                                    v => _options.RestoreReferences = true },
                                { "H|fixhintpaths", "convert hint paths in csproj files to point to nuget packages folder relative to generated sln",
                                    v => _options.FixHintPaths = true },
                                { "U|restorehintpaths", "unconvert hint paths in csproj files to point to their original nuget packages folder",
                                    v => _options.RestoreHintPaths = true},
                                { "open", "open the solution in Visual Studio", 
                                    v => _options.OpenInVisualStudio = true },
                                { "l|loadandsave", "load the solution in Visual Studio and save it",
                                    v => _options.LoadAndSaveSolutionWithVS= true },
                                { "debug", "attach debugger", 
                                    v => Debugger.Launch() },
                                { "q|quiet", "reduce logging verbosity (can specify multiple times)",
                                    v => _options.LoggingThreshold = _options.LoggingThreshold.DecreaseVerbosity() },
                                { "v|verbose", "increase logging verbosity (can specify multiple times)",
                                    v => _options.LoggingThreshold = _options.LoggingThreshold.IncreaseVerbosity() }
                            };

            try
            {
                _optionSet.Parse(args);
            }
            catch (OptionException optEx)
            {
                ParseError?.Invoke(optEx.Message);
            }
        }

        public void WriteHelpMessage()
        {
            var helpMessage = new StringBuilder();
            using (var helpMessageWriter = new StringWriter(helpMessage))
            {
                helpMessageWriter.WriteLine("Usage: slimjim [OPTIONS]+");
                helpMessageWriter.WriteLine(
                    "Generate a Visual Studio .sln file for a given directory of projects and one or more target project names.");
                helpMessageWriter.WriteLine();
                helpMessageWriter.WriteLine("Options:");
                _optionSet.WriteOptionDescriptions(helpMessageWriter);
            }

            _log.Info(helpMessage.ToString());
        }

        private VisualStudioVersion TryParseVersionNumber(string versionNumber)
        {
            VisualStudioVersion parsedVersion = VisualStudioVersion.ParseVersionString(versionNumber);

            if (parsedVersion == null)
            {
                parsedVersion = VisualStudioVersion.VS2010;
            }

            return parsedVersion;
        }

        public event Action<string> ParseError;
    }
}