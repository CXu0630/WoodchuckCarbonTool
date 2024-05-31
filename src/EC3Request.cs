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

namespace EC3CarbonCalculator.src
{
    internal class EC3Request
    {
        // To Colin (or whoever else is working on this):
        // This API Key is linked to my account on EC3. It's set to expire in may in 2025,
        // if there are server related errors, it is almost certainly due to this key.
        // Feel free to ask me for a new one if needed (guangyu.xu0630@gmail.com).
        // This is of course a temporary solution. The plan is to build a "public client"
        // system for authentication. Let's see if we get there.Reference information:
        // https://buildingtransparency.org/ec3/manage-apps/api-doc/guide#/02_Accessing_API/03_Dev_oauth2_app.md
        static string apiKey = "zjQ5yUsKYWTiAaQWMWoJSz7c4LnZOU";

        public EC3Request() { }

        /// <summary>
        /// Sends a request for category data to EC3 for one category.
        /// </summary>
        public static string GetCategoryData(string category)
        {
            var url = "https://buildingtransparency.org/api/categories/" + category;
            return SendGetRequest(url);
        }

        /// <summary>
        /// Sends a request for a material search based on a MaterialFilter formatted by 
        /// the EC3MaterialFilter class.
        /// </summary>
        public static string GetMaterialData(string mf)
        {
            var baseUrl = "https://buildingtransparency.org/api/materials";
            var query = "?mf=" + mf;
            string fullUrl = baseUrl + query;
            return SendGetRequest(fullUrl);
        }

        /// <summary>
        /// Sends a request for the entire category tree of EC3. Can be parsed recursively
        /// by EC3CategoryTree.
        /// </summary>
        public static string GetCategoryTree()
        {
            var url = "https://buildingtransparency.org/api/categories/root";
            return SendGetRequest(url);
        }

        /// <summary>
        /// Once requests are formatted by the corresponding methods, this class deals
        /// with the actual sending of the request.
        /// </summary>
        private static string SendGetRequest(string url)
        {
            string response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "application/json";
                request.Headers["Authorization"] = "Bearer " + apiKey;

                // This is such a pain, caused so much trouble...
                // Note to self: declare encoding protocol, even when it says automatically
                // decodes using UTF-8.
                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
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
