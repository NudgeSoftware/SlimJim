using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SlimJim.Model;
using log4net;
using System.Reflection;

namespace SlimJim.Infrastructure
{
    public class CsProjReader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly XNamespace Ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        public virtual CsProj Read(FileInfo csProjFile)
        {
            var xml = LoadXml(csProjFile);
            var properties = xml.Element(Ns + "PropertyGroup");

            var assemblyName = properties?.Element(Ns + "AssemblyName");

            if (assemblyName == null) return null;

            return new CsProj
            {
                Path = GetRelativePath(csProjFile.FullName, Environment.CurrentDirectory),
                AssemblyName = assemblyName.Value,
                Guid = properties.Element(Ns + "ProjectGuid").ValueOrDefault()?.ToUpper(),
                ProjectTypeGuid = GetMainProjectTypeGuid(properties.Element(Ns + "ProjectTypeGuids").ValueOrDefault()),
                TargetFrameworkVersion = properties.Element(Ns + "TargetFrameworkVersion").ValueOrDefault(),
                ReferencedAssemblyNames = ReadReferencedAssemblyNames(xml),
                ReferencedProjectGuids = ReadReferencedProjectGuids(xml, csProjFile),
                UsesMsBuildPackageRestore = FindImportedNuGetTargets(xml)
            };
        }

        private static string GetMainProjectTypeGuid(string projectTypeGuidsString)
        {
            if (string.IsNullOrEmpty(projectTypeGuidsString))
            {
                return "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
            }

            var guids = projectTypeGuidsString.Split(';');

            if (guids.Length == 1)
            {
                return guids.FirstOrDefault()?.ToUpperInvariant();
            }


            return guids[1].ToUpperInvariant();
        }

        public static string GetRelativePath(string absolutePath, string folder)
        {
            Uri pathUri = new Uri(absolutePath);

            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private XElement LoadXml(FileInfo csProjFile)
        {
            XElement xml;
            using (var reader = csProjFile.OpenText())
            {
                xml = XElement.Load(reader);
            }
            return xml;
        }

        private List<string> ReadReferencedAssemblyNames(XElement xml)
        {
            var rawAssemblyNames = (from r in xml.DescendantsAndSelf(Ns + "Reference")
                                    where r.Parent.Name == Ns + "ItemGroup"
                                    select r.Attribute("Include").Value).ToList();
            var unQualifiedAssemblyNames = rawAssemblyNames.ConvertAll(UnQualify);
            return unQualifiedAssemblyNames;
        }

        private string UnQualify(string name)
        {
            if (!name.Contains(",")) return name;

            return name.Substring(0, name.IndexOf(","));
        }

        private List<string> ReadReferencedProjectGuids(XElement xml, FileInfo csprojFile)
        {
            return (from pr in xml.DescendantsAndSelf(Ns + "ProjectReference")
                    select ReadProjectGuid(pr, csprojFile)).ToList();
        }

        private string ReadProjectGuid(XElement projectReference, FileInfo csprojFile)
        {
            var project = projectReference.Element(Ns + "Project");
            if (project == null)
            {
                Log.WarnFormat("No project Guid for {0}. Fixing...", projectReference.Element(Ns + "Name").Value);
                var filename = projectReference.Attribute("Include")?.Value;
                if (filename == null)
                {
                    Log.WarnFormat("No Include= attribute for project {0}.", projectReference.Element(Ns + "Name").Value);
                    return null;
                }
                filename = Path.Combine(csprojFile.Directory.FullName, filename);
                var projectFile = LoadXml(new FileInfo(filename));
                var projectGuid = projectFile?.Element(Ns + "PropertyGroup")?.Element(Ns + "ProjectGuid");
                if (projectGuid == null)
                {
                    Log.WarnFormat("ProjectGuid not found in {0}", filename);
                    return null;
                }
                return projectGuid.Value.ToUpper();
            }
            return project.Value.ToUpper();
        }

        private bool FindImportedNuGetTargets(XElement xml)
        {
            var importPaths = (from import in xml.DescendantsAndSelf(Ns + "Import")
                               select import.Attribute("Project").Value);
            return importPaths.Any(p => p.EndsWith(@"\.nuget\nuget.targets", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}