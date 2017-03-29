using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using tubeLoadNative.Services;
using System.Net;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid.Utils
{
    public static class Downloader
    {
        public static async Task<bool> DownloadSong(string videoId, string fileName)
        {
            fileName = AndroidSongsManager.Instance.CorrectSongNameForSave(fileName);

            HttpResponseMessage response = await YoutubeApiClient.downloadStream(videoId);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (await AndroidSongsManager.Instance.SaveSong(FileManager.PATH, fileName, videoId, await response.Content.ReadAsStreamAsync()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}