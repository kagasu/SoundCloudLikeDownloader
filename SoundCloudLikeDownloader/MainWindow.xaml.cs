﻿using SoundCloud.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace SoundCloudLikeDownloader
{
    public partial class MainWindow : Window
    {
        private const string ClientId = "577cb16946b0657e1dbdbd1dc964ec18";
        private const string ClientSecret = "a1104c50a0f7618e405dec4569f065bc";

        private ObservableCollection<Like> _likeCollection = new ObservableCollection<Like>();
        public ObservableCollection<Like> LikeCollection { get { return _likeCollection; } }

        public class Like
        {
            public int Index { get; set; }
            public string Title { get; set; }
        }

        public MainWindow()
        {
            if (!Directory.Exists("mp3"))
            {
                Directory.CreateDirectory("mp3");
            }

            InitializeComponent();
        }

        // Move Window
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var username = this.Username.Text;
            var passwrod = this.Password.Password;

            new Thread(() =>
            {
                var credentials = new SoundCloudCredentials(ClientId, ClientSecret, username, passwrod);
                var client = new SoundCloudClient(credentials);
                SoundCloudAccessToken token = null;

                try
                {
                    token = client.Authenticate();
                }
                catch (Exception)
                {
                    MessageBox.Show("login failed.");
                    return;
                }
                var downloader = new Downloader(ClientId, token.AccessToken);

                //fetch some data
                if (client.IsAuthenticated)
                {
                    //Fetch current user info
                    var mySelf = User.Me();
                    var clientID = client.getClientID();
                    var trackList = new List<Track>();
                    var i = 1;
                    var limit = 200;

                    var favorites = User.Me().GetFavorites(limit, 1);

                    while (favorites.NextHref != null)
                    {
                        foreach(var favorite in favorites.Tracks)
                        {
                            var track = Track.GetTrack(favorite.Id);
                            if (downloader.DownloadMusicWithTrackId(track))
                            {
                                ListView1.Dispatcher.Invoke(
                                    new Action(() =>
                                    {
                                        _likeCollection.Add(new Like() { Index = i++, Title = track.Title });
                                    }));
                            }
                        }

                        favorites = JsonSerializer.Deserialize<Favorite>(new WebClient(){ Encoding = new UTF8Encoding()}.DownloadString(favorites.NextHref));
                    }
                    
                    MessageBox.Show("download successfully finished.");
                }
            }).Start();
        }

        // Exit Button
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}