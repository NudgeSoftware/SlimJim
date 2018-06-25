using System.Xml;
using log4net;
using SlimJim.Model;

namespace SlimJim
{
    public abstract class CsProjConverter
    {
        protected const string MsBuildXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        protected readonly ILog Log;
        protected XmlNamespaceManager NsMgr;

        protected CsProjConverter()
        {
            Log = LogManager.GetLogger(GetType());
        }

        protected static XmlElement CreateElementWithInnerText(XmlDocument doc, string elementName, string text)
        {
            var e = doc.CreateElement(elementName, MsBuildXmlNamespace);
            e.InnerText = text;
            return e;
        }

        protected XmlDocument LoadProject(CsProj project)
        {
            var doc = new XmlDocument();
            doc.Load(project.Path);
            NsMgr = new XmlNamespaceManager(doc.NameTable);
            NsMgr.AddNamespace("msb", MsBuildXmlNamespace);
            return doc;
        }
    }
}