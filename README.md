## What is this tool?

This plugin is used to provide easy access to the EC3 carbon database (and hopefully other carbon databases in near future) from within Rhino. It includes calculator functionality that can be used to quickly calculate embodied carbon of modeled geometry.

## How to use this tool?

Use the SearchEPD command to search the carbon databases for materials that fit your requirement. For more details on each material, use the View in Browser button to see the original EC3 site. Embodied carbon materials can be assigned to Rhino Objects similar to rendering materials. To assign a material to geometry, use the Assign to Geometry button and pick out the geometry you want to assign to.

## Details

### What does it do?
- Database connection
  - makes various carbon databases available from Rhino (all the current tools in the carbon => Rhino / Grasshopper category are either no longer maintained or not yet well developed)
  - EC3 (real-time-ish data from manufacturers), CLF Materials Baseline (compiled industry
    - wide material data), Kaleidoscope (compiled industry-wide assembly data)
- Geometric calculator
  - extracts material quantities from Rhino geometry and quickly calculates carbon
- Carbon education (?) tool
  - tooltips and material-specific information that is not in the Embodied Carbon Playbook
  - graphics and captions for descriptions
- Graphics generation (todo)
  - generate graphs / custom views of carbon intensities of modeled components

### What we have so far
- EPD assignment, display, and storage framework that is independent of database
- Databases
  - EC3, CLF
  - UI and information retrieval system for each
    - EC3 uses API calls to the EC3 server
    - CLF uses a formatted csv embedded into project resources
- Geometric calculator
  - currently only retrieves whatever volume, surface area, or length that is modeled out
  - results are currently only a total gwp value

### What is being worked on
- Databases
  - integrating the Kaleidoscope database
- Carbon education(?)
  - category-specific information that displays as the user is searching for EPDs

### What is scheduled for future
- Graphics generation
  - refined material quantity options (ie. percentages)
- API verification process that does not involve us putting our key in the code
  - possibility for wider distribution too

### What to do with it after I'm outta here
- API Key
  - currently linked to my school account and supposed to be valid until next may
