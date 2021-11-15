using System;

namespace YoutubeLight
{
    public class VideoNotAvailableException : Exception
    {
        public VideoNotAvailableException()
        { }

        public VideoNotAvailableException(string message)
            : base(message)
        { }
    }
}

namespace YoutubeLight
{
    public class YoutubeParseException : Exception
    {
        public YoutubeParseException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}