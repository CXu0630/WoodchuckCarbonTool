## What is Woodchuck?

Woodchuck is used to provide easy access to various embodied carbon databases from within Rhino. It includes calculator functionality that can be used to quickly calculate embodied carbon of modeled geometry.

## How to use this tool?

Use the SearchEPD command to search the carbon databases for materials that fit your requirement. Embodied carbon materials can be assigned to Rhino Objects similar to rendering materials. To assign a material to geometry, use the Assign to Geometry button and pick out the geometry you want to assign to. The embodied carbon properties tab displays total embodied carbon and assigned EPDs of selected objects.

## Details

### What does it do?
- __Database connection__
  - makes various carbon databases available from Rhino (all the current tools in the carbon => Rhino / Grasshopper category are either no longer maintained or not yet well developed)
  - EC3 (real-time-ish data from manufacturers), CLF Materials Baseline (compiled industry-wide material data), Kaleidoscope (compiled industry-wide assembly data)
- __Geometric calculator__
  - extracts material quantities from Rhino geometry and quickly calculates carbon
- __Carbon information tool__
  - tooltips and material-specific information that is not in the Embodied Carbon Playbook
  - graphics and captions for descriptions
- __Graphics generation__
  - generate graphs / custom views of carbon intensities of modeled components

### What is being worked on
- __Carbon information__
  - we're working to provide information on carbon origins within the production process of different materials and material-specific tips for cutting down on carbon.
- __Carbon view__
  - we're working on a way to graphically represent carbon intensity of building components and generate custom diagrams.
