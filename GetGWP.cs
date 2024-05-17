using System;
using System.Runtime.Remoting;
using EC3CarbonCalculator.UI;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using UnitsNet;

namespace EC3CarbonCalculator
{
    public class GetGWP : Command
    {
        private ResultForm form { get; set; }

        public GetGWP()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static GetGWP Instance { get; private set; }

        public override string EnglishName => "GetGWP";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SimpleSelector getSelector = new SimpleSelector();
            getSelector.SetPrompt("Select geometry to calculate GWP for.");
            Rhino.DocObjects.ObjRef[] objRefs = getSelector.GetSelection();

            double totalGWP = 0;
            int dimension = -1;

            foreach (Rhino.DocObjects.ObjRef objRef in objRefs)
            {
                EPD epd = null;
                if (objRef != null) { epd = EPDManager.Get(objRef); }
                if (epd == null) { continue; }

                IQuantity unit = UnitManager.GetSystemUnit(doc, epd.dimension);
                double quantity = GeometryProcessor.GetDimensionalInfo(objRef, epd.dimension);
                double unitGWP = epd.GetGwpConverted(unit).Value;
                if (dimension == -1) dimension = epd.dimension;

                totalGWP += quantity * unitGWP;
            }

            string rsltStr = "Total GWP of Selected Objects: \n" + totalGWP.ToString("F3") +
                "kgCO2e";

            if (form == null)
            {
                form = new ResultForm(rsltStr) { Owner = RhinoEtoApp.MainWindow };
                form.Closed += OnFormClosed;
                form.Show();
            }

            return Result.Success;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            form.Dispose();
            form = null;
        }
    }
}