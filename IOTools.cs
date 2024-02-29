using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EC3CarbonCalculator
{
    internal class IOTools
    {
        public static string GetRhpLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string cleanFullPath = Uri.UnescapeDataString(uri.Path);
            return cleanFullPath;
        }
    }
}
