﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
    }
}
