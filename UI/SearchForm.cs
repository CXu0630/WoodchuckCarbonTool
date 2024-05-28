using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using Rhino.UI;
using System.ComponentModel;
using Rhino.Geometry;
using System.Runtime.Remoting.Channels;
using Rhino;
using static System.Net.Mime.MediaTypeNames;
using UnitsNet;

namespace EC3CarbonCalculator.UI
{
    /// <summary>
    /// This class defines an ETO form used to search the EC3 database for EPDs and
    /// and provides an interface for assigning EPDs to Rhino geometry. This class only
    /// defines the UI aspect of the search. Calculations and assignment are handled by
    /// SearchEPD.
    /// </summary>
     internal class SearchForm : Form
    {
        // All search prameters are stored in the mf instead of individually
        EC3MaterialFilter mf;
        Button search = new Button { Text = "Search" };
        RhinoDoc doc;

        // This is public as it needs to be edditable by multiple methods.
        Scrollable resPanel;

        // This event is used to signal when the search button has been pressed:
        // search parameters are locked in and an EPD search is performed.
        public delegate void SearchEventHandler(object sender, EventArgs e);
        public event SearchEventHandler SearchEvent;

        // This event is used to signal when the assign button has been pressed, actions
        // relating to this event are delt with by the SearchEPD class
        public delegate void AssignEventHandler(object sender, AssignEventArgs e);
        public event AssignEventHandler AssignEvent;

        /// <summary>
        /// Constructs an instance of SearchForm. 
        /// </summary>
        /// <param name="mf"> Material Filter object in which search criteria for EPDs 
        /// will be stored. </param>
        public SearchForm(EC3MaterialFilter mf)
        {
            this.mf = mf;
            WindowStyle = WindowStyle.Default;
            Maximizable = true;
            Minimizable = true;
            Padding = new Eto.Drawing.Padding(5);
            Resizable = true;
            ShowInTaskbar = true;
            Title = "EC3 Material GWP Search";
            MinimumSize = new Size(900, 500);

            // Twwo basic layouts: one for search parameters, one for search results
            DynamicLayout mfLayout = this.MaterialFilterLayout();

            this.doc = RhinoDoc.ActiveDoc;

            // Results exist in a panel to be updated with new content each search
            resPanel = new Scrollable
            {
                ExpandContentWidth = true
            };
            resPanel.BackgroundColor = Colors.LightGrey;

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
            layout.Add(mfLayout); 
            layout.Add(resPanel);
            layout.EndHorizontal();

            Content = layout;
        }

        /// <summary>
        /// This method creates a dynamic layout for selecting a category, inputting 
        /// material search parameters and a search confirm button.
        /// </summary>
        private DynamicLayout MaterialFilterLayout()
        {
            DynamicLayout mfLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10),
                Size = new Size(380, 500)
            };

            mfLayout.Add(this.CategoryLayout());
            mfLayout.Add(this.Spacer(Colors.DarkGray));
            mfLayout.Add(this.GeneralSearchLayout());

            mfLayout.Add(null);
            mfLayout.Add(this.ConfirmLayout());

