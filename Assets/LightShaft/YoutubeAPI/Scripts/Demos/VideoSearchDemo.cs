using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSearchDemo : MonoBehaviour {
    YoutubeAPIManager youtubeapi;

    public Text searchField;
    public Text categoryField;
    public Toggle categoryFilter;
    public Dropdown mainFilters;
    public YoutubeVideoUi[] videoListUI;
    public GameObject videoUIResult;
    public GameObject mainUI;

    void Start () {
        //Get the api component
        youtubeapi = GameObject.FindObjectOfType<YoutubeAPIManager>();
        if (youtubeapi == null)
        {
            youtubeapi = gameObject.AddComponent<YoutubeAPIManager>();
        }
	}

    public string customFilter;

    private void CustomFilterCheck()
    {
        //customFilter = "";
        if (customFilter == "" || customFilter == null)
        {
            customFilter = "";
        }
        else
        {
            customFilter = "&" + customFilter;
            Debug.Log(customFilter);
        }
    }
    public string regionCode;
    public void GetTrendingVideos()
    {
        youtubeapi.TrendingVideos(regionCode, 10, OnSearchDone);
    }

	public void Search()
    {
        CustomFilterCheck();
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

        if (categoryFilter.isOn)
            youtubeapi.SearchByCategory(searchField.text,categoryField.text, 10, mainFilter, YoutubeAPIManager.YoutubeSafeSearchFilter.none, OnSearchDone);
        else
            youtubeapi.Search(searchField.text, 10, mainFilter, YoutubeAPIManager.YoutubeSafeSearchFilter.none, customFilter, OnSearchDone);
    }

    public void SearchByLocation(string location)
    {
        CustomFilterCheck();
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
        string[] splited = location.Split(',');
        float latitude = float.Parse(splited[0]);
        float longitude = float.Parse(splited[1]);
        int locationRadius = 10;
        youtubeapi.SearchByLocation(searchField.text, 10, locationRadius, latitude, longitude, mainFilter, YoutubeAPIManager.YoutubeSafeSearchFilter.none, customFilter, OnSearchDone);
    }

    void OnSearchDone(YoutubeData[] results)
    {
        videoUIResult.SetActive(true);
        LoadVideosOnUI(results);
    }

    void LoadVideosOnUI(YoutubeData[] videoList)
    {
        for (int x = 0; x < videoList.Length; x++)
        {
            Debug.Log(x);
            videoListUI[x].GetComponent<YoutubeVideoUi>().videoName.text = videoList[x].snippet.title;
            videoListUI[x].GetComponent<YoutubeVideoUi>().videoId = videoList[x].id;
            videoListUI[x].GetComponent<YoutubeVideoUi>().thumbUrl = videoList[x].snippet.thumbnails.defaultThumbnail.url;
            videoListUI[x].GetComponent<YoutubeVideoUi>().LoadThumbnail();
        }
    }
}
