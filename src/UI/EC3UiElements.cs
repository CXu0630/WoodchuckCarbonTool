using EC3CarbonCalculator.src.EC3;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EC3CarbonCalculator.src.UI
{
    internal class EC3UiElements
    {
        /// <summary>
        /// This method creates a dynamic layout for material search parameters.
        /// </summary>
        public static DynamicLayout EC3CategoryLayout(MaterialFilter mf)
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
            mf.SetEC3Category(catDD.SelectedKey);
            // set listener
            catDD.SelectedValueChanged += (sender, e) =>
            {
                mf.SetEC3Category(catDD.SelectedKey);
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
        public static DynamicLayout EC3GeneralSearchLayout(MaterialFilter mf)
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
            mf.SetEC3Country(countryDD.SelectedKey);
            // set listener
            countryDD.SelectedKeyChanged += (s, e) =>
            {
                mf.SetEC3Country(countryDD.SelectedKey);
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
            mf.SetEC3State(stateDD.SelectedKey);
            // set listener
            stateDD.SelectedKeyChanged += (s, e) =>
            {
                mf.SetEC3State(stateDD.SelectedKey);
            };
            Label stateLabel = new Label { Text = "State" };

            // set listener to disable state selection when not in states
            countryDD.SelectedValueChanged += (s, e) =>
            {
                if (countryDD.SelectedKey == "US") stateDD.Enabled = true;
                else { stateDD.Enabled = false; stateDD.SelectedValue = null; }
            };

            DateTimePicker datePicker = new DateTimePicker
            {
                Value = DateTime.Now,
                Mode = DateTimePickerMode.Date
            };
            mf.SetEC3ExpirationDate((DateTime)datePicker.Value);
            datePicker.ValueChanged += (s, e) =>
            {
                mf.SetEC3ExpirationDate((DateTime)datePicker.Value);
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
    }
}
