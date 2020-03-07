using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using QuickType;
using System.Web;

namespace NostalgiaGenerator.AppCode
{
    public class SpotifyApiController
    {
        public string GetClientCredentialsAuthToken(string clientId, string clientSecret)
        {
            // TODO: Should probably utilize the token refresh feature, instead of always asking for a new key.

            var webClient = new WebClient();

            var postParams = new NameValueCollection();
            postParams.Add("grant_type", "client_credentials");

            var authHeader = Convert.ToBase64String(Encoding.Default.GetBytes($"{clientId}:{clientSecret}"));
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + authHeader);

            var tokenResponse = webClient.UploadValues("https://accounts.spotify.com/api/token", postParams);

            var textResponse = Encoding.UTF8.GetString(tokenResponse);

            var objResponse = JsonConvert.DeserializeObject<SpotifyAccountsServiceModel>(textResponse);

            return objResponse.access_token;
        }

        public List<Track> GetBillboardTopByYear(string token, int year)
        {
            List<Track> tracks = new List<Track>();

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

            // Search playlists
            string query = "Billboard Top " + year;
            var objResponse = SearchForPlaylists(webClient, query);
            // Spotify searches in both name and description field, so remove all that only matched in description field
            objResponse.Playlists.Items = objResponse.Playlists.Items.Where(item => item.Name.Contains(year.ToString())).ToList(); 

            // Loop through all playlists until we find 10 tracks that have audio previews and correct year
            foreach (var playlistItem in objResponse.Playlists.Items) { 
                var playlist = GetPlaylist(webClient, playlistItem.Id);

                // Remove tracks with no audio preview available or wrong year
                playlist.Tracks.Items = playlist.Tracks.Items.Where(item => !(item.Track.PreviewUrl is null) && isCorrectYear(item, year)).Take(10).ToList();

                if(playlist.Tracks.Items.Count == 10)
                {
                    tracks.AddRange(playlist.Tracks.Items.Select(item => item.Track));
                    break;
                }
            }

            return tracks;
        }

        public List<Track> GetHipsterMusicByYear(string token, int year)
        {
            List<Track> tracks = new List<Track>();

            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

            // Search playlists
            string query = "a year:" + year + " tag:hipster";
            var objResponse = SearchForAlbums(webClient, query);

            // Loop through all albums until we find 10 tracks that have audio previews
            foreach (var album in objResponse.Albums.Items)
            {
                var tracksSimple = GetTracksFromAlbum(webClient, album.Id, 5);
                tracksSimple.Items = tracksSimple.Items.Where(item => !(item.PreviewUrl is null)).ToList();

                if (tracksSimple.Items.Count() > 0)
                {
                    var trackFull = GetFullTrack(webClient, tracksSimple.Items.First().Id);
                    tracks.Add(trackFull);
                    if (tracks.Count > 10)
                    {
                        break;
                    }
                }
            }
            return tracks;
        }

        private SpotifyPlaylistSearchModel SearchForPlaylists(WebClient webClient, string query)
        {
            query = HttpUtility.UrlEncode(query);
            string url = string.Format("https://api.spotify.com/v1/search?q={0}&type=playlist", query);
            var jsonString = webClient.DownloadString(url);
            return SpotifyPlaylistSearchModel.FromJson(jsonString);
        }

        private SpotifyPlaylistModel GetPlaylist(WebClient webClient, string id)
        {
            string url = string.Format("https://api.spotify.com/v1/playlists/{0}", id);
            var jsonString = webClient.DownloadString(url);
            return SpotifyPlaylistModel.FromJson(jsonString);
        }

        private SpotifyAlbumSearchModel SearchForAlbums(WebClient webClient, string query)
        {
            query = HttpUtility.UrlEncode(query);
            string url = string.Format("https://api.spotify.com/v1/search?q={0}&type=album", query);
            var jsonString = webClient.DownloadString(url);
            return SpotifyAlbumSearchModel.FromJson(jsonString);
        }

        private SpotifyAlbumTracksModel GetTracksFromAlbum(WebClient webClient, string id, int limit)
        {
            string url = string.Format("https://api.spotify.com/v1/albums/{0}/tracks?offset=0&limit={1}", id, limit);
            var jsonString = webClient.DownloadString(url);
            return SpotifyAlbumTracksModel.FromJson(jsonString);
        }

        private Track GetFullTrack(WebClient webClient, string id)
        {
            string url = string.Format("https://api.spotify.com/v1/tracks/{0}", id);
            var jsonString = webClient.DownloadString(url);
            return JsonConvert.DeserializeObject<Track>(jsonString);
        }

        private bool isCorrectYear(TracksItem tracksItem, int year)
        {
            if (tracksItem.Track.Album.ReleaseDate.DateTime is null)
            {
                return tracksItem.Track.Album.ReleaseDate.Integer <= year;
            }
            else
            {
                return tracksItem.Track.Album.ReleaseDate.DateTime.Value.Year <= year;
            }
        }
    }
}
