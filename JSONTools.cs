using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EC3CarbonCalculator
{
    internal class JSONTools
    {
        static public JObject JsonToJObject(string json)
        {
            return JObject.Parse(json);
        }
    }
}
