using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace tubeLoadNative.Services
{
    public static class VersionChecker
    {
        private const string URL = "http://tubeloadweb.com/version.txt";

        public static bool isVersionUpToDate(string currentVersion)
        {
            string latestVersion;
            try
            {
                using (var client = new HttpClient())
                {
                    latestVersion = client.GetStringAsync(URL).Result;
                }

                return latestVersion.Equals(currentVersion);
            }
            catch
            {
                return true;
            }
        }
    }
}
