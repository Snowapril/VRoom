using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;

public class YoutubeApiGetUnlimitedVideos : MonoBehaviour {

    public string APIKey = "AIzaSyDD-lxGLHsBIFPFPt2i31fc0tAHGeAb8mc";
    public string searchKeyword = "Unity";
    [SerializeField]
    List<YoutubeData> searchResults;
    private int currentResults = 0;
    public int maxResult;

    private void Start()
    {
        searchResults = new List<YoutubeData>();
        Debug.Log("Demo call");
        StartCoroutine(YoutubeSearch("Unity"));
    }

    IEnumerator YoutubeSearch(string keyword)
    {
        keyword = keyword.Replace(" ", "%20");
        int tempMaxResult = 0;
        if (maxResult > 50)
            tempMaxResult = 50;
        else
            tempMaxResult = maxResult;


        string newurl = UnityWebRequest.EscapeURL("https://www.googleapis.com/youtube/v3/search/?q=" + keyword + "&type=video&maxResults=" + tempMaxResult + "&part=snippet,id&key=" + APIKey + "");
        UnityWebRequest request = UnityWebRequest.Get(UnityWebRequest.UnEscapeURL(newurl));
        yield return request.SendWebRequest();

        JSONNode result = JSON.Parse(request.downloadHandler.text);
        currentResults += result["items"].Count;

        for (int itemsCounter = 0; itemsCounter < result["items"].Count; itemsCounter++)
        {
            YoutubeData ytItem = new YoutubeData();
            ytItem.id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out ytItem.snippet);

            searchResults.Add(ytItem);
        }

        if (currentResults < maxResult)
        {
            StartCoroutine(YoutubeGetNextPage(result["nextPageToken"]));
        }
        else
        {
            Debug.Log("List is done");
        }
    }

    IEnumerator YoutubeGetNextPage(string pageToken)
    {
        int tempMaxResult = 0;
        if (maxResult > 50)
            tempMaxResult = 50;
        else
            tempMaxResult = maxResult;
        string newurl = UnityWebRequest.EscapeURL("https://www.googleapis.com/youtube/v3/search/?pageToken="+pageToken+"&type=video&maxResults=" + tempMaxResult + "&part=snippet,id&key=" + APIKey + "");

        UnityWebRequest request = UnityWebRequest.Get(UnityWebRequest.UnEscapeURL(newurl));
        yield return request.SendWebRequest();

        JSONNode result = JSON.Parse(request.downloadHandler.text);
        currentResults += result["items"].Count;

        for (int itemsCounter = 0; itemsCounter < result["items"].Count; itemsCounter++)
        {
            YoutubeData ytItem = new YoutubeData();
            ytItem.id = result["items"][itemsCounter]["id"]["videoId"];
            SetSnippet(result["items"][itemsCounter]["snippet"], out ytItem.snippet);

            searchResults.Add(ytItem);
        }

        if (currentResults < maxResult)
        {
            StartCoroutine(YoutubeGetNextPage(result["nextPageToken"]));
        }
        else
        {
            Debug.Log("List is done");
        }
    }


    private void SetSnippet(JSONNode resultSnippet, out YoutubeSnippet data)
    {
        data = new YoutubeSnippet();
        data.publishedAt = resultSnippet["publishedAt"];
        data.channelId = resultSnippet["channelId"];
        data.title = resultSnippet["title"];
        Debug.Log(data.title);
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
}
