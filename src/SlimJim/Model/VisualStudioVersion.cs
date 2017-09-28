using System.Linq;

namespace SlimJim.Model
{
    public sealed class VisualStudioVersion
    {
        private VisualStudioVersion(string year, string slnFileVersionNumber, string pathVersionNumber)
        {
            Year = year;
            SlnFileVersionNumber = slnFileVersionNumber;
            PathVersionNumber = pathVersionNumber;
        }

        public string Year { get; }
        public string SlnFileVersionNumber { get; }
        public string PathVersionNumber { get; }
		public string SlnVisualStudioVersion => PathVersionNumber.Split('.')[0];

        public static VisualStudioVersion VS2010 { get; } = new VisualStudioVersion("2010", "11.00", "10.0");

        public static VisualStudioVersion VS2012 { get; } = new VisualStudioVersion("2012", "12.00", "12.0");

        public static VisualStudioVersion VS2013 { get; } = new VisualStudioVersion("2013", "12.00", "13.0");

        public static VisualStudioVersion VS2015 { get; } = new VisualStudioVersion("2015", "12.00", "14.0");

        public static VisualStudioVersion VS2017 { get; } = new VisualStudioVersion("2017", "12.00", "15.0");

        public static VisualStudioVersion ParseVersionString(string versionNumber)
        {
            var versions = new[] { VS2010, VS2012, VS2013, VS2015 };

            return versions.FirstOrDefault(v => versionNumber.Contains(v.Year)) ?? VS2017;
        }

        public override string ToString()
        {
            return $"{Year} ({SlnFileVersionNumber})";
        }
    }
}