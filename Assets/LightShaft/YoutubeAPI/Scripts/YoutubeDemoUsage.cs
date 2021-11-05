using System.Collections;
using System.Collections.Generic;
using LightShaft.Scripts;
using UnityEngine;
using UnityEngine.Video;

public class YoutubeDemoUsage : MonoBehaviour {
    public GameObject mainUI;

	public void DemoPlayback()
    {
        //search for the low quality if not find search for highquality
        if (GameObject.FindObjectOfType<YoutubePlayer>() != null)
        {
            GameObject.FindObjectOfType<YoutubePlayer>().Play("bc0sJvtKrRM");
            GameObject.FindObjectOfType<YoutubePlayer>().videoPlayer.loopPointReached += OnVideoFinished;
        }
        mainUI.SetActive(false);
    }

    public UnityEngine.UI.Text videoUrlInput;

    public void PlayFromInput()
    {
        //search for the low quality if not find search for highquality
        if (GameObject.FindObjectOfType<YoutubePlayer>() != null)
        {
            GameObject.FindObjectOfType<YoutubePlayer>().Play(videoUrlInput.text);
            GameObject.FindObjectOfType<YoutubePlayer>().videoPlayer.loopPointReached += OnVideoFinished;
        }
       
        mainUI.SetActive(false);
    }

    private void OnVideoFinished(VideoPlayer vPlayer)
    {
        if (GameObject.FindObjectOfType<YoutubePlayer>() != null)
        {
            GameObject.FindObjectOfType<YoutubePlayer>().videoPlayer.loopPointReached -= OnVideoFinished;
        }
        mainUI.SetActive(true);
    }
}
