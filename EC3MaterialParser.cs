using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EC3CarbonCalculator
{
    internal class EC3MaterialParser
    {
        JArray matArray;
        int matCount;

        public EC3MaterialParser(string matData) 
        {
            matArray = JArray.Parse(matData);
            matCount = matArray.Count;
        }

        public JToken[] GetAttributeForEach(string attribute) 
        {
            JToken[] attributes = new JToken[matCount];
            for (int i = 0; i < matCount; i++)
            {
                JObject mat = matArray[i] as JObject;
                if (mat == null) { continue; }
                attributes[i] = mat[attribute];
            }
            return attributes;
        }

        public string[] GetStringAttributeForEach(string attribute)
        {
            string[] attributes = new string[matCount];
            for (int i = 0; i < matCount; i++)
            {
                JObject mat = matArray[i] as JObject;
                if (mat == null) { continue; }
                attributes[i] = mat[attribute]?.ToString();
            }
            return attributes;
        }

        public float[] GetGwpAttributeForEach(string attribute)
        {
            float[] gwps = new float[matCount];
            for (int i = 0; i < matCount; i++)
            {
                JObject mat = matArray[i] as JObject;
                if (mat == null) { continue; }
                string attrStr = mat[attribute]?.ToString();
                float gwp = float.Parse(attrStr.Split(' ')[0]);
                gwps[i] = gwp;
            }
            return gwps;
        }

        public float GetAverageGwp()
        {
            float[] gwps = this.GetGwpAttributeForEach("gwp");
            List<float> nonZero = gwps.Where(x => x > 0).ToList();
            return nonZero.Average();
        }

        public int GetMaterialCount() { return this.matCount; }
    }
}
