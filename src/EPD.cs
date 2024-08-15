using Newtonsoft.Json.Linq;
using Rhino;
using System;
using System.Collections.Generic;
using UnitsNet;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// Container for GWP data and corresponding metadata. This class is currently built
    /// to accomodate EPDs from EC3, but should be expanded to be usable for different
    /// databases.
    /// </summary>
    public class EPD
    {
        JObject JObj;
        public string name { get; set; }
        public string id { get; set; }

        public Mass gwp;
        public IQuantity unitMaterial;
        public Density density;

        public string ec3id { get; }
        public string category { get; }
        public string manufacturer { get; }
        public int dimension { get; }

        public string description = null;
        public string tooltip = null;


        // a material is invalid if it is defined by gwp per unit mass and does not have
        // density data to convert to per unit volume or if the material is missing critical
        // data like gwp or declaired unit.
        public bool valid = true;

        public MaterialFilter mf { get; }

        /// <summary>
        /// Constructor used to create an EPD from a JObject retreived from the EC3
        /// database.
        /// </summary>
        /// <param name="obj"> A JObject retreived from the EC3 database. </param>
        /// <param name="searchPar"> Search parameters in the form of an EC3MaterialFilter
        /// object used to replicate the search that retreived the JObject.</param>
        public EPD(JObject obj, MaterialFilter searchPar = null)
        {
            JObj = obj;
            mf = searchPar;

            // basic stuff
            name = obj["name"]?.ToString();
            ec3id = obj["id"]?.ToString();
            category = mf.categoryName?.ToString();
            manufacturer = obj["manufacturer"]["name"]?.ToString();

            // parse GWP
            double gwpVal = UnitManager.ParseDoubleWithUnit(obj, "gwp", out string gwpUnit);
            if (gwpUnit == null) { valid = false; }
            // GWP always assumed to be defined in kgCO2e (MAYBE UPGRADE TO OTHER MASS UNITS)
            gwp = (Mass)Quantity.FromUnitAbbreviation(gwpVal, "kg");

            // get declared unit of the gwp
            unitMaterial = UnitManager.ParseQuantity(obj, "declared_unit", out valid);

            // parse density
            IQuantity d = UnitManager.ParseQuantity(obj, "density", out bool validDensity);
            if (validDensity) { density = (Density)d; }
            if (unitMaterial == null) { valid = false; }
            if (!validDensity && unitMaterial != null)
            {
                if (unitMaterial.GetType() == typeof(Mass)) { valid = false; }
            }

            // if everything works out, refactor mass to volume
            if (valid && unitMaterial.GetType() == typeof(Mass))
            {
                MassToVolume();
            }

            if (valid) { dimension = UnitManager.GetUnitDimension(unitMaterial); }
        }

        /// <summary>
        /// Constructor for EPD based on user-defined inputs. Should be used to construct
        /// custom industry average EPDs or to recreate EPDs that have been reduced to 
        /// string format.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="gwp"></param>
        /// <param name="unit"></param>
        /// <param name="density"></param>
        /// <param name="densityUnit"></param>
        /// <param name="category"></param>
        /// <param name="searchPar"></param>
        public EPD(string name, double gwp, string unit, double density, string densityUnit,
            string category, int dimension, MaterialFilter searchPar,
            string manufacturer, string id = null)
        {
            this.name = name;
            this.category = category;
            mf = searchPar;
            this.manufacturer = manufacturer;
            this.dimension = dimension;
            this.ec3id = id;

            this.gwp = (Mass)Quantity.FromUnitAbbreviation(gwp, "kg");

            unitMaterial = UnitManager.ParseQuantity("1 " + unit, out bool valid);

            if (density != 0 && densityUnit != null)
            {
                densityUnit = density.ToString() + " " + densityUnit;
                densityUnit = UnitManager.ParseDensityUnit(densityUnit);
                IQuantity roh = UnitManager.ParseQuantity(densityUnit, out bool validDensity);
                if (validDensity) { this.density = (Density)roh; }
            }
            else if (densityUnit == null && unitMaterial.GetType() == typeof(Mass))
            {
                this.valid = false;
            }

            if (this.valid && unitMaterial.GetType() == typeof(Mass))
            {
                MassToVolume();
            }
        }

        public EPD(string name, Mass gwp, IQuantity unitMat, Density density,
            string category, MaterialFilter searchPar, string manufacturer)
        {
            this.name = name;
            this.category = category;
            mf = searchPar;
            this.manufacturer = manufacturer;

            this.gwp = gwp;
            unitMaterial = unitMat;
            this.density = density;

            if (valid && unitMaterial.GetType() == typeof(Mass))
            {
                MassToVolume();
            }

            if (valid) { dimension = UnitManager.GetUnitDimension(unitMaterial); }
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
            if (unitMat.GetType() != unitMaterial.Unit.GetType()) { return 0; }
            double convertedUnit = (double)unitMaterial.ToUnit(unitMat).Value;
            double gwpVal = (double)gwp.Value;
            return gwpVal / convertedUnit;
        }

        public Mass GetGwpConverted(IQuantity unitReq)
        {
            if (unitMaterial == null) { return new Mass(); }
            if (unitReq.GetType() != unitMaterial.GetType()) { return new Mass(); }
            IQuantity convertedUnit = unitMaterial.ToUnit(unitReq.Unit);
            return gwp * (double)unitReq.Value / (double)convertedUnit.Value;
        }

        public Mass GetGwpPerSystemUnit(RhinoDoc doc)
        {
            IQuantity systemUnit = UnitManager.GetSystemUnit(doc, this.dimension);
            return GetGwpConverted(systemUnit);
        }

        /// <summary>
        /// Converts the unitMaterial of this EPD from mass to volume based on defined
        /// density.
        /// </summary>
        private void MassToVolume()
        {
            Mass mass = (Mass)unitMaterial;
            Volume volume = mass / density;
            unitMaterial = volume;
        }

        /// <summary>
        /// Used for debugging. Retreives data from EPD to be printed.
        /// </summary>
        /// <returns> A list of strings containing printable EPD data. </returns>
        public List<string> GetPrintableData()
        {
            List<string> printable = new List<string>();
            printable.Add("Name: " + name);
            printable.Add("GWP: " + gwp.ToString());
            printable.Add("Declared Unit: " + unitMaterial.ToString());
            printable.Add("Density: " + density.ToString());
            printable.Add("Category: " + category);
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
            foreach (EPD epd in epds)
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
            foreach (EPD epd in epds)
            {
                if (!epd.valid) { continue; }
                if (epd.density.Value == 0) { continue; }
                sumDensity += epd.density;
                count++;
            }
            if (count == 0) { return sumDensity; }
            return sumDensity / count;
        }

        public bool Equals (EPD epd)
        {
            if (epd == null) { return false; }

            if (
                epd.name == name &&
                epd.category == category && 
                epd.dimension == dimension &&
                epd.GetGwpPerUnit(unitMaterial.Unit) == this.GetGwpPerUnit(unitMaterial.Unit) &&
                epd.manufacturer == manufacturer
                )
            {
                return true;
            }

            return false;
        }
    }
}
