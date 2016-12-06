using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using System.Net;
using System.Net.Http;
using System.IO;

namespace tubeLoadNative
{
    public static class YoutubeHandler
    {
        public static List<SearchResult> Search(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyD99DDTmpBI79gKReSgYj001I7IK2jBGao",
                ApplicationName = "TubeLoad"
            });


            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query; // Replace with your search term.
            searchListRequest.MaxResults = 10;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = searchListRequest.Execute();

            List<SearchResult> Videos = new List<SearchResult>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        Videos.Add(searchResult);
                        break;

                    case "youtube#channel":
                        channels.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        break;

                    case "youtube#playlist":
                        playlists.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        break;
                }
            }
            return Videos;
        }

        public static async Task<HttpWebResponse> downloadStream(string videoId)
        {
            try
            {
                string videoUrl = "https://www.youtube.com/watch?v=" + videoId;

                using (var client = new HttpClient())
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://www.youtubeinmp3.com/fetch/?video=" + videoUrl);
                    httpRequest.Method = "GET";
                    WebResponse response = await httpRequest.GetResponseAsync();
                    
                    if (response.ContentType.Equals("audio/mpeg"))
                    {
                        return (HttpWebResponse)response;
                    }
                    else
                    {
                        Stream dataStream = ((HttpWebResponse)response).GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string strResponse = reader.ReadToEnd();
                        throw new Exception(strResponse);
                    }
                }
            }
            catch
            {
               throw new Exception("Could not connect to Youtube");
            }
        }
    }
}

