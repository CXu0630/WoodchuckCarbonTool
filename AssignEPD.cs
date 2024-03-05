using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    public class AssignEPD : Command
    {

        public AssignEPD()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static AssignEPD Instance { get; private set; }
        public override string EnglishName => "AssignEPD";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            EC3MaterialFilter mf = new EC3MaterialFilter();
            int dimension = 3;
            IQuantity unit;
            string category;

            // get material filter properties
            Result catRes = this.SetCategory(mf, out category, out dimension);
            if (catRes == Result.Failure) { return Result.Failure; }
            this.SetJurisdiction(mf);
            this.SetExpireDate(mf);

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

            // send API request and get all EPDs
            string matData = EC3Request.GetMaterialData(mf.GetMaterialFilter());
            JArray matArray = JArray.Parse(matData);
            List<EPD> epds = EC3MaterialParser.ParseEPDs(matArray, mf);

            // RhinoApp.WriteLine(epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, unit);
            EPD avgEpd = new EPD("Average EPD", avgGwp, unit, avgDensity, category, mf);

            EC3Selector geoSelector = new EC3Selector(dimension);
            ObjRef[] geo = geoSelector.GetSelection();

            if (geo == null) { return Result.Cancel; }

            foreach (ObjRef objRef in geo)
            {
                if (objRef == null) continue;
                double geoData = GeometryProcessor.GetDimensionalInfo(objRef, dimension);
                
                double gwp = geoData * avgGwp.Value;

                RhinoObject obj = objRef.Object();

                obj.Attributes.SetUserString("Category", category);
                obj.Attributes.SetUserString("GWP", (geoData * avgGwp).ToString());
                obj.Attributes.SetUserString("MaterialFilter", mf.GetMaterialFilter());
            }

            return Result.Success;
        }

        private Result SetCategory(EC3MaterialFilter mf, out string category, out int dimension)
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material category");
            userText.UserInputText();
            category = userText.GetInputText();

            EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
            int catIdx = categoryTree.GetCategoryIdx(category);
            if (catIdx == -1) 
            {
                RhinoApp.WriteLine("Entered category is not a valid EC3 category.");
                dimension = 0;
                return Result.Failure;
            }
            mf.SetCategory(categoryTree.names[catIdx]);

            dimension = categoryTree.dimensions[catIdx];

            return Result.Success;
        }

        private Result SetJurisdiction(EC3MaterialFilter mf)
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material source jurisdiction");
            userText.UserInputText();
            string jurisdiction = userText.GetInputText();

            string[] splitJurisdiction= jurisdiction.Split('-');
            if (splitJurisdiction[0] == "US")
            {
                if (splitJurisdiction.Length == 2) { mf.SetState(splitJurisdiction[1]); }
            }
            if (!mf.SetCountry(splitJurisdiction[0]))
            {
                RhinoApp.WriteLine("Not a valid jurisdiction, will proceed with Global" +
                    "calculation.");
                return Result.Failure;
            }
            return Result.Success;
        }

        private Result SetExpireDate(EC3MaterialFilter mf)
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set minimum expiration date of EPD in format yyyy-MM-dd");
            userText.UserInputText();
            string date = userText.GetInputText();

            if (!mf.SetExpirationDate(date)) 
            {
                RhinoApp.WriteLine("Not a valid date, will proceed with current date.");
                return Result.Failure; 
            }
            return Result.Success;
        }
    }
}