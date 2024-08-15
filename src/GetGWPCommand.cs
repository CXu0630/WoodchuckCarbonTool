using WoodchuckCarbonTool.src.UI;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using System;
using UnitsNet;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// Rhino command that retreives the GWP of a selection of objects. For now, this
    /// command only creates a textbox output with total GWP. Hope for future is that
    /// it can build data plots for selected geometry and create a custom display of the
    /// model based on GWP data.
    /// </summary>
    public class GetGwpCommand : Command
    {
        private ResultForm form { get; set; }

        public GetGwpCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static GetGwpCommand Instance { get; private set; }

        public override string EnglishName => "WoodchuckGetGWP";

        /// <summary>
        /// The command asks for a user selection of Rhino Objects and draws up a textbox
        /// window that displays their total GWP.
        /// </summary>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Get user selection of Rhino Objects
            // WARNING: something will likely go wrong with this code when used on blocks
            SimpleSelector getSelector = new SimpleSelector();
            getSelector.SetPrompt("Select geometry to calculate GWP for");
            Rhino.DocObjects.ObjRef[] objRefs = getSelector.GetSelection();

            double totalGWP = GwpCalculator.GetTotalGwp(doc, objRefs);

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