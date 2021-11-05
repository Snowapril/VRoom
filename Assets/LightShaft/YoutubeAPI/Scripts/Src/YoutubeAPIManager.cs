using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using UnityEngine.Networking;

public class YoutubeAPIManager : MonoBehaviour {

    private YoutubeData data;
    private YoutubeData[] searchResults;
    private YoutubeComments[] comments;
    private YoutubePlaylistItems[] playslistItems;
    private YoutubeChannel[] channels;

    //REMEMBER TO CHANGE HERE IF YOU NEED TO POINT TO YOUR GOOGLE APP. 
    /* 
     * TO CREATE YOUR GOOGLE APP AND USE YOUR API GO TO:
     * https://code.google.com/apis/console
     * -Create a project.
     * -Go to APIs -> YouTube APIs -> YouTube Data API and enable that.
     * -then go to credentials create a new key for Public API access
     * - now you have the API key just copy and change the variable APIKey with your new API Key to monitor the use of youtube api calls.
     * Any question? mail me: kelvinparkour@gmail.com
     * 
     * */

    private const string APIKey = "AIzaSyCSh8NNjutCdJ3pAMJjWrGFbB8_onIMn1Y";
    /*AIzaSyCSh8NNjutCdJ3pAMJjWrGFbB8_onIMn1Y*/
        /*"AIzaSyAyctJVli2oEUoXZ8ta_4O0nKyknwXzvaw";*/

    public void GetVideoData(string videoId, Action<YoutubeData> callback)
    {
        StartCoroutine(LoadSingleVideo(videoId, callback));
    }

    private void Start()
    {
        Debug.LogWarning("REMEMBER TO CHANGE THE API KEY TO YOUR OWN KEY - REMOVE THIS IF YOU CHANGED");
    }


    public void GetChannelVideos(string channelId, int maxResults, Action<YoutubeData[]> callback)
    {
        StartCoroutine(GetVideosFromChannel(channelId, maxResults,callback));
    }

    public void Search(string keyword, int maxResult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, string customFilters, Action<YoutubeData[]> callback)
    {
        StartCoroutine(YoutubeSearch(keyword, maxResult, order, safeSearch, customFilters, callback));
    }

    public void TrendingVideos(string regionCode, int maxResult, Action<YoutubeData[]> callback)
    {
        StartCoroutine(GetTrendingVideos(regionCode,maxResult,callback));
    }

    public void SearchForChannels(string keyword, int maxResult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, Action<YoutubeChannel[]> callback)
    {
        StartCoroutine(YoutubeSearchChannel(keyword, maxResult, order, safeSearch, callback));
    }

    public void SearchByCategory(string keyword, string category, int maxResult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, Action<YoutubeData[]> callback)
    {
        StartCoroutine(YoutubeSearchUsingCategory(keyword, category, maxResult, order, safeSearch, callback));
    }

    public void SearchByLocation(string keyword, int maxResult,int locationRadius, float latitude, float longitude, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, string customFilters, Action<YoutubeData[]> callback)
    {
        StartCoroutine(YoutubeSearchByLocation(keyword, maxResult, locationRadius, latitude, longitude, order, safeSearch, customFilters, callback));
    }

    public void GetComments(string videoId, Action<YoutubeComments[]> callback)
    {
        StartCoroutine(YoutubeCallComments(videoId, callback));
    }

    public void GetPlaylistItems(string playlistId,int maxResults, Action<YoutubePlaylistItems[]> callback)
    {
        StartCoroutine(YoutubeCallPlaylist(playlistId, maxResults, callback));
    }

