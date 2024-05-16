using Rhino;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EC3CarbonCalculator
{
    internal class EC3Request
    {
        static string apiKey = "zjQ5yUsKYWTiAaQWMWoJSz7c4LnZOU";

        public EC3Request() { }

        public static string GetCategoryData (string category)
        {
            var url = "https://buildingtransparency.org/api/categories/" + category;
            return SendGetRequest(url);
        }

        public static string GetMaterialData (string mf)
        {
            var baseUrl = "https://buildingtransparency.org/api/materials";
            var query = "?mf=" + mf;
            string fullUrl = baseUrl + query;
            return SendGetRequest(fullUrl);
        }

        public static string GetCategoryTree()
        {
            var url = "https://buildingtransparency.org/api/categories/root";
            return  SendGetRequest(url);
        }

        private static string SendGetRequest(string url)
        {
            string response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "application/json";
                request.Headers["Authorization"] = "Bearer " + apiKey;

                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8)) // Use UTF-16 here
                {
                    response = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    HttpStatusCode statusCode = errorResponse.StatusCode;
                    // Handle the non-200 status code (e.g., log, throw, or return an error message)
                    RhinoApp.WriteLine($"Non-200 Status Code: {(int)statusCode} ({statusCode})");
                    RhinoApp.WriteLine($"Error: {ex.Message}");
                }
                else
                {
                    // Handle other exceptions (e.g., network issues)
                    RhinoApp.WriteLine($"Error: {ex.Message}");
                }
            }

            return response;
        }
    }
}
