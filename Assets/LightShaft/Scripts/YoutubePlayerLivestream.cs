using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using YoutubeLight;

public class YoutubePlayerLivestream : MonoBehaviour {

    public string _livestreamUrl;

	void Start () {
        GetLivestreamUrl(_livestreamUrl);
    }

    public void GetLivestreamUrl(string url)
    {
        StartProcess(OnLiveUrlLoaded, url);
    }

    public void StartProcess(System.Action<string> callback, string url)
    {
        StartCoroutine(DownloadYoutubeUrl(url, callback));
    }

    //this function will be called when the url is ready to use in the HLS player
    void OnLiveUrlLoaded(string url)
    {
        //Dont know how to use
        //Some examples: I recommend you to put that script in the same object that the player script that you are using.
        //If you are using some of that players you can uncomment the player part.

        //AVPRO Part
        //MediaPlayer mplayer = GetComponent<MediaPlayer>();
        //mplayer.m_VideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
        //mplayer.m_VideoPath = url;
        //mplayer.OpenVideoFromFile(mplayer.m_VideoLocation, mplayer.m_VideoPath, mplayer.m_AutoStart);

        //Easy Movie Texture (Good for mobile only[sometimes stuck in editor])
        //MediaPlayerCtrl easyPlayer = GetComponent<MediaPlayerCtrl>();
        //easyPlayer.m_strFileName = url;
        //easyPlayer.Play();

        //MPMP
        //MPMP mpPlayer = GetComponent<MPMP>();
        //mpPlayer.videoPath = url;
        //mpPlayer.Load();
        //mpPlayer.Play();

        Debug.Log("You can check how to use double clicking in that log");
        Debug.Log("This is the live url, pass to the player: " + url);
    }

    IEnumerator DownloadYoutubeUrl(string url, System.Action<string> callback)
    {
        downloadYoutubeUrlResponse = new DownloadUrlResponse();
        var videoId = url.Replace("https://youtube.com/watch?v=", "");

        var newUrl = "https://www.youtube.com/watch?v=" + videoId + "&gl=US&hl=en&has_verified=1&bpctr=9999999999";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20100101 Firefox/10.0 (Chrome)");
        yield return request.SendWebRequest();
        downloadYoutubeUrlResponse.httpCode = request.responseCode;
        if (request.isNetworkError) { Debug.Log("Youtube UnityWebRequest isNetworkError!"); }
        else if (request.isHttpError) { Debug.Log("Youtube UnityWebRequest isHttpError!"); }
        else if (request.responseCode == 200)
        {

            //Debug.Log("Youtube UnityWebRequest responseCode 200: OK!");
            if (request.downloadHandler != null && request.downloadHandler.text != null)
            {
                if (request.downloadHandler.isDone)
                {
                    downloadYoutubeUrlResponse.isValid = true;
                    downloadYoutubeUrlResponse.data = request.downloadHandler.text;
                }
            }
            else { Debug.Log("Youtube UnityWebRequest Null response"); }
        }
        else
        { Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode); }

        StartCoroutine(GetUrlFromJson(callback, videoId, request.downloadHandler.text));
    }

    IEnumerator GetUrlFromJson(System.Action<string> callback, string _videoID, string pageSource)
    {
        //var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);
        //string extractedJson = dataRegex.Match(downloadYoutubeUrlResponse.data).Result("$1");

        var videoId = _videoID;
        videoId = videoId.Replace("https://www.youtube.com/watch?v=", "");
        videoId = videoId.Replace("https://youtube.com/watch?v=", "");
        //jsonforHtml
        var player_response = string.Empty;
        bool tempfix = false;
        
        if (Regex.IsMatch(pageSource, @"[""\']status[""\']\s*:\s*[""\']LOGIN_REQUIRED") || tempfix)
        {
            var url = "https://www.docs.google.com/get_video_info?video_id=" + videoId + "&eurl=https://youtube.googleapis.com/v/" + videoId + "&html5=1&c=TVHTML5&cver=6.20180913";
            Debug.Log(url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("User-Agent", pageSource);
            yield return request.SendWebRequest();
            if (request.isNetworkError) { Debug.Log("Youtube UnityWebRequest isNetworkError!"); }
            else if (request.isHttpError) { Debug.Log("Youtube UnityWebRequest isHttpError!"); }
            else if (request.responseCode == 200)
            {
                //ok;
            }
            else
            { Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode); }
            Debug.Log(request.downloadHandler.text);
            player_response = UnityWebRequest.UnEscapeURL(HTTPHelperYoutube.ParseQueryString(request.downloadHandler.text)["player_response"]);
        }
        else
        {
            var dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
            var dataMatch = dataRegexOption.Match(pageSource);
            if (dataMatch.Success)
            {
                string extractedJson = dataMatch.Result("$1");
                if (!extractedJson.Contains("raw_player_response:ytInitialPlayerResponse"))
                {
                    //Debug.Log(extractedJson);
                    player_response = JObject.Parse(extractedJson).ToString();
                    //player_response = JObject.Parse(extractedJson)["args"]["player_response"].ToString();
                }
            }
    
            dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
            dataMatch = dataRegexOption.Match(pageSource);
            if (dataMatch.Success)
            {
                player_response = dataMatch.Result("$1");
            }
    
            dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
            dataMatch = dataRegexOption.Match(pageSource);
            if (dataMatch.Success)
            {
                player_response = dataMatch.Result("$1");
            }
    
            dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;", RegexOptions.Multiline);
            dataMatch = dataRegexOption.Match(pageSource);
            if (dataMatch.Success)
            {
                player_response = dataMatch.Result("$1");
            }
        }

        JObject json = JObject.Parse(player_response);
        //string playerResponseRaw = json["args"]["player_response"].ToString();
        //JObject playerResponseJson = JObject.Parse(playerResponseRaw);
        bool isLive = json["videoDetails"]["isLive"].Value<bool>();

        if (isLive)
        {
            //WriteLog("kelvin", player_response);
            string liveUrl = json["streamingData"]["hlsManifestUrl"].ToString();
            callback.Invoke(liveUrl);
        }
        else
        {
            Debug.Log("This is not a livestream url");
        }


        
    }

    public static void WriteLog(string filename, string c)
    {
        string filePath = Application.persistentDataPath + "/" + filename + "_" + DateTime.Now.ToString("ddMMyyyyhhmmssffff") + ".txt";
        Debug.Log("Log written in: " + Application.persistentDataPath);
        //Debug.Log("DownloadUrl content saved to " + filePath);
        File.WriteAllText(filePath, c);
    }



    private class DownloadUrlResponse
    {
        public string data = null;
        public bool isValid = false;
        public long httpCode = 0;
        public DownloadUrlResponse() { data = null; isValid = false; httpCode = 0; }
    }
    private DownloadUrlResponse downloadYoutubeUrlResponse;
}
