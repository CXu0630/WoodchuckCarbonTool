using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    public class Tester : Command
    {
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

        private void TestUnitConversion()
        {
            EPD epd1 = new EPD("epd1", 125, "kg", 2.5, "kg/m3", "RebarSteel");
            string type = epd1.unitMaterial.GetType().ToString();
            RhinoApp.WriteLine(type);
        }

        private void TestEPDParse()
        {
            string materialData = null;

            try
            {
                EC3MaterialFilter filter = new EC3MaterialFilter();
                filter.SetCategory("RebarSteel");
                filter.SetExpirationDate("2027-10-31");
                filter.SetCountry("US");
                filter.SetState("NY");
                RhinoApp.WriteLine(filter.GetMaterialFilter());

                materialData = EC3Request.GetMaterialData(
                    filter.GetMaterialFilter()
                    );
                RhinoApp.WriteLine(materialData);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    HttpStatusCode statusCode = errorResponse.StatusCode;
                    // Handle the non-200 status code (e.g., log, throw, or return an error message)
                    RhinoApp.WriteLine($"Non-200 Status Code: {(int)statusCode} ({statusCode})");
                    RhinoApp.WriteLine($"Error: {ex.Message}");
                }
                else
                {
                    // Handle other exceptions (e.g., network issues)
                    RhinoApp.WriteLine($"Error: {ex.Message}");
                }
            }

            if (materialData == null) { return; }

            JArray matArray = JArray.Parse(materialData);
            List<EPD> epds = EC3MaterialParser.ParseEPDs(matArray);

            Mass averageGwp = EPD.AverageGwp(epds, Volume.FromCubicMeters(2));

            Density averageDenstiy = EPD.AverageDensity(epds);

            RhinoApp.WriteLine(averageGwp.ToString());
            RhinoApp.WriteLine(averageDenstiy.ToString());

            foreach (EPD epd in epds)
            {
                List<string> epdData = epd.GetPrintableData();
                foreach (string data in epdData)
                {
                    RhinoApp.WriteLine(data);
                }
            }
        }

        private void TestMaterialFilter()
        {
            EC3MaterialFilter filter = new EC3MaterialFilter();
            filter.SetCategory("RebarSteel");
            filter.SetExpirationDate("2024-10-31");
            filter.SetCountry("US");
            filter.SetState("NY");

            RhinoApp.WriteLine(filter.GetMaterialFilter());
        }

        private void TestGetInputText()
        {
            UserText ut = new UserText();
            ut.SetPrompt("Set your category");
            ut.UserInputText();
            string input = ut.GetInputText();
            RhinoApp.WriteLine("User input: " + input);
        }

        private void TestUserListOption()
        {
            EC3Selector selector = new EC3Selector(3);
            selector.GetSelection();
        }

        private void TestCategoryTree()
        {
            EC3CategoryTree tree = EC3CategoryTree.Instance;
            RhinoApp.WriteLine(tree.masterformats.Count.ToString());
        }
    }
}