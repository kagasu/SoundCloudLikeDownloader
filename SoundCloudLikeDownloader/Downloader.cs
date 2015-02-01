using Codeplex.Data;
using SoundCloud.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoundCloudLikeDownloader
{
    class Downloader
    {
        private string clientId;
        private string accessToken;

        public Downloader(string clientID, string accessToken)
        {
            this.clientId = clientID;
            this.accessToken = accessToken;
        }

        private static string ValidFileName(string s)
        {
            string valid = s;
            char[] invalidch = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalidch)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }

        private string GetMp3Url(int trackId)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization: OAuth " + this.accessToken);
            wc.Headers.Add("Accept: application/json");
            var str = wc.DownloadString("https://api.soundcloud.com/i1/tracks/" + trackId + "/streams?client_id=" + this.clientId);
            var json = DynamicJson.Parse(str);

            return json.http_mp3_128_url;
        }

        public bool DownloadMusicWithTrackId(Track track)
        {
            var fileName = ValidFileName(track.Title + ".mp3");

            if (File.Exists("mp3/" + fileName))
            {
                // finish if the same file already exists
                MessageBox.Show("download successfully finished.");
                Environment.Exit(0);
            }

            var mp3Url = this.GetMp3Url(track.Id);

            if (mp3Url == null)
            {
                return false;
            }
            else
            {
                new WebClient().DownloadFile(mp3Url, "mp3/" + fileName);
                return true;
            }
        }
    }
}