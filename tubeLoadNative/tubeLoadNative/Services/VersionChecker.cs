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

        public enum VersionStatus
        {
            UpToDate,
            MissingHotFix,
            NeedUpdate
        }

        public static async Task<VersionStatus> isVersionUpToDate(string currentVersion)
        {
            string latestVersion;
            try
            {
                using (var client = new HttpClient())
                {
                    latestVersion = client.GetStringAsync(URL).Result;
                }

                if (!latestVersion.Equals(currentVersion))
                {
                    if (latestVersion.Split('.')[0].Equals(currentVersion.Split('.')[0])
                        && latestVersion.Split('.')[1].Equals(currentVersion.Split('.')[1]))
                    {
                        return VersionStatus.MissingHotFix;
                    }
                    else
                    {
                        return VersionStatus.NeedUpdate;
                    }
                }
                else
                {
                    return VersionStatus.UpToDate;
                }
            }
            catch
            {
                return VersionStatus.UpToDate;
            }
        }
    }
}