            return mfLayout;
        }

        /// <summary>
        /// This method creates a dynamic layout for material search parameters.
        /// </summary>
        private DynamicLayout CategoryLayout()
        {
            // Use category names retreived from EC3 by the Category Tree as dropdown
            // option names.
            EC3CategoryTree ct = EC3CategoryTree.Instance;

            List<ListItem> catOptions = new List<ListItem>();
            for (int i = 0; i < ct.names.Count; i++)
            {
                // TODO: Add Masterformat code to the display names and sort in order of
                // masterformat?
                string strdName;
                if (string.IsNullOrWhiteSpace(ct.names[i])) strdName = "";
                string newText = Regex.Replace(ct.names[i], "([a-z])([A-Z])", "$1 $2");
                newText = Regex.Replace(newText, "([A-Z]+)([A-Z][a-z])", "$1 $2");
                strdName = newText;

                catOptions.Add(new ListItem
                {
                    Text = strdName,
                    Key = ct.names[i]
                });
            }
            DropDown catDD = new DropDown();
            catDD.DataStore = catOptions;
            // set default values
            catDD.SelectedIndex = 47;
            mf.SetCategory(catDD.SelectedKey);
            // set listener
            catDD.SelectedValueChanged += (sender, e) =>
            {
                mf.SetCategory(catDD.SelectedKey);
            };

            Label catLabel = new Label { Text = "Category" };

            DynamicLayout catLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
            };

            catLayout.BeginHorizontal();
            catLayout.Add(catLabel);
            catLayout.Add(null);
            catLayout.Add(catDD);
            catLayout.EndHorizontal();

            return catLayout;
        }

        /// <summary>
        /// Material search parameters that are used accross all searches
        /// Preliminary search parameter layout used in our prototype.
        /// </summary>
        /// <returns> dynamic layout of search fields </returns>
        private DynamicLayout GeneralSearchLayout()
        {
            GeographyCodes gc = GeographyCodes.Instance;
            
            // country options dropdown
            List<ListItem> countryOptions = new List<ListItem>();
            // blank option for default
            countryOptions.Add(new ListItem { Key = null, Text = "Global" });
            for (int i = 0; i < gc.CountryCodes.Count; i++)
            {
                countryOptions.Add(new ListItem 
                { 
                    Text = gc.CountryNames[i], 
                    Key = gc.CountryCodes[i]
                });
            }
            DropDown countryDD = new DropDown();
            countryDD.DataStore = countryOptions;
            // set default value
            countryDD.SelectedKey = "US";
            mf.SetCountry(countryDD.SelectedKey);
            // set listener
            countryDD.SelectedKeyChanged += (s, e) =>
            {
                mf.SetCountry(countryDD.SelectedKey);
            };
            Label countryLabel = new Label { Text = "Country" };

            // state options dropdown
            List<ListItem> stateOptions = new List<ListItem>();
            // blank option for default
            stateOptions.Add(new ListItem { Key = null, Text = "None" });
            for (int i = 0; i < gc.StateCodes.Count; i++)
            {
                stateOptions.Add(new ListItem
                {
                    Text = gc.StateNames[i],
                    Key = gc.StateCodes[i]
                });
            }
            DropDown stateDD = new DropDown();
            stateDD.DataStore = stateOptions;
            // set default value
            stateDD.SelectedKey = "NY";
            mf.SetState(stateDD.SelectedKey);
            // set listener
            stateDD.SelectedKeyChanged += (s, e) =>
            {
                mf.SetState(stateDD.SelectedKey);
            };
            Label stateLabel = new Label { Text = "State" };

            // set listener to disable state selection when not in states
            countryDD.SelectedValueChanged += (s, e) =>
            {
                if (countryDD.SelectedKey == "US") stateDD.Enabled = true;
                else {stateDD.Enabled = false; stateDD.SelectedValue = null; }
            };

            DateTimePicker datePicker = new DateTimePicker
            {
                Value = DateTime.Now,
                Mode = DateTimePickerMode.Date
            };
            mf.SetExpirationDate((DateTime)datePicker.Value);
            datePicker.ValueChanged += (s, e) =>
            {
                mf.SetExpirationDate((DateTime)datePicker.Value);
            };
            Label dateLabel = new Label { Text = "Valid Before" };

            // add options to layout
            DynamicLayout genLayout = new DynamicLayout 
            { 
                DefaultSpacing = new Size(5, 5)
            };

            genLayout.BeginHorizontal();
            genLayout.Add(countryLabel);
            genLayout.Add(null);
            genLayout.Add(countryDD);
            genLayout.EndHorizontal();

            genLayout.BeginHorizontal();
            genLayout.Add(stateLabel);
            genLayout.Add(null);
            genLayout.Add(stateDD);
            genLayout.EndHorizontal();

            genLayout.BeginHorizontal();
            genLayout.Add(dateLabel);
            genLayout.Add(null);
            genLayout.Add(datePicker);
            genLayout.EndHorizontal();
            genLayout.Add(null);

            return genLayout;
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
        public void RepopulateSearchResult(List<EPD> epds, EPD avgEPD)
        {
            DynamicLayout epdLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            epdLayout.Add(EPDPanel(avgEPD));
            epdLayout.Add(Spacer(Colors.WhiteSmoke));

            // NOTE: right now only 20 EPDs are displayed per search
            // Consider implementing with more EPDs displayed...
            foreach(EPD epd in epds.Take(20))
            {
                epdLayout.Add(EPDPanel(epd));
            }

            epdLayout.Add(null);

            this.resPanel.Content = epdLayout;
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
            this.resPanel.Content = msgLayout;
        }

        /// <summary>
        /// Creates a panel to display EPD information.
        /// </summary>
        /// <param name="epd"> EPD to display </param>
        private Panel EPDPanel(EPD epd)
        {
            Panel bkg = new Panel { Width = resPanel.Width - 40};
            bkg.BackgroundColor = Colors.WhiteSmoke;

            Label gwp = new Label
            {
                Text = epd.GetGwpConverted(UnitManager.GetSystemUnit(doc, epd.dimension))
                + "CO2e/" + UnitManager.GetSystemUnitStr(doc, epd.dimension),
                Font = new Font(SystemFonts.Default().FamilyName, 12)
            };
            Label epdName = new Label 
            { 
                Text = epd.name, 
                Font = new Font(SystemFonts.Default().FamilyName, 12)
            };
            Label manufacturer = new Label
            {
                Text = "Manufacturer: " + epd.manufacturer,
                Font = new Font(SystemFonts.Default().FamilyName, 10),
                TextColor = Colors.DarkSlateGray
            };

            Button browserView = new Button { Text = "View in Browser" };
            Button assignButton = new Button { Text = "Assign to Object" };

            // Makes a system call to open the EPD's page on EC3 in the default browser
            // window. This is nice because it does not interfere with Rhino and also
            // opens a tab on an existing window if there is one already.
            browserView.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start("https://buildingtransparency.org/ec3/epds/" + epd.id);
            };

            // The assign button is pressed. Non-UI events are not delt with in this class.
            assignButton.Click += (s, e) =>
            {
                AssignEvent?.Invoke(this, new AssignEventArgs(epd));
            };

            // General container
            DynamicLayout epdLayout = new DynamicLayout();

            // Container for display information
            DynamicLayout infoLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10),
                Size = new Size(-1, -1)
            };
            infoLayout.BeginHorizontal();
            infoLayout.Add(epdName);
            infoLayout.Add(gwp);
            infoLayout.EndBeginHorizontal();
            infoLayout.Add(manufacturer);
            infoLayout.EndBeginHorizontal();
            // These spacer lines are to make sure that the EPD name does not extend off
            // the screen... These are the times when I hate frontend.
            infoLayout.Add(null);
            infoLayout.Add(new Panel());

            // Maybe this layout should be put in a separate method to encapsulate it?
            DynamicLayout buttonLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            buttonLayout.BeginHorizontal();
            buttonLayout.Add(null);
            if(epd.id != null)
            {
                buttonLayout.Add(browserView);
            }
            buttonLayout.Add(assignButton);

            epdLayout.Add(infoLayout);
            epdLayout.Add(null);
            epdLayout.Add(buttonLayout);

            bkg.Content = epdLayout;

            return bkg;
        }

        /// <summary>
        /// A spacer with a line in the middle
        /// </summary>
        /// <param name="bkgColor"> Color of the line </param>
        /// <returns>A spacer</returns>
        private DynamicLayout Spacer(Color bkgColor)
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

        public EC3MaterialFilter GetMaterialFilter()
        {
            return this.mf;
        }

        // unused for now...
        private DynamicLayout TwoColomnLayout(List<Control> column1, List<Control> column2)
        {
            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(5, 5) };

            int max = Math.Max(column1.Count, column2.Count);

            for (int i = 0; i < max; i++)
            {
                Control ctrl1;
                try { ctrl1 = column1[i]; }
                catch (Exception) { ctrl1 = null; }

                Control ctrl2;
                try { ctrl2 = column2[i]; }
                catch (Exception) { ctrl2 = null; }

                layout.BeginHorizontal();
                layout.Add(ctrl1);
                layout.Add(ctrl2);
                layout.EndHorizontal();
            }

            return layout;
        }

        /// <summary>
        /// Custom EventArgs class to pass the EPD for which "Assign to Object" was
        /// pressed to the class that's actually implementing the assigning (which would
        /// be SearchEPD)
        /// </summary>
        internal class AssignEventArgs: EventArgs
        {
            public EPD Epd { get; set; }
            public AssignEventArgs(EPD epd)
            {
                Epd = epd;
            }
        }
    }
}
