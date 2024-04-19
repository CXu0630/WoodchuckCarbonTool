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

namespace EC3CarbonCalculator.UI
{
    internal class SearchForm : Form
    {
        public SearchForm()
        {
            WindowStyle = WindowStyle.Default;
            Maximizable = true;
            Minimizable = true;
            Padding = new Eto.Drawing.Padding(5);
            Resizable = true;
            ShowInTaskbar = true;
            Title = "EC3 Material GWP Search";
            MinimumSize = new Size(900, 500);

            DynamicLayout mfLayout = this.MaterialFilterLayout();
            DynamicLayout resLayout = this.SearchResultLayout();

            Panel resPanel = new Panel { Content = resLayout };
            resPanel.BackgroundColor = Colors.LightGrey;

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
            GeographyCodes gc = GeographyCodes.Instance;
            EC3CategoryTree ct = EC3CategoryTree.Instance;

            List<ListItem> catOptions = new List<ListItem>();
            for (int i = 0; i < ct.names.Count; i++)
            {
                catOptions.Add(new ListItem
                {
                    Text = ct.names[i],
                    Key = ct.names[i]
                });
            }
            DropDown catDD = new DropDown();
            catDD.DataStore = catOptions;
            Label catLabel = new Label { Text = "Category" };

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
            countryDD.SelectedIndex = 235;
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
            Label stateLabel = new Label { Text = "State" };

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
            Label dateLabel = new Label { Text = "Valid Before" };

            // add options to layout
            DynamicLayout mfLayout = new DynamicLayout 
            { 
                DefaultSpacing = new Size(5, 5), 
                Padding = new Padding(10), 
                Size = new Size(400, 500)
            };

            mfLayout.BeginHorizontal();
            mfLayout.Add(catLabel);
            mfLayout.Add(null);
            mfLayout.Add(catDD);
            mfLayout.EndHorizontal();
            mfLayout.Add(new Panel { Height = 10 });

            mfLayout.BeginHorizontal();
            mfLayout.Add(countryLabel);
            mfLayout.Add(null);
            mfLayout.Add(countryDD);
            mfLayout.EndHorizontal();

            mfLayout.BeginHorizontal();
            mfLayout.Add(stateLabel);
            mfLayout.Add(null);
            mfLayout.Add(stateDD);
            mfLayout.EndHorizontal();

            mfLayout.BeginHorizontal();
            mfLayout.Add(dateLabel);
            mfLayout.Add(null);
            mfLayout.Add(datePicker);
            mfLayout.EndHorizontal();
            mfLayout.Add(null);

            return mfLayout;
        }

        private DynamicLayout SearchResultLayout()
        {
            DynamicLayout resLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            return resLayout;
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
    }
}
