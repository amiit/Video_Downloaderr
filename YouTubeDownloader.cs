using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Web;
using System.Text.RegularExpressions;

namespace YouTubeDownloader
{ 
    /// <summary>
    /// Contains information about the video url extension and dimension
    /// </summary>
    public class YouTubeVideoQuality 
    {
        /// <summary>
        /// Gets or Sets the file name
        /// </summary>
        public string VideoTitle { get; set; }
        /// <summary>
        /// Gets or Sets the file extention
        /// </summary>
        public string Extention { get; set; }
        /// <summary>
        /// Gets or Sets the file url
        /// </summary>
        public string DownloadUrl { get; set; }
        /// <summary>
        /// Gets or Sets the youtube video url
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// Gets or Sets the file size
        /// </summary>
        public Size Dimension { get; set; }

        public override string ToString()
        {
            return Extention + " File " + Dimension.Width + "x" + Dimension.Height;
        }

        public void SetQuality(string Extention, Size Dimension)
        {
            this.Extention = Extention;
            this.Dimension = Dimension;
        }
    }
    /// <summary>
    /// Use this class to get youtube video urls
    /// </summary>
    public class YouTubeDownloader
    {
        public static List<YouTubeVideoQuality> GetYouTubeVideoUrls(params string[] VideoUrls)
        {
            List<YouTubeVideoQuality> urls = new List<YouTubeVideoQuality>();
            foreach (var VideoUrl in VideoUrls)
            {
                string html = Helper.DownloadWebPage(VideoUrl);
                string title = GetTitle(html);
                foreach (var videoLink in ExtractUrls(html))
                {
                    YouTubeVideoQuality q = new YouTubeVideoQuality();
                    q.VideoUrl = VideoUrl;
                    q.VideoTitle = title;
                    q.DownloadUrl = videoLink + "&title=" + title;
                    if (getQuality(q))
                        urls.Add(q);
                }
            }
            return urls;
        }

        private static string GetTitle(string RssDoc)
        {
            string str14 = Helper.GetTxtBtwn(RssDoc, "'VIDEO_TITLE': '", "'", 0);
            if (str14 == "") str14 = Helper.GetTxtBtwn(RssDoc, "\"title\" content=\"", "\"", 0);
            if (str14 == "") str14 = Helper.GetTxtBtwn(RssDoc, "&title=", "&", 0);
            str14 = str14.Replace(@"\", "").Replace("'", "&#39;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("+", " ");
            return str14;
        }


        private static List<string> ExtractUrls(string html)
        {
            html = Uri.UnescapeDataString(Regex.Match(html, "url_encoded_fmt_stream_map=(.+?)&", RegexOptions.Singleline).Groups[1].ToString());
            MatchCollection matchs = Regex.Matches(html, "url=(.+?)&quality=(.+?)&fallback_host=(.+?)&type=(.+?)&itag=(.+?),", RegexOptions.Singleline);
            bool firstTry = matchs.Count > 0;
            if (!firstTry)
                matchs = Regex.Matches(html, "itag=(.+?)&url=(.+?)&type=(.+?)&fallback_host=(.+?)&sig=(.+?)&quality=(.+?),{0,1}", RegexOptions.Singleline);
            List<string> urls = new List<string>();
            foreach (Match match in matchs)
            {
                if (firstTry)
                    urls.Add(Uri.UnescapeDataString(match.Groups[1] + ""));
                else urls.Add(Uri.UnescapeDataString(match.Groups[2] + "") + "&signature=" + match.Groups[5]);
            }
            return urls;
        }

        private static bool getQuality(YouTubeVideoQuality q)
        {
            if (q.DownloadUrl.Contains("itag=5"))
                q.SetQuality("flv", new Size(320, 240));
            else if (q.DownloadUrl.Contains("itag=34"))
                q.SetQuality("flv", new Size(400, 226));
            else if (q.DownloadUrl.Contains("itag=6"))
                q.SetQuality("flv", new Size(480, 360));
            else if (q.DownloadUrl.Contains("itag=35"))
                q.SetQuality("flv", new Size(640, 380));
            else if (q.DownloadUrl.Contains("itag=18"))
                q.SetQuality("mp4", new Size(480, 360));
            else if (q.DownloadUrl.Contains("itag=22"))
                q.SetQuality("mp4", new Size(1280, 720));
            else if (q.DownloadUrl.Contains("itag=37"))
                q.SetQuality("mp4", new Size(1920, 1280));
            else if (q.DownloadUrl.Contains("itag=38"))
                q.SetQuality("mp4", new Size(4096, 72304));
            else return false;
            return true;
        }
    }
}
