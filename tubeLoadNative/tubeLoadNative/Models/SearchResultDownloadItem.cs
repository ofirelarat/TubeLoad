using Google.Apis.YouTube.v3.Data;

namespace tubeLoadNative.Models
{
    public class SearchResultDownloadItem
    {
        public enum State
        {
            Downloaded,
            Downloading,
            Downloadable
        }

        public State DownloadState { get; set; }
        public SearchResult YoutubeResult { get; set; }

        public SearchResultDownloadItem(SearchResult result) : base()
        {
            YoutubeResult = result;
            DownloadState = State.Downloadable;
        }
    }
}
