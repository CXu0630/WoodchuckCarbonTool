using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using WoodchuckCarbonTool.src;
using WoodchuckCarbonTool.src.UI;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using UnitsNet;
using UnitsNet.Units;
using WoodchuckCarbonTool.src.Kaleidoscope;

namespace WoodchuckCarbonTool
{
    /// <summary>
    /// Rhino command used for testing. Change the build action to C# if using, and 
    /// remember to change if back before building a release.
    /// </summary>
    public class Tester : Command
    {
        private SearchForm form {  get; set; }

        public Tester()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static Tester Instance { get; private set; }

        public override string EnglishName => "Tester";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                TestKaleidoscopeRead();
            }catch (Exception e)
            {
                RhinoApp.WriteLine(e.Message);
            }
            return Result.Success;
        }

        //private void TestGetCategory()
        //{
        //    EC3Request request = new EC3Request("suUNpZ8ORcN94YgEdDSxpf4YYmOiAw");
        //    string categoryData = request.GetCategoryData("484df282d43f4b0e855fad6b351ce006");
        //    RhinoApp.WriteLine(categoryData);
        //}

        //private void TestGetMaterial()
        //{
        //    string materialData = null;
            
        //    try
        //    {
        //        EC3Request request = new EC3Request("suUNpZ8ORcN94YgEdDSxpf4YYmOiAw");
        //        materialData = request.GetMaterialData(
        //            "!pragma eMF(\"2.0/1\"), " +
        //            "lcia(\"TRACI 2.1\") " +
        //            "category:\"03 21 00 Reinforcement Bars\" " +
        //            "epd__date_validity_ends:>\"2024-10-31\" " +
        //            "jurisdiction:IN(\"US-NY\")"
        //            );
        //        //RhinoApp.WriteLine(materialData.Length.ToString());
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Response is HttpWebResponse errorResponse)
        //        {
        //            HttpStatusCode statusCode = errorResponse.StatusCode;
        //            // Handle the non-200 status code (e.g., log, throw, or return an error message)
        //            RhinoApp.WriteLine($"Non-200 Status Code: {(int)statusCode} ({statusCode})");
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //        else
        //        {
        //            // Handle other exceptions (e.g., network issues)
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //    }

        //    try
        //    {
        //        JArray matArray = JArray.Parse(materialData);
        //        RhinoApp.WriteLine(matArray.Count.ToString());

        //        JObject mat = matArray[0] as JObject;
        //        if (mat != null)
        //        {
        //            string name = mat["name"]?.ToString();
        //            string gwp = mat["gwp"]?.ToString();

        //            RhinoApp.WriteLine(name);
        //            RhinoApp.Write(gwp);
        //        }
        //    }catch(Exception ex)
        //    {
        //        RhinoApp.WriteLine($"{ex.Message}");
        //    }
            
        //}

        //private void TestMaterialParser()
        //{
        //    string materialData = null;

        //    try
        //    {
        //        EC3Request request = new EC3Request("suUNpZ8ORcN94YgEdDSxpf4YYmOiAw");
        //        materialData = request.GetMaterialData(
        //            "!pragma eMF(\"2.0/1\"), " +
        //            "lcia(\"TRACI 2.1\") " +
        //            "category:\"RebarSteel\" " +
        //            "epd__date_validity_ends:>\"2024-10-31\" " +
        //            "jurisdiction:IN(\"US\")"
        //            );
        //        //RhinoApp.WriteLine(materialData.Length.ToString());
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Response is HttpWebResponse errorResponse)
        //        {
        //            HttpStatusCode statusCode = errorResponse.StatusCode;
        //            // Handle the non-200 status code (e.g., log, throw, or return an error message)
        //            RhinoApp.WriteLine($"Non-200 Status Code: {(int)statusCode} ({statusCode})");
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //        else
        //        {
        //            // Handle other exceptions (e.g., network issues)
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //    }

        //    if (materialData == null) { return; }

        //    EC3MaterialParser matParser = new EC3MaterialParser(materialData);
        //    RhinoApp.WriteLine(matParser.GetMaterialCount().ToString());
        //    RhinoApp.WriteLine(matParser.GetAverageGwp().ToString());
        // }

        //private void TestCategoryParse()
        //{
        //    EC3CategoryTree categories = new EC3CategoryTree("suUNpZ8ORcN94YgEdDSxpf4YYmOiAw");
        //    categories.UpdateEC3CategoriesToFile();
        //}

        //private void TestCategoryFilePath()
        //{
        //    EC3CategoryTree categories = new EC3CategoryTree("suUNpZ8ORcN94YgEdDSxpf4YYmOiAw");
        //    RhinoApp.WriteLine(categories.GetFilePath());
        //}

        //private void TestUnitConversion()
        //{
        //    EPD epd1 = new EPD("epd1", 125, "kg", 2.5, "kg/m3", "RebarSteel");
        //    string type = epd1.unitMaterial.GetType().ToString();
        //    RhinoApp.WriteLine(type);
        //}

        //private void TestEPDParse()
        //{
        //    string materialData = null;

        //    try
        //    {
        //        EC3MaterialFilter filter = new EC3MaterialFilter();
        //        filter.SetCategory("RebarSteel");
        //        /*filter.SetExpirationDate("2027-10-31");*/
        //        filter.SetCountry("US");
        //        filter.SetState("NY");
        //        RhinoApp.WriteLine(filter.GetMaterialFilter());

        //        materialData = EC3Request.GetMaterialData(
        //            filter.GetMaterialFilter()
        //            );
        //        RhinoApp.WriteLine(materialData);
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Response is HttpWebResponse errorResponse)
        //        {
        //            HttpStatusCode statusCode = errorResponse.StatusCode;
        //            // Handle the non-200 status code (e.g., log, throw, or return an error message)
        //            RhinoApp.WriteLine($"Non-200 Status Code: {(int)statusCode} ({statusCode})");
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //        else
        //        {
        //            // Handle other exceptions (e.g., network issues)
        //            RhinoApp.WriteLine($"Error: {ex.Message}");
        //        }
        //    }

        //    if (materialData == null) { return; }

        //    JArray matArray = JArray.Parse(materialData);
        //    List<EPD> epds = UnitManager.ParseEPDs(matArray);

        //    Mass averageGwp = EPD.AverageGwp(epds, Volume.FromCubicMeters(2));

        //    Density averageDenstiy = EPD.AverageDensity(epds);

        //    RhinoApp.WriteLine(averageGwp.ToString());
        //    RhinoApp.WriteLine(averageDenstiy.ToString());

        //    foreach (EPD epd in epds)
        //    {
        //        List<string> epdData = epd.GetPrintableData();
        //        foreach (string data in epdData)
        //        {
        //            RhinoApp.WriteLine(data);
        //        }
        //    }
        //}

        //private void TestMaterialFilter()
        //{
        //    EC3MaterialFilter filter = new EC3MaterialFilter();
        //    filter.SetCategory("RebarSteel");
        //    /*filter.SetExpirationDate("2024-10-31");*/
        //    filter.SetCountry("US");
        //    filter.SetState("NY");

        //    RhinoApp.WriteLine(filter.GetMaterialFilter());
        //}

        //private void TestGetInputText()
        //{
        //    UserText ut = new UserText();
        //    ut.SetPrompt("Set your category");
        //    ut.UserInputText();
        //    string input = ut.GetInputText();
        //    RhinoApp.WriteLine("User input: " + input);
        //}

        //private void TestUserListOption()
        //{
        //    EC3Selector selector = new EC3Selector(3);
        //    selector.GetSelection();
        //}

        //private void TestCategoryTree()
        //{
        //    EC3CategoryTree tree = EC3CategoryTree.Instance;
        //    RhinoApp.WriteLine(tree.masterformats.Count.ToString());
        //}

        //private void TestEmbeddedCSV()
        //{
        //    //SearchForm sf = new SearchForm();
        //    //List<string> names;
        //    //List<string> codes;
        //    //sf.PopulateCountryLists(out names, out codes);

        //    //RhinoApp.WriteLine(codes[0]);
        //}

        //private void TestSearchForm1()
        //{
        //    if (form == null)
        //    {
        //        form = new SearchForm (new EC3MaterialFilter()) { Owner = RhinoEtoApp.MainWindow };
        //        form.Closed += OnFormClosed;
        //        form.Show();
        //    }
        //}

        //private void OnFormClosed(object sender, EventArgs e)
        //{
        //    form.Dispose();
        //    form = null;
        //}

        private void TestKaleidoscopeRead()
        {
            KaleidoscopeSearch ksSearch = new KaleidoscopeSearch();
            MaterialFilter mf = new MaterialFilter();
            mf.SetKaleidoscopeCategory("Wall");
            mf.includeBC = true;
            mf.includeBiogen = true;

            List<EPD> epds = ksSearch.Search(mf);

            foreach(EPD epd in epds)
            {
                List<string> epdData = epd.GetPrintableData();
                foreach(string s in epdData)
                {
                    RhinoApp.WriteLine(s);
                }
                RhinoApp.WriteLine("------------");
            }
        }
    }
}