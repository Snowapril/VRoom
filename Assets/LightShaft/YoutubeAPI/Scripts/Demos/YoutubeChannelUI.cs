using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class YoutubeChannelUI : MonoBehaviour {

    public Text videoName;
    public string videoId, thumbUrl;
    public Image videoThumb;

    public void LoadChannel()
    {
        GameObject.FindObjectOfType<ChannelSearchDemo>().LoadChannelResult(videoId);
    }

    public void LoadThumbnail()
    {
        StartCoroutine(DownloadThumb());
    }

    IEnumerator DownloadThumb()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(thumbUrl);
        //request.SetRequestHeader("User-Agent", USER_AGENT);
        yield return request.SendWebRequest();
        Texture2D thumb = DownloadHandlerTexture.GetContent(request);
        videoThumb.sprite = Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), new Vector2(0.5f, 0.5f), 100);
    }

}
