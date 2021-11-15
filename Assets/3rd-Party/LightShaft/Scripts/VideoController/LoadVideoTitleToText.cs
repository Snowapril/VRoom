using System.Collections;
using System.Collections.Generic;
using LightShaft.Scripts;
using UnityEngine;
using UnityEngine.Video;

public class LoadVideoTitleToText : MonoBehaviour {

    public TextMesh textMesh;
    public YoutubePlayer player;

    public void SetVideoTitle()
    {
        textMesh.text = player.GetVideoTitle();
    }

}
