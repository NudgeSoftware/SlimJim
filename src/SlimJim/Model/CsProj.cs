using System;
using System.Collections.Generic;
using System.Linq;

namespace SlimJim.Model
{
    public class CsProj
    {
        public CsProj()
        {
            ReferencedAssemblyNames = new List<string>();
            //            ReferencedProjectGuids = new List<string>();
            ReferencedProjects = new List<(string assemblyName, string guid)>();
        }

        public string Guid { get; set; }
        public string ProjectTypeGuid { get; set; }
        public string Path { get; set; }
        public string AssemblyName { get; set; }
        public string TargetFrameworkVersion { get; set; }
        public List<string> ReferencedAssemblyNames { get; set; }
        //public List<string> ReferencedProjectGuids { get; set; }
        public List<(string assemblyName, string guid)> ReferencedProjects { get; set; }
        public string Platform { get; set; } = "Any CPU";

        public string ProjectName => System.IO.Path.GetFileNameWithoutExtension(Path);

        public void ReferencesAssemblies(params CsProj[] assemblyReferences)
        {
            foreach (CsProj reference in assemblyReferences)
            {
                if (!ReferencedAssemblyNames.Contains(reference.AssemblyName))
                {
                    ReferencedAssemblyNames.Add(reference.AssemblyName);
                }
            }
        }

        public void ReferencesProjects(params CsProj[] projectReferences)
        {
            foreach (CsProj reference in projectReferences)
            {
                if (!ReferencedProjects.Select(tuple => tuple.guid).Contains(reference.Guid))
                {
                    ReferencedProjects.Add((reference.AssemblyName, reference.Guid));
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() +
                   $@" {{AssemblyName=""{AssemblyName}"", Guid=""{Guid}"", Path=""{Path}""}}";
        }
    }
}
