using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.UI;
using System.IO.Ports;
using System.Linq;
using WoodchuckCarbonTool.src.UI;

namespace WoodchuckCarbonTool.src
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class WoodchuckCarbonToolPlugin : Rhino.PlugIns.PlugIn
    {
        public DocumentEpdMap DocumentEPDs;

        public WoodchuckCarbonToolPlugin()
        {
            Instance = this;
            DocumentEPDs = new DocumentEpdMap();
        }

        ///<summary>Gets the only instance of the WoodchuckCarbonToolPlugin plug-in.</summary>
        public static WoodchuckCarbonToolPlugin Instance { get; private set; }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.

        protected override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection)
        {
            var sample_page = new WckObjectProperties();
            collection.Add(sample_page);
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            return base.OnLoad(ref errorMessage);
        }

        private void OnCloseDocument(object sender, DocumentEventArgs e)
        {
            DocumentEPDs.Clear();
        }

        protected override bool ShouldCallWriteDocument(FileWriteOptions options)
        {
            return !options.WriteGeometryOnly && !options.WriteSelectedObjectsOnly;
        }

        protected override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options)
        {
            DocumentEPDs.WriteDocument(archive);
        }

        protected override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options)
        {
            DocumentEpdMap epdDict = new DocumentEpdMap();
            epdDict.ReadDocument(archive);

            if (!options.ImportMode && !options.ImportReferenceMode)
            {
                foreach (var pair in epdDict)
                {
                    if (DocumentEPDs.Keys.Contains(pair.Key))
                    {
                        string newId = pair.Value.AssignNewId();
                        DocumentEPDs.Add(newId, pair.Value);
                    }
                    else
                    {
                        DocumentEPDs.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

    }
}