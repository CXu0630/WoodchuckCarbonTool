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

namespace EC3CarbonCalculator.UI
{
     internal class SearchForm : Form
    {
        EC3MaterialFilter mf;
        Button search = new Button { Text = "Search" };
        int searchCount = 0;

        public delegate void SearchEventHandler(object sender, EventArgs e);
        public event SearchEventHandler SearchEvent;
        Panel resPanel;

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

            DynamicLayout mfLayout = this.MaterialFilterLayout();
            DynamicLayout resLayout = this.SearchResultLayout(searchCount);

            resPanel = new Panel { Content = resLayout };
            resPanel.BackgroundColor = Colors.LightGrey;

            search.Click += (s, e) =>
            {
                SearchEvent?.Invoke(this, e);
            };

            DynamicLayout layout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            layout.BeginHorizontal();
            layout.Add(mfLayout); 
            layout.Add(resPanel);
            layout.EndHorizontal();

            Content = layout;
        }

        private DynamicLayout MaterialFilterLayout()
        {
            // add options to layout
            DynamicLayout mfLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10),
                Size = new Size(380, 500)
            };

            mfLayout.Add(this.CategoryLayout());
            mfLayout.Add(this.Spacer());
            mfLayout.Add(this.GeneralSearchLayout());

            mfLayout.Add(null);
            mfLayout.Add(this.ConfirmLayout());

            return mfLayout;
        }

        private DynamicLayout CategoryLayout()
        {
            EC3CategoryTree ct = EC3CategoryTree.Instance;

            List<ListItem> catOptions = new List<ListItem>();
            for (int i = 0; i < ct.names.Count; i++)
            {
                // TODO: implement a display names field for the csv?
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

        private DynamicLayout GeneralSearchLayout()
        {
            GeographyCodes gc = GeographyCodes.Instance;
            
            // country options dropdown
            List<ListItem> countryOptions = new List<ListItem>();
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
            countryDD.SelectedIndex = 235;
            mf.SetCountry(countryDD.SelectedKey);
            // set listener
            countryDD.SelectedKeyChanged += (s, e) =>
            {
                mf.SetCountry(countryDD.SelectedKey);
            };
            Label countryLabel = new Label { Text = "Country" };

            // state options dropdown
            List<ListItem> stateOptions = new List<ListItem>();
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
            stateDD.SelectedIndex = 34;
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

        private DynamicLayout ConfirmLayout()
        {
            DynamicLayout cfLayout = new DynamicLayout();

            cfLayout.BeginHorizontal();
            cfLayout.Add(null);
            cfLayout.Add(search);

            return cfLayout;
        }

        // unused for now...
        private DynamicLayout TwoColomnLayout (List<Control> column1, List<Control> column2)
        {
            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(5, 5) };

            int max = Math.Max(column1.Count, column2.Count);

            for (int i = 0; i < max; i++)
            {
                Control ctrl1;
                try { ctrl1 = column1[i]; }
                catch(Exception) { ctrl1 = null; }

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

        private DynamicLayout SearchResultLayout(int count)
        {
            Label countlbl = new Label { Text = "You pressed search " + count.ToString() + " times!" };

            DynamicLayout resLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            resLayout.Add(countlbl);
            return resLayout;
        }

        public void RepopulateSearchResult(List<EPD> epds, EPD avgEPD)
        {
            DynamicLayout epdLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            epdLayout.Add(EPDPanel(avgEPD));

            foreach(EPD epd in epds.Take(4))
            {
                epdLayout.Add(EPDPanel(epd));
            }

            epdLayout.Add(null);

            this.resPanel.Content = epdLayout;
        }

        private Panel EPDPanel(EPD epd)
        {
            Panel bkg = new Panel { Height = 80 };
            bkg.BackgroundColor = Colors.WhiteSmoke;

            Label epdName = new Label 
            { 
                Text = epd.name, 
                Font = new Font(SystemFonts.Default().FamilyName, 12) 
            };

            DynamicLayout epdLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            epdLayout.Add(epdName);

            bkg.Content = epdLayout;

            return bkg;
        }

        private DynamicLayout Spacer()
        {
            DynamicLayout spacer = new DynamicLayout();

            Panel blank1 = new Panel { Height = 10 };
            Panel blank2 = new Panel { Height = 10 };
            Panel line = new Panel
            {
                Height = 1,
                BackgroundColor = Colors.DarkGray
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
    }
}
