using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace YouTubeDownloader
{
    public partial class frmFileDownloader : Form
    {
        FileDownloader downloader;
        public frmFileDownloader(string Url,string SaveTo)
        {
            InitializeComponent();
            var folder = Path.GetDirectoryName(SaveTo);
            string file = Path.GetFileName(SaveTo);
            downloader = new FileDownloader(Url, folder, file);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.RunWorkerCompleted += downloader_RunWorkerCompleted;
           
            downloader.RunWorkerAsync();
        }

        void downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshStatus();
            if (e.Cancelled) Close();
        }
        private bool processing;
        void downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (processing) return;

            if (InvokeRequired) Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;
                    progressBar1.Value = e.ProgressPercentage > 100 ? 100 : e.ProgressPercentage;
                    this.Text = "File Downloader- " + e.ProgressPercentage + " %";
                    string speed = String.Format(new FileSizeFormatProvider(), "{0:fs}", downloader.DownloadSpeed);
                    string ETA = downloader.ETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)downloader.ETA) * 1000) + " ]";
                    labelStatus.Text = speed + ETA;
                    RefreshStatus();
                }
                catch { }
                finally { processing = false; }
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            downloader.CancelAsync();
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (downloader.DownloadStatus == DownloadStatus.Downloading)
                downloader.Pause();
            else if (downloader.DownloadStatus == DownloadStatus.Paused)
                downloader.Resume();
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            buttonPause.Visible = buttonStop.Visible = downloader.DownloadStatus == DownloadStatus.Downloading || downloader.DownloadStatus == DownloadStatus.Paused;
            buttonOpen.Visible = downloader.DownloadStatus == DownloadStatus.Success;
            if (downloader.DownloadStatus == DownloadStatus.Success)
                labelStatus.Text = "Completed";
          else  if (downloader.DownloadStatus == DownloadStatus.Downloading)
                buttonPause.Text = "Pause";
            else if (downloader.DownloadStatus == DownloadStatus.Paused)
            {
                buttonPause.Text = "Resume";
                labelStatus.Text = "Paused";
            }

        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try { Process.Start(Path.GetDirectoryName(downloader.DestFileName)); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void frmFileDownloader_FormClosing(object sender, FormClosingEventArgs e)
        {
            downloader.Abort();
        }
    }
}
