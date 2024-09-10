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
        private static string accessToken;
        private bool isAuthenticating = false;

        private static HttpListener listener;

        public string GetAccessTokenAsync()
        {
            if (accessToken != null)
            {
                return accessToken;
            }

            if (!isAuthenticating)
            {
                isAuthenticating = true;

                // Start OAuth2 authentication in the background
                Task.Run(async () =>
                {
                    string returnToken = await PerformOAuth2AuthenticationAsync();
                    if (returnToken != null) { accessToken = returnToken; }
                    isAuthenticating = false;
                });
            }

            // Return null for now, and later checks can await or retry until authentication completes
            return null;
        }

        private async Task<string> PerformOAuth2AuthenticationAsync()
        {
            // Implement the actual OAuth2 flow here as described in previous responses
            string authorizationUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&redirect_uri={Uri.EscapeDataString(RedirectUri)}&response_type=code&scope=read";

            if (listener != null) 
            { 
                listener.Close();
            }

            using (listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri + "/");
                listener.Start();

                // Open the authorization URL in the user's default browser
                Process browserProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = authorizationUrl,
                    UseShellExecute = true
                });

                // Wait for either the browser process to exit or a 5-minute timeout
                Task delayTask = Task.Delay(TimeSpan.FromMinutes(5));
                Task exitTask = Task.Run(() =>
                {
                    browserProcess.WaitForExit(); // Waits until the process exits
                });

                // Wait for either the browser to close or the timer to finish
                if (await Task.WhenAny(exitTask, delayTask) == delayTask)
                {
                    // Timer expired
                    Console.WriteLine("Authorization timeout after 5 minutes.");
                    return null; // Set to null or handle the failure case
                }

                // Wait for the authorization response
                HttpListenerContext context = await listener.GetContextAsync();

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

        private string ExtractAuthorizationCode(Uri uri)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
            return query.Get("code");
        }

        private async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
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
