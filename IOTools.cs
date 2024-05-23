using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EC3CarbonCalculator
{
    /// <summary>
    /// This is supposed to be a helper class with static methods to aid file IO. Not sure
    /// we need it.
    /// </summary>
    internal class IOTools
    {
        /// <summary>
        /// Gets the location of the rhp file that is being executed.
        /// </summary>
        public static string GetRhpLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string cleanFullPath = Uri.UnescapeDataString(uri.Path);
            return cleanFullPath;
        }
    }
}
