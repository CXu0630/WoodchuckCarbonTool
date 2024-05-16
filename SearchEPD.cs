using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EC3CarbonCalculator.UI;
using Eto.Forms;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.UI;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    public class SearchEPD : Rhino.Commands.Command
    {
        private SearchForm form { get; set; }

        public SearchEPD()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static SearchEPD Instance { get; private set; }
        public override string EnglishName => "SearchEPD";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            EC3MaterialFilter mf = new EC3MaterialFilter();
            

            if (form == null)
            {
                form = new SearchForm(mf) { Owner = RhinoEtoApp.MainWindow };
                form.Closed += OnFormClosed;
                form.Show();
            }

            List<EPD> epds = new List<EPD>();
            EPD avgEPD;
            form.SearchEvent += (s, e) =>
            {
                mf = form.GetMaterialFilter();
                epds = RequestEC3(doc, mf, out avgEPD);
                // ERROR: the server did not return anything, most likely than not an
                // authentication error.
                if (epds == null)
                {
                    Application.Instance.Invoke(() => 
                    form.RepopulateResultMessage("There was an error accessing the EC3 server."));
                    return;
                }
                // NO RES: there weren't any EPDs found with these particular 
                // parameters.
                if (epds.Count == 0)
                {
                    Application.Instance.Invoke(() =>
                    form.RepopulateResultMessage("No results were found for your " +
                        "input parameters.\nTry broadening your search."));
                    return;
                }
                Application.Instance.Invoke(() => form.RepopulateSearchResult(epds, avgEPD));
            };

            form.AssignEvent += (s, e) =>
            {
                form.WindowState = WindowState.Minimized;
                EPDManager.SelectAssign(e.Epd);
                form.WindowState = WindowState.Normal;
            };

            return Result.Success;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            form.Dispose();
            form = null;
        }

        private List<EPD> RequestEC3 (RhinoDoc doc, EC3MaterialFilter mf, out EPD avgEPD)
        {
            // this portion that calculates the unit system should be migrated to assigning
            // epds...

            int dimension = EC3CategoryTree.Instance.GetCategoryDimension(mf.categoryName);
            IQuantity unit;

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

            string[] mfPrintable = mf.GetPrintableData();
            RhinoApp.WriteLine("Searching for materials that meet requirements:");
            foreach (string str in mfPrintable)
            {
                RhinoApp.WriteLine(str);
            }

            string matData = EC3Request.GetMaterialData(mf.GetMaterialFilter());
            // this mostly happens when there is an authentication error. Error is
            // handled by the "main method" of the command
            if(matData  == null)
            {
                avgEPD = null;
                return null;
            }
            JArray matArray = JArray.Parse(matData);
            List<EPD> epds = EC3MaterialParser.ParseEPDs(matArray, mf);

            RhinoApp.WriteLine("Total EPDs fount: " + epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, unit);
            string jurisdiction = mf.countryCode;
            if (mf.stateCode != null) { jurisdiction += ("-" + mf.stateCode); }
            avgEPD = new EPD(
                $"Average of Search Result",
                avgGwp, unit, avgDensity, mf.categoryName, mf);

            return epds;
        }
    }
}