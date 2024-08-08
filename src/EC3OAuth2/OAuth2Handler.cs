using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Rhino.FileIO;
using Rhino.Geometry;

namespace WoodchuckCarbonTool.src.EC3OAuth2
{
    public class OAuth2Handler
    {
        private const string RedirectUri = "http://localhost:8000/callback";
        private const string ClientId = "e4XzLm6W9s61J59Q5pRIoly58Q0EARp2ZJESaqYq";
        private const string AuthorizationEndpoint = "https://buildingtransparency.org/oauth2/authorize";
        private const string TokenEndpoint = "https://buildingtransparency.org/api/oauth2/token";

        public static async Task<string> GetAccessTokenAsync()
        {
            string authorizationUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&redirect_uri={Uri.EscapeDataString(RedirectUri)}&response_type=code&scope=read";

            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri + "/");
                listener.Start();

                // Open the authorization URL in the user's default browser
                Process.Start(new ProcessStartInfo { FileName = authorizationUrl, UseShellExecute = true });

                // Wait for the authorization response
                HttpListenerContext context = await listener.GetContextAsync();

                // Extract the authorization code from the request
                string authorizationCode = ExtractAuthorizationCode(context.Request.Url);

                string message = "";
                if (authorizationCode == null) message = "Authorization failed. Please close this window and try again.";
                else message = "Authorization successful! You can close this window.";

                // Respond to the browser
                byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
                context.Response.ContentLength64 = responseBuffer.Length;
                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                context.Response.OutputStream.Close();

                // Exchange the authorization code for an access token
                string accessToken = await ExchangeCodeForTokenAsync(authorizationCode);
                if (authorizationCode == null) return null;
                return accessToken;
            }
        }

        private static string ExtractAuthorizationCode(Uri uri)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
            return query.Get("code");
        }

        private static async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
        {
            using (HttpClient client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                    new KeyValuePair<string, string>("client_id", ClientId)
                });

                HttpResponseMessage response = await client.PostAsync(TokenEndpoint, requestContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                return jsonResponse.access_token;
            }
        }
    }
}
