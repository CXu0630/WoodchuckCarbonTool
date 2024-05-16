using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnitsNet;

namespace EC3CarbonCalculator
{
    internal class EC3MaterialParser
    {
        public static List<EPD> ParseEPDs(JArray jsonArray, EC3MaterialFilter mf = null)
        {
            List<EPD> epds = new List<EPD>();
            foreach(JObject jobj in jsonArray)
            {
                EPD epd = new EPD(jobj, mf);
                epds.Add(epd);
            }
            return epds;
        }

        public static double ParseDoubleWithUnit(string attrStr, out string unit)
        {
            double flt;

            if (attrStr == null)
            {
                unit = null;
                return 0;
            }

            string[] splitAttr = attrStr.Split(' ');
            try
            {
                flt = double.Parse(splitAttr[0]);
            }
            catch (FormatException)
            {
                unit = null;
                return 0;
            }

            unit = string.Join("", splitAttr, 1, splitAttr.Length - 1);
            if (unit[unit.Length - 1] == '3')
            {
                unit = unit.Remove(unit.Length - 1) + "^3";
            }
            if (unit[unit.Length - 1] == '2')
            {
                unit = unit.Remove(unit.Length - 1) + "^2";
            }
            return flt;
        }

        public static double ParseDoubleWithUnit (JObject mat, string attribute, out string unit)
        {
            string attrStr = mat[attribute]?.ToString();
            return ParseDoubleWithUnit(attrStr, out unit);
        }

        public static double ParseDoubleWithUnit(JObject mat, string attribute)
        {
            string unit;
            return ParseDoubleWithUnit(mat, attribute, out unit);
        }

        public static string ParseDensityUnit(string densityUnit)
        {
            string newDensityUnit = densityUnit;
            if (densityUnit == null) { return null; }
            if (densityUnit[0] == 't' && densityUnit[1] == '/')
            {
                newDensityUnit = "ton" + densityUnit.TrimStart('t');
            }
            return newDensityUnit;
        }

        public static IQuantity ParseQuantity (string attrstr, out bool valid)
        {
            double unitMultiplier = ParseDoubleWithUnit(attrstr, out string unit);
            IQuantity unitMaterial = null;
            valid = true;

            if (unit == null) { valid = false; return unitMaterial; }
            // "t" could be different units and "ton" isn't recognized as an abbreviation

            string[] unitSplit = unit.Split('/');
            if (unitSplit.Length == 2)
            {
                unit = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Density), unit, out unitMaterial);
                return unitMaterial;
            }

            if (unit == "t" || unit == "ton")
            {
                unit = "t";
                string unitMat = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Mass), unitMat, out unitMaterial);
            }
            else if (unit == "m")
            {
                string unitMat = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Length), unitMat, out unitMaterial);
            }
            else if (unit == "sqft" || unit == "sf")
            {
                unit = "ft^2";
                string unitMat = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Area), unitMat, out unitMaterial);
            }
            else if (unit[unit.Length - 1] == '3')
            {
                string unitMat = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Volume), unitMat, out unitMaterial);
            }
            else if (unit[unit.Length - 1] == '2')
            {
                string unitMat = unitMultiplier.ToString() + " " + unit;
                valid = Quantity.TryParse(typeof(Area), unitMat, out unitMaterial);
            }
            else
            {
                try
                {
                    unitMaterial = Quantity.FromUnitAbbreviation(unitMultiplier, unit);
                } catch (Exception)
                {
                    valid = false;
                    return null;
                }
                
            }
            return unitMaterial;
        }

        public static IQuantity ParseQuantity(JObject mat, string attribute, out bool valid)
        {
            string attrStr = mat[attribute]?.ToString();
            try
            {
                string qty = mat[attribute]["qty"]?.ToString();
                string unit = mat[attribute]["unit"]?.ToString();
                if (qty != null && unit != null) { attrStr = qty + " " + unit; }
            }
            catch (Exception) { }
            
            return ParseQuantity(attrStr, out valid);
        }
    }
}
