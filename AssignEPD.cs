using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using UnitsNet;

namespace EC3CarbonCalculator
{
    public class AssignEPD : Command
    {
        EC3MaterialFilter mf = new EC3MaterialFilter();
        int dimension = 3;
        IQuantity unit;
        string category;

        public AssignEPD()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static AssignEPD Instance { get; private set; }
        public override string EnglishName => "AssignEPD";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // get material filter properties
            Result catRes = this.SetCategory();
            if (catRes == Result.Failure) { return Result.Failure; }
            this.SetJurisdiction();
            this.SetExpireDate();

            RhinoApp.WriteLine(this.mf.GetMaterialFilter());

            // get document units
            string unitSystem = doc.GetUnitSystemName(true, false, true, true);
            RhinoApp.WriteLine(unitSystem);

            Length lengthUnit = (Length)Quantity.Parse(typeof(Length), "1 " + unitSystem);
            Area areaUnit = lengthUnit * lengthUnit;
            Volume volumeUnit = lengthUnit * lengthUnit * lengthUnit;

            switch (dimension)
            {
                case 3:
                    this.unit = volumeUnit;
                    break;
                case 2:
                    this.unit = areaUnit;
                    break;
                case 1:
                    this.unit = lengthUnit;
                    break;
            }

            RhinoApp.WriteLine(volumeUnit.ToString());

            // send API request and get all EPDs
            string matData = EC3Request.GetMaterialData(this.mf.GetMaterialFilter());
            JArray matArray = JArray.Parse(matData);
            List<EPD> epds = EC3MaterialParser.ParseEPDs(matArray, this.mf);

            RhinoApp.WriteLine(epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, this.unit);
            EPD avgEpd = new EPD("Average EPD", avgGwp, this.unit, avgDensity, this.category, this.mf);

            EC3Selector geoSelector = new EC3Selector(this.dimension);
            ObjRef[] geo = geoSelector.GetSelection();

            if (geo.Length == 0 || geo == null) { return Result.Cancel; }

            foreach (ObjRef objRef in geo)
            {
                if (objRef == null) continue;
                double geoData = GeometryProcessor.GetDimensionalInfo(objRef, this.dimension);
                double gwp = geoData * avgGwp.Value;

                objRef.Object().UserDictionary.Set("GWP", gwp);
                objRef.Object().UserDictionary.Set("Category", this.category);
                objRef.Object().UserDictionary.Set("MaterialFilter", this.mf.GetMaterialFilter());
            }

            return Result.Success;
        }

        private Result SetCategory()
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material category");
            userText.UserInputText();
            this.category = userText.GetInputText();

            EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
            int catIdx = categoryTree.GetCategoryIdx(category);
            if (catIdx == -1) 
            {
                RhinoApp.WriteLine("Entered category is not a valid EC3 category.");
                return Result.Failure;
            }
            mf.SetCategory(categoryTree.names[catIdx]);

            dimension = categoryTree.dimensions[catIdx];

            return Result.Success;
        }

        private Result SetJurisdiction()
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material source jurisdiction");
            userText.UserInputText();
            string jurisdiction = userText.GetInputText();

            string[] splitJurisdiction= jurisdiction.Split('-');
            if (splitJurisdiction[0] == "US")
            {
                mf.SetState(splitJurisdiction[1]);
            }
            if (!mf.SetCountry(splitJurisdiction[0]))
            {
                RhinoApp.WriteLine("Not a valid jurisdiction, will proceed with Global" +
                    "calculation.");
                return Result.Failure;
            }
            return Result.Success;
        }

        private Result SetExpireDate()
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