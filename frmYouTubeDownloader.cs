using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace YouTubeDownloader
{
    public partial class frmYouTubeDownloader : Form
    {
        string[] videoUrls;
        public frmYouTubeDownloader()
        {
            InitializeComponent();
            panel1.Dock = panel2.Dock = DockStyle.Fill;
            ShowPanel(0);
        }
        private void ShowPanel(int windowNo)
        {
            panel1.Visible = windowNo == 0;
            panel2.Visible = windowNo == 1;
        }
        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count == 0)
                    throw new Exception("Please select one video");
                if (DialogResult.OK != saveFileDialog1.ShowDialog(this)) return;
                YouTubeVideoQuality video = listView1.SelectedItems[0].Tag as YouTubeVideoQuality;
                new frmFileDownloader(video.DownloadUrl, saveFileDialog1.FileName).Show(this);
             //   Process.Start(video.DownloadUrl);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = YouTubeDownloader.GetYouTubeVideoUrls(videoUrls);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            try
            {
                UseWaitCursor = false;
                if (e.Error!=null)
                    throw e.Error;
                List<YouTubeVideoQuality> urls = e.Result as List<YouTubeVideoQuality>;
                foreach (var item in urls)
                {
                    ListViewItem listItem = new ListViewItem(item.Extention);
                   // listItem.SubItems.Add(item.Extention);
                    listItem.SubItems.Add(formatSize(item.Dimension));
                    listItem.Tag = item;
                    listView1.Items.Add(listItem);
                    this.Text = "YouTube Downloader- " + item.VideoTitle;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private string formatSize(object value)
        {
            string s = ((Size)value).Height >= 720 ? " HD" : "";
            if (value is Size) return ((Size)value).Width+"x"+((Size)value).Height + s;
            return "";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Helper.isValidUrl(textBoxUrl.Text) || !textBoxUrl.Text.ToLower().Contains("www.youtube.com/watch?"))
                    MessageBox.Show("Please Enter valid youtube video url");
                else
                {

                    
                    this.videoUrls = new string[] { textBoxUrl.Text };
                    pictureBox2.ImageLocation = string.Format("http://i3.ytimg.com/vi/{0}/default.jpg", Helper.GetVideoIDFromUrl(textBoxUrl.Text));
                   listView1.Items.Clear();
                    backgroundWorker1.RunWorkerAsync();
                    UseWaitCursor = true;
                    ShowPanel(1);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowPanel(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count == 0)
                    throw new Exception("Please select one video");
                YouTubeVideoQuality video = listView1.SelectedItems[0].Tag as YouTubeVideoQuality;
                Clipboard.SetText(video.DownloadUrl);
                // Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void textBoxUrl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                button1_Click(null, null);
            }
        }
    }
}
