using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using Rhino;

namespace WoodchuckCarbonTool.src
{
    internal class GWPCalculator
    {
        /// <summary>
        /// Retreives the total GWP of a selection of Rhino Object References.
        /// </summary>
        public static double GetTotalGwp(RhinoDoc doc, Rhino.DocObjects.ObjRef[] objRefs)
        {
            double totalGWP = 0;
            int dimension = -1;

            // Access the epd stored in the UserData of each selected object
            foreach (Rhino.DocObjects.ObjRef objRef in objRefs)
            {
                EPD epd = null;
                if (objRef != null) { epd = EPDManager.Get(objRef); }
                if (epd == null) { continue; }

                IQuantity unit = UnitManager.GetSystemUnit(doc, epd.dimension);
                double quantity = GeometryProcessor.GetDimensionalInfo(objRef, epd.dimension);
                double unitGWP = epd.GetGwpConverted(unit).Value;
                if (dimension == -1) dimension = epd.dimension;

                totalGWP += quantity * unitGWP * epd.percentageSolid / 100;
            }

            return totalGWP;
        }

        public static string FormatDoubleWithLengthLimit(double number, int maxLength)
        {
            string fixedPoint = number.ToString("F2"); // Attempt with fixed-point
            if (fixedPoint.Length <= maxLength)
            {
                return fixedPoint;
            }
            else
            {
                string scientific = number.ToString("E" + (maxLength - 6)); // Convert to scientific notation
                return scientific.Length <= maxLength ? scientific : scientific.Substring(0, maxLength);
            }
        }
    }
}
