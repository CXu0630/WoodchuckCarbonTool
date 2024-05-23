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
    /// <summary>
    /// This is the core Rhino command used to search EPDs and assign them to Rhino
    /// objects. It creates a search window and listens to events from the search window
    /// to perform requests through the EC3 API and to assign EPDs to objects.
    /// </summary>
    public class SearchEPD : Rhino.Commands.Command
    {
        // ETO form that hosts the search window
        private SearchForm form { get; set; }

        public SearchEPD()
        {
            Instance = this;
        }

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
            // Event listener: a search event is called
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
                    form.RepopulateResultMessage("No valid results were found for your " +
                        "input parameters.\nTry broadening your search."));
                    return;
                }
                Application.Instance.Invoke(() => form.RepopulateSearchResult(epds, avgEPD));
            };

            // Event listener: an assign event is called
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

        /// <summary>
        /// Sends API request to EC3 and retreives the resultant EPDs and the average EPD.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mf"></param>
        /// <param name="avgEPD"></param>
        /// <returns></returns>
        private List<EPD> RequestEC3 (RhinoDoc doc, EC3MaterialFilter mf, out EPD avgEPD)
        {
            // this portion that calculates the unit system should be migrated to assigning
            // epds...

            int dimension = EC3CategoryTree.Instance.GetCategoryDimension(mf.categoryName);
            IQuantity unit = UnitManager.GetSystemUnit(doc, dimension);

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
            List<EPD> epds = ParseEPDs(matArray, true, mf);

            RhinoApp.WriteLine("Total EPDs fount: " + epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, unit);
            string jurisdiction = mf.countryCode;
            if (mf.stateCode != null) { jurisdiction += ("-" + mf.stateCode); }
            avgEPD = new EPD(
                $"Average of Search Result",
                avgGwp, unit, avgDensity, mf.categoryName, mf, "None");

            return epds;
        }

        /// <summary>
        /// Parses the Newtonsoft JArray returned from the EC3 API call and parses a list
        /// of EPDs based on that JArray.
        /// </summary>
        public static List<EPD> ParseEPDs(JArray jsonArray, bool discardInvalid, EC3MaterialFilter mf = null)
        {
            List<EPD> epds = new List<EPD>();
            foreach (JObject jobj in jsonArray)
            {
                EPD epd = new EPD(jobj, mf);
                if (discardInvalid) { if (epd.valid) { epds.Add(epd); } }
            }
            return epds;
        }
    }
}