using System;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using WoodchuckCarbonTool.src.UI;

namespace WoodchuckCarbonTool.src
{
    public class CustomEpdCommand : Command
    {
        private CustomEpdForm customEpdForm {  get; set; }
        public CustomEpdCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CustomEpdCommand Instance { get; private set; }

        public override string EnglishName => "WoodchuckCustomEPD";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (customEpdForm == null) 
            { 
                customEpdForm = new CustomEpdForm(doc) { Owner = RhinoEtoApp.MainWindow };
                customEpdForm.Closed += OnFormClosed;
                customEpdForm.Show();
            }

            customEpdForm.BringToFront();
            customEpdForm.PopulateForm();

            return Result.Success;
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            customEpdForm.Dispose();
            customEpdForm = null;
        }
    }
}