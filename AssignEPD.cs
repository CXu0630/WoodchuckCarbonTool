﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    public class AssignEPD
    {

        public AssignEPD() { }

        protected Result RunCommand(RhinoDoc doc, EPD epd)
        {
            EC3MaterialFilter mf = new EC3MaterialFilter();
            int dimension = 3;
            IQuantity unit;
            string category;

            // get document units
            string unitSystem = doc.GetUnitSystemName(true, false, true, true);

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

            // Print information to console
            string[] mfPrintable = mf.GetPrintableData();
            RhinoApp.WriteLine("Searching for materials that meet requirements:");
            foreach (string str in mfPrintable)
            {
                RhinoApp.WriteLine(str);
            }

            // send API request and get all EPDs
            string matData = EC3Request.GetMaterialData(mf.GetMaterialFilter());
            JArray matArray = JArray.Parse(matData);
            List<EPD>  epds = EC3MaterialParser.ParseEPDs(matArray, mf);            

            RhinoApp.WriteLine("Total EPDs fount: " + epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, unit);
            EPD avgEpd = new EPD(
                $"Average of {category} products produced at {jurisdiction} valid after {date}", 
                avgGwp, unit, avgDensity, category, mf);

            string[] EPDPrintable = avgEpd.GetPrintableData().ToArray();
            RhinoApp.WriteLine("Calculated following based on search:");
            foreach (string str in EPDPrintable)
            {
                RhinoApp.WriteLine(str);
            }

            EC3Selector geoSelector = new EC3Selector(dimension);
            ObjRef[] geo = geoSelector.GetSelection();

            if (geo == null) { return Result.Cancel; }

            double totalGwp = 0;
            foreach (ObjRef objRef in geo)
            {
                if (objRef == null) continue;
                double geoData = GeometryProcessor.GetDimensionalInfo(objRef, dimension);
                
                double gwp = geoData * avgGwp.Value;
                totalGwp += gwp;

                RhinoObject obj = objRef.Object();

                obj.Attributes.SetUserString("Category", category);
                obj.Attributes.SetUserString("GWP", (geoData * avgGwp).ToString("0.###") + "CO2e");
                obj.Attributes.SetUserString("MaterialFilter", mf.GetMaterialFilter());
                obj.Attributes.SetUserString("GWP per unit " + 
                    unit.GetType().ToString().Split('.')[1], avgGwp.Value.ToString("0.###") + " kgCO2e");
                obj.Attributes.SetUserString("Expiration date after", date);
                obj.Attributes.SetUserString("Jurisdiction", jurisdiction);
            }

            RhinoApp.WriteLine("Total GWP of selected objects: " + totalGwp.ToString("0.###") + " kgCO2e");

            return Result.Success;
        }
    }
}