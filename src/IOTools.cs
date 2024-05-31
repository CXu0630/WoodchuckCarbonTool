using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace EC3CarbonCalculator.src
{
    /// <summary>
    /// This is supposed to be a helper class with static methods to aid file IO. Not sure
    /// we need it.
    /// </summary>
    internal class IOTools
    {
        /// <summary>
        /// Gets the location of the rhp file that is being executed.
        /// </summary>
        public static string GetRhpLocation()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string cleanFullPath = Uri.UnescapeDataString(uri.Path);
            return cleanFullPath;
        }

        public static List<List<string>> ReadCSVFromEmbedded(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Create a list to hold each line's list of items
            var resourceName = "EC3CarbonCalculator.EmbeddedResources." + filename + ".csv";


            var csvData = ReadCsvFromEmbeddedResource(resourceName);
            var parsedData = ParseCsv(csvData);

            return parsedData;
        }

        static string ReadCsvFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        static List<List<string>> ParseCsv(string csvData)
        {
            var parsedData = new List<List<string>>();

            using (var reader = new StringReader(csvData))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // Set to true if your CSV has a header row
                BadDataFound = null // Ignores bad data (optional)
            }))
            {
                while (csv.Read())
                {
                    var row = new List<string>();
                    for (int i = 0; csv.TryGetField<string>(i, out var field); i++)
                    {
                        row.Add(field);
                    }
                    parsedData.Add(row);
                }
            }

            return parsedData;
        }
    }
}
