using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YoutubeLogo : MonoBehaviour {

    public string youtubeurl;

    private void OnMouseDown()
    {
        Application.OpenURL(youtubeurl);
    }

}
