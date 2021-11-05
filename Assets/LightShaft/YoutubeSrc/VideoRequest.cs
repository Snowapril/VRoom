using System;

namespace YoutubeLight
{
    
    public abstract class Downloader
    {
        /// <param name="video">The video.</param>
        /// <param name="savePath">The path to save the video if want</param>
        /// /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        protected Downloader(VideoInfo video, string savePath, int? bytesToDownload = null)
        {
            if (video == null)
                throw new ArgumentNullException("video");

            if (savePath == null)
                throw new ArgumentNullException("savePath");

            this.Video = video;
            this.SavePath = savePath;
            this.BytesToDownload = bytesToDownload;
        }

        public event EventHandler DownloadFinished;

       
        public event EventHandler DownloadStarted;

       
        public int? BytesToDownload { get; private set; }

       
        public string SavePath { get; private set; }

       
        public VideoInfo Video { get; private set; }

       
        public abstract void Execute();

        protected void OnDownloadFinished(EventArgs e)
        {
            if (this.DownloadFinished != null)
            {
                this.DownloadFinished(this, e);
            }
        }

        protected void OnDownloadStarted(EventArgs e)
        {
            if (this.DownloadStarted != null)
            {
                this.DownloadStarted(this, e);
            }
        }
    }
}