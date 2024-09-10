using WoodchuckCarbonTool.src.EC3;
using WoodchuckCarbonTool.src.UI;
using Eto.Forms;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using System;
using System.Collections.Generic;
using UnitsNet;
using WoodchuckCarbonTool.src.Kaleidoscope;
using System.Security.Authentication;
using System.Net;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// This is the core Rhino command used to search EPDs and assign them to Rhino
    /// objects. It creates a search window and listens to events from the search window
    /// to perform requests through the EC3 API and to assign EPDs to objects.
    /// </summary>
    public class SearchEpdCommand : Rhino.Commands.Command
    {
        // ETO form that hosts the search window
        private SearchForm searchForm { get; set; }

        public SearchEpdCommand()
        {
            Instance = this;
        }

        public static SearchEpdCommand Instance { get; private set; }
        public override string EnglishName => "WoodchuckSearchEPD";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            return this.SearchEPD(doc);
        }

        public Result SearchEPD(RhinoDoc doc)
        {
            MaterialFilter mf = new MaterialFilter();

            if (searchForm == null)
            {
                searchForm = new SearchForm(doc, mf) { Owner = RhinoEtoApp.MainWindow };
                searchForm.Closed += OnFormClosed;
                searchForm.Show();
            }

            searchForm.BringToFront();
            searchForm.PopulateStartupMessage();

            List<EPD> epds = new List<EPD>();
            EPD avgEPD = null;
            // Event listener: a search event is called
            searchForm.SearchEvent += (s, e) =>
            {
                mf = searchForm.GetMaterialFilter();

                switch (mf.dataBase)
                {
                    case "EC3":
                        epds = RequestEC3(doc, mf, out avgEPD);
                        break;
                    case "CLF":
                        epds = CLFSearch.Instance.Search(mf);
                        avgEPD = null;
                        break;
                    case "Kaleidoscope":
                        epds = KaleidoscopeSearch.Instance.Search(mf);
                        avgEPD = null;
                        break;
                }
                // ERROR: the server did not return anything, most likely than not an
                // authentication error.
                if (epds == null)
                {
                    Application.Instance.Invoke(() =>
                    searchForm.RepopulateResultMessage("Please authenticate and try again." +
                    "\nIf you have trouble authenticating, please contact me."));
                    return;
                }
                // NO RES: there weren't any EPDs found with these particular 
                // parameters.
                if (epds.Count == 0)
                {
                    Application.Instance.Invoke(() =>
                    searchForm.RepopulateResultMessage("No valid results were found for your " +
                        "input parameters.\nTry broadening your search."));
                    return;
                }
                Application.Instance.Invoke(() => searchForm.RepopulateSearchResult(epds, avgEPD));
            };

            return Result.Success;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            searchForm.Dispose();
            searchForm = null;
        }

        /// <summary>
        /// Sends API request to EC3 and retreives the resultant EPDs and the average EPD.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mf"></param>
        /// <param name="avgEPD"></param>
        /// <returns></returns>
        private List<EPD> RequestEC3(RhinoDoc doc, MaterialFilter mf, out EPD avgEPD)
        {
            // the dimension used to calculate the average, defaults to category dimension
            int dimension = EC3CategoryTree.Instance.GetCategoryDimension(mf.categoryName);
            IQuantity unit = UnitManager.GetSystemUnit(doc, dimension);

            string[] mfPrintable = mf.GetPrintableData();
            RhinoApp.WriteLine("Searching for materials that meet requirements:");
            foreach (string str in mfPrintable)
            {
                RhinoApp.WriteLine(str);
            }

            string matData;

            matData = EC3Request.GetMaterialData(mf.GetEC3MaterialFilter());

            // this mostly happens when there is an authentication error. Error is
            // handled by the "main method" of the command
            if (matData == null)
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
            string jurisdiction = mf.country;
            if (mf.state != null) { jurisdiction += "-" + mf.state; }
            avgEPD = new EPD(
                $"Average of Search Result",
                avgGwp, unit, avgDensity, mf.categoryName, mf, "None");

            return epds;
        }

        /// <summary>
        /// Parses the Newtonsoft JArray returned from the EC3 API call and parses a list
        /// of EPDs based on that JArray.
        /// </summary>
        public static List<EPD> ParseEPDs(JArray jsonArray, bool discardInvalid, MaterialFilter mf = null)
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