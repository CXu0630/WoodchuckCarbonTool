using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src.UI
{
    /// <summary>
    /// This class defines an ETO form used to search the EC3 database for EPDs and
    /// and provides an interface for assigning EPDs to Rhino geometry. This class only
    /// defines the UI aspect of the search. Calculations and assignment are handled by
    /// SearchEPD.
    /// </summary>
    internal class SearchForm : Form
    {
        CLFUiElements clfUi;
        EC3UiElements ec3Ui;
        KaleidoscopeUiElements ksUi;
        // All search prameters are stored in the mf instead of individually
        MaterialFilter mf;
        Button search = new Button { Text = "Search" };
        RhinoDoc doc;

        // This is public as it needs to be edditable by multiple methods.
        Scrollable resPanel;
        Panel searchPanel;
        Panel descriptionPanel;

        // This event is used to signal when the search button has been pressed:
        // search parameters are locked in and an EPD search is performed.
        public delegate void SearchEventHandler(object sender, EventArgs e);
        public event SearchEventHandler SearchEvent;

        /// <summary>
        /// Constructs an instance of SearchForm. 
        /// </summary>
        /// <param name="mf"> Material Filter object in which search criteria for EPDs 
        /// will be stored. </param>
        public SearchForm(RhinoDoc doc, MaterialFilter mf)
        {
            ec3Ui = new EC3UiElements();
            clfUi = new CLFUiElements();
            ksUi = new KaleidoscopeUiElements();

            this.mf = mf;
            WindowStyle = WindowStyle.Default;
            Maximizable = true;
            Minimizable = true;
            Padding = new Padding(5);
            Resizable = true;
            ShowInTaskbar = true;
            Title = "Woodchuck Material Search";
            MinimumSize = new Size(900, 500);

            this.doc = doc;

            // Results exist in a panel to be updated with new content each search
            resPanel = new Scrollable
            {
                ExpandContentWidth = true
            };
            resPanel.BackgroundColor = Colors.LightGrey;
            //

            descriptionPanel = new Panel();

            ec3Ui.CategoryChangeEvent += (s, e) =>
            {
                RepopulateDescriptionPanel(e.category);
            };
            clfUi.CategoryChangeEvent += (s, e) =>
            {
                RepopulateDescriptionPanel(e.category);
            };
            ksUi.CategoryChangeEvent += (s, e) =>
            {
                RepopulateDescriptionPanel(e.category);
            };

            searchPanel = new Panel();
            // Two basic layouts: one for search parameters, one for search results
            DynamicLayout searchLayout = SearchLayout();
            
            // Sends an event signal to listeners in other classes when a search is 
            // performed.
            search.Click += async (s, e) =>
            {
                RepopulateResultMessage("Loading...");
                await Task.Run(() => SearchEvent?.Invoke(this, e));
            };

            DynamicLayout layout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            // The contents of the panel is divided in two, with search parameters and 
            // criteria on the left, and search results on the right.
            layout.BeginHorizontal();
            layout.Add(searchLayout);
            layout.Add(resPanel);
            layout.EndHorizontal();

            Content = layout;
        }

        /// <summary>
        /// This method creates a dynamic layout for selecting a category, inputting 
        /// material search parameters and a search confirm button.
        /// </summary>
        private DynamicLayout SearchLayout()
        {
            DynamicLayout searchLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10),
                Size = new Size(380, 500)
            };

            searchLayout.Add(DatabaseLayout());
            searchLayout.Add(Spacer(Colors.DarkGray));

            searchLayout.Add(searchPanel);
            RepopulateSearchPanel();
            searchLayout.Add(Spacer(Colors.DarkGray));

            searchLayout.Add(null);

            searchLayout.Add(descriptionPanel);

            searchLayout.Add(ConfirmLayout());

            return searchLayout;
        }

        private void RepopulateSearchPanel()
        {
            DynamicLayout searchLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5)
            };

            switch (mf.dataBase)
            {
                case "EC3":
                    searchLayout.Add(ec3Ui.EC3CategoryLayout(mf));
                    searchLayout.Add(Spacer(Colors.DarkGray));
                    searchLayout.Add(ec3Ui.EC3GeneralSearchLayout(mf));
                    break;
                case "CLF":
                    searchLayout.Add(clfUi.CLFCategoryLayout(mf));
                    searchLayout.Add(clfUi.clfPanel);
                    break;
                case "Kaleidoscope":
                    searchLayout.Add(ksUi.KaleidoscopeLayout(mf));
                    searchLayout.Add(Spacer(Colors.DarkGray));
                    searchLayout.Add(ksUi.scopeLayout(mf));
                    break;
            }

            searchPanel.Content = searchLayout;
        }

        private DynamicLayout DatabaseLayout()
        {
            List<ListItem> dbOptions = new List<ListItem>
            {
                new ListItem
                {
                    Text = "EC3 Carbon Database",
                    Key = "EC3"
                },
                new ListItem
                {
                    Text = "CLF Material Baselines",
                    Key = "CLF"
                },
                new ListItem
                {
                    Text = "Kaleidoscope Assembly Database",
                    Key = "Kaleidoscope"
                }
            };
            /*DBOPTIONS.ADD(NEW LISTITEM
            {
                TEXT = "KALEIDOSCOPE",
                KEY = "KALEIDOSCOPE"
            });*/

            DropDown dbDD = new DropDown();
            dbDD.DataStore = dbOptions;
            // set default values
            dbDD.SelectedIndex = 0;
            mf.dataBase = dbDD.SelectedKey;
            dbDD.SelectedValueChanged += (sender, e) =>
            {
                mf = new MaterialFilter();
                mf.dataBase = dbDD.SelectedKey;
                RepopulateSearchPanel();
            };

            Label dbLabel = new Label { Text = "Database" };

            DynamicLayout dbLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
            };

            dbLayout.BeginHorizontal();
            dbLayout.Add(dbLabel);
            dbLayout.Add(null);
            dbLayout.Add(dbDD);
            dbLayout.EndHorizontal();

            return dbLayout;
        }

        public void RepopulateDescriptionPanel(string category)
        {
            string description = CategoryDescriptions.Instance.GetCategoryDescription(category);
            if (description == null) { descriptionPanel.Content = null; return; }

            Label titleLbl = new Label
            {
                Text = "Tips on " + category,
                Width = 380 - 20,
                TextColor = Colors.Gray,
            };

            Label descriptionLbl = new Label
            {
                Text = CategoryDescriptions.Instance.GetCategoryDescription(category),
                Font = new Font(SystemFonts.Default().FamilyName, 8),
                TextColor = Colors.Gray,
                Width = 380 - 20
            };

            DynamicLayout lyt = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Size = new Size(380-20, 200)
            };

            lyt.Add(titleLbl);
            lyt.Add(descriptionLbl);
            lyt.Add(null);

            descriptionPanel.Content = lyt;
            return;
        }

        /// <summary>
        /// A separate dynamic layout for the confirm search button
        /// </summary>
        /// <returns> A dynamic layout containing just the confirm button </returns>
        private DynamicLayout ConfirmLayout()
        {
            DynamicLayout cfLayout = new DynamicLayout();

            cfLayout.BeginHorizontal();
            cfLayout.Add(null);
            cfLayout.Add(search);

            return cfLayout;
        }

        /// <summary>
        /// Repopulates the search result panel with EPD search results from the EC3 API. 
        /// This method should never be called from inside the SearchForm class but 
        /// instead in whichever class is performing the searching...
        /// </summary>
        /// <param name="epds"></param>
        /// <param name="avgEPD"></param>
        public void RepopulateSearchResult(List<EPD> epds, EPD avgEPD = null)
        {
            DynamicLayout epdLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            if (avgEPD != null)
            {
                EpdPanel avgPanel = new UI.EpdPanel(this.doc, avgEPD, this.resPanel.Width - 40, this);
                epdLayout.Add(avgPanel);
                epdLayout.Add(Spacer(Colors.WhiteSmoke));
            }

            // NOTE: right now only 20 EPDs are displayed per search
            // Consider implementing with more EPDs displayed...
            foreach (EPD epd in epds.Take(30))
            {
                if (epd == null) { continue; }
                EpdPanel epdPanel = new UI.EpdPanel(this.doc, epd, this.resPanel.Width - 40, this);
                epdLayout.Add(epdPanel);
            }

            epdLayout.Add(null);

            resPanel.Content = epdLayout;
        }

        /// <summary>
        /// Used to display a text message to the user in the search result panel
        /// </summary>
        /// <param name="msg"> Text message to display on the search result panel. </param>
        public void RepopulateResultMessage(string msg)
        {
            DynamicLayout msgLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            Label msgLabel = new Label
            {
                Text = msg,
                Font = new Font(SystemFonts.Default().FamilyName, 12)
            };
            msgLayout.BeginHorizontal();
            msgLayout.Add(null);
            msgLayout.Add(msgLabel);
            msgLayout.Add(null);

            // Reassigning panel content updates the current content, but simply changing
            // the dynamic layout does not.
            resPanel.Content = msgLayout;
        }

        /// <summary>
        /// A spacer with a line in the middle
        /// </summary>
        /// <param name="bkgColor"> Color of the line </param>
        /// <returns>A spacer</returns>
        public static DynamicLayout Spacer(Color bkgColor)
        {
            DynamicLayout spacer = new DynamicLayout();

            Panel blank1 = new Panel { Height = 10 };
            Panel blank2 = new Panel { Height = 10 };
            Panel line = new Panel
            {
                Height = 1,
                BackgroundColor = bkgColor
            };

            spacer.Add(blank1);
            spacer.Add(line);
            spacer.Add(blank2);

            return spacer;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.SavePosition();
            base.OnClosing(e);
        }

        public MaterialFilter GetMaterialFilter()
        {
            return mf;
        }

        public void PopulateStartupMessage()
        {
            DynamicLayout dl = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            Label whatTitle = new Label
            {
                Text = "What is this tool?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label whatLabel = new Label
            {
                Text = "This tool is used to provide easy access to the EC3 carbon database, " +
                "Carbon Leadership Forum material baselines database, and the Kaleidoscope " +
                "assemblies database. It includes geometric calculator functionalities " +
                "that can be used to quickly calculate emboded carbon of modeled " +
                "Rhino geometry. This is not a reporting tool. This is not a whole " +
                "building life cycle assessment tool. Reach out to me at gx7@rice.edu or " +
                "guangyu.xu0630@gmail.com with any questions, feedback, or bug reports.",
                Width = this.resPanel.Width - 40
            };
            Label searchTitle = new Label
            {
                Text = "WoodchuckSearchEPD",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label searchLabel = new Label
            {
                Text = "Search carbon databases for materials " +
                "that fit your requirements. More details can be seen by hovering over " +
                "material names or clicking View in Browser when available. Embodied " +
                "carbon materials can be assigned to Rhino geometry (similar to rendering " +
                "materials) by clicking the Assign to Object button. Once materials are" +
                "assigned, they will apprear as an object property under the Woodchuck properties " +
                "tab.",
                Width = this.resPanel.Width - 40
            };
            Label viewTitle = new Label
            {
                Text = "WoodchuckCarbonView",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label viewLabel = new Label
            {
                Text = "Generate a diagramatic view of carbon densities in your model. " +
                "Objects are color coded based on their carbon content.",
                Width = this.resPanel.Width - 40
            };
            Label customTitle = new Label
            {
                Text = "WoodchuckCustomEPD",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label customLabel = new Label
            {
                Text = "If your material is not present in any of the databases, " +
                "create a custom EPD for your material.",
                Width = this.resPanel.Width - 40
            };
            Label carbonTitle = new Label
            {
                Text = "What is Embodied Carbon?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label carbonLabel = new Label
            {
                Text = "Embodied carbon refers to greenhouse gas emissions generated by " +
                "the extraction, production, transportation, installation, maintenance, and " +
                "eventual demolition of building materials.",
                Width = this.resPanel.Width - 40
            };
            Label gwpTitle = new Label
            {
                Text = "What is Global Warming Potential (GWP)?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label gwpLabel = new Label
            {
                Text = "Global Warming Potential is a measure of the environmental " +
                "impact of a product of building. It is used to quantify Embodied Carbon. " +
                "GWP is measured in mass of CO2 Equivalents, commonly represented by " +
                "kgCO2e.",
                Width = this.resPanel.Width - 40
            };
            Label epdTitle = new Label
            {
                Text = "What are Environmental Product Declarations (EPD)?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40

            };
            Label epdLabel = new Label
            {
                Text = "EPDs are standardized documents that report the results of a " +
                "Lifecycle Assessment for a material or product. They capture the " +
                "manufacturing and supply chain impacts of a material. EPDs report on the " +
                "GWP of a material.",
                Width = this.resPanel.Width - 40
            };
            Label ec3Title = new Label
            {
                Text = "What is Embodied Carbon in Construction Calculator (EC3)?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label ec3Label = new Label
            {
                Text = "The EC3 tool is a database that allows benchmarking, " +
                "assessments, and reductions in embodied carbon. It has a robust " +
                "database of digital, third-party verified EPDs. This plugin uses EC3's " +
                "database to access real-time EPDs.",
                Width = this.resPanel.Width - 40
            };
            Label clfTitle = new Label
            {
                Text = "What is Carbon Leadership Forum (CLF)?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label clfLabel = new Label
            {
                Text = "The Carbon Leadership Forum is an independend nonprofit organization " +
                "with the goal of reducing embodied carbon in buildings and infrastructure. " +
                "The CLF provides various research initiatives and reports pertaining to " +
                "embodied carbon. This tool uses the CLF's Material Baseline Report from 2023.",
                Width = this.resPanel.Width - 40
            };
            Label kldsTitle = new Label
            {
                Text = "What is Kaleidoscope?",
                Font = new Font(SystemFonts.Default().FamilyName, 12),
                Width = this.resPanel.Width - 40
            };
            Label kldsLabel = new Label
            {
                Text = "Kaleidoscope provides a comparative analysis of different " +
                "assemblies’ environmental impacts, allowing designers to " +
                "think in terms of embodied carbon early in the design process. " +
                "Kaleidoscope includes information for wall, ceiling, floor, and " +
                "partition assemblies. It does not include structure.",
                Width = this.resPanel.Width - 40
            };

            dl.Add(whatTitle);
            dl.Add(whatLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(searchTitle);
            dl.Add(searchLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(customTitle);
            dl.Add(customLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(viewTitle);
            dl.Add(viewLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(carbonTitle);
            dl.Add(carbonLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(gwpTitle);
            dl.Add(gwpLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(epdTitle);
            dl.Add(epdLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(ec3Title);
            dl.Add(ec3Label);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(clfTitle);
            dl.Add(clfLabel);
            dl.Add(Spacer(Colors.WhiteSmoke));
            dl.Add(kldsTitle);
            dl.Add(kldsLabel);
            dl.Add(null);

            this.resPanel.Content = dl;
        }

    }
}
