using Eto.Forms;
using Rhino.Geometry;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WoodchuckCarbonTool.src.EC3;

namespace WoodchuckCarbonTool.src.EC3OAuth2
{
    internal class EC3Authenticator
    {
        string filePath;
        public string apiKey;

        private static readonly Lazy<EC3Authenticator> _ec3AuthenticatorInstance = 
            new Lazy<EC3Authenticator>(() => new EC3Authenticator());

        public static EC3Authenticator Instance = _ec3AuthenticatorInstance.Value;

        public EC3Authenticator()
        {
            string rhpLocation = IOTools.GetRhpLocation();
            string[] locationSplit = rhpLocation.Split('/');
            locationSplit[locationSplit.Length - 1] = "EC3Key.txt";
            filePath = string.Join("/", locationSplit);

            if (!ReadStoredKey()) GetStoreNewAPIKey();
        }

        public bool ReadStoredKey()
        {
            if (!File.Exists(filePath)) return false;
            var allLines = File.ReadAllLines(filePath);
            if (allLines.Length == 0) return false;
            string key = allLines[0];

            if(VerifyEC3Key(key)) { apiKey = key; return true; }

            return false;
        }

        public void StoreKey()
        {
            List<string> keyContainer = new List<string> { apiKey };
            File.WriteAllLines(filePath, keyContainer);
        }

        public static bool VerifyEC3Key(string key)
        {
            if(EC3Request.GetUserStatus(key) == null)
            {
                return false;
            }
            return true;
        }

        public static string GetNewAPIKey()
        {
            Task<string> keyTask = OAuth2Handler.GetAccessTokenAsync();
            keyTask.Wait();
            string key = keyTask.Result;

            if (key == null || !VerifyEC3Key(key))
            {
                return null;
            }

            return key;
        }

        public string GetStoreNewAPIKey()
        {
            string key = GetNewAPIKey();
            apiKey = key;
            StoreKey();

            return key;
        }

        public string GetAPIKey()
        {
            if (apiKey != null && apiKey != "" && VerifyEC3Key(apiKey)) return apiKey;
            string key = GetStoreNewAPIKey();
            return key;
        }
    }
}
