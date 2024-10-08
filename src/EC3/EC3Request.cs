using Rhino;
using System.IO;
using System.Net;
using System.Text;
using WoodchuckCarbonTool.src.EC3OAuth2;

namespace WoodchuckCarbonTool.src.EC3
{
    internal class EC3Request
    {
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

        public static string GetUserStatus(string apiKey)
        {
            var url = "https://buildingtransparency.org/api/users/me/status";
            return SendGetRequest(url, apiKey);
        }

        private static string SendGetRequest(string url)
        {
            EC3Authenticator authenticator = EC3Authenticator.Instance;
            string apiKey = authenticator.GetAPIKey();
            return SendGetRequest(url, apiKey);
        }

        /// <summary>
        /// Once requests are formatted by the corresponding methods, this class deals
        /// with the actual sending of the request.
        /// </summary>
        private static string SendGetRequest(string url, string apiKey)
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

                    return null;
                }
                else
                {
                    // Handle other exceptions (e.g., network issues)
                    RhinoApp.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }

            return response;
        }
    }
}
