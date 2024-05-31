using Rhino;
using Rhino.DocObjects;

namespace EC3CarbonCalculator.src.EC3
{
    // WARNING: something will likely go wrong with this code when used on blocks
    /// <summary>
    /// A child of the SimpleSelector class, customized for selecting geometry for
    /// assigning EPD information to from the EC3 database. The customization is mostly
    /// just telling the user about dimensions and how that will affect calculation
    /// method... These specific customizations might become unnecessary once we use a
    /// more robust system for getting material quantity.
    /// </summary>
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

            switch (dimension)
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
            getObject.SetCommandPrompt("Select geometry to assign EPD to");
            getObject.GroupSelect = true;
            getObject.EnableClearObjectsOnEntry(false);
            getObject.EnableUnselectObjectsOnExit(false);
            getObject.DeselectAllBeforePostSelect = false;
        }
    }
}
