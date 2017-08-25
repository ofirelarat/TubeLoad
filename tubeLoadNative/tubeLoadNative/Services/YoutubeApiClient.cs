using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tubeLoadNative.Models;

namespace tubeLoadNative.Services
{
    public static class YoutubeApiClient
    {
        static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "AIzaSyCUiBlKoz4pvKVtQz9WPOwUnYdJj5YFw4I",
            ApplicationName = "TubeLoad"
        });

        private static SearchResource.ListRequest youtubeSearcher;

        public static List<SearchResult> MostPopularSongs { get; private set; }

        static YoutubeApiClient()
        {
            youtubeSearcher = youtubeService.Search.List("snippet");
            youtubeSearcher.MaxResults = 10;
            youtubeSearcher.VideoCategoryId = "10";
            youtubeSearcher.Type = "video";
        }

        public static async Task<List<SearchResult>> Search(string query)
        {
            youtubeSearcher.Q = query;
            youtubeSearcher.Fields = "items/id,items/snippet";
            youtubeSearcher.MaxResults = 10;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await youtubeSearcher.ExecuteAsync();

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
                    default:
                        break;
                }
            }

            return Videos;
        }

        public static async Task<IEnumerable<string>> SearchTitles(string query)
        {
            youtubeSearcher.Q = query;
            youtubeSearcher.Fields = "items/snippet/title";
            youtubeSearcher.MaxResults = 3;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await youtubeSearcher.ExecuteAsync();

            List<string> Videos = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.

            return searchListResponse.Items.Select(x => x.Snippet.Title);
        }

        // Gets most popular songs
        public static async Task<List<SearchResult>> Search()
        {
            List<SearchResult> songs = new List<SearchResult>();
            HttpClient client = new HttpClient();

            HttpResponseMessage res = await client.GetAsync("https://www.googleapis.com/youtube/v3/videos?part=snippet&chart=mostPopular&maxResults=10&regionCode=US&videoCategoryId=10&key=AIzaSyD99DDTmpBI79gKReSgYj001I7IK2jBGao");

            if (res.StatusCode == HttpStatusCode.OK)
            {
                var jsonString = res.Content.ReadAsStringAsync();
                jsonString.Wait();
                JObject json = JObject.Parse(jsonString.Result);
                var parser = json["items"].Children().ToList();

                // Getting youtube most popular songs and converting them to SearchResult class
                List<YoutubeOutput> outputSongs = parser.Select((x) => JsonConvert.DeserializeObject<YoutubeOutput>(x.ToString())).ToList();
                songs = ConvertYoutubeOutputToSearchResults(outputSongs);
            }

            MostPopularSongs = songs;

            return songs;
        }

        // Converting the YoutubeOutput class we have created to youtube's API SearchResult class
        public static List<SearchResult> ConvertYoutubeOutputToSearchResults(List<YoutubeOutput> youtubeOutput)
        {
            return new List<SearchResult>(youtubeOutput.Select((result) => new SearchResult()
            {
                ETag = result.etag,
                Id = new ResourceId()
                {
                    ETag = result.etag,
                    ChannelId = result.snippet.ChannelId,
                    Kind = result.kind,
                    PlaylistId = string.Empty,
                    VideoId = result.id
                },
                Kind = result.kind,
                Snippet = result.snippet
            }));
        }

        public static async Task<HttpResponseMessage> downloadStream(string videoId)
        {
            string videoUrl = "https://www.youtube.com/watch?v=" + videoId;
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                try
                {
                    response = await client.GetAsync("http://www.youtubeinmp3.com/fetch/?video=" + videoUrl);
                }
                catch
                {
                    throw new Exception("Could not connect to Youtube");
                }

                if (response.Content.Headers.ContentType.MediaType.Equals("audio/mpeg"))
                {
                    return response;
                }
                
                throw new Exception("This video has been blocked");
            }
        }

    }
}

