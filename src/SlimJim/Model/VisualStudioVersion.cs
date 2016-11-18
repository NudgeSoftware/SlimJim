using System;
using System.Linq;

namespace SlimJim.Model
{
    public sealed class VisualStudioVersion
    {
        private static readonly VisualStudioVersion vs2010 = new VisualStudioVersion("2010", "11.00", "10.0");
        private static readonly VisualStudioVersion vs2012 = new VisualStudioVersion("2012", "12.00", "12.0");
        private static readonly VisualStudioVersion vs2013 = new VisualStudioVersion("2013", "12.00", "13.0");
        private static readonly VisualStudioVersion vs2015 = new VisualStudioVersion("2015", "12.00", "14.0");

        private VisualStudioVersion(string year, string slnFileVersionNumber, string pathVersionNumber)
        {
            Year = year;
            SlnFileVersionNumber = slnFileVersionNumber;
            PathVersionNumber = pathVersionNumber;
        }

        public string Year { get; private set; }
        public string SlnFileVersionNumber { get; private set; }
        public string PathVersionNumber { get; private set; }
		public string SlnVisualStudioVersion => PathVersionNumber.Split('.')[0];

        public static VisualStudioVersion VS2010 => vs2010;

        public static VisualStudioVersion VS2012 => vs2012;

        public static VisualStudioVersion VS2013 => vs2013;

        public static VisualStudioVersion VS2015 => vs2015;

        public static VisualStudioVersion ParseVersionString(string versionNumber)
        {
            var versions = new[] { vs2010, vs2012, vs2013, vs2015 };

            return versions.FirstOrDefault(v => versionNumber.Contains(v.Year)) ?? VS2015;
        }

        public override string ToString()
        {
            return $"{Year} ({SlnFileVersionNumber})";
        }
    }
}