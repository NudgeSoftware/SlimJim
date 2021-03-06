using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SlimJim.Model;

namespace SlimJim
{
    public class ReferenceConverter : CsProjConverter
    {
        public void ConvertToProjectReferences(Sln solution)
        {
            var projectsByName = solution.Projects.ToDictionary(p => p.AssemblyName, p => p);

            foreach (var project in solution.Projects)
            {
                var assemblyNamesInSolution = project.ReferencedAssemblyNames.Intersect(projectsByName.Keys).ToArray();

                if (assemblyNamesInSolution.Length == 0) continue;

                ConvertToProjectReference(project, assemblyNamesInSolution.Select(a => projectsByName[a]));
            }
        }

        public void RestoreAssemblyReferences(Sln solution)
        {
            foreach (var project in solution.Projects) RestoreAssemblyReferencesInProject(project);
        }

        private void RestoreAssemblyReferencesInProject(CsProj project)
        {
            var doc = LoadProject(project);
            var nav = doc.CreateNavigator();

            XPathNavigator projectReference;

            while ((projectReference =
                       nav.SelectSingleNode("//msb:ProjectReference[msb:SlimJimReplacedReference and 1]", NsMgr)) !=
                   null)
            {
                var original = projectReference.SelectSingleNode("./msb:SlimJimReplacedReference/msb:Reference", NsMgr);
                Log.InfoFormat("Restoring project {0} assembly reference to {1}", project.ProjectName,
                    projectReference.GetAttribute("Include", ""));
                projectReference.ReplaceSelf(original);
            }

            doc.Save(project.Path);
        }

        private void ConvertToProjectReference(CsProj project, IEnumerable<CsProj> references)
        {
            var doc = LoadProject(project);
            var nav = doc.CreateNavigator();

            foreach (var reference in references)
            {
                Log.InfoFormat("Converting project {0} assembly reference {1} to project reference {2}.",
                    project.AssemblyName, reference.AssemblyName, reference.Path);

                var xpath =
                    $"//msb:ItemGroup/msb:Reference[substring-before(concat(@Include, ','), ',') = '{reference.AssemblyName}']";

                var element = nav.SelectSingleNode(xpath, NsMgr);

                if (element == null)
                {
                    Log.ErrorFormat("Failed to locate Reference element in {0} for assembly {1}.", project.Path,
                        reference.AssemblyName);
                    continue;
                }

                var projectReference = doc.CreateElement("ProjectReference", MsBuildXmlNamespace);
                projectReference.SetAttribute("Include", reference.Path);
                projectReference.AppendChild(CreateElementWithInnerText(doc, "Project", reference.Guid));
                projectReference.AppendChild(CreateElementWithInnerText(doc, "Name", reference.ProjectName));

                var wrapper = doc.CreateElement("SlimJimReplacedReference", MsBuildXmlNamespace);
                wrapper.AppendChild(((XmlNode) element.UnderlyingObject).Clone());

                projectReference.AppendChild(wrapper);

                element.ReplaceSelf(new XmlNodeReader(projectReference));
            }

            doc.Save(project.Path);
        }

        private new static XmlElement CreateElementWithInnerText(XmlDocument doc, string elementName, string text)
        {
            var e = doc.CreateElement(elementName, MsBuildXmlNamespace);
            e.InnerText = text;
            return e;
        }
    }
}