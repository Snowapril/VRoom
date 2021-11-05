using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
using System.Collections;
using LightShaft.Scripts;

public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public YoutubePlayer player;


    public void OnDrag(PointerEventData eventData)
    {
        player.TrySkip(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        player.TrySkip(eventData);
    }

}