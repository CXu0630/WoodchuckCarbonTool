using System;
using System.Collections.Generic;
using System.Linq;
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
                form.RepopulateSearchResult(epds, avgEPD);
            };

            //string[] epdprintable = avgepd.getprintabledata().toarray();
            //rhinoapp.writeline("calculated following based on search:");
            //foreach (string str in epdprintable)
            //{
            //    rhinoapp.writeline(str);
            //}

            //ec3selector geoselector = new ec3selector(dimension);
            //objref[] geo = geoselector.getselection();

            //if (geo == null) { return result.cancel; }

            //double totalgwp = 0;
            //foreach (objref objref in geo)
            //{
            //    if (objref == null) continue;
            //    double geodata = geometryprocessor.getdimensionalinfo(objref, dimension);
                
            //    double gwp = geodata * avggwp.value;
            //    totalgwp += gwp;

            //    rhinoobject obj = objref.object();

            //    obj.attributes.setuserstring("category", mf.categoryname);
            //    obj.attributes.setuserstring("gwp", (geodata * avggwp).tostring("0.###") + "co2e");
            //    obj.attributes.setuserstring("materialfilter", mf.getmaterialfilter());
            //    obj.attributes.setuserstring("gwp per unit " + 
            //        unit.gettype().tostring().split('.')[1], avggwp.value.tostring("0.###") + " kgco2e");
            //    obj.attributes.setuserstring("expiration date after", mf.expirationdate);
            //    obj.attributes.setuserstring("jurisdiction", jurisdiction);
            //}

            //rhinoapp.writeline("total gwp of selected objects: " + totalgwp.tostring("0.###") + " kgco2e");

            return Result.Success;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            form.Dispose();
            form = null;
        }

        private List<EPD> RequestEC3 (RhinoDoc doc, EC3MaterialFilter mf, out EPD avgEPD)
        {
            int dimension = 3;
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
            JArray matArray = JArray.Parse(matData);
            List<EPD> epds = EC3MaterialParser.ParseEPDs(matArray, mf);

            RhinoApp.WriteLine("Total EPDs fount: " + epds.Count.ToString());

            // get averages for EPDs
            Density avgDensity = EPD.AverageDensity(epds);
            Mass avgGwp = EPD.AverageGwp(epds, unit);
            string jurisdiction = mf.countryCode;
            if (mf.stateCode != null) { jurisdiction += ("-" + mf.stateCode); }
            avgEPD = new EPD(
                $"Average of  products produced at {jurisdiction} valid after {mf.expirationDate}",
                avgGwp, unit, avgDensity, mf.categoryName, mf);

            return epds;
        }
    }
}