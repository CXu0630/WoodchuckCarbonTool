using Rhino.Input.Custom;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Input;

namespace EC3CarbonCalculator
{
    internal class EC3Selector : SimpleSelector
    {
        int dimension = 3;

        public EC3Selector(int dimension) 
        { 
            this.dimension = dimension;
        }

        protected override void SetupGetObject()
        {
            string prompt = "";

            switch (this.dimension)
            {
                case 3:
                    prompt = "Your selected category reportes GWP per volume. Select " +
                        "volumetric solid objects to calculate for.";
                    getObject.GeometryFilter =
                        ObjectType.Brep |
                        ObjectType.PolysrfFilter;
                    break;
                case 2:
                    prompt = "Your selected category reports GWP per area. Select surfaces" +
                        "to calculate for. If solids are selected, the total area of their" +
                        "faces will be used.";
                    getObject.GeometryFilter =
                        ObjectType.Brep |
                        ObjectType.Surface |
                        ObjectType.PolysrfFilter;
                    break;
                case 1:
                    prompt = "Your selected category reports GWP per length. Select curves" +
                        "to calculate for. If surfaces or solids are selected, the total" +
                        "length of their edges will be used.";
                    getObject.GeometryFilter = 
                        ObjectType.Curve |
                        ObjectType.Surface |
                        ObjectType.Brep |
                        ObjectType.PolysrfFilter;
                    break;
            }

            RhinoApp.WriteLine(prompt);

            getObject.SubObjectSelect = true;
            getObject.SetCommandPrompt("Select geometry to calculate carbon for");
            getObject.GroupSelect = true;
            getObject.EnableClearObjectsOnEntry(false);
            getObject.EnableUnselectObjectsOnExit(false);
            getObject.DeselectAllBeforePostSelect = false;
        }
    }
}