    IEnumerator GetVideosFromChannel(string channelId, int maxResults, Action<YoutubeData[]> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search?order=date&type=video&part=snippet&channelId=" + channelId + "&maxResults=" + maxResults + "&key=" + APIKey);
        yield return request.SendWebRequest();
        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        searchResults = new YoutubeData[result["items"].Count];
        for (int itemsCounter = 0; itemsCounter < searchResults.Length; itemsCounter++)
        {
            searchResults[itemsCounter] = new YoutubeData();
            searchResults[itemsCounter].id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out searchResults[itemsCounter].snippet);
        }
        callback.Invoke(searchResults);
    }

    IEnumerator YoutubeCallPlaylist(string playlistId,int maxResults, Action<YoutubePlaylistItems[]> callback)
    {

        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/playlistItems/?playlistId=" + playlistId + "&maxResults=" + maxResults + "&part=snippet%2CcontentDetails&key=" + APIKey);
        yield return request.SendWebRequest();

        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        playslistItems = new YoutubePlaylistItems[result["items"].Count];
        for (int itemsCounter = 0; itemsCounter < playslistItems.Length; itemsCounter++)
        {
            playslistItems[itemsCounter] = new YoutubePlaylistItems();
            playslistItems[itemsCounter].videoId = result["items"][itemsCounter]["snippet"]["resourceId"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out playslistItems[itemsCounter].snippet);
        }
        callback.Invoke(playslistItems);
    }

    IEnumerator YoutubeCallComments(string videoId, Action<YoutubeComments[]> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/commentThreads/?videoId=" + videoId + "&part=snippet%2Creplies&key=" + APIKey);
        yield return request.SendWebRequest();

        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        comments = new YoutubeComments[result["items"].Count];
        for (int itemsCounter = 0; itemsCounter < comments.Length; itemsCounter++)
        {
            comments[itemsCounter] = new YoutubeComments();
            SetComment(result["items"][itemsCounter]["snippet"], out comments[itemsCounter]);
        }
        callback.Invoke(comments);
    }

    IEnumerator YoutubeSearchUsingCategory(string keyword, string category, int maxresult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, Action<YoutubeData[]> callback)
    {
        keyword = keyword.Replace(" ", "%20");
        category = category.Replace(" ", "%20");

        string orderFilter, safeSearchFilter;
        orderFilter = "";
        if (order != YoutubeSearchOrderFilter.none)
        {
            orderFilter = "&order=" + order.ToString();
        }
        safeSearchFilter = "&safeSearch=" + safeSearch.ToString();

        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search/?q=" + keyword + "&videoCategoryId=" + category + "&maxResults=" + maxresult + "&type=video&part=snippet,id&key=" + APIKey + "" + orderFilter + "" + safeSearchFilter);
        yield return request.SendWebRequest();

        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        searchResults = new YoutubeData[result["items"].Count];
        Debug.Log(searchResults.Length);
        for (int itemsCounter = 0; itemsCounter < searchResults.Length; itemsCounter++)
        {
            searchResults[itemsCounter] = new YoutubeData();
            searchResults[itemsCounter].id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out searchResults[itemsCounter].snippet);
        }
        callback.Invoke(searchResults);
    }

    IEnumerator YoutubeSearchChannel(string keyword, int maxresult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, Action<YoutubeChannel[]> callback)
    {
        keyword = keyword.Replace(" ", "%20");

        string orderFilter, safeSearchFilter;
        orderFilter = "";
        if (order != YoutubeSearchOrderFilter.none)
        {
            orderFilter = "&order=" + order.ToString();
        }
        safeSearchFilter = "&safeSearch=" + safeSearch.ToString();

        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search/?q=" + keyword + "&type=channel&maxResults=" + maxresult + "&part=snippet,id&key=" + APIKey + "" + orderFilter + "" + safeSearchFilter);
        yield return request.SendWebRequest();

        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        channels = new YoutubeChannel[result["items"].Count];
        for (int itemsCounter = 0; itemsCounter < channels.Length; itemsCounter++)
        {
            channels[itemsCounter] = new YoutubeChannel();
            channels[itemsCounter].id = result["items"][itemsCounter]["id"]["channelId"];
            channels[itemsCounter].title = result["items"][itemsCounter]["snippet"]["title"];
            channels[itemsCounter].description = result["items"][itemsCounter]["snippet"]["description"];
            channels[itemsCounter].thumbnail = result["items"][itemsCounter]["snippet"]["thumbnails"]["high"]["url"];
        }
        callback.Invoke(channels);
    }

    IEnumerator GetTrendingVideos(string regionCode, int maxresult, Action<YoutubeData[]> callback)
    {

        string newurl = UnityWebRequest.EscapeURL("https://www.googleapis.com/youtube/v3/videos?part=snippet,id&chart=mostPopular&regionCode=" + regionCode+"&maxResults="+maxresult+"&key="+APIKey);

        UnityWebRequest request = UnityWebRequest.Get(UnityWebRequest.UnEscapeURL(newurl));
        yield return request.SendWebRequest();

        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        searchResults = new YoutubeData[result["items"].Count];
        Debug.Log(searchResults.Length);
        for (int itemsCounter = 0; itemsCounter < searchResults.Length; itemsCounter++)
        {
            searchResults[itemsCounter] = new YoutubeData();
            searchResults[itemsCounter].id = result["items"][itemsCounter]["id"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out searchResults[itemsCounter].snippet);
        }
        callback.Invoke(searchResults);
    }

    IEnumerator YoutubeSearch(string keyword,int maxresult, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch, string customFilters, Action<YoutubeData[]> callback)
    {
        keyword = keyword.Replace(" ", "%20");

        string orderFilter,safeSearchFilter;
        orderFilter = "";
        if (order != YoutubeSearchOrderFilter.none)
        {
            orderFilter = "&order="+order.ToString();
        }
        safeSearchFilter = "&safeSearch=" + safeSearch.ToString();

        string newurl = UnityWebRequest.EscapeURL("https://www.googleapis.com/youtube/v3/search/?q=" + keyword + "&type=video&maxResults=" + maxresult + "&part=snippet,id&key=" + APIKey + "" + orderFilter + "" + safeSearchFilter+""+customFilters);
        Debug.Log(newurl);

        UnityWebRequest request = UnityWebRequest.Get(UnityWebRequest.UnEscapeURL(newurl));
        yield return request.SendWebRequest();
        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        searchResults = new YoutubeData[result["items"].Count];
        Debug.Log(searchResults.Length);
        for (int itemsCounter = 0; itemsCounter < searchResults.Length; itemsCounter++)
        {
            searchResults[itemsCounter] = new YoutubeData();
            searchResults[itemsCounter].id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out searchResults[itemsCounter].snippet);
        }
        callback.Invoke(searchResults);
    }

    IEnumerator YoutubeSearchByLocation(string keyword, int maxResult, int locationRadius, float latitude, float longitude, YoutubeSearchOrderFilter order, YoutubeSafeSearchFilter safeSearch,string customFilters, Action<YoutubeData[]> callback)
    {
        keyword = keyword.Replace(" ", "%20");
        string orderFilter, safeSearchFilter;
        orderFilter = "";
        if (order != YoutubeSearchOrderFilter.none)
        {
            orderFilter = "&order=" + order.ToString();
        }
        safeSearchFilter = "&safeSearch=" + safeSearch.ToString();

        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search/?type=video&q=" + keyword + "&type=video&locationRadius=" + locationRadius + "mi&location=" + latitude + "%2C" + longitude + "&part=snippet,id&maxResults=" + maxResult + "&key=" + APIKey + "" + orderFilter + "" + safeSearchFilter + "" + customFilters);
        yield return request.SendWebRequest();
        Debug.Log(request.url);
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        searchResults = new YoutubeData[result["items"].Count];
        Debug.Log(searchResults.Length);
        for(int itemsCounter = 0; itemsCounter < searchResults.Length; itemsCounter++)
        {
            searchResults[itemsCounter] = new YoutubeData();
            searchResults[itemsCounter].id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out searchResults[itemsCounter].snippet);
        }
        callback.Invoke(searchResults);
    }


    IEnumerator LoadSingleVideo(string videoId, Action<YoutubeData> callback)
    {

        UnityWebRequest request = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/videos?id=" + videoId + "&part=snippet,id,contentDetails,statistics&key=" + APIKey);
        yield return request.SendWebRequest();
        Debug.Log(request.url);
        data = new YoutubeData();
        JSONNode result = JSON.Parse(request.downloadHandler.text);
        result = result["items"][0];   //using items
        data.id = result["id"];
        //Populate snippet data
        SetSnippet(result["snippet"], out data.snippet);
        SetStatistics(result["statistics"], out data.statistics);
        SetContentDetails(result["contentDetails"], out data.contentDetails);
        callback.Invoke(data);
    }

    private void SetSnippet(JSONNode resultSnippet, out YoutubeSnippet data)
    {
        data = new YoutubeSnippet();
        data.publishedAt = resultSnippet["publishedAt"];
        data.channelId = resultSnippet["channelId"];
        data.title = resultSnippet["title"];
        data.description = resultSnippet["description"];
        //Thumbnails
        data.thumbnails = new YoutubeTumbnails();
        data.thumbnails.defaultThumbnail = new YoutubeThumbnailData();
        data.thumbnails.defaultThumbnail.url = resultSnippet["thumbnails"]["default"]["url"];
        data.thumbnails.defaultThumbnail.width = resultSnippet["thumbnails"]["default"]["width"];
        data.thumbnails.defaultThumbnail.height = resultSnippet["thumbnails"]["default"]["height"];
        data.thumbnails.mediumThumbnail = new YoutubeThumbnailData();
        data.thumbnails.mediumThumbnail.url = resultSnippet["thumbnails"]["medium"]["url"];
        data.thumbnails.mediumThumbnail.width = resultSnippet["thumbnails"]["medium"]["width"];
        data.thumbnails.mediumThumbnail.height = resultSnippet["thumbnails"]["medium"]["height"];
        data.thumbnails.highThumbnail = new YoutubeThumbnailData();
        data.thumbnails.highThumbnail.url = resultSnippet["thumbnails"]["high"]["url"];
        data.thumbnails.highThumbnail.width = resultSnippet["thumbnails"]["high"]["width"];
        data.thumbnails.highThumbnail.height = resultSnippet["thumbnails"]["high"]["height"];
        data.thumbnails.standardThumbnail = new YoutubeThumbnailData();
        data.thumbnails.standardThumbnail.url = resultSnippet["thumbnails"]["standard"]["url"];
        data.thumbnails.standardThumbnail.width = resultSnippet["thumbnails"]["standard"]["width"];
        data.thumbnails.standardThumbnail.height = resultSnippet["thumbnails"]["standard"]["height"];
        data.channelTitle = resultSnippet["channelTitle"];
        //TAGS
        data.tags = new string[resultSnippet["tags"].Count];
        for (int index = 0; index < data.tags.Length; index++)
        {
            data.tags[index] = resultSnippet["tags"][index];
        }
        data.categoryId = resultSnippet["categoryId"];
    }
    private void SetStatistics(JSONNode resultStatistics, out YoutubeStatistics data)
    {
        data = new YoutubeStatistics();
        data.viewCount = resultStatistics["viewCount"];
        data.likeCount = resultStatistics["likeCount"];
        data.dislikeCount = resultStatistics["dislikeCount"];
        data.favoriteCount = resultStatistics["favoriteCount"];
        data.commentCount = resultStatistics["commentCount"];
    }
    private void SetContentDetails(JSONNode resultContentDetails, out YoutubeContentDetails data)
    {
        data = new YoutubeContentDetails();
        data.duration = resultContentDetails["duration"];
        data.dimension = resultContentDetails["dimension"];
        data.definition = resultContentDetails["definition"];
        data.caption = resultContentDetails["caption"];
        data.licensedContent = resultContentDetails["licensedContent"];
        data.projection = resultContentDetails["projection"];

        if(resultContentDetails["contentRating"] != null)
        {
            Debug.Log("Age restrict found!");
            if (resultContentDetails["contentRating"]["ytRating"] == "ytAgeRestricted")
                data.ageRestrict = true;
            else
                data.ageRestrict = false;
        }
        else
            data.ageRestrict = false;

    }

    private void SetComment(JSONNode commentsData, out YoutubeComments data)
    {
        data = new YoutubeComments();
        data.videoId = commentsData["videoId"];
        JSONNode commentDetail = commentsData["topLevelComment"]["snippet"];
        data.authorDisplayName = commentDetail["authorDisplayName"];
        data.authorProfileImageUrl = commentDetail["authorProfileImageUrl"];
        data.authorChannelUrl = commentDetail["authorChannelUrl"];
        data.authorChannelId = commentDetail["authorChannelId"]["value"];
        data.textDisplay = commentDetail["textDisplay"];
        data.textOriginal = commentDetail["textOriginal"];
        data.canRate = commentDetail["canRate"].AsBool;
        data.viewerRating = commentDetail["viewerRating"];
        data.likeCount = commentDetail["likeCount"].AsInt;
        data.publishedAt = commentDetail["publishedAt"];
        data.updatedAt = commentDetail["updatedAt"];
    }

    public enum YoutubeSearchOrderFilter
    {
        none,
        date,
        rating,
        relevance,
        title,
        videoCount,
        viewCount
    }

    public enum YoutubeSafeSearchFilter
    {
        none,
        moderate,
        strict
    }
}
public class YoutubeData
{
    public YoutubeSnippet snippet;
    public YoutubeStatistics statistics;
    public YoutubeContentDetails contentDetails;
    public string id;
}

public class YoutubeComments{
    public string authorDisplayName;
    public string authorProfileImageUrl;
    public string authorChannelUrl;
    public string authorChannelId;
    public string videoId;
    public string textDisplay;
    public string textOriginal;
    public bool canRate;
    public string viewerRating;
    public int likeCount;
    public string publishedAt;
    public string updatedAt;
}

public class YoutubePlaylistItems
{
    public string videoId;
    public YoutubeSnippet snippet;
}

public class YoutubeChannel
{
    public string id;
    public string title;
    public string description;
    public string thumbnail;
}
