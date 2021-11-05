using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YoutubeSnippet  {

    public string publishedAt;
    public string channelId;
    public string title;
    public string description;
    public YoutubeTumbnails thumbnails;
    public string channelTitle;
    public string[] tags;
    public string categoryId;
}

public class YoutubeTumbnails
{
    public YoutubeThumbnailData defaultThumbnail;
    public YoutubeThumbnailData mediumThumbnail;
    public YoutubeThumbnailData highThumbnail;
    public YoutubeThumbnailData standardThumbnail;
}

public class YoutubeThumbnailData
{
    public string url;
    public string width;
    public string height;
}

