using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class IndividualVideoDataDemo : MonoBehaviour {

    YoutubeAPIManager youtubeapi;

    public Text videoIdInput,UI_title, UI_description, UI_duration, UI_likes, UI_dislikes, UI_favorites, UI_comments, UI_views;
    public Image UI_thumbnail;

    void Start()
    {
        //Get the api component
        youtubeapi = GameObject.FindObjectOfType<YoutubeAPIManager>();
        if (youtubeapi == null)
        {
            youtubeapi = gameObject.AddComponent<YoutubeAPIManager>();
        }
    }

    public void GetVideoData()
    {
        youtubeapi.GetVideoData(videoIdInput.text, OnFinishLoadingData);
    }

    void OnFinishLoadingData(YoutubeData result)
    {
        UI_title.text = result.snippet.title;
        UI_description.text = result.snippet.description;
        UI_duration.text = "Duration: "+result.contentDetails.duration.Replace("PT", "");
        UI_likes.text = "Likes: " + result.statistics.likeCount;
        UI_dislikes.text = "Dislikes: " + result.statistics.dislikeCount;
        UI_favorites.text = "Favs: " + result.statistics.favoriteCount;
        UI_comments.text = "Comments: " + result.statistics.commentCount;
        UI_views.text = "Views: " + result.statistics.viewCount;
        LoadThumbnail(result.snippet.thumbnails.defaultThumbnail.url);
    }

    void LoadThumbnail(string url)
    {
        StartCoroutine(DownloadThumb(url));
    }

    IEnumerator DownloadThumb(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        //request.SetRequestHeader("User-Agent", USER_AGENT);
        yield return request.SendWebRequest();
        
        Texture2D thumb = DownloadHandlerTexture.GetContent(request);
        UI_thumbnail.sprite = Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), new Vector2(0.5f, 0.5f), 100);
    }
}
