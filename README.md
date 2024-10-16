## What is Woodchuck?

Woodchuck is used to provide easy access to the EC3 carbon database, Carbon Leadership Forum material baselines database, and the Kaleidoscope assemblies database. It includes geometric calculator functionalities that can be used to quickly calculate embodied carbon of modeled Rhino geometry. This is not a reporting tool. This is not a whole building life cycle assessment tool. 

Reach out to me at gx7@rice.edu or guangyu.xu0630@gmail.com with any questions, feedback, or bug reports.

![240730_WCKSearchGraphic_v001-01](https://github.com/user-attachments/assets/5e97d8d6-7c54-4af3-9703-0d420be7499c)

## Details

### System Requirements
Windows machine running Rhino 7 & 8
* Note: when using with Rhino 7, there may be an error with the rui file, this does not impact plugin use.

### Installation
1. Press win+R
2. Enter “cmd”
3. Copy the following line into the window that pops up, include the quotation marks, and replace “Rhino 7” with “Rhino 8” if that is what you’re using.

__"C:\Program Files\Rhino 7\System\Yak.exe" install woodchuck__

4. Once the window displays install success, restart Rhino if you have it open

### Updates
Woodchuck is under regular development, so you may want to check for updates every now and then. There are two ways to download an update:
1. Repeat the installation process (you don’t need to uninstall beforehand)
2. In the PackageManager in Rhino, look for Woodchuck in your installed plugins and select an update

### Operations
__WoodchuckSearchEPD__

Search carbon databases for materials that fit your requirements. More details can be seen by hovering over material names or clicking View in Browser when available. Embodied carbon materials can be assigned to Rhino geometry (similar to rendering materials) by clicking the Assign to Object button. Once materials are assigned, they will appear as an object property under the Woodchuck properties tab.

<img width="667" alt="WCKSearch" src="https://github.com/user-attachments/assets/5bc16f6c-0418-42af-8012-f7c4f5ea6e70">

__WoodchuckCustomEPD__

If your material is not present in any of the databases, create a custom EPD for your material. Input your material name, carbon intensity per unit, and dimension of unit, then assign your custom material to modeled objects. For now, the only way to reuse custom materials is to view them by checking the object properties of an object already assigned with that material and clicking assign to object from the properties panel.

__WoodchuckCarbonView__

Generate a diagrammatic view of carbon densities in your model. Objects are color-coded based on their carbon content. You can set the min and max values for your legend for consistency across diagrams. Right now, carbon intensities that are calculated per unit length or per unit area can only be displayed if the modeled object is a volume. Carbon intensities will be displayed per unit volume of the modeled object.

<img width="890" alt="WCKView" src="https://github.com/user-attachments/assets/a5a5b602-e9c1-42d6-8a1c-8e1d468b4556">

__Properties: Embodied Carbon Material__

Look for the woodchuck icon under the object properties tab to the right of your screen. This is where you can view the carbon properties of selected objects, such as assigned material, percentage solid, and total GWP.

<img width="736" alt="WCKProperties" src="https://github.com/user-attachments/assets/ad03ce07-b204-4852-95bc-ff1dfb301285">

Note 1: if you downloaded before 09/26/2024, there have been updates to the way EPDs declared with per unit area and per unit length are calculated. Consider updating to 2.0.5-alpha.

Note 2: if your Rhino software is installed somewhere other than the default location, you may need to locate the Yak.exe for installation instruction step 3.
