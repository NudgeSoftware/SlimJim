using System;
using System.IO;
using SlimJim.Model;
using Antlr3.ST;

namespace SlimJim.Infrastructure
{
	public class SlnFileRenderer
	{
		private static readonly StringTemplateGroup TemplateGroup;
		private readonly Sln _solution;

		static SlnFileRenderer()
		{
			var stream = typeof (SlnFileRenderer).Assembly.GetManifestResourceStream("SlimJim.Templates.SolutionTemplate.st");
			TemplateGroup = new StringTemplateGroup("SlnTemplates");
			using (stream)
			{
				TemplateGroup.DefineTemplate("SolutionTemplate", new StreamReader(stream ?? throw new InvalidOperationException()).ReadToEnd());
			}
		}
		public SlnFileRenderer(Sln solution)
		{
			_solution = solution;
		}

		public string Render()
		{
			var slnTemplate = TemplateGroup.GetInstanceOf("SolutionTemplate");
			slnTemplate.SetAttribute("sln", _solution);
			return slnTemplate.ToString();
		}
	}
}