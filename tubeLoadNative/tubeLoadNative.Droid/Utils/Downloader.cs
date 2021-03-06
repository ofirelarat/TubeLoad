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
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid.Utils
{
    public static class Downloader
    {
        public static async Task<bool> DownloadSong(string videoId, string fileName)
        {
            fileName = AndroidSongsManager.Instance.CorrectSongNameForSave(fileName);

            HttpResponseMessage songStream = await YoutubeApiClient.downloadStream(videoId);

            if (songStream.StatusCode == HttpStatusCode.OK)
            {
                return await AndroidSongsManager.Instance.SaveSong(FileManager.PATH, fileName, videoId, await songStream.Content.ReadAsStreamAsync());
            }

            return false;
        }
    }
}