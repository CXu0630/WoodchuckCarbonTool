using Newtonsoft.Json.Linq;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    internal class EPD
    {
        JObject JObj;
        public string name { get; set; }

        public Mass gwp;
        public IQuantity unitMaterial;
        public Density density;

        public string id { get; }
        public string uuid { get; }
        public string category { get; }

        // a material is invalid if it is defined by gwp per unit mass and does not have
        // density data to convert to per unit volume or if the material is missing critical
        // data like gwp or declaired unit.
        public bool valid = true;

        public EC3MaterialFilter searchParameters { get; }

        /// <summary>
        /// Constructor used to create an EPD from a JObject retreived from the EC3
        /// database.
        /// </summary>
        /// <param name="obj"> A JObject retreived from the EC3 database. </param>
        /// <param name="searchPar"> Search parameters in the form of an EC3MaterialFilter
        /// object used to replicate the search that retreived the JObject.</param>
        public EPD(JObject obj, EC3MaterialFilter searchPar = null)
        {
            this.JObj = obj;

            // basic stuff
            this.name = obj["name"]?.ToString();
            this.id = obj["id"]?.ToString();
            this.uuid = obj["open_xpd_uuid"]?.ToString();
            this.category = obj["category"]["name"]?.ToString();

            // parse GWP
            double gwpVal = EC3MaterialParser.ParseDoubleWithUnit(obj, "gwp", out string gwpUnit);
            if (gwpUnit == null) { this.valid = false; }
            // GWP always assumed to be defined in kgCO2e (MAYBE UPGRADE TO OTHER MASS UNITS)
            this.gwp = (Mass)Quantity.FromUnitAbbreviation(gwpVal, "kg");

            // get declared unit of the gwp
            double unitMultiplier = EC3MaterialParser.ParseDoubleWithUnit(obj, "declared_unit", out string unit);
            if (unit == null) { this.valid = false; }
            // "t" could be different units and "ton" isn't recognized as an abbreviation
            else if (unit == "t" || unit == "ton")
            {
                unit = "t";
                string unitMat = unitMultiplier.ToString() + " " + unit;
                this.unitMaterial = Quantity.Parse(typeof(Mass), unitMat);
            }
            else
            {
                this.unitMaterial = Quantity.FromUnitAbbreviation(unitMultiplier, unit);
            }

            // parse density
            double densityVal = EC3MaterialParser.ParseDoubleWithUnit(obj, "density", out string densityUnit);
            // parse the density unit separately as it sometimes needs formatting
            densityUnit = EC3MaterialParser.ParseDensityUnit(densityUnit);
            if (densityUnit == null)
            {
                if (unitMaterial.GetType() == typeof(Mass)) { this.valid = false; }
            }
            else 
            {
                // density doesn't parse unless we explicitly parse density :(
                densityUnit = densityVal.ToString() + " " + densityUnit;
                this.density = (Density)Quantity.Parse(typeof(Density), densityUnit); 
            }
            
            // if everything works out, refactor mass to volume
            if (this.valid && this.unitMaterial.GetType() == typeof(Mass))
            {
                this.MassToVolume();
            }
        }

        /// <summary>
        /// Constructor for EPD based on user-defined inputs. Should be used to construct
        /// custom industry average EPDs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="gwp"></param>
        /// <param name="unit"></param>
        /// <param name="density"></param>
        /// <param name="densityUnit"></param>
        /// <param name="category"></param>
        /// <param name="searchPar"></param>
        public EPD(string name, double gwp, string unit, double density, string densityUnit, 
            string category, EC3MaterialFilter searchPar = null)
        {
            this.name = name;
            this.category = category;
            this.searchParameters = searchPar;

            this.gwp = (Mass)Quantity.FromUnitAbbreviation(gwp, "kg"); 

            if(unit == "t") { this.unitMaterial = Quantity.Parse(typeof(Mass), "1 " + unit); }
            else { this.unitMaterial = Quantity.FromUnitAbbreviation(1, unit); }

            if (density != 0 &&  densityUnit != null) 
            {
                densityUnit = density.ToString() + " " + densityUnit;
                densityUnit = EC3MaterialParser.ParseDensityUnit(densityUnit);
                this.density = (Density)Quantity.Parse(typeof(Density), densityUnit);
            } 
            else if (densityUnit == null && unitMaterial.GetType() == typeof(Mass))
            {
                this.valid = false;
            }

            if (this.valid && this.unitMaterial.GetType() == typeof(Mass))
            {
                this.MassToVolume();
            }
        }

        public EPD(string name, Mass gwp, IQuantity unitMat, Density density, 
            string category, EC3MaterialFilter searchPar = null)
        {
            this.name = name;
            this.category = category;
            this.searchParameters = searchPar;

            this.gwp = gwp;
            this.unitMaterial = unitMat;
            this.density = density;

            if (this.valid && this.unitMaterial.GetType() == typeof(Mass))
            {
                this.MassToVolume();
            }
        }

        /// <summary>
        /// Returns the gwp value of this EPD per requested unit material. If the unit
        /// material is of a different type than stored in the EPD (m^2 as opposed to m^3
        /// for instance), returns 0.
        /// </summary>
        /// <param name="unitMat"></param>
        /// <returns></returns>
        public double GetGwpPerUnit(Enum unitMat)
        {
            if (unitMat.GetType() != this.unitMaterial.Unit.GetType()) { return 0; }
            double convertedUnit = (double)this.unitMaterial.ToUnit(unitMat).Value;
            double gwpVal = (double)this.gwp.Value;
            return gwpVal / convertedUnit;
        }

        public Mass GetGwpConverted(IQuantity unitReq)
        {
            if (unitReq.GetType() != this.unitMaterial.GetType()) { return new Mass(); }
            IQuantity convertedUnit = this.unitMaterial.ToUnit(unitReq.Unit);
            return gwp * (double)unitReq.Value / (double)convertedUnit.Value;
        }

        /// <summary>
        /// Converts the unitMaterial of this EPD from mass to volume based on defined
        /// density.
        /// </summary>
        private void MassToVolume()
        {
            Mass mass = (Mass)this.unitMaterial;
            Volume volume = mass / this.density;
            this.unitMaterial = volume;
        }

        /// <summary>
        /// Used for debugging. Retreives data from EPD to be printed.
        /// </summary>
        /// <returns> A list of strings containing printable EPD data. </returns>
        public List<string> GetPrintableData()
        {
            List<string> printable = new List<string>();
            printable.Add("Name: " + this.name);
            printable.Add("GWP: " + this.gwp.ToString());
            printable.Add("Declared Unit: " + this.unitMaterial.ToString());
            printable.Add("Density: " + this.density.ToString());
            printable.Add("Category: " + this.category);
            return printable;
        }

        /// <summary>
        /// Calculates the average GWP of a list of EPDs per unit material. If the EPDs
        /// were defined with a different unit material (m^2 as opposed to m^3 for instance) 
        /// than requested or if the EPDs did not contain valid gwp data, skips the EPD.
        /// </summary>
        /// <param name="epds"> A list of environmental product declaration objects </param>
        /// <param name="unitReq"> A UnitsNet unit, defines the material unit to take
        /// the average for. </param>
        /// <returns> The average GWP of the requested EPDs per unit. </returns>
        public static Mass AverageGwp(List<EPD> epds, IQuantity unitReq)
        {
            Mass gwps = new Mass();
            int count = 0;
            foreach(EPD epd in epds)
            {
                if (!epd.valid) { continue; }
                Mass gwpVal = epd.GetGwpConverted(unitReq);
                if (gwpVal.Value != 0) { gwps += gwpVal; count++; }
            }
            if (count == 0) { return gwps; }
            return gwps / count;
        }

        public static Density AverageDensity(List<EPD> epds)
        {
            Density sumDensity = new Density();
            int count = 0;
            foreach(EPD epd in epds)
            {
                if (!epd.valid) { continue; }
                if (epd.density.Value == 0) { continue; }
                sumDensity += epd.density;
                count++;
            }
            if (count == 0) { return sumDensity; }
            return sumDensity / count;
        }
    }
}
