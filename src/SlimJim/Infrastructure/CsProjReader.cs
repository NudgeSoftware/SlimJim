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
        private static readonly XNamespace LegacyNs = "http://schemas.microsoft.com/developer/msbuild/2003";

        public virtual CsProj Read(FileInfo csProjFile)
        {
            var xml = LoadXml(csProjFile);
            var properties = xml.Element(LegacyNs + "PropertyGroup");

            var assemblyName = properties?.Element(LegacyNs + "AssemblyName");

            if (assemblyName != null)
            {
                return new CsProj
                {
                    Path = GetRelativePath(csProjFile.FullName, Environment.CurrentDirectory),
                    AssemblyName = assemblyName.Value,
                    Guid = properties.Element(LegacyNs + "ProjectGuid").ValueOrDefault()?.ToUpper(),
                    ProjectTypeGuid = GetMainProjectTypeGuid(properties.Element(LegacyNs + "ProjectTypeGuids").ValueOrDefault()),
                    TargetFrameworkVersion = properties.Element(LegacyNs + "TargetFrameworkVersion").ValueOrDefault(),
                    ReferencedAssemblyNames = ReadReferencedAssemblyNames(xml),
                    ReferencedProjects = ReadLegacyProjectReferences(xml, csProjFile),
                    Platform = FindPlatformTargetLegacy(xml)
                };
            }

            // new CsProj format
            return new CsProj
            {
                Path = GetRelativePath(csProjFile.FullName, Environment.CurrentDirectory),
                AssemblyName = csProjFile.Name.Replace(".csproj", string.Empty),
                ProjectTypeGuid = GetMainProjectTypeGuid("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"),
                Guid = Guid.Empty.ToString(), // will be filled in later once we find what it is from other projects that reference it
                ReferencedProjects = ReadNewProjectReferences(xml),
                Platform = FindPlatformTarget(xml)
            };
        }

        private static string GetMainProjectTypeGuid(string projectTypeGuidsString)
        {
            if (string.IsNullOrEmpty(projectTypeGuidsString))
            {
                // Default to legacy csproj Guid
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
            var rawAssemblyNames = (from r in xml.DescendantsAndSelf(LegacyNs + "Reference")
                                    where r.Parent?.Name == LegacyNs + "ItemGroup"
                                    select r.Attribute("Include")?.Value).ToList();
            var unQualifiedAssemblyNames = rawAssemblyNames.ConvertAll(UnQualify);
            return unQualifiedAssemblyNames;
        }

        private string UnQualify(string name)
        {
            if (!name.Contains(",")) return name;

            return name.Substring(0, name.IndexOf(",", StringComparison.Ordinal));
        }

        private Dictionary<string, string> ReadLegacyProjectReferences(XElement xml, FileInfo csProjFile)
        {
            return (from pr in xml.DescendantsAndSelf(LegacyNs + "ProjectReference")
                    select (GetProjectNameFromPath(pr), ReadProjectGuid(pr, csProjFile))).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private static string GetProjectNameFromPath(XElement pr)
        {
            var file = new FileInfo(pr.Attribute("Include")?.Value ?? throw new InvalidOperationException());

            return file.Name.Replace(".csproj", string.Empty);
        }

        private Dictionary<string, string> ReadNewProjectReferences(XElement xml)
        {
            // Guid will be populated later
            return (from pr in xml.DescendantsAndSelf("ProjectReference")
                    select (GetProjectNameFromPath(pr), Guid.Empty.ToString())).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private string ReadProjectGuid(XElement projectReference, FileInfo csprojFile)
        {
            var project = projectReference.Element(LegacyNs + "Project");
            if (project == null)
            {
                Log.WarnFormat("No project Guid for {0}. Fixing...", projectReference.Element(LegacyNs + "Name")?.Value);
                var filename = projectReference.Attribute("Include")?.Value;
                if (filename == null)
                {
                    Log.WarnFormat("No Include= attribute for project {0}.",
                        projectReference.Element(LegacyNs + "Name")?.Value);
                    return null;
                }
                filename = Path.Combine(csprojFile.Directory?.FullName ?? throw new InvalidOperationException(), filename);
                var projectFile = LoadXml(new FileInfo(filename));
                var projectGuid = projectFile?.Element("PropertyGroup")?.Element(LegacyNs + "ProjectGuid");
                if (projectGuid == null)
                {
                    Log.WarnFormat("ProjectGuid not found in {0}", filename);
                    return null;
                }
                return projectGuid.Value.ToUpper();
            }

            return project.Value.ToUpper();
        }

        private static string FindPlatformTargetLegacy(XElement xml)
        {
            var propGroups = from propGroup in xml.DescendantsAndSelf(LegacyNs + "PropertyGroup")
                             select propGroup;

            var propertyGroupList = propGroups as IList<XElement> ?? propGroups.ToList();

            foreach (var propGroup in propertyGroupList)
            {
                var platformTarget = from x in propGroup.DescendantsAndSelf(LegacyNs + "PlatformTarget")
                                     select x;

                var platformTargets = platformTarget as IList<XElement> ?? platformTarget.ToList();
                if (platformTargets.Any() && !string.IsNullOrWhiteSpace(platformTargets.First().Value)) return platformTargets.First().Value;
            }

            return CsProj.AnyCPU;
        }

        private static string FindPlatformTarget(XElement xml)
        {
            var propGroups = from propGroup in xml.DescendantsAndSelf("PropertyGroup")
                             select propGroup;

            var propertyGroupList = propGroups as IList<XElement> ?? propGroups.ToList();

            foreach (var propGroup in propertyGroupList)
            {
                var platformTarget = from x in propGroup.DescendantsAndSelf("PlatformTarget")
                                     select x;

                var platformTargets = platformTarget as IList<XElement> ?? platformTarget.ToList();
                if (platformTargets.Any() && !string.IsNullOrWhiteSpace(platformTargets.First().Value)) return platformTargets.First().Value;
            }

            return CsProj.AnyCPU;
        }
    }
}