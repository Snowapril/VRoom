using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChannelSearchDemo : MonoBehaviour {
    YoutubeAPIManager youtubeapi;

    public Text searchField;
    public Dropdown mainFilters;
    public GameObject[] channelListUI;
    public GameObject[] videoListUI;
    public GameObject videoUIResult;
    public GameObject channelUIResult;
    public GameObject mainUI;

    void Start()
    {
        //Get the api component
        youtubeapi = GameObject.FindObjectOfType<YoutubeAPIManager>();
        if (youtubeapi == null)
        {
            youtubeapi = gameObject.AddComponent<YoutubeAPIManager>();
        }
    }

    public void SearchChannel()
    {

        YoutubeAPIManager.YoutubeSearchOrderFilter mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.none;
        switch (mainFilters.value)
        {
            case 0:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.none;
                break;
            case 1:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.date;
                break;
            case 2:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.rating;
                break;
            case 3:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.relevance;
                break;
            case 4:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.title;
                break;
            case 5:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.videoCount;
                break;
            case 6:
                mainFilter = YoutubeAPIManager.YoutubeSearchOrderFilter.viewCount;
                break;

        }
        youtubeapi.SearchForChannels(searchField.text, 10, mainFilter, YoutubeAPIManager.YoutubeSafeSearchFilter.none, OnSearchDone);
    }


    void OnSearchDone(YoutubeChannel[] results)
    {
        channelUIResult.SetActive(true);
        LoadChannelsOnUI(results);
    }

    void LoadChannelsOnUI(YoutubeChannel[] videoList)
    {
        for (int x = 0; x < videoList.Length; x++)
        {
            //Debug.Log(videoList[x].thumbnail);
            channelListUI[x].GetComponent<YoutubeChannelUI>().videoName.text = videoList[x].title;
            channelListUI[x].GetComponent<YoutubeChannelUI>().videoId = videoList[x].id;
            channelListUI[x].GetComponent<YoutubeChannelUI>().thumbUrl = videoList[x].thumbnail;
            channelListUI[x].GetComponent<YoutubeChannelUI>().LoadThumbnail();
        }
    }

    public void LoadChannelResult(string channelId)
    {
        youtubeapi.GetChannelVideos(channelId, 10, OnChannelResultLoaded);
    }

    void OnChannelResultLoaded(YoutubeData[] videoList)
    {
        channelUIResult.SetActive(false);
        videoUIResult.SetActive(true);
        for (int x = 0; x < videoList.Length; x++)
        {
            videoListUI[x].GetComponent<YoutubeVideoUi>().videoName.text = videoList[x].snippet.title;
            videoListUI[x].GetComponent<YoutubeVideoUi>().videoId = videoList[x].id;
            videoListUI[x].GetComponent<YoutubeVideoUi>().thumbUrl = videoList[x].snippet.thumbnails.defaultThumbnail.url;
            videoListUI[x].GetComponent<YoutubeVideoUi>().LoadThumbnail();
        }
    }
}
