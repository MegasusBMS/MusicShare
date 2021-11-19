using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MusicShare
{
    public partial class Form1 : Form
    {
        private static Dictionary<string, SearchResult> videos = new Dictionary<string, SearchResult>();
        private static readonly string FilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+ "\\VideoMegShare\\Playlists.txt";


        public Form1()
        {
            InitializeComponent();
        }

        private void Music_Play(SearchResult video)
        {
            label2.Text = video.Id.VideoId;
            axVLCPlugin21.playlist.add("https://www.youtube.com/watch?v=" + video.Id.VideoId);
            axVLCPlugin21.playlist.play();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //initiere serviciu
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.yt_key(),
                ApplicationName = this.GetType().ToString()
            });

            //formular cerere
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = textBox1.Text;
            searchListRequest.MaxResults = 15;

            //trimitere formular
            var searchListResponse = searchListRequest.ExecuteAsync();

            //filtrarea rezultatelor
            foreach (var searchResult in searchListResponse.Result.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                    case "youtube#playlist":
                        videos.Add(searchResult.Snippet.Title, searchResult);
                        listBox1.Items.Add(searchResult.Snippet.Title);
                        break;
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            string title = listBox1.SelectedItem.ToString();
            SearchResult video;
            if(!videos.TryGetValue(title, out video))
            {
                MessageBox.Show("Acesta valoare nu exista, incercati alta!","Eroare!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var request = System.Net.WebRequest.Create(video.Snippet.Thumbnails.Medium.Url);

            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            pictureBox1.Image = Bitmap.FromStream(stream);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Nu ai selectat nici un videoclip!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string title = listBox1.SelectedItem.ToString();
            if (!videos.TryGetValue(title, out SearchResult video))
            {
                MessageBox.Show("Acesta valoare nu exista, incercati alta!", "Eroare!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Music_Play(video);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            File.Open(FilePath,FileMode.OpenOrCreate);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string JString = File.ReadAllText(FilePath);
            JArray ja = JArray.Parse(JString);
            foreach(JObject jo in ja)
            {
                string videoName = (string) jo.GetValue("Name");

                JArray playlist = (JArray) jo.GetValue("Playlist");

                List<string> VideoIDs = new List<string>();

                foreach(JObject video in playlist)
                {
                    string title = (string) video.GetValue("Title");
                    string id = (string)video.GetValue("VideoID");
                    VideoIDs.Add(id);
                    Form2.listBox1.Items.Add(title);
                    // Nu e bine aici , gandestete la alt mode de a muta playlistul in form2 !!!!
                }

                Form2.load.Add(videoName,VideoIDs);
            }
        }
    }
}
