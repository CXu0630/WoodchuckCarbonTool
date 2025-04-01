using Newtonsoft.Json.Linq;
using Rhino;
using System;
using UnitsNet;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// Helper class containing static methods used to parse and retreive units. Depends
    /// on the UnitsNet library to operate.
    /// </summary>
    internal class UnitManager
    {
        /// <summary>
        /// Parses a string representing a value and a unit separated by a space into
        /// a double representing the value and a string representing the unit. This 
        /// method is to be used specifically to format units that will be readable by
        /// UnitsNet, so do not hisitate to add any weird formatting requirements that
        /// are needed by UnitsNet.
        /// </summary>
        /// <param name="attrStr"> String containing value and unit to parse </param>
        /// <param name="unit"> Output parsed unit string formatted for UnitsNet </param>
        /// <returns> Double representing the value from the input attrStr </returns>
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
                // Fail: the first value after splitting is not a number
                unit = null;
                return 0;
            }

            unit = string.Join("", splitAttr, 1, splitAttr.Length - 1);
            if (unit.Length < 2) { return flt; }
            if (unit[unit.Length - 1] == '3' && unit[unit.Length - 2] != '^')
            {
                unit = unit.Remove(unit.Length - 1) + "^3";
            }
            if (unit[unit.Length - 1] == '2' && unit[unit.Length - 2] != '^')
            {
                unit = unit.Remove(unit.Length - 1) + "^2";
            }
            return flt;
        }

        /// <summary>
        /// Override for the previous method, input is an attribute of a JsonObeject
        /// </summary>
        public static double ParseDoubleWithUnit(JObject mat, string attribute, out string unit)
        {
            string attrStr = mat[attribute]?.ToString();
            return ParseDoubleWithUnit(attrStr, out unit);
        }

        /// <summary>
        /// Override of the ParseDoubleWithUnit method, when the output unit string does
        /// not matter.
        /// </summary>
        public static double ParseDoubleWithUnit(JObject mat, string attribute)
        {
            string unit;
            return ParseDoubleWithUnit(mat, attribute, out unit);
        }

        /// <summary>
        /// Method used to parse density units. This is very specific to buggs 
        /// </summary>
        /// <param name="densityUnit"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Method that parses a UnitsNet IQuantity from a string. This method also
        /// includes formatting that is specific to UnitsNet.
        /// </summary>
        /// <param name="attrstr"> Input string to parse. </param>
        /// <param name="valid"> Whether the quantity was successfully parsed (is this 
        /// necessary if we're already returning null on unsuccessful parsing?)</param>
        /// <returns> Parsed IQuantity </returns>
        public static IQuantity ParseQuantity(string attrstr, out bool valid)
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

            // weird formatting requirements and conditions to make parsing more likely to
            // be successful.
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
                }
                catch (Exception)
                {
                    // Could not be parsed, return null
                    valid = false;
                    return null;
                }
            }
            return unitMaterial;
        }

        /// <summary>
        /// Method to parse an attribute of a JObject. Overrides ParseQuantity (also uses
        /// the original ParseQuantity)
        /// </summary>
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

        /// <summary>
        /// Retreive an IQuantity equal to 1 unit of the document unit for the input
        /// RhinoDoc of the input dimension.
        /// </summary>
        /// <param name="doc"> RhinoDoc from which to get the unit needed </param>
        /// <param name="dimension"> An integer from 1 to 3 indicating the dimension of
        /// the unit to retreive </param>
        /// <returns> IQuantity equal to 1 unit of the document unit for the input
        /// RhinoDoc of the input dimension </returns>
        public static IQuantity GetSystemUnit(RhinoDoc doc, int dimension)
        {
            IQuantity unit;

            string unitSystem = RhinoDoc.ActiveDoc.GetUnitSystemName(true, false, true, true);

            Length lengthUnit = (Length)Quantity.Parse(typeof(Length), "1 " + unitSystem);
            Area areaUnit = lengthUnit * lengthUnit;
            Volume volumeUnit = lengthUnit * lengthUnit * lengthUnit;

            switch (dimension)
            {
                case 3:
                    unit = volumeUnit;
                    break;
                case 2:
                    unit = areaUnit;
                    break;
                default:
                    unit = lengthUnit;
                    break;
            }

            return unit;
        }

        /// <summary>
        /// Returns a string with the unit of that the RhinoDoc is in along with dimensions
        /// formatted in superscript. For printing or UI use.
        /// </summary>
        public static string GetSystemUnitStr(RhinoDoc doc, int dimension)
        {
            string unit = doc.GetUnitSystemName(true, false, true, true);

            // Gets unigode for superscripted 2 and 3
            switch (dimension)
            {
                case 2:
                    unit += "^2";
                    break;
                case 3:
                    unit += "^3";
                    break;
            }

            return unit;
        }

        public static int GetUnitDimension(IQuantity unit)
        {
            int dim = 0;

            dim = unit.Dimensions.Length;
            if (dim == 0 && unit.Dimensions.Mass != 0) { dim = 3; }

            return dim;
        }

    }
}
