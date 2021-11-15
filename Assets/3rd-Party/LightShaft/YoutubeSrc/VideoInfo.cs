﻿using System.Collections.Generic;

namespace YoutubeLight
{
    public class VideoInfo
    {
        internal static IEnumerable<VideoInfo> Defaults = new List<VideoInfo>
        {
            /* Non-adaptive */
            new VideoInfo(5, VideoType.Flash, 240, false, AudioType.Mp3, 64, AdaptiveType.None),
            new VideoInfo(6, VideoType.Flash, 270, false, AudioType.Mp3, 64, AdaptiveType.None),
            new VideoInfo(13, VideoType.Mobile, 0, false, AudioType.Aac, 0, AdaptiveType.None),
            new VideoInfo(17, VideoType.Mobile, 144, false, AudioType.Aac, 24, AdaptiveType.None),
            new VideoInfo(18, VideoType.Mp4, 360, false, AudioType.Aac, 96, AdaptiveType.None),
            new VideoInfo(22, VideoType.Mp4, 720, false, AudioType.Aac, 192, AdaptiveType.None),
            new VideoInfo(34, VideoType.Flash, 360, false, AudioType.Aac, 128, AdaptiveType.None),
            new VideoInfo(35, VideoType.Flash, 480, false, AudioType.Aac, 128, AdaptiveType.None),
            new VideoInfo(36, VideoType.Mobile, 240, false, AudioType.Aac, 38, AdaptiveType.None),
            new VideoInfo(37, VideoType.Mp4, 1080, false, AudioType.Aac, 192, AdaptiveType.None),
            new VideoInfo(38, VideoType.Mp4, 3072, false, AudioType.Aac, 192, AdaptiveType.None),
            new VideoInfo(43, VideoType.WebM, 360, false, AudioType.Vorbis, 128, AdaptiveType.None),
            new VideoInfo(44, VideoType.WebM, 480, false, AudioType.Vorbis, 128, AdaptiveType.None),
            new VideoInfo(45, VideoType.WebM, 720, false, AudioType.Vorbis, 192, AdaptiveType.None),
            new VideoInfo(46, VideoType.WebM, 1080, false, AudioType.Vorbis, 192, AdaptiveType.None),
            new VideoInfo(59, VideoType.Mp4, 480, false, AudioType.Aac, 128, AdaptiveType.None),
            new VideoInfo(78, VideoType.Mp4, 480, false, AudioType.Aac, 128, AdaptiveType.None),

            /* 3d */
            new VideoInfo(82, VideoType.Mp4, 360, true, AudioType.Aac, 96, AdaptiveType.None),
            new VideoInfo(83, VideoType.Mp4, 240, true, AudioType.Aac, 96, AdaptiveType.None),
            new VideoInfo(84, VideoType.Mp4, 720, true, AudioType.Aac, 152, AdaptiveType.None),
            new VideoInfo(85, VideoType.Mp4, 520, true, AudioType.Aac, 152, AdaptiveType.None),
            new VideoInfo(100, VideoType.WebM, 360, true, AudioType.Vorbis, 128, AdaptiveType.None),
            new VideoInfo(101, VideoType.WebM, 360, true, AudioType.Vorbis, 192, AdaptiveType.None),
            new VideoInfo(102, VideoType.WebM, 720, true, AudioType.Vorbis, 192, AdaptiveType.None),

            /* Apple HTTP Live Streaming */
            new VideoInfo(91, VideoType.Mp4, 144, false, AudioType.Aac, 48, AdaptiveType.None),
            new VideoInfo(92, VideoType.Mp4, 240, false, AudioType.Aac, 48, AdaptiveType.None),
            new VideoInfo(93, VideoType.Mp4, 360, false, AudioType.Aac, 128, AdaptiveType.None),
            new VideoInfo(94, VideoType.Mp4, 480, false, AudioType.Aac, 128, AdaptiveType.None),
            new VideoInfo(95, VideoType.Mp4, 720, false, AudioType.Aac, 256, AdaptiveType.None),
            new VideoInfo(96, VideoType.Mp4, 1080, false, AudioType.Aac, 256, AdaptiveType.None),
            new VideoInfo(132, VideoType.Mp4, 240, false, AudioType.Aac, 48, AdaptiveType.None),
            new VideoInfo(151, VideoType.Mp4, 72, false, AudioType.Aac, 24, AdaptiveType.None),

            /* Adaptive (aka DASH) - MP4 Video */
            new VideoInfo(133, VideoType.Mp4, 240, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(134, VideoType.Mp4, 360, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(135, VideoType.Mp4, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(136, VideoType.Mp4, 720, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(137, VideoType.Mp4, 1080, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(138, VideoType.Mp4, 2160, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(160, VideoType.Mp4, 144, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(212, VideoType.Mp4, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(264, VideoType.Mp4, 1440, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(298, VideoType.Mp4, 720, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(299, VideoType.Mp4, 1080, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(266, VideoType.Mp4, 2160, false, AudioType.Unknown, 0, AdaptiveType.Video),            

            /* Adaptive (aka DASH) - Audio */
            new VideoInfo(139, VideoType.Mp4, 0, false, AudioType.Aac, 48, AdaptiveType.Audio),
            new VideoInfo(140, VideoType.Mp4, 0, false, AudioType.Aac, 128, AdaptiveType.Audio),
            new VideoInfo(141, VideoType.Mp4, 0, false, AudioType.Aac, 256, AdaptiveType.Audio),
            new VideoInfo(256, VideoType.Mp4, 0, false, AudioType.Aac, 0, AdaptiveType.Audio),
            new VideoInfo(258, VideoType.Mp4, 0, false, AudioType.Aac, 0, AdaptiveType.Audio),
            new VideoInfo(325, VideoType.Mp4, 0, false, AudioType.Aac, 0, AdaptiveType.Audio),
            new VideoInfo(328, VideoType.Mp4, 0, false, AudioType.Aac, 0, AdaptiveType.Audio),

            /* Adaptive (aka DASH) - WebM Video Height > Width */
            new VideoInfo(167, VideoType.WebM, 360, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(168, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(169, VideoType.WebM, 720, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(170, VideoType.WebM, 1080, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(218, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(219, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),

            /* Adaptive (aka DASH) - WebM Video */
            new VideoInfo(278, VideoType.WebM, 144, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(242, VideoType.WebM, 240, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(243, VideoType.WebM, 360, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(244, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(245, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(246, VideoType.WebM, 480, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(247, VideoType.WebM, 720, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(248, VideoType.WebM, 1080, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(271, VideoType.WebM, 1440, false, AudioType.Unknown, 0, AdaptiveType.Video),
            // itag 272 videos are either 3840x2160 or 7680x4320
            new VideoInfo(272, VideoType.WebM, 2160, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(302, VideoType.WebM, 720, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(303, VideoType.WebM, 1080, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(308, VideoType.WebM, 1440, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(313, VideoType.WebM, 2160, false, AudioType.Unknown, 0, AdaptiveType.Video),
            new VideoInfo(315, VideoType.WebM, 2160, false, AudioType.Unknown, 0, AdaptiveType.Video),
            
            /* Adaptive (aka DASH) - WebM Audio */
            new VideoInfo(171, VideoType.WebM, 0, false, AudioType.Vorbis, 128, AdaptiveType.Audio),
            new VideoInfo(172, VideoType.WebM, 0, false, AudioType.Vorbis, 192, AdaptiveType.Audio)
        };

        internal VideoInfo(int formatCode)
            : this(formatCode, VideoType.Unknown, 0, false, AudioType.Unknown, 0, AdaptiveType.None)
        { }

        internal VideoInfo(VideoInfo info)
            : this(info.FormatCode, info.VideoType, info.Resolution, info.Is3D, info.AudioType, info.AudioBitrate, info.AdaptiveType)
        { }

        private VideoInfo(int formatCode, VideoType videoType, int resolution, bool is3D, AudioType audioType, int audioBitrate, AdaptiveType adaptiveType)
        {
            this.FormatCode = formatCode;
            this.VideoType = videoType;
            this.Resolution = resolution;
            this.Is3D = is3D;
            this.AudioType = audioType;
            this.AudioBitrate = audioBitrate;
            this.AdaptiveType = adaptiveType;
        }

        /// <value>
        /// <c>AdaptiveType.Audio</c> or <c>AdaptiveType.Video</c> if the format is adaptive;
        /// otherwise, <c>AdaptiveType.None</c>.
        /// </value>
        public AdaptiveType AdaptiveType { get; private set; }

        /// <value>The approximate audio bitrate in kbit/s, or 0 if the bitrate is unknown.</value>
        public int AudioBitrate { get; private set; }

        /// <value>The audio extension, or <c>null</c> if the audio extension is unknown.</value>
        public string AudioExtension
        {
            get
            {
                switch (this.AudioType)
                {
                    case AudioType.Aac:
                        return ".aac";

                    case AudioType.Mp3:
                        return ".mp3";

                    case AudioType.Opus:
                        return ".webm";

                    case AudioType.Vorbis:
                        return ".ogg";
                }

                return null;
            }
        }

        public AudioType AudioType { get; private set; }


        /// <value>
        /// <c>true</c> if the audio of this video can be extracted by YoutubeLight; otherwise, <c>false</c>.
        /// </value>
        public bool CanExtractAudio
        {
            get { return this.VideoType == VideoType.Flash ; }
        }

        public string DownloadUrl { get; internal set; }

        public int FormatCode { get; private set; }

        public bool Is3D { get; private set; }

        public bool HDR { get; private set; }

        public bool RequiresDecryption { get; internal set; }

        /// <value>The resolution of the video, or 0 if the resolution is unkown.</value>
        public int Resolution { get; private set; }

        public string Title { get; internal set; }

        /// <value>The video extension, or <c>null</c> if the video extension is unknown.</value>
        public string VideoExtension
        {
            get
            {
                switch (this.VideoType)
                {
                    case VideoType.Mp4:
                        return ".mp4";

                    case VideoType.Mobile:
                        return ".3gp";

                    case VideoType.Flash:
                        return ".flv";

                    case VideoType.WebM:
                        return ".webm";
                }

                return null;
            }
        }

        public VideoType VideoType { get; private set; }

        internal string HtmlPlayerVersion { get; set; }

        internal string HtmlscriptName { get; set; }

        public override string ToString()
        {
            return string.Format("Full Title: {0}, Type: {1}, Resolution: {2}p", this.Title + this.VideoExtension, this.VideoType, this.Resolution);
        }
    }
}