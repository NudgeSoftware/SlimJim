using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SlimJim.Infrastructure;
using SlimJim.Model;
using SlimJim.Test.SampleFiles;

namespace SlimJim.Test.Infrastructure
{
    [TestFixture]
    public class CsProjReaderTests
    {
        private FileInfo _file;

        private CsProj GetProject(string fileName)
        {
            _file = SampleFileHelper.GetCsProjFile(fileName);
            var reader = new CsProjReader();
            return reader.Read(_file);
        }

        [Test]
        public void IgnoresNestedReferences()
        {
            var project = GetProject("ConvertedReference");

            Assert.That(project.ReferencedAssemblyNames, Is.Not.Contains("log4net"));
        }

        [Test]
        public void NoProjectReferencesDoesNotCauseNre()
        {
            var project = GetProject("NoProjectReferences");

            Assert.That(project.ReferencedProjects, Is.Empty);
        }

        [Test]
        public void ReadsFileContentsIntoObject()
        {
            var project = GetProject("Simple");

            Assert.That(project.Guid, Is.EqualTo("{4A37C916-5AA3-4C12-B7A8-E5F878A5CDBA}"));
            Assert.That(project.AssemblyName, Is.EqualTo("MyProject"));
            Assert.That(project.Path,
                Is.EqualTo(CsProjReader.GetRelativePath(_file.FullName, Environment.CurrentDirectory)));
            Assert.That(project.TargetFrameworkVersion, Is.EqualTo("v4.0"));
            Assert.That(project.ReferencedAssemblyNames, Is.EqualTo(new[]
            {
                "System",
                "System.Core",
                "System.Xml.Linq",
                "System.Data.DataSetExtensions",
                "Microsoft.CSharp",
                "System.Data",
                "System.Xml"
            }));
            Assert.That(project.ReferencedProjects, Is.EqualTo(new Dictionary<string, string>
            {
                {"MyApp.Core", "{99036BB6-4F97-4FCC-AF6C-0345A5089099}"},
                {"MyOtherProject", "{69036BB3-4F97-4F9C-AF2C-0349A5049060}"}
            }));
        }

        [Test]
        public void TakesOnlyNameOfFullyQualifiedAssemblyName()
        {
            var project = GetProject("FQAssemblyName");

            Assert.That(project.ReferencedAssemblyNames, Contains.Item("NHibernate"));
        }
    }
}