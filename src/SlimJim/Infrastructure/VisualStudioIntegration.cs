using System;
using System.Diagnostics;
using System.IO;
using EnvDTE80;
using log4net;
using Microsoft.Win32;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
    public static class VisualStudioIntegration
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SlnFileGenerator));

        public static void OpenSolution(string solutionPath, VisualStudioVersion visualStudioVersion)
        {
            var devenvPath = FindDevEnv(visualStudioVersion);

            if (devenvPath == null || !File.Exists(devenvPath))
            {
                Log.ErrorFormat("Unable to locate Visual Studio {0} install directory.", visualStudioVersion.Year);
                return;
            }

            var info = new ProcessStartInfo(devenvPath, '"' + solutionPath + '"');

            Process.Start(info);
        }

        private static string FindDevEnv(VisualStudioVersion visualStudioVersion)
        {
            var key = @"Software\Microsoft\VisualStudio\" + visualStudioVersion.PathVersionNumber;
            var wowKey = @"Software\Wow6432Node\Microsoft\VisualStudio\" + visualStudioVersion.PathVersionNumber;

            var r = Registry.LocalMachine.OpenSubKey(wowKey) ?? Registry.LocalMachine.OpenSubKey(key);

            var val = r?.GetValue("InstallDir");

            return val == null ? null : Path.Combine(val.ToString(), "devenv.exe");
        }

        public static void LoadAndSaveSolutionInVS(string solutionPath, VisualStudioVersion visualStudioVersion)
        {
            if (visualStudioVersion.Year == "2015" || visualStudioVersion.Year == "2017")
            {
                Log.Info($"Loading and Saving the Solution in Visual Studio {visualStudioVersion.Year}");
                var t = Type.GetTypeFromProgID("VisualStudio.DTE.14.0", true);
                var dte = (DTE2) Activator.CreateInstance(t, true);

                dte.Solution.Open(solutionPath);
                dte.Solution.SaveAs(solutionPath);

                dte.Quit();
            }
            else
            {
                Log.Error($"LoadAndSaveSolutionInVS is yet supported for Visual Studio {visualStudioVersion.Year}");
            }
        }
    }
}