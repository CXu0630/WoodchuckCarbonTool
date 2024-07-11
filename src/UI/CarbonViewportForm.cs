using Eto.Forms;
using Rhino.Commands;
using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.UI.Controls;

namespace WoodchuckCarbonTool.src.UI
{
    internal class CarbonViewportForm : Dialog<Result>
    {
        ViewportControl VpControl;

        public CarbonViewportForm() 
        {
            Title = "Carbon Viewport";
            Resizable = true;
            VpControl = new ViewportControl();

            
        }
    }
}
