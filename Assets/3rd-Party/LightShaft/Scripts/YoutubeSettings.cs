using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using SimpleJSON;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using YoutubeLight;

namespace LightShaft.Scripts
{
    [RequireComponent(typeof(YoutubeVideoController))]
    [RequireComponent(typeof(YoutubeVideoEvents))]
    public class YoutubeSettings : MonoBehaviour
    {
        protected string UserAgent = "Mozilla/5.0 (Linux; Android 8.0.0; SM-G960F Build/R16NW) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.84 Mobile Safari/537.36";

        #region ENUMS
        public enum YoutubeVideoQuality
        {
            STANDARD,
            HD,
            FULLHD,
            UHD1440,
            UHD2160
        }

        public enum VideoFormatType
        {
            MP4,
            WEBM
        }
        
        public enum Layout3D
        {
            SideBySide,
            OverUnder,
            EAC
        }
        #endregion

        [HideInInspector]
        public YoutubeVideoController _controller;
        [HideInInspector]
        public YoutubeVideoEvents _events;
        
        [Space]
        [Tooltip("You can put urls that start at a specific time example: 'https://youtu.be/1G1nCxxQMnA?t=67'")]
        public string youtubeUrl;

        [Space]
        [Space]
        [Tooltip("The desired video quality you want to play. It's in experimental mod, because we need to use 2 video players in qualities 720+, you can expect some desync, but we are working to find a definitive solution to that. Thanks to DASH format.")]
        public YoutubeVideoQuality videoQuality;

        [Space] [Tooltip("If it is a 360 degree video")]
        public bool is360;
    
        [Space]
        [Header("Playback Options")]
        [Space]
        [Tooltip("Play the video when the script initialize")]
        public bool autoPlayOnStart = true;
        [Space]
        [Tooltip("Start playing the video from a desired time")]
        public bool startFromSecond = false;
        [DrawIf("startFromSecond", true)]
        public int startFromSecondTime = 0;

        protected bool prepareVideoToPlayLater = false;
        [Space]
        public bool showThumbnailBeforeVideoLoad = false;
        [DrawIf("showThumbnailBeforeVideoLoad", true)]
        public Renderer thumbnailObject;
        protected string thumbnailVideoID;

        [Space]
        public bool customPlaylist = false;
        [DrawIf("customPlaylist", true)]
        public bool autoPlayNextVideo;

        [Header("If is a custom playlist put urls here")]
        public string[] youtubeUrls;

        protected int currentUrlIndex = 0;
        private string youtubeVideoID;


        [Header("You can Try different formats")]
        public VideoFormatType videoFormat;

        [Space]
        [Header("Start Load and Play Url On enable this gameobject")]
        [Tooltip("Play or continue when OnEnable is called")]
        public bool autoPlayOnEnable = false;

        [Space]
        [Header("Use Device Video player (Standard quality only)")]
        [Tooltip("Play video in mobiles using the mobile device video player not unity internal player")]
        public bool playUsingInternalDevicePlayer = false;

        [Space]
        [Header("Only load the url to use in a custom player.")]
        [Space]
        [Tooltip("If you want to use your custom player, you can enable this and set the callback OnYoutubeUrlLoaded to your custom function sending the loaded url.")]
        public bool loadYoutubeUrlsOnly = false;

        [Space]
        [Header("Render the same video to more objects")]
        [Tooltip("Render the same video player material to a different materials, if you want")]
        public GameObject[] objectsToRenderTheVideoImage;

        [Space]
        [Header("Option for 3D video Only.")]
        [Tooltip("If the video is a 3D video sidebyside or Over/Under")]
        public bool is3DLayoutVideo = false;

        [DrawIf("is3DLayoutVideo", true)]
        public Layout3D layout3d;

        [Space]
        public Camera mainCamera;

        [Space]
        [Header("The unity video players")]
        [Tooltip("The unity video player")]
        public VideoPlayer videoPlayer;

        [Tooltip("The audio player, (Needed for videos that dont have audio included 720p+)")]
        public VideoPlayer audioPlayer;

        [Space]
        [Tooltip("Show the output in the console")]
        public bool debug;

        [Space]
        [Tooltip("Ignore timeout is good for very low connections")]
        public bool ignoreTimeout = false;

        [Header("If you are having issues with sync try check this and change video format to WEBM")]
        public bool _skipOnDrop = false;


        //Youtube formated urls
        [HideInInspector]
        public string videoUrl;
        [HideInInspector]
        public string audioUrl;

        [HideInInspector] //deprecated.
        public bool ForceGetWebServer = false;
        
        [HideInInspector]
        public bool progressStartDrag;

        //Request from youtube url timeout
        private int maxRequestTime = 5;
        private float currentRequestTime;
        //When the video fails how much time we will try until try to get from the webserver system.
        private int retryTimeUntilToRequestFromServer = 1;
        private int currentRetryTime = 0;

        //Check when we are trying to get the url
        private bool gettingYoutubeURL = false;

        //When the urls are done and the video are ready to start playing
        private bool videoAreReadyToPlay = false;


        private float lastPlayTime;

        //When a video needs decryption, most common in music videos
        private bool audioDecryptDone = false;
        private bool videoDecryptDone = false;


        //Video ready checkers experimental
        //private bool videoPrepared;
        //private bool audioPrepared;

        //Retry checker
        private bool isRetry = false;

        private string lastTryVideoId;

        private float lastStartedTime;
        protected bool youtubeUrlReady = false;

        protected bool fullscreenModeEnabled = false;

        #region SERVER VARIABLES

        private YoutubeResultIds newRequestResults;
        private static string jsUrl;

        /*PRIVATE INFO DO NOT CHANGE THESE URLS OR VALUES, ONLY IF YOU WANT HOST YOUR OWN SERVER| TURORIALS IN THE PROJECT FILES*/
        private const string serverURI = "https://lightshaftstream.herokuapp.com/api/info?url=";
        private const string formatURI = "&format=best&flatten=true";
        private const string VIDEOURIFORWEBGLPLAYER = "https://youtubewebgl.herokuapp.com/download.php?mime=video/mp4&title=generatedvideo&token=";
        /*END OF PRIVATE INFO*/

        #endregion



        #region BIG CODE HERE - I will redo this in the future
        //Setup the skybox for 3D or VR videos
        protected void Skybox3DSettup()
        {
            if (is3DLayoutVideo)
            {
                if (layout3d == Layout3D.OverUnder)
                {
                    RenderSettings.skybox = (Material)Resources.Load("Materials/PanoramicSkybox3DOverUnder") as Material;
                }
                else if (layout3d == Layout3D.SideBySide)
                {
                    RenderSettings.skybox = (Material)Resources.Load("Materials/PanoramicSkybox3Dside") as Material;
                }else if (layout3d == Layout3D.EAC)
                {
                    RenderSettings.skybox = (Material)Resources.Load("Materials/PanoramicSkyboxEAC") as Material;
                }
            }
        }

        public void YoutubeGeneratorSys(string _videoUrl, string _formatCode)
        {
            StartCoroutine(YoutubeGenerateUrlUsingClient());
            //StartCoroutine(YoutubeGeneratorSysCall(_videoUrl, _formatCode)); //TODO add using webserver.
        }

        IEnumerator YoutubeGenerateUrlUsingClient()
        {
            CheckVideoUrlAndExtractThevideoId(youtubeUrl);
            WWWForm form = new WWWForm();
            string f = "{\"context\": {\"client\": {\"clientName\": \"ANDROID\",\"clientVersion\": \"16.20\",\"hl\": \"en\"}},\"videoId\": \""+youtubeVideoID+"\",}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(f);
            UnityWebRequest request = UnityWebRequest.Post("https://www.youtube.com/youtubei/v1/player?key=AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8", form);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type","application/json");
            //request.SetRequestHeader("Origin","https://www.youtube.com");
            request.SetRequestHeader("X-YouTube-Client-Name","3");
            request.SetRequestHeader("X-YouTube-Client-Version","16.20");
            yield return request.SendWebRequest();
            if (request.error != null)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(JObject.Parse(request.downloadHandler.text));
                youtubeVideoInfos = GetVideoInfos(downloadUrls, videoTitle).ToList();
                UrlsLoaded();   //call direct to extract the video infos.
            }
        }

        IEnumerator YoutubeGeneratorSysCall(string _videoUrl, string _formatCode)
        {
            UnityWebRequest request = UnityWebRequest.Get("https://lightshaftstream.herokuapp.com/api/utubePlay?url="+_videoUrl+"&format=best&flatten=true&formatId="+_formatCode);
            yield return request.SendWebRequest();
            gettingYoutubeURL = false;
            if (request.isHttpError)
            {
                Debug.Log(request.error);
                StartCoroutine(WaitAndTryConnectToServerAgain());
            }
            else
            {
                var requestedData = JSON.Parse(request.downloadHandler.text);
                videoTitle = requestedData["title"]["title"];
                audioUrl = requestedData["audio"]["url"];
                videoUrl = requestedData["video"]["url"];
                if (string.IsNullOrEmpty(videoUrl))
                {
                    videoUrl = audioUrl;
                    videoQuality = YoutubeVideoQuality.STANDARD;
                }
                OnYoutubeUrlsLoaded();
            }
        }

        IEnumerator WaitAndTryConnectToServerAgain()
        {
            yield return new WaitForSeconds(2);
            RetryPlayYoutubeVideo();
        }

        protected void FixCameraEvent()
        {
            if (mainCamera == null)
            {
                if (Camera.main != null)
                    mainCamera = Camera.main;
                else
                {
                    mainCamera = GameObject.FindObjectOfType<Camera>();
                    Debug.Log("Add the main camera to the mainCamera field");
                }

            }
            
            if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane || videoPlayer.renderMode == VideoRenderMode.CameraNearPlane)
                videoPlayer.targetCamera = mainCamera;
        }

        protected string CheckVideoUrlAndExtractThevideoId(string url)
        {
            if (url.Contains("?t="))
            {
                int last = url.LastIndexOf("?t=");
                string copy = url;
                string newString = copy.Remove(0, last);
                newString = newString.Replace("?t=", "");
                startFromSecond = true;
                startFromSecondTime = int.Parse(newString);
                url = url.Remove(last);
            }

            /*if (!url.Contains("youtu"))
        {
            url = "youtube.com/watch?v=" + url;
        }*/

            bool isYoutubeUrl = TryNormalizeYoutubeUrlLocal(url, out url);
            if (!isYoutubeUrl)
            {
                url = "none";
                OnYoutubeError("Not a Youtube Url");
            }

            return url;
        }

        public void OnYoutubeError(string errorType)
        {
            Debug.Log("<color=red>" + errorType + "</color>");
        }

        private bool TryNormalizeYoutubeUrlLocal(string url, out string normalizedUrl)
        {
            url = url.Trim();
            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "https://youtube.com" + new System.Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");
            IDictionary<string, string> query = HTTPHelperYoutube.ParseQueryString(url);

            string v;


            if (!query.TryGetValue("v", out v))
            {
                normalizedUrl = null;
                return false;
            }

            youtubeVideoID = v;
            normalizedUrl = "https://youtube.com/watch?v=" + v;

            return true;
        }


        private void Awake()
        {
            
            if (!loadYoutubeUrlsOnly)
            {
                if (GetComponent<YoutubeVideoController>() == null)
                {
                    Debug.LogError("You need a VideoController attached to YoutubePlayer");
                    return;
                }
                if (GetComponent<YoutubeVideoEvents>() == null)
                {
                    Debug.LogError("You need a VidepoEvents attached to YoutubePlayer");
                    return;
                }
                _controller = GetComponent<YoutubeVideoController>();
                _events = GetComponent<YoutubeVideoEvents>();
            }
            
            
            magicResult = new MagicContent();
            
        
            if (!playUsingInternalDevicePlayer && !loadYoutubeUrlsOnly)
            {
                //if is 360
                if (is360)
                {
                    if (videoQuality == YoutubeVideoQuality.STANDARD) videoQuality = YoutubeVideoQuality.HD; //Does not play Standard for 360 degree videos.
                }
            
                if (videoQuality == YoutubeVideoQuality.STANDARD) //Disable the second video player to eco resource;
                {
                    if(videoFormat == VideoFormatType.WEBM) videoPlayer.skipOnDrop = _skipOnDrop;
                    if (audioPlayer != null)
                        audioPlayer.transform.gameObject.SetActive(false);
                }

                //Check if fullscreen Mode is active at start.
                if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane ||
                    videoPlayer.renderMode == VideoRenderMode.CameraNearPlane)
                    fullscreenModeEnabled = true;
                else fullscreenModeEnabled = false;
            }
        }

        long lastFrame = -1;

        void VerifyFrames()
        {
            if (!playUsingInternalDevicePlayer)
            {
                if (videoPlayer.isPlaying)
                {
                    if (lastFrame == videoPlayer.frame)
                    {
                        audioPlayer.Pause();
                        videoPlayer.Pause();
                        StartCoroutine(WaitSync());
                    }
                    lastFrame = videoPlayer.frame;
                    Invoke("VerifyFrames", 2);
                }
            }
        }

        public virtual void Start()
        {
            if (playUsingInternalDevicePlayer)
                loadYoutubeUrlsOnly = true;
#if UNITY_WEBGL
        videoQuality = YoutubeVideoQuality.STANDARD;
#endif

            if (!loadYoutubeUrlsOnly)
            {
                Invoke("VerifyFrames", 2);
                FixCameraEvent();
                Skybox3DSettup();

                //I used this in version 5.1 but some users don't like, you may enable if you want to test, this prevent the video to be out of sync sometimes, but there's a lot of lag in playback
                if (videoFormat == VideoFormatType.WEBM)
                {
                    videoPlayer.skipOnDrop = _skipOnDrop;
                    audioPlayer.skipOnDrop = _skipOnDrop;
                }

                audioPlayer.seekCompleted += AudioSeeked;
                videoPlayer.seekCompleted += VideoSeeked;

                //Experimental tests
                //videoPlayer.frameDropped += VideoPlayer_frameDropped;
                //audioPlayer.frameDropped += AudioPlayer_frameDropped;

#if UNITY_WEBGL
        ForceGetWebServer = true;
#endif
            }

            PrepareVideoPlayerCallbacks();

            if (autoPlayOnStart)
            {
                if (customPlaylist)
                {
                    PlayYoutubeVideo(youtubeUrls[currentUrlIndex]);
                }
                else
                {
                    PlayYoutubeVideo(youtubeUrl);
                }
            }

            //VideoController Area
            if (videoQuality == YoutubeVideoQuality.STANDARD)
                lowRes = true;
            else
                lowRes = false;

        }

        public void DisableThumbnailObject()
        {
            if(thumbnailObject != null)
                thumbnailObject.gameObject.SetActive(false);
        }

        public void EnableThumbnailObject()
        {
            if (thumbnailObject != null)
                thumbnailObject.gameObject.SetActive(true);
            else Debug.Log("Thumbnail object is null");
        }

        private void TryToLoadThumbnailBeforeOpenVideo(string id)
        {
            string tempId = id.Replace("https://youtube.com/watch?v=", "");
            StartCoroutine(DownloadThumbnail(tempId));
        }

        IEnumerator DownloadThumbnail(string videoId)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://img.youtube.com/vi/" + videoId + "/0.jpg");
            //request.SetRequestHeader("User-Agent", USER_AGENT);
            yield return request.SendWebRequest();
            EnableThumbnailObject();
            Texture2D thumb = DownloadHandlerTexture.GetContent(request);
            thumbnailObject.material.mainTexture = thumb;
        }

        //double lastTimePlayed = Mathf.Infinity;

        void FixedUpdate()
        {
            if (!loadYoutubeUrlsOnly)
            {
                 //buffering detect 
                 // if (videoPlayer.isPlaying && (Time.frameCount % (int)(videoPlayer.frameRate + 1)) == 0)
                 // {
                 //     if (lastTimePlayed == videoPlayer.time)//buffering
                 //     {
                 //         if (!prepareVideoToPlayLater)
                 //             ShowLoading();
                 //         Pause();
                 //         Debug.Log("Buffering");
                 //     }
                 //     else//not buffering
                 //     {
                 //         HideLoading();
                 //         //Debug.Log("Not buffering");
                 //     } lastTimePlayed = videoPlayer.time; 
                 // } 
                 if (!playUsingInternalDevicePlayer) 
                 {
                    if (videoPlayer.isPlaying)
                        HideLoading();
                    else
                    {
                        if (!pauseCalled && !prepareVideoToPlayLater)
                            ShowLoading();
                    }
                 }
            }


            if (!loadYoutubeUrlsOnly)
            {
                if (_controller.showPlayerControl)
                {
                    if (videoPlayer.isPlaying)
                    {
                        totalVideoDuration = Mathf.RoundToInt(videoPlayer.frameCount / videoPlayer.frameRate);
                        if (!lowRes)
                        {
                            audioDuration = Mathf.RoundToInt(audioPlayer.frameCount / audioPlayer.frameRate);
                            if (audioDuration < totalVideoDuration && (audioPlayer.url != ""))
                            {
                                currentVideoDuration = Mathf.RoundToInt(audioPlayer.frame / audioPlayer.frameRate);
                            }
                            else
                            {
                                currentVideoDuration = Mathf.RoundToInt(videoPlayer.frame / videoPlayer.frameRate);
                            }
                        }
                        else
                        {
                            currentVideoDuration = Mathf.RoundToInt(videoPlayer.frame / videoPlayer.frameRate);
                        }

                    }
                }

                if (videoPlayer.frameCount > 0)
                {
                    if (_controller != null && _controller.showPlayerControl)
                    {
                        if (_controller.useSliderToProgressVideo) //use slider
                        {
                            if(!progressStartDrag)
                                _controller.playbackSlider.value = (float)videoPlayer.time;
                        }
                        else //use rectangle sprite.
                        {
                            if (_controller.progressRectangle != null)
                                _controller.progressRectangle.fillAmount = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
                        }
                    }
                }
            }

            if (gettingYoutubeURL)
            {
                currentRequestTime += Time.deltaTime;
                if (currentRequestTime >= maxRequestTime)
                {
                    if (!ignoreTimeout)
                    {
                        gettingYoutubeURL = false;
                        if (debug)
                            Debug.Log("<color=blue>Max time reached, trying again!</color>");

                        RetryPlayYoutubeVideo();
                    }
                }
            }
            //Debug.Log(videoPlayer.time +" | "+audioPlayer.time+" "+ videoPlayer.clockTime + " | "+ audioPlayer.clockTime);
            //used this to play in main thread.
            if (videoAreReadyToPlay)
            {
                videoAreReadyToPlay = false;


            }

            ErrorCheck();

            if (!loadYoutubeUrlsOnly)
            {
                //Video controller area
                if (_controller.showPlayerControl)
                {
                    if (videoQuality != YoutubeVideoQuality.STANDARD)
                        lowRes = false;
                    else
                        lowRes = true;

                    if (_controller.currentTime != null && _controller.totalTime != null)
                    {
                        _controller.currentTime.text = FormatTime(Mathf.RoundToInt(currentVideoDuration));
                        if (!lowRes)
                        {
                            if (audioDuration < totalVideoDuration && (audioPlayer.url != ""))
                                _controller.totalTime.text = FormatTime(Mathf.RoundToInt(audioDuration));
                            else
                                _controller.totalTime.text = FormatTime(Mathf.RoundToInt(totalVideoDuration));
                        }
                        else
                        {
                            _controller.totalTime.text = FormatTime(Mathf.RoundToInt(totalVideoDuration));
                        }

                    }
                }

                if (!_controller.showPlayerControl)
                {
                    if (_controller.controllerMainUI != null)
                        _controller.controllerMainUI.SetActive(false);
                }
                else
                    _controller.controllerMainUI.SetActive(true);
                //End video controller area
            }


            if (decryptedUrlForAudio)
            {
                decryptedUrlForAudio = false;
                DecryptAudioDone(decryptedAudioUrlResult);
                decryptedUrlForVideo = true;
            }

            if (decryptedUrlForVideo)
            {
                decryptedUrlForVideo = false;
                DecryptVideoDone(decryptedVideoUrlResult);
            }

            if (!loadYoutubeUrlsOnly)
            {
                //webgl
                //if (videoPlayer.isPrepared)
                //{
                //    if (!startedPlayingWebgl)
                //    {
                //        logTest = "Play!";
                //        startedPlayingWebgl = true;
                //        StartCoroutine(WebGLPlay());
                //    }
                //}

                if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
                {
                    if (audioPlayer != null)
                    {
                        if (audioPlayer.isPrepared)
                        {
                            if (!videoStarted)
                            {
                                videoStarted = true;
                                VideoStarted(videoPlayer);
                            }
                        }
                    }
                    else
                    {
                        if (!videoStarted)
                        {
                            videoStarted = true;
                            VideoStarted(videoPlayer);
                        }
                    }

                }
            }

            if (!loadYoutubeUrlsOnly)
            {
                if (videoPlayer.frame != 0 && !videoEnded && videoPlayer.isPlaying)
                {
                    if (videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
                    {
                        videoEnded = true;
                        PlaybackDone(videoPlayer);
                    }
                }

                if (videoPlayer.isPrepared)
                {
                    if (videoQuality != YoutubeVideoQuality.STANDARD)
                    {
                        if (audioPlayer.isPrepared)
                        {
                            if (!startedPlayingWebgl)
                            {
                                startedPlayingWebgl = true;
                                StartPlayingWebgl();
                            }
                        }
                    }
                    else
                    {
                        if (!startedPlayingWebgl)
                        {
                            startedPlayingWebgl = true;
                            StartPlayingWebgl();
                        }
                    }
                }
            }
        }


        private void PrepareVideoPlayerCallbacks()
        {
            //videoPlayer.started += VideoStarted;
            videoPlayer.errorReceived += VideoErrorReceived;
            if (videoQuality != YoutubeVideoQuality.STANDARD)
                audioPlayer.errorReceived += VideoErrorReceived;
        }


        public void ShowLoading()
        {
            if (_controller.loading != null)
                _controller.loading.SetActive(true);
        }

        public void HideLoading()
        {
            if (_controller.loading != null)
                _controller.loading.SetActive(false);
        }


        private void ResetThings()
        {
            gettingYoutubeURL = false;
            progressStartDrag = false;

            videoAreReadyToPlay = false;


            audioDecryptDone = false;
            videoDecryptDone = false;


            //videoPrepared = false;
            //audioPrepared = false;

            isRetry = false;
            youtubeUrlReady = false;

            if (audioPlayer != null)
                audioPlayer.seekCompleted += AudioSeeked;
            videoPlayer.seekCompleted += VideoSeeked;
            videoPlayer.frameDropped += VideoPlayer_frameDropped;
            if (audioPlayer != null)
                audioPlayer.frameDropped += AudioPlayer_frameDropped;

            //notSeeking = false;
            waitAudioSeek = false;
            //seekUsingLowQuality = false;
        }


        private string GetFormatCode()
        {
            if (!is360)
            {
                switch (videoQuality)
                {
                    case YoutubeVideoQuality.STANDARD:
                        return "18";
                    case YoutubeVideoQuality.HD:
                        return videoFormat == VideoFormatType.MP4 ? "136" : "247";
                    case YoutubeVideoQuality.FULLHD:
                        return videoFormat == VideoFormatType.MP4 ? "137" : "248";
                    case YoutubeVideoQuality.UHD1440:
                        return videoFormat == VideoFormatType.MP4 ? "264" : "271";
                    case YoutubeVideoQuality.UHD2160:
                        return videoFormat == VideoFormatType.MP4 ? "266" : "313";
                }
            }
            else
            {
                switch (videoQuality)
                {
                    case YoutubeVideoQuality.STANDARD:
                        return "134";
                    case YoutubeVideoQuality.HD:
                        return videoFormat == VideoFormatType.MP4 ? "136" : "247";
                    case YoutubeVideoQuality.FULLHD:
                        return videoFormat == VideoFormatType.MP4 ? "137" : "248";
                    case YoutubeVideoQuality.UHD1440:
                        return videoFormat == VideoFormatType.MP4 ? "264" : "271";
                    case YoutubeVideoQuality.UHD2160:
                        return videoFormat == VideoFormatType.MP4 ? "266" : "313";
                }
                
            }
            return "18";
        }
        
        
        protected void PlayYoutubeVideo(string _videoId)
        {
            if (videoQuality == YoutubeVideoQuality.STANDARD)
                lowRes = true;
            else
                lowRes = false;
            ResetThings();
            _videoId = CheckVideoUrlAndExtractThevideoId(_videoId);
            if (_videoId == "none")
            {
                return;
            }
            else
            {
                //Thumbnail
                if (showThumbnailBeforeVideoLoad)
                    TryToLoadThumbnailBeforeOpenVideo(_videoId);
                youtubeUrlReady = false;
                //Show loading
                ShowLoading();

                youtubeUrl = _videoId;
                //loading for fist time, so it's not a retry
                isRetry = false;
                //store some variables to control
                lastTryVideoId = _videoId;
                lastPlayTime = Time.time;

#if UNITY_WEBGL
                StartCoroutine(WebGlRequest(youtubeUrl));
#else
                if (!ForceGetWebServer)
                {
                    currentRequestTime = 0;
                    gettingYoutubeURL = true;
                    //TODO fix this in the c# code then add the server as a option.
                    //GetDownloadUrls(UrlsLoaded, youtubeUrl, this);
                    YoutubeGeneratorSys(youtubeUrl, GetFormatCode());
                }
                else
                {
                    StartCoroutine(WebRequest(youtubeUrl));
                }
                    
#endif
            }
        }

        //When the audio decryption is done
        public void DecryptAudioDone(string url)
        {
            audioUrl = url;
            audioDecryptDone = true;

            if (videoDecryptDone)
            {
                if (string.IsNullOrEmpty(decryptedAudioUrlResult))
                {
                    RetryPlayYoutubeVideo();
                }
                else
                {
                    videoAreReadyToPlay = true;

                    OnYoutubeUrlsLoaded();
                }
            }

        }

        //When the Video decryption is done
        public void DecryptVideoDone(string url)
        {
            videoUrl = url;
            videoDecryptDone = true;

            if (audioDecryptDone)
            {
                if (string.IsNullOrEmpty(decryptedVideoUrlResult))
                {
                    RetryPlayYoutubeVideo();
                }
                else
                {
                    videoAreReadyToPlay = true;
                    OnYoutubeUrlsLoaded();
                }
            }
            else
            {
                if (videoQuality == YoutubeVideoQuality.STANDARD)
                {
                    if (string.IsNullOrEmpty(decryptedVideoUrlResult))
                    {
                        RetryPlayYoutubeVideo();
                    }
                    else
                    {
                        videoAreReadyToPlay = true;
                        OnYoutubeUrlsLoaded();
                    }
                }
            }


        }

        bool videoEnded = false;
        protected bool noAudioAtacched = false;
        protected string videoTitle = "";

        //The callback when the url's are loaded.
        private void UrlsLoaded()
        {
            gettingYoutubeURL = false;
            List<VideoInfo> videoInfos = youtubeVideoInfos;
            //foreach(VideoInfo v in videoInfos)
            //{
            //    Debug.Log(v.FormatCode + " " + v.Resolution + " " + v.VideoExtension + " " + v.VideoType + " " + v.Is3D);
            //}
            videoDecryptDone = false;
            audioDecryptDone = false;

            decryptedUrlForVideo = false;
            decryptedUrlForAudio = false;
            
            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                foreach (var info in videoInfos.Where(info => info.FormatCode == 18))
                {
                    if (info.RequiresDecryption)
                    {
                        //The string is the video url with audio
                        DecryptDownloadUrl(info.DownloadUrl, "", info.HtmlPlayerVersion, true);
                    }
                    else
                    {
                        videoUrl = info.DownloadUrl;
                        videoAreReadyToPlay = true;
                        OnYoutubeUrlsLoaded();
                    }
                    videoTitle = info.Title;
                }
            }
            else
            {
                bool needDecryption = false;
                string _temporaryAudio = "", _temporaryVideo = "", _tempHtmlPlayerVersion = "";
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                //Get the video with audio first
                videoInfos.Reverse();


                foreach (var info in videoInfos.Where(info => info.FormatCode == 18))
                {
                    if (info.RequiresDecryption)
                    {
                        needDecryption = true;

                        //The string is the video url with audio
                        _tempHtmlPlayerVersion = info.HtmlPlayerVersion;
                        _temporaryAudio = info.DownloadUrl;
                    }
                    else
                    {
                        _temporaryAudio = info.DownloadUrl;
                        audioUrl = info.DownloadUrl;
                    }
                    videoTitle = info.Title;
                    break;
                }

                //Then we will get the desired video quality.
                int quality = 360;
                switch (videoQuality)
                {
                    case YoutubeVideoQuality.STANDARD:
                        quality = 360;
                        break;
                    case YoutubeVideoQuality.HD:
                        quality = 720;
                        break;
                    case YoutubeVideoQuality.FULLHD:
                        quality = 1080;
                        break;
                    case YoutubeVideoQuality.UHD1440:
                        quality = 1440;
                        break;
                    case YoutubeVideoQuality.UHD2160:
                        quality = 2160;
                        break;
                }

                bool foundVideo = false;
                videoInfos.Reverse();
                //Get the high quality video


                foreach (VideoInfo info in videoInfos)
                {
                    VideoType t = (videoFormat == VideoFormatType.MP4) ? VideoType.Mp4 : VideoType.WebM;
                    if (info.VideoType == t && info.Resolution == (quality))
                    {
                        /*formats for 360 videos
                        360 = 134(mp4) 243(webm)
                        720 = 136(mp4) 247(webm)
                        1080 = 137(mp4) 248(webm)
                        1440 = 264(mp4) 271(webm)
                        2160 = 266(mp4) 313(webm)*/
                        if (is360)
                        {
                            bool found360 = false;
                            switch (info.Resolution)
                            {
                                case 720:
                                    if (t == VideoType.Mp4)
                                    {
                                        if (info.FormatCode == 136)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    else
                                    {
                                        if (info.FormatCode == 247)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    break;
                                case 1080:
                                    if (t == VideoType.Mp4)
                                    {
                                        if (info.FormatCode == 137)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    else
                                    {
                                        if (info.FormatCode == 248)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    break;
                                case 1440 : 
                                    if (t == VideoType.Mp4)
                                    {
                                        if (info.FormatCode == 264)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    else
                                    {
                                        if (info.FormatCode == 271)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    break;
                                case 2160:
                                    if (t == VideoType.Mp4)
                                    {
                                        if (info.FormatCode == 266)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    else
                                    {
                                        if (info.FormatCode == 313)
                                        {
                                            found360 = true;
                                        }
                                    }
                                    break;
                            }

                            if (found360)
                            {
                                if (info.RequiresDecryption)
                                {
                                    if (debug)
                                        Debug.Log("REQUIRE DECRYPTION!");
                                    logTest = "Decry";
                                    //The string is the video url
                                    needDecryption = true;
                                    _temporaryVideo = info.DownloadUrl;
                                }
                                else
                                {
                                    if(debug) Debug.Log(info.DownloadUrl);
                                    _temporaryVideo = info.DownloadUrl;
                                    videoUrl = info.DownloadUrl;
                                    videoAreReadyToPlay = true;
                                    OnYoutubeUrlsLoaded();
                                }
                                foundVideo = true;
                                break;
                            }
                        }
                        else
                        {
                            if (info.RequiresDecryption)
                            {
                                if (debug)
                                    Debug.Log("REQUIRE DECRYPTION!");
                                logTest = "Decry";
                                //The string is the video url
                                needDecryption = true;
                                _temporaryVideo = info.DownloadUrl;
                            }
                            else
                            {
                                if(debug) Debug.Log(info.DownloadUrl);
                                _temporaryVideo = info.DownloadUrl;
                                videoUrl = info.DownloadUrl;
                                videoAreReadyToPlay = true;
                                OnYoutubeUrlsLoaded();
                            }
                            foundVideo = true;

                            //if (info.AudioType != YoutubeLight.AudioType.Unknown)//there's no audio atacched.
                            //{
                            //    noAudioAtacched = true;
                            //    audioPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}
                            //else
                            //{
                            //    noAudioAtacched = false;
                            //    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}

                            break;
                        }
                        
                    }
                }

                //TRY TO GET WEBM
                if (!foundVideo && quality == 1440)
                {
                    foreach (VideoInfo info in videoInfos)
                    {
                        if (info.FormatCode == 271)
                        {
                            Debug.Log("FIXING!! " + info.Resolution + " | " + info.VideoType + " | " + info.FormatCode);
                            if (info.RequiresDecryption)
                            {
                                needDecryption = true;
                                //The string is the video url
                                _temporaryVideo = info.DownloadUrl;
                            }
                            else
                            {
                                _temporaryVideo = info.DownloadUrl;
                                videoUrl = info.DownloadUrl;
                                videoAreReadyToPlay = true;
                                OnYoutubeUrlsLoaded();
                            }
                            foundVideo = true;

                            //if (info.AudioType != YoutubeLight.AudioType.Unknown)//there's no audio atacched.
                            //{
                            //    noAudioAtacched = true;
                            //    audioPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}
                            //else
                            //{
                            //    noAudioAtacched = false;
                            //    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}

                            break;
                        }
                    }
                }


                if (!foundVideo && quality == 2160)
                {
                    foreach (VideoInfo info in videoInfos)
                    {
                        if (info.FormatCode == 313)
                        {
                            if (debug)
                                Debug.Log("Found but with unknow format in results, check to see if the video works normal.");
                            if (info.RequiresDecryption)
                            {
                                needDecryption = true;
                                //The string is the video url
                                _temporaryVideo = info.DownloadUrl;
                            }
                            else
                            {
                                _temporaryVideo = info.DownloadUrl;
                                videoUrl = info.DownloadUrl;
                                videoAreReadyToPlay = true;
                                OnYoutubeUrlsLoaded();
                            }
                            foundVideo = true;

                            //if (info.AudioType != YoutubeLight.AudioType.Unknown)//there's no audio atacched.
                            //{
                            //    noAudioAtacched = true;
                            //    audioPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}
                            //else
                            //{
                            //    noAudioAtacched = false;
                            //    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}

                            break;
                        }
                    }
                }

                //if desired quality not found try another lower quality.
                if (!foundVideo)
                {
                    if (debug)
                        Debug.Log("Desired quality not found, playing with low quality, check if the video id: " + youtubeUrl + " support that quality!");
                    foreach (VideoInfo info in videoInfos)
                    {
                        if (info.VideoType == VideoType.Mp4 && info.Resolution == (360))
                        {
                            if (info.RequiresDecryption)
                            {
                                videoQuality = YoutubeVideoQuality.STANDARD;
                                //The string is the video url
                                needDecryption = true;
                                _temporaryVideo = info.DownloadUrl;
                            }
                            else
                            {
                                _temporaryVideo = info.DownloadUrl;
                                videoUrl = info.DownloadUrl;
                                videoAreReadyToPlay = true;
                                OnYoutubeUrlsLoaded();
                            }
                            //if (info.AudioType != YoutubeLight.AudioType.Unknown)//there's no audio atacched.
                            //{
                            //    noAudioAtacched = true;
                            //    audioPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}
                            //else
                            //{
                            //    noAudioAtacched = false;
                            //    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            //}
                            break;
                        }
                    }
                }

                if (needDecryption)
                    DecryptDownloadUrl(_temporaryVideo, _temporaryAudio, _tempHtmlPlayerVersion, false);
            }
        }

        private void StartPlayingWebgl()
        {
            _events.OnVideoReadyToStart.Invoke();
           

            if (playUsingInternalDevicePlayer && Application.isMobilePlatform) //Works in mobiles only!!
            {
                //Play using the internal player of the device 
                StartCoroutine(HandHeldPlayback());
            }
            else
            {
                StartPlayback();
            }
        }

        IEnumerator HandHeldPlayback()
        {
#if UNITY_ANDROID || UNITY_IOS
        if (videoQuality == YoutubeVideoQuality.STANDARD)
            Handheld.PlayFullScreenMovie(videoUrl, Color.black, FullScreenMovieControlMode.Minimal, FullScreenMovieScalingMode.AspectFit); //Use only the video with audio integrated. Working to get a url with high quality and audio included.
        else
            Handheld.PlayFullScreenMovie(audioUrl, Color.black, FullScreenMovieControlMode.Minimal, FullScreenMovieScalingMode.AspectFit); //Use only the video with audio integrated. Working to get a url with high quality and audio included.
#else
            Debug.Log("This runs in mobile devices only!");
#endif
            yield return new WaitForSeconds(1f);
            PlaybackDone(videoPlayer);
        }

        public IEnumerator DelayPlay()
        {
            yield return new WaitForSeconds(.4f);
            audioPlayer.Play();
        }
        
        
        private void StartPlayback()
        {
            //Render to more materials
            if (objectsToRenderTheVideoImage.Length > 0)
            {
                foreach (GameObject obj in objectsToRenderTheVideoImage)
                {
                    obj.GetComponent<Renderer>().material.mainTexture = videoPlayer.texture;
                }
            }

            videoEnded = false;

            _events.OnVideoStarted.Invoke();

            if (videoQuality != YoutubeVideoQuality.STANDARD)
            {
                //if (!noAudioAtacched)
                //audioPlayer.Play();
            }

            //videoPlayer.Play();

            HideLoading();

            waitAudioSeek = true;

            if (videoQuality != YoutubeVideoQuality.STANDARD)
            {
                if (!noAudioAtacched)
                {
                    audioPlayer.Pause();
                    videoPlayer.Pause();
                    //audioPlayer.time = 1;
                    //videoPlayer.time = 0;
                }
            }

            if (!prepareVideoToPlayLater)
            {
                DisableThumbnailObject();
            }

            if (!prepareVideoToPlayLater)
            {
                _events.OnVideoStarted.Invoke();
                if (videoQuality != YoutubeVideoQuality.STANDARD)
                {
                    //audioPlayer.Play();
                    videoPlayer.Play();
                    StartCoroutine(DelayPlay());
                }
                else
                {
                    videoPlayer.Play();
                }
            }

            if (startFromSecond)
            {
                startedFromTime = true;
                if (videoQuality == YoutubeVideoQuality.STANDARD)
                {
                    //seekUsingLowQuality = true;
                    videoPlayer.time = startFromSecondTime;
                }
                else
                {
                    audioPlayer.time = startFromSecondTime;
                }
            }
            
            if (videoQuality != YoutubeVideoQuality.STANDARD)
            {
                //disabled for now, is as test
                // if (checkIfSync)
                // {
                //     CancelInvoke("CheckIfIsSync");
                //     InvokeRepeating("CheckIfIsSync", 1, 5);
                // }

            }
        }

        protected bool startedFromTime = false;

        private void ErrorCheck()
        {
            if (!ForceGetWebServer)
            {
                if (!isRetry && lastStartedTime < lastErrorTime && lastErrorTime > lastPlayTime)
                {
                    if (debug)
                        Debug.Log("Error detected!, retry with low quality!");
                    isRetry = true;
                }
            }
        }

        //It's not in use, maybe can be usefull to you, it's just a test.
        public int GetMaxQualitySupportedByDevice()
        {
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                //use the height
                return Screen.currentResolution.height;
            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
                //use the width
                return Screen.currentResolution.width;
            }
            else
            {
                return Screen.currentResolution.height;
            }
        }

        IEnumerator WebRequest(string videoID)
        {
            UnityWebRequest request = UnityWebRequest.Get(serverURI + "" + videoID + "" + formatURI);
            yield return request.SendWebRequest();
            newRequestResults = new YoutubeResultIds();
            var requestData = JSON.Parse(request.downloadHandler.text);
            var videos = requestData["videos"][0]["formats"];
            Debug.Log(request.downloadHandler.text);
            newRequestResults.bestFormatWithAudioIncluded = requestData["videos"][0]["url"];
            for (int counter = 0; counter < videos.Count; counter++)
            {
                if (videos[counter]["format_id"] == "160")
                {
                    newRequestResults.lowQuality = videos[counter]["url"];
                }
                else if (videos[counter]["format_id"] == "133")
                {
                    newRequestResults.lowQuality = videos[counter]["url"];   //if have 240p quality overwrite the 144 quality as low quality.
                }
                else if (videos[counter]["format_id"] == "134")
                {
                    newRequestResults.standardQuality = videos[counter]["url"];  //360p
                }
                else if (videos[counter]["format_id"] == "136")
                {
                    newRequestResults.hdQuality = newRequestResults.bestFormatWithAudioIncluded;  //720p
                }
                else if (videos[counter]["format_id"] == "137")
                {
                    newRequestResults.fullHdQuality = videos[counter]["url"];  //1080p
                }
                else if (videos[counter]["format_id"] == "266")
                {
                    newRequestResults.ultraHdQuality = videos[counter]["url"];  //@2160p 4k
                }
                else if (videos[counter]["format_id"] == "139")
                {
                    newRequestResults.audioUrl = videos[counter]["url"];  //AUDIO
                }
            }


            audioUrl = newRequestResults.bestFormatWithAudioIncluded;
            videoUrl = newRequestResults.bestFormatWithAudioIncluded;

            switch (videoQuality)
            {
                case YoutubeVideoQuality.STANDARD:
                    videoUrl = newRequestResults.bestFormatWithAudioIncluded;
                    break;
                case YoutubeVideoQuality.HD:
                    videoUrl = newRequestResults.hdQuality;
                    break;
                case YoutubeVideoQuality.FULLHD:
                    videoUrl = newRequestResults.fullHdQuality;
                    break;
                case YoutubeVideoQuality.UHD1440:
                    videoUrl = newRequestResults.fullHdQuality;
                    break;
                case YoutubeVideoQuality.UHD2160:
                    videoUrl = newRequestResults.ultraHdQuality;
                    break;
            }

            if (videoUrl == "")
                videoUrl = newRequestResults.bestFormatWithAudioIncluded;

#if UNITY_WEBGL
        videoUrl = ConvertToWebglUrl(videoUrl);
#endif
            videoAreReadyToPlay = true;
            OnYoutubeUrlsLoaded();
        }

        private string ConvertToWebglUrl(string url)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            if (debug)
                Debug.Log(url);
            string newUrl = VIDEOURIFORWEBGLPLAYER + "" + encodedText;
            return newUrl;
        }

        private void RetryPlayYoutubeVideo()
        {
            Stop();
            currentRetryTime++;
            logTest = "Retry!!";
            if (currentRetryTime < retryTimeUntilToRequestFromServer)
            {
                if (!ForceGetWebServer)
                {
                    StopIfPlaying();
                    if (debug)
                        Debug.Log("Youtube Retrying...:" + lastTryVideoId);
                    logTest = "retry";
                    isRetry = true;
                    ShowLoading();
                    youtubeUrl = lastTryVideoId;
                    PlayYoutubeVideo(youtubeUrl);
                }
            }
            else
            {
                currentRetryTime = 0;
                StopIfPlaying();
                if (debug)
                    Debug.Log("Youtube Retrying...:" + lastTryVideoId);
                logTest = "retry";
                isRetry = true;
                ShowLoading();
                youtubeUrl = lastTryVideoId;
                PlayYoutubeVideo(youtubeUrl);
            }

        }

        private void StopIfPlaying()
        {
            if (!loadYoutubeUrlsOnly)
            {
                if (debug)
                    Debug.Log("Stopping video");
                if (videoPlayer.isPlaying) { videoPlayer.Stop(); }
                if (audioPlayer.isPlaying) { audioPlayer.Stop(); }
            }
        }

        private void OnYoutubeUrlsLoaded()
        {
            youtubeUrlReady = true;
            if (!loadYoutubeUrlsOnly) //If want to load urls only the video will not play
            {
                // var url = new Uri(videoUrl);
                // videoUrl = videoUrl.Replace(url.Host, "redirector.googlevideo.com");
                // if (videoQuality != YoutubeVideoQuality.STANDARD)
                // {
                //     url = new Uri(audioUrl);
                //     audioUrl = audioUrl.Replace(url.Host, "redirector.googlevideo.com");
                // }

                if (debug)
                    Debug.Log("Url Generated to play!!" + videoUrl);
                startedPlayingWebgl = false;

                //LoadPrepareCallbacks();
                // IDictionary<string, string> videoQuery = HTTPHelperYoutube.ParseQueryString(videoUrl);
                // if (videoQuery.ContainsKey("n"))
                // {
                //     //TODO Create a system to work with the nparam to prevent future errors.
                //         Debug.Log("has n param");
                //     string nparam = videoQuery["n"];
                //         Debug.Log(nparam);
                // }
                // else
                // {
                //     if(debug)
                //         Debug.Log("Does not Contain NParam");
                // }
                
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = videoUrl;
                videoPlayer.EnableAudioTrack(0, true);
                videoPlayer.SetTargetAudioSource(0, videoPlayer.GetComponent<AudioSource>());
// #if UNITY_IOS && !UNITY_EDITOR
//             Debug.Log("this set the audio to direct only in ios, if you are having issues, mail the utube support team.")
//             videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
// #endif
                //TODO rework on this
                //CheckIfCanPlayUrl(videoUrl, PrepareVideoAfterCheck); //Added this to prevent WindowsVideoMedia error 0x80070005 while reading
                videoPlayer.Prepare();
                if (videoQuality != YoutubeVideoQuality.STANDARD)
                {
                    audioPlayer.source = VideoSource.Url;
                    audioPlayer.url = audioUrl;
                    audioPlayer.Prepare();
                    //CheckIfCanPlayUrl(audioUrl, PrepareAudioAfterCheck);    //Added this to prevent WindowsVideoMedia error 0x80070005 while reading
                }
            }
            else
            {
                if (playUsingInternalDevicePlayer)
                {
                    StartCoroutine(HandHeldPlayback());
                }
            }
        }

        void CheckIfCanPlayUrl(string url, Action callback)
        {
            StartCoroutine(VideoRequest(url, callback));
        }

        IEnumerator VideoRequest(string url, Action callback)
        {
            UnityWebRequest request = UnityWebRequest.Head(url);
            request.SetRequestHeader("User-Agent", UserAgent);
            yield return request.SendWebRequest();
            int length = int.Parse(request.GetResponseHeader("Content-Length"));
            if (length > 0)
            {
                callback.Invoke();
            }
            else
            {
                Debug.Log("Retry... WindowsMediaXError!");
                RetryPlayYoutubeVideo();
            }
        }

        void PrepareVideoAfterCheck()
        {
            videoPlayer.Prepare();
            _events.OnYoutubeUrlAreReady.Invoke(videoUrl);
        }

        void PrepareAudioAfterCheck()
        {
            audioPlayer.Prepare();
        }

        protected bool finishedCalled = false;
        protected IEnumerator PreventFinishToBeCalledTwoTimes()
        {
            yield return new WaitForSeconds(1);
            finishedCalled = false;
        }

        private void PlaybackDone(VideoPlayer vPlayer)
        {
            videoStarted = false;
            _events.OnVideoFinished.Invoke();
        }

        private bool videoStarted = false;

        private void VideoStarted(VideoPlayer source)
        {
            if (!videoStarted)
            {
                lastStartedTime = Time.time;
                lastErrorTime = lastStartedTime;
                if (debug)
                    Debug.Log("Youtube Video Started");
            }
        }

        float lastErrorTime;
        private void VideoErrorReceived(VideoPlayer source, string message)
        {
            
            lastErrorTime = Time.time;
            RetryPlayYoutubeVideo();
            Debug.Log("Youtube VideoErrorReceived! Retry: " + message);
        }

        [HideInInspector]
        public bool pauseCalled = false;

        public void Pause()
        {
            pauseCalled = true;
            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Pause();
                audioPlayer.Pause();
            }
            _events.OnVideoPaused.Invoke();
        }

        #region VIDEO CONTROLLER

        private float totalVideoDuration;
        private float currentVideoDuration;
        private bool videoSeekDone = false;
        private bool videoAudioSeekDone = false;
        private bool lowRes;
        private float hideScreenTime = 0;
        private float audioDuration;

        private void Update()
        {
            
            if (loadYoutubeUrlsOnly) return;
            if (!_controller.showPlayerControl) return;
            if (_controller.hideScreenControlTime <= 0) return;
            
            if (UserInteract())
            {
                hideScreenTime = 0;
                if (_controller.controllerMainUI != null)
                    _controller.controllerMainUI.SetActive(true);
            }
            else
            {
                hideScreenTime += Time.deltaTime;
                if (!(hideScreenTime >= _controller.hideScreenControlTime)) return;
                //not increment
                hideScreenTime = _controller.hideScreenControlTime;
                _controller.HideControllers();
            }
        }
        //experimental
        //bool seekUsingLowQuality = false;

        public void Seek(float time)
        {
            waitAudioSeek = true;
            Pause();

            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                //seekUsingLowQuality = true;
                videoPlayer.time = time;
            }
            else
            {
                audioPlayer.time = time;
            }
        }

        /*
    IEnumerator WorkAroundToUnityBackwardSeek()
    {
        yield return new WaitForSeconds(0.1f);
        videoPrepared = false;
        audioPrepared = false;
        if (!lowRes)
            audioPlayer.prepareCompleted += AudioPreparedSeek;
        videoPlayer.prepareCompleted += VideoPreparedSeek;
        if (!lowRes)
            audioPlayer.Prepare();
        videoPlayer.Prepare();
    }*/

        void VideoPreparedSeek(VideoPlayer p)
        {
            //videoPrepared = true;
        }

        void AudioPreparedSeek(VideoPlayer p)
        {
            //audioPrepared = true;
        }

        public void Stop()
        {
            prepareVideoToPlayLater = false;
            startedPlayingWebgl = false;
            if (!playUsingInternalDevicePlayer)
            {
                if (audioPlayer != null)
                    audioPlayer.seekCompleted -= AudioSeeked;
                videoPlayer.seekCompleted -= VideoSeeked;

                videoPlayer.frameDropped -= VideoPlayer_frameDropped;
                if (audioPlayer != null)
                    audioPlayer.frameDropped -= AudioPlayer_frameDropped;

                videoPlayer.Stop();
                if (!lowRes && audioPlayer != null)
                    audioPlayer.Stop();
            }
        }

        void SeekVideoDone(VideoPlayer vp)
        {
            videoSeekDone = true;
            videoPlayer.seekCompleted -= SeekVideoDone;
            if (!lowRes)
            {
                //check if the two videos are done the seek | if are play the videos
                if (videoSeekDone && videoAudioSeekDone)
                {
                    isSyncing = false;
                    StartCoroutine(SeekFinished());
                }
            }
            else
            {
                isSyncing = false;

                HideLoading();
            }
        }

        void SeekVideoAudioDone(VideoPlayer vp)
        {
            Debug.Log("NAAN");
            videoAudioSeekDone = true;
            audioPlayer.seekCompleted -= SeekVideoAudioDone;
            if (!lowRes)
            {
                if (videoSeekDone && videoAudioSeekDone)
                {
                    isSyncing = false;
                    StartCoroutine(SeekFinished());
                }
            }
        }

        IEnumerator SeekFinished()
        {
            yield return new WaitForEndOfFrame();
            HideLoading();
        }

        private string FormatTime(int time)
        {
            int hours = time / 3600;
            int minutes = (time % 3600) / 60;
            int seconds = (time % 3600) % 60;
            if (hours == 0 && minutes != 0)
            {
                return minutes.ToString("00") + ":" + seconds.ToString("00");
            }
            else if (hours == 0 && minutes == 0)
            {
                return "00:" + seconds.ToString("00");
            }
            else
            {
                return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            }
        }

        bool UserInteract()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touches.Length >= 1)
                    return true;
                else
                    return false;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                    return true;
                return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
            }

        }



        #endregion

        private string lsigForVideo;
        private string lsigForAudio;


        #region Decryption System

        public void DecryptDownloadUrl(string encryptedUrlVideo, string encrytedUrlAudio, string html, bool videoOnly)
        {
            EncryptUrlForAudio = encrytedUrlAudio;
            EncryptUrlForVideo = encryptedUrlVideo;
            //string t = "https://www.youtube.com/s/player/5168268b/player_ias.vflset/en_US/base.js"
            //string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player_ias-{0}.js", htmlVersion);

            IDictionary<string, string> videoQuery = HTTPHelperYoutube.ParseQueryString(EncryptUrlForVideo);
            if (videoQuery.ContainsKey("n"))
            {
                //TODO Create a system to work with the nparam to prevent future errors.
                if(debug)
                    Debug.Log("has n param");
                string nparam = videoQuery["n"];
                if(debug)
                    Debug.Log(nparam);
            }
            else
            {
                if(debug)
                    Debug.Log("Does not Contain NParam");
            }
            
            if (videoOnly)
            {

                //IDictionary<string, string> videoQuery = HTTPHelperYoutube.ParseQueryString(EncryptUrlForVideo);

                string o = EncryptUrlForVideo;
                o = o.Replace("&sig=", "|");
                o = o.Replace("lsig=", "|");
                o = o.Replace("&ratebypass=yes", "");
                string[] split = o.Split('|');
                lsigForVideo = split[split.Length - 2];
                encryptedSignatureVideo = split[split.Length - 1];
                StartCoroutine(Downloader(jsUrl, false));

                
                //if (videoQuery.ContainsKey("signature") || videoQuery.ContainsKey(sp))
                //{
                //    if (videoQuery.ContainsKey(sp))
                //    {
                //        encryptedSignatureVideo = videoQuery[sp];
                //    }
                //    else
                //    {
                //        encryptedSignatureVideo = videoQuery["signature"];
                //    }

                //    Debug.Log(encryptedUrlVideo);
                //    Debug.Log(encryptedSignatureVideo);
                //    Debug.Log(videoQuery["signature"]);

                //    StartCoroutine(Downloader(jsUrl, false));   //Use for single video only
                //}
            }
            else
            {
                //IDictionary<string, string> audioQuery = HTTPHelperYoutube.ParseQueryString(EncryptUrlForAudio);
                //IDictionary<string, string> videoQuery = HTTPHelperYoutube.ParseQueryString(EncryptUrlForVideo);

                
                string o = EncryptUrlForVideo;
                if (debug)
                {
                    Debug.Log("Encrypted Normal: "+o);
                    Debug.Log("Encrypted Changed: "+o);
                }
                o = o.Replace("&sig=", "|");
                o = o.Replace("lsig=", "|");
                o = o.Replace("&ratebypass=yes", "");
                string[] split = o.Split('|');
                lsigForVideo = split[split.Length - 2];
                encryptedSignatureVideo = split[split.Length - 1];

                o = EncryptUrlForAudio;
                o = o.Replace("&sig=", "|");
                o = o.Replace("lsig=", "|");
                o = o.Replace("&ratebypass=yes", "");
                string[] splitn = o.Split('|');
                lsigForAudio = splitn[splitn.Length - 2];
                encryptedSignatureAudio = splitn[splitn.Length - 1];


                //if (videoQuery.ContainsKey(SignatureQuery))
                //{
                //    encryptedSignatureVideo = videoQuery[SignatureQuery];
                //}

                //if (audioQuery.ContainsKey(SignatureQuery))
                //{
                //    encryptedSignatureAudio = audioQuery[SignatureQuery];
                //}

                StartCoroutine(Downloader(jsUrl, true));   //Use for audio and video
            }
        }

        private Thread thread1;
        public void ReadyForExtract(string r, bool audioExtract)
        {
            if (audioExtract)
            {

                SetMasterUrlForAudio(r);
#if UNITY_WEBGL
            DoRegexFunctionsForAudio(r);
#else
                
                if (SystemInfo.processorCount > 1)
                {
                    thread1 = new Thread(() => DoRegexFunctionsForAudio(r));
                    thread1.Start();
                }
                else
                {
                    DoRegexFunctionsForAudio(r);
                }
#endif
            }
            else
            {
                SetMasterUrlForVideo(r);
#if UNITY_WEBGL
            DoRegexFunctionsForVideo(r);
#else
               
                if (SystemInfo.processorCount > 1)
                {
                    thread1 = new Thread(() => DoRegexFunctionsForVideo(r));
                    thread1.Start();
                }
                else
                {
                    DoRegexFunctionsForVideo(r); 
                }
#endif
            }

        }

        IEnumerator Downloader(string uri, bool audio)
        {
            UnityWebRequest request = UnityWebRequest.Get(uri);
            request.SetRequestHeader("User-Agent", UserAgent);
            yield return request.SendWebRequest();
            //WriteLog("js", request.downloadHandler.text);

            ReadyForExtract(request.downloadHandler.text, audio);
        }
        #endregion

        #region WEBGLREQUEST
        YoutubeResultIds webGlResults;
        IEnumerator WebGlRequest(string videoID)
        {
            UnityWebRequest request = UnityWebRequest.Get(serverURI + "" + videoID + "" + formatURI);
            request.SetRequestHeader("User-Agent", UserAgent);
            yield return request.SendWebRequest();
            startedPlayingWebgl = false;
            webGlResults = new YoutubeResultIds();
            var requestData = JSON.Parse(request.downloadHandler.text);
            var videos = requestData["videos"][0]["formats"];
            webGlResults.bestFormatWithAudioIncluded = requestData["videos"][0]["url"];
            for (int counter = 0; counter < videos.Count; counter++)
            {
                if (videos[counter]["format_id"] == "160")
                {
                    webGlResults.lowQuality = videos[counter]["url"];
                }
                else if (videos[counter]["format_id"] == "133")
                {
                    webGlResults.lowQuality = videos[counter]["url"];   //if have 240p quality overwrite the 144 quality as low quality.
                }
                else if (videos[counter]["format_id"] == "134")
                {
                    webGlResults.standardQuality = videos[counter]["url"];  //360p
                }
                else if (videos[counter]["format_id"] == "136")
                {
                    webGlResults.hdQuality = videos[counter]["url"];  //720p
                }
                else if (videos[counter]["format_id"] == "137")
                {
                    webGlResults.fullHdQuality = videos[counter]["url"];  //1080p
                }
                else if (videos[counter]["format_id"] == "266")
                {
                    webGlResults.ultraHdQuality = videos[counter]["url"];  //@2160p 4k
                }
                else if (videos[counter]["format_id"] == "139")
                {
                    webGlResults.audioUrl = videos[counter]["url"];  //AUDIO
                }
            }
            //quality selection will be implemented later for webgl, i recomend use the  webGlResults.bestFormatWithAudioIncluded
            WebGlGetVideo(webGlResults.bestFormatWithAudioIncluded);
        }



        //WEBGL only...
        public void WebGlGetVideo(string url)
        {
            logTest = "Getting Url Player";
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            videoUrl = VIDEOURIFORWEBGLPLAYER + "" + encodedText;
            videoQuality = YoutubeVideoQuality.STANDARD;
            logTest = videoUrl + " Done";
            Debug.Log("Play!! " + videoUrl);
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoUrl;
            videoPlayer.Prepare();
            //videoPrepared = false;
            videoPlayer.prepareCompleted += WeblPrepared; ;
        }

        private void WeblPrepared(VideoPlayer source)
        {
            startedPlayingWebgl = true;

            StartCoroutine(WebGLPlay());
            logTest = "Playing!!";
        }

        IEnumerator WebGLPlay() //The prepare not respond so, i forced to play after some seconds
        {
            yield return new WaitForSeconds(2f);
            StartPlayingWebgl();
        }
        bool startedPlayingWebgl = false;



        protected string logTest = "/";
        private void OnGUI()
        {
            if (debug)
                GUI.Label(new Rect(0, 0, 400, 30), logTest);
        }
        #endregion
        [HideInInspector]
        public bool isSyncing = false;


        #region YOUTUBESYSTEM
        private const string RateBypassFlag = "ratebypass";
        [HideInInspector]
        public static string SignatureQuery = "sig";
        [HideInInspector]
        public string encryptedSignatureVideo;
        [HideInInspector]
        public string encryptedSignatureAudio;
        [HideInInspector]
        private string masterURLForVideo;
        [HideInInspector]
        private string masterURLForAudio;

        private void SetMasterUrlForAudio(string url)
        {
            masterURLForAudio = url;
        }

        private void SetMasterUrlForVideo(string url)
        {
            masterURLForVideo = url;
        }


        private string[] patternNames = {
            ""
        };
        //private int patternIndex = 0;

        private void DoRegexFunctionsForVideo(string jsF)
        {
            masterURLForVideo = jsF;
            string js = jsF;
            //Find "C" in this: var A = B.sig||C (B.s)
            //string functNamePattern = @"(\w+)\s*=\s*function\(\s*(\w+)\s*\)\s*{\s*\2\s*=\s*\2\.split\(\""\""\)\s*;(.+)return\s*\2\.join\(\""\""\)\s*}\s*;";
            var funcName = "";

            patternNames = @magicResult.defaultFuncName;

            foreach (string pat in patternNames)
            {
                string h = Regex.Match(js, pat).Groups[1].Value;
                if (!string.IsNullOrEmpty(h))
                {
                    funcName = h;
                    break;
                }
            }

            if (funcName.Contains("$"))
            {
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape
            }


            string funcPattern = @"(?!h\.)" + @funcName + @"=function\(\w+\)\{.*?\}"; //Escape funcName string
            var funcBody = Regex.Match(js, funcPattern, RegexOptions.Singleline).Value; //Entire sig function


            var lines = funcBody.Split(';'); //Each line in sig function

            string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
            string functionIdentifier = "";
            string operations = "";

            foreach (var line in lines.Skip(1).Take(lines.Length - 2)) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {
                if (!string.IsNullOrEmpty(idReverse) && !string.IsNullOrEmpty(idSlice) &&
                    !string.IsNullOrEmpty(idCharSwap))
                {
                    break; //Break loop if all three cipher methods are defined
                }

                functionIdentifier = GetFunctionFromLine(line);
                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                if (functionIdentifier == "nt")
                {
                    string test = @"encodeURIComponent:\bfunction\b\(\w+\)";
                    if (Regex.Match(js, test).Success)
                    {
                        //if found uri component dont use it igore;
                    }
                    else
                    {
                        if (Regex.Matches(js, reReverse).Count > 1)
                        {
                            if (idReverse == "")
                                idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                        }
                    }
                }
                else
                {
                    if (Regex.Match(js, reReverse).Success)
                    {
                        if (idReverse == "")
                            idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                    }
                }

                if (Regex.Match(js, reSlice).Success)
                {
                    if (idSlice == "")
                        idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
                }

                if (Regex.Match(js, reSwap).Success)
                {
                    if (idCharSwap == "")
                        idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
                }

            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = GetFunctionFromLine(line);
                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap)
                {
                    operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice)
                {
                    operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }

                if (functionIdentifier == idReverse) //No regex required for reverse (reverse method has no parameters)
                {
                    operations += "r "; //operation is a reverse
                }
            }






            //New method:::For possible study and application in the future.
            //var fnname = Regex.Match(js, @"yt\.akamaized\.net.*encodeURIComponent\((\w+)").Groups[1].Value;
            //var _argnamefnbodyresult = Regex.Match(js, fnname + @"=function\((.+?)\){(.+?)}");
            //var helpername = Regex.Match(_argnamefnbodyresult.Groups[2].Value, @";(.+?)\..+?\(").Groups[1].Value;
            //var helperresult = Regex.Match(js, "var " + helpername + "={[\\S\\s]+?};");
            //var result = helperresult.Groups[0].Value;

            //MatchCollection matches = Regex.Matches(result, @"[A-Za-z0-9]+:function\s*([A-z0-9]+)?\s*\((?:[^)(]+|\((?:[^)(]+|\([^)(]*\))*\))*\)\s*\{(?:[^}{]+|\{(?:[^}{]+|\{[^}{]*\})*\})*\}");
            //var funcs = _argnamefnbodyresult.Groups[2].Value.Split(';');

            ////string test = "AA0qz64iwNKTsnE3bAg6_WsuQ_c8DH-qx-rCg4QtABTAiAuFtEwS3jPXlvdZV5VqN=c-Fh2QYc52hOwGOn5dXYzKMAhIQRwA2IxgLAw";
            //var sign = encryptedSignatureVideo.ToCharArray();

            //foreach (string func in funcs)
            //{
            //    foreach (Match group in matches)
            //    {
            //        if (group.Value.Contains("reverse"))
            //        {
            //            var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;
            //            if (func.Contains(test))
            //            {
            //                sign = ReverseFunction(sign);
            //            }
            //        }
            //        else if (group.Value.Contains("splice"))
            //        {
            //            var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;
            //            if (func.Contains(test))
            //            {
            //                sign = SpliceFunction(sign, GetOpIndex(func));
            //            }
            //        }
            //        else
            //        {
            //            var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;
            //            if (func.Contains(test))
            //            {

            //                sign = SwapFunction(sign, GetOpIndex(func));
            //            }
            //        }
            //    }
            //}

            //string n = new string(sign);

            //Debug.Log(n);
            operations = operations.Trim();



            if (string.IsNullOrEmpty(operations))
            {
                Debug.Log("Operation is empty for low qual, trying again.");
                if (canUpdate)
                {
                    canUpdate = false;
                }
                decryptedVideoUrlResult = null;
                return;
            }
            else
            {

                string magicResult = MagicHands.DecipherWithOperations(encryptedSignatureVideo, operations);
                decryptedVideoUrlResult = HTTPHelperYoutube.ReplaceQueryStringParameter(EncryptUrlForVideo, SignatureQuery, magicResult, lsigForVideo);
                //var url = new Uri(decryptedVideoUrlResult);
                //decryptedVideoUrlResult = decryptedVideoUrlResult.Replace(url.Host, "redirector.googlevideo.com");
            }


            decryptedUrlForVideo = true;
        }

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");

            int index = Int32.Parse(parsed);
            return index;
        }

        private static char[] SpliceFunction(char[] a, int b)
        {
            return a.Splice(b);
        }
        private static char[] SwapFunction(char[] a, int b)
        {
            var c = a[0];
            a[0] = a[b % a.Length];
            a[b % a.Length] = c;
            return a;
        }

        private static char[] ReverseFunction(char[] a)
        {
            Array.Reverse(a);
            return a;
        }

        private void DoRegexFunctionsForAudio(string jsF)
        {
            masterURLForAudio = jsF;
            string js = masterURLForAudio;
            //Find "C" in this: var A = B.sig||C (B.s)
            //string functNamePattern = @"(\w+)\s*=\s*function\(\s*(\w+)\s*\)\s*{\s*\2\s*=\s*\2\.split\(\""\""\)\s*;(.+)return\s*\2\.join\(\""\""\)\s*}\s*;";

            //string functNamePattern = @"\b[cs]\s*&&\s*[adf]\.set\([^,]+\s*,\s*encodeURIComponent\s*\(\s*([\w$]+)\(";
            //string functNamePattern = patternNames[patternIndex];
            patternNames = magicResult.defaultFuncName;
            var funcName = "";
            foreach (string pat in patternNames)
            {
                string r = Regex.Match(js, pat).Groups[1].Value;
                if (!string.IsNullOrEmpty(r))
                {
                    funcName = r;
                    break;
                }
            }

            if (funcName.Contains("$"))
            {
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape
            }

            // string nCodeFn = "&&\\(b=a.get\\(\"n\"\\)\\)&&\\(b=(.+)\\(b\\)";
            // Debug.Log(nCodeFn);
            // string nFuncName = Regex.Match(js, nCodeFn).Groups[1].Value;
            // string nFunctionStart = nFuncName+"=function(a)";
            // Debug.Log(nFunctionStart);
            // int ndx = js.IndexOf(nFunctionStart);
            // string nSubBody = js.Substring(ndx+nFunctionStart.Length);
            // Debug.Log(nSubBody);
            
            //string fb = "var "+nFunctionStart + utils.cutAfterJSON(nSubBody);
            string funcPattern = @"(?!h\.)" + @funcName + @"=function\(\w+\)\{.*?join.*\};"; //Escape funcName string

            var funcBody = Regex.Match(js, funcPattern).Value; //Entire sig function old entire function....

            //Debug.Log(funcBody);
            var lines = funcBody.Split(';'); //Each line in sig function

            string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
            string functionIdentifier = "";
            string operations = "";


            foreach (var line in lines.Skip(1).Take(lines.Length - 2)) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {

                if (!string.IsNullOrEmpty(idReverse) && !string.IsNullOrEmpty(idSlice) &&
                    !string.IsNullOrEmpty(idCharSwap))
                {
                    break; //Break loop if all three cipher methods are defined
                }


                functionIdentifier = GetFunctionFromLine(line);


                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                if (functionIdentifier == "nt")
                {
                    string test = @"encodeURIComponent:\bfunction\b\(\w+\)";
                    if (Regex.Match(js, test).Success)
                    {
                        //if found uri component dont use it igore;
                    }
                    else
                    {
                        if (Regex.Matches(js, reReverse).Count > 1)
                        {
                            if (idReverse == "")
                                idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                        }
                    }
                }
                else
                {
                    if (Regex.Match(js, reReverse).Success)
                    {
                        if (idReverse == "")
                            idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                    }
                }

                if (Regex.Match(js, reSlice).Success)
                {
                    if (idSlice == "")
                        idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
                }

                if (Regex.Match(js, reSwap).Success)
                {
                    if (idCharSwap == "")
                        idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
                }
            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = GetFunctionFromLine(line);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap)
                {

                    operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice)
                {

                    operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }

                if (functionIdentifier == idReverse) //No regex required for reverse (reverse method has no parameters)
                {

                    operations += "r "; //operation is a reverse
                }
            }

            operations = operations.Trim();

            if (string.IsNullOrEmpty(operations))
            {
                Debug.Log("Operation is empty, trying again.");
                if (canUpdate)
                {
                    canUpdate = false;
                }
                decryptedAudioUrlResult = null;
                decryptedVideoUrlResult = null;
            }
            else
            {
                string AudioMagicResult = MagicHands.DecipherWithOperations(encryptedSignatureAudio, operations);
                string VideoMagicResult = MagicHands.DecipherWithOperations(encryptedSignatureVideo, operations);

                decryptedAudioUrlResult = HTTPHelperYoutube.ReplaceQueryStringParameter(EncryptUrlForAudio, SignatureQuery, AudioMagicResult, lsigForAudio);
                decryptedVideoUrlResult = HTTPHelperYoutube.ReplaceQueryStringParameter(EncryptUrlForVideo, SignatureQuery, VideoMagicResult, lsigForVideo);

                //var url = new Uri(decryptedAudioUrlResult);
                //decryptedAudioUrlResult = decryptedAudioUrlResult.Replace(url.Host, "redirector.googlevideo.com");
                //url = new Uri(decryptedVideoUrlResult);
                //decryptedVideoUrlResult = decryptedVideoUrlResult.Replace(url.Host, "redirector.googlevideo.com");
            }

            decryptedUrlForAudio = true;
            //decryptedUrlForVideo = true;
        }
        void DelayForAudio()
        {
            decryptedUrlForVideo = true;
        }

        [HideInInspector]
        public bool decryptedUrlForVideo = false;
        [HideInInspector]
        public bool decryptedUrlForAudio = false;
        [HideInInspector]
        public string decryptedVideoUrlResult = "";
        [HideInInspector]
        public string decryptedAudioUrlResult = "";


        private static string GetFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
        }

        public IEnumerator WebGlRequest(Action<string> callback, string id, string host)
        {
            Debug.Log(host + "getvideo.php?videoid=" + id + "&type=Download");
            UnityWebRequest request = UnityWebRequest.Get(host + "getvideo.php?videoid=" + id + "&type=Download");
            request.SetRequestHeader("User-Agent" ,UserAgent);
            yield return request.SendWebRequest();
            callback.Invoke(request.downloadHandler.text);
        }

        public List<VideoInfo> youtubeVideoInfos;
        private void GetDownloadUrls(Action callback, string videoUrl, YoutubeSettings player)
        {
            if (videoUrl != null) { /*Debug.Log("Youtube: " + videoUrl);*/ } else { /*Debug.Log("Youtube url null!");*/ }
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            //#if UNITY_WSA
            //            videoUrl = "https://youtube.com/watch?v=" + videoUrl;
            //#else
            //            Uri uriResult;
            //            bool result = Uri.TryCreate(videoUrl, UriKind.Absolute, out uriResult)
            //                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            //            if (!result)
            //                videoUrl = "https://youtube.com/watch?v=" + videoUrl;
            //#endif


            bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);
            if (!isYoutubeUrl)
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }
            StartCoroutine(DownloadYoutubeUrl(videoUrl, callback, player));

        }

        private string htmlVersion = "";

        IEnumerator YoutubeURLDownloadFinished(Action callback, YoutubeSettings player)
        {
            var videoId = youtubeUrl.Replace("https://youtube.com/watch?v=", "");
            //jsonforHtml
            var player_response = string.Empty;
            bool tempfix = false;

            if (Regex.IsMatch(jsonForHtmlVersion, @"[""\']status[""\']\s*:\s*[""\']LOGIN_REQUIRED") || tempfix)
            {
                var url = "https://www.docs.google.com/get_video_info?video_id=" + videoId + "&eurl=https://youtube.googleapis.com/v/" + videoId + "&html5=1&c=TVHTML5&cver=6.20180913";
                Debug.Log(url);
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.SetRequestHeader("User-Agent", UserAgent);
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
                var dataMatch = dataRegexOption.Match(jsonForHtmlVersion);
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
                dataMatch = dataRegexOption.Match(jsonForHtmlVersion);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }

                dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;\s*(?:var\s+meta|</script|\n)", RegexOptions.Multiline);
                dataMatch = dataRegexOption.Match(jsonForHtmlVersion);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }

                dataRegexOption = new Regex(@"ytInitialPlayerResponse\s*=\s*({.+?})\s*;", RegexOptions.Multiline);
                dataMatch = dataRegexOption.Match(jsonForHtmlVersion);
                if (dataMatch.Success)
                {
                    player_response = dataMatch.Result("$1");
                }
            }

            JObject json = JObject.Parse(player_response);



            if (downloadYoutubeUrlResponse.isValid)
            {
                if (IsVideoUnavailable(downloadYoutubeUrlResponse.data)) { throw new VideoNotAvailableException(); }

                try
                {
                    //var dataRegex = new Regex(@"""jsUrl""\s*:\s*""([^""]+)""");
                    var dataRegex = new Regex("\"(?:PLAYER_JS_URL|jsUrl)\"\\s*:\\s*\"([^\"]+)\"");
                    string js = dataRegex.Match(jsonForHtmlVersion).Result("$1").Replace("\\/", "/");
                    jsUrl = "https://www.youtube.com" + js;

                        Debug.Log(jsUrl);


                    string htmlPlayerVersion = TryMatchHtmlVersion(js, magicResult.defaultHtmlPlayerVersion);

                    Debug.Log(htmlPlayerVersion);
                    if (debug)
                        Debug.Log(htmlPlayerVersion);

                    htmlVersion = htmlPlayerVersion;
                    string videoTitle = GetVideoTitle(json);
                    if (debug)
                        Debug.Log(videoTitle);
                    

                    //if (!json.ContainsItem("assets"))
                    //{
                    //    Debug.Log("no Contain");
                    //    //var secondaryDataRegex = new Regex(@"ytplayer\.web_player_context_config\s*=\s*(\{.+?\});", RegexOptions.Multiline);
                    //    //JObject secondaryJson = JObject.Parse(secondaryDataRegex.Match(jsonForHtmlVersion).Result("$1"));

                    //    //string js = secondaryJson["jsUrl"].ToString();
                    //    //jsUrl = "https://www.youtube.com" + js;

                    //}
                    //else
                    //{
                    //    Debug.Log("contains");
                    //    htmlPlayerVersion = GetHtml5PlayerVersion(json, magicResult.regexForHtmlPlayerVersion);
                    //    htmlVersion = htmlPlayerVersion;
                    //}
                    
                    IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);
                    List<VideoInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

                    if (string.IsNullOrEmpty(htmlVersion))
                    {
                        RetryPlayYoutubeVideo();
                    }

                    youtubeVideoInfos = infos;
                    foreach (VideoInfo info in youtubeVideoInfos)
                    {
                        info.HtmlPlayerVersion = htmlPlayerVersion;
                    }
                    callback.Invoke();

                }
                catch (Exception e)
                {
                    if (!loadYoutubeUrlsOnly)
                    {
                        Debug.Log("Resolver Exception!: " + e.Message);
                        Debug.Log(e.Source + " " + e.StackTrace);
                        Debug.Log(Application.persistentDataPath);
                        //string filePath = Application.persistentDataPath + "/log_download_exception_" + DateTime.Now.ToString("ddMMyyyyhhmmssffff") + ".txt";
                        //Debug.Log("DownloadUrl content saved to " + filePath);
                        if (Application.isEditor && debug)
                            WriteLog("log_download_exception", "jsonForHtml: " + jsonForHtmlVersion);
                        //File.WriteAllText(filePath, downloadUrlResponse.data);
                        Debug.Log("retry!");
                        if (player != null)
                        {
                            player.RetryPlayYoutubeVideo();
                        }
                        else
                        {
                            Debug.LogError("Connection to Youtube Server Error! Try Again");
                        }
                    }

                }
            }
        }
        static string sp = "";

        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "https://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HTTPHelperYoutube.ParseQueryString(url);

            string v;

            if (!query.TryGetValue("v", out v))
            {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "https://youtube.com/watch?v=" + v;

            return true;
        }

        private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(JObject json)
        {
            List<string> urls = new List<string>();
            List<string> ciphers = new List<string>();
            JObject newJson = json;

            if (newJson["streamingData"]["formats"][0]["cipher"] != null)
            {
                foreach (var j in newJson["streamingData"]["formats"])
                {
                    ciphers.Add(j["cipher"].ToString());
                }

                foreach (var j in newJson["streamingData"]["adaptiveFormats"])
                {
                    ciphers.Add(j["cipher"].ToString());
                }
            }
            else if (newJson["streamingData"]["formats"][0]["signatureCipher"] != null)
            {
                foreach (var j in newJson["streamingData"]["formats"])
                {
                    ciphers.Add(j["signatureCipher"].ToString());
                }

                foreach (var j in newJson["streamingData"]["adaptiveFormats"])
                {
                    ciphers.Add(j["signatureCipher"].ToString());
                }
            }
            else
            {
                foreach (var j in newJson["streamingData"]["formats"])
                {
                    urls.Add(j["url"].ToString());
                }

                foreach (var j in newJson["streamingData"]["adaptiveFormats"])
                {
                    urls.Add(j["url"].ToString());
                }
            }

            foreach (string s in ciphers)
            {
                IDictionary<string, string> queries = HTTPHelperYoutube.ParseQueryString(s);

                string url;

                bool requiresDecryption = false;

                if (queries.ContainsKey("sp"))
                    SignatureQuery = "sig";
                else
                    SignatureQuery = "signatures";




                if (queries.ContainsKey("s") || queries.ContainsKey("signature"))
                {
                    requiresDecryption = queries.ContainsKey("s");
                    string signature = queries.ContainsKey("s") ? queries["s"] : queries["signature"];

                    if (sp != "none")
                    {
                        url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);
                    }
                    else
                    {
                        url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);
                    }


                    string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

                    url += fallbackHost;
                }

                else
                {
                    url = queries["url"];
                }

                url = HTTPHelperYoutube.UrlDecode(url);
                url = HTTPHelperYoutube.UrlDecode(url);

                IDictionary<string, string> parameters = HTTPHelperYoutube.ParseQueryString(url);
                if (!parameters.ContainsKey(RateBypassFlag))
                    url += string.Format("&{0}={1}", RateBypassFlag, "yes");
                yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
            }

            foreach (string s in urls)
            {
                string url = s;
                url = HTTPHelperYoutube.UrlDecode(url);
                url = HTTPHelperYoutube.UrlDecode(url);

                IDictionary<string, string> parameters = HTTPHelperYoutube.ParseQueryString(url);
                if (!parameters.ContainsKey(RateBypassFlag))
                    url += string.Format("&{0}={1}", RateBypassFlag, "yes");
                yield return new ExtractionInfo { RequiresDecryption = false, Uri = new Uri(url) };
            }
        }

        private static string GetAdaptiveStreamMap(JObject json)
        {
            //Debug.Log("Ok");
            JToken streamMap = json["args"]["adaptive_fmts"];
            //Debug.Log("fine");
            // bugfix: adaptive_fmts is missing in some videos, use url_encoded_fmt_stream_map instead
            if (streamMap == null)
            {
                //Debug.Log("32");
                streamMap = json["args"]["url_encoded_fmt_stream_map"];
                //Debug.Log("33");
                if (streamMap == null)
                {
                    //Debug.Log("45");
                    //string unescaped = Regex.Unescape(json["args"]["player_response"].ToString());
                    JObject newJson = JObject.Parse(json["args"]["player_response"].ToString());
                    streamMap = newJson["streamingData"]["adaptiveFormats"];
                    //WriteLog("NewJson", newJson.ToString());
                    //Debug.Log("53");
                    //Debug.Log(streamMap);
                }
            }

            return streamMap.ToString();
        }

        public class Html5PlayerResult
        {
            public string scriptName;
            public string result;
            public bool isValid = false;
            public Html5PlayerResult(string _script, string _result, bool _valid)
            {
                scriptName = _script;
                result = _result;
                isValid = _valid;
            }
        }

        private static string GetHtml5PlayerVersion(JObject json, string regexForHtmlPVersions)
        {

            var regex = new Regex(regexForHtmlPVersions);
            string js = json["assets"]["js"].ToString();
            jsUrl = "https://www.youtube.com" + js;

            Match match = regex.Match(js);
            if (match.Success) return match.Result("$1");

            regex = new Regex(@"player_ias(.+?).js");
            match = regex.Match(js);
            if (match.Success) return match.Result("$1");

            regex = new Regex(@"player-(.+?).js");

            return regex.Match(js).Result("$1");
        }

        private static string TryMatchHtmlVersion(string input, string regexForHtmlPVersions)
        {
            var regex = new Regex(regexForHtmlPVersions);
            Match match = regex.Match(input);
            if (match.Success) return match.Result("$1");
            regex = new Regex(@"player_ias(.+?).js");
            match = regex.Match(input);
            if (match.Success) return match.Result("$1");
            regex = new Regex(@"player-(.+?).js");
            return regex.Match(input).Result("$1");
        }

        private static string GetStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

            if (streamMap == null)
            {
                JObject newJson = JObject.Parse(json["args"]["player_response"].ToString());
                streamMap = newJson["streamingData"]["formats"];
            }

            string streamMapString = streamMap == null ? null : streamMap.ToString();

            if (streamMapString == null || streamMapString.Contains("been+removed"))
            {
                if (streamMapString.Contains("been+removed"))
                    throw new VideoNotAvailableException("Video is removed or has an age restriction.");
                else
                    return null;
            }

            return streamMapString;
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (ExtractionInfo extractionInfo in extractionInfos)
            {
                string itag = HTTPHelperYoutube.ParseQueryString(extractionInfo.Uri.Query)["itag"];

                int formatCode = int.Parse(itag);

                VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

                if (info != null)
                {
                    info = new VideoInfo(info)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString(),
                        Title = videoTitle,
                        RequiresDecryption = extractionInfo.RequiresDecryption
                    };
                }

                else
                {
                    info = new VideoInfo(formatCode)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString()
                    };
                }

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
        }

        private static string GetVideoTitle(JObject json)
        {
            //JObject t = JObject.Parse(json["player_response"].ToString());
            JToken title = json["videoDetails"]["title"];

            return title == null ? String.Empty : title.ToString();
        }

        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
        }

        [HideInInspector]
        public string EncryptUrlForVideo;
        [HideInInspector]
        public string EncryptUrlForAudio;

        private class DownloadUrlResponse
        {
            public string data = null;
            public bool isValid = false;
            public long httpCode = 0;
            public DownloadUrlResponse() { data = null; isValid = false; httpCode = 0; }
        }
        private DownloadUrlResponse downloadYoutubeUrlResponse;


        IEnumerator DownloadUrl(string url, Action<string> callback, VideoInfo videoInfo)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("User-Agent", UserAgent);
            yield return request.SendWebRequest();
            if (request.isNetworkError) { Debug.Log("Youtube UnityWebRequest isNetworkError!"); }
            else if (request.isHttpError) { Debug.Log("Youtube UnityWebRequest isHttpError!"); }
            else if (request.responseCode == 200)
            {
                //FinishDecrypt(callback, videoInfo, request.downloadHandler.text);
            }
            else
            { Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode); }
        }

        [HideInInspector]
        public string jsonForHtmlVersion = "";

        IEnumerator DownloadYoutubeUrl(string url, Action callback, YoutubeSettings player)
        {
            downloadYoutubeUrlResponse = new DownloadUrlResponse();
            var videoId = url.Replace("https://youtube.com/watch?v=", "");

            var newUrl = "https://www.youtube.com/watch?v=" + videoId + "&gl=US&hl=en&has_verified=1&bpctr=9999999999";
            UnityWebRequest request = UnityWebRequest.Get(newUrl);
            request.SetRequestHeader("User-Agent", UserAgent);
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
                        jsonForHtmlVersion = request.downloadHandler.text;
                        downloadYoutubeUrlResponse.data = request.downloadHandler.text;
                    }
                }
                else { Debug.Log("Youtube UnityWebRequest Null response"); }
            }
            else
            { Debug.Log("Youtube UnityWebRequest responseCode:" + request.responseCode); }

            StartCoroutine(YoutubeURLDownloadFinished(callback, player));
        }

        public static void WriteLog(string filename, string c)
        {
            string filePath = "C:/log/" + filename + "_" + DateTime.Now.ToString("ddMMyyyyhhmmssffff") + ".txt";
            //Debug.Log("Log written in: " + Application.persistentDataPath);
            //Debug.Log("DownloadUrl content saved to " + filePath);
            File.WriteAllText(filePath, c);
        }

        private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
        {
            throw new YoutubeParseException("Could not parse the Youtube page for URL " + videoUrl + "\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at kelvinparkour@gmail.com with a subject message 'Parse Error' ", innerException);
        }

        private class ExtractionInfo
        {
            public bool RequiresDecryption { get; set; }

            public Uri Uri { get; set; }
        }
        #endregion

        #region Progress BAR



        private bool waitAudioSeek = false;

        public void TrySkip(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _controller.progressRectangle.rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                float pct = Mathf.InverseLerp(_controller.progressRectangle.rectTransform.rect.xMin, _controller.progressRectangle.rectTransform.rect.xMax, localPoint.x);
                SkipToPercent(pct);
            }
        }

        private float oldVolume;

        public void SkipToPercent(float pct)
        {
            //audio issue fix...check all unity versions.
            if (videoQuality != YoutubeVideoQuality.STANDARD)
            {
                
                oldVolume = videoPlayer.GetComponent<AudioSource>().volume;
                videoPlayer.GetComponent<AudioSource>().volume = 0;
            }
//            notSeeking = false;
            var frame = videoPlayer.frameCount * pct;
            videoPlayer.Pause();
            if (videoQuality != YoutubeVideoQuality.STANDARD)
                audioPlayer.Pause();
            waitAudioSeek = true;

            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                //seekUsingLowQuality = true;
                videoPlayer.frame = (long)frame;
            }
            else
            {
                audioPlayer.frame = (long)frame;
            }
            videoPlayer.Pause();
            if (videoQuality != YoutubeVideoQuality.STANDARD)
                audioPlayer.Pause();
        }

        IEnumerator VideoSeekCall()
        {
            yield return new WaitForSeconds(1f);
            videoPlayer.time = audioPlayer.time;
        }
        //experimental
//        private bool notSeeking = false;

        private void VideoSeeked(VideoPlayer source)
        {
            //notSeeking = true;
            if (!waitAudioSeek)
            {
                if (startedFromTime)
                    StartCoroutine(PlayNowFromTime(2f));
                else
                    StartCoroutine(PlayNow());
            }
            else
            {
                if (startedFromTime)
                    StartCoroutine(PlayNowFromTime(2f));
                else
                    StartCoroutine(PlayNow());
            }
        }


        private void AudioSeeked(VideoPlayer source)
        {
            if (!waitAudioSeek)
            {
                StartCoroutine(VideoSeekCall());
            }
            else
            {
                StartCoroutine(VideoSeekCall());
            }
        }

        public virtual void Play()
        {
            //audio issue fix...check all unity versions.
            if (videoQuality != YoutubeVideoQuality.STANDARD)
            {
                videoPlayer.GetComponent<AudioSource>().volume = oldVolume;
            }
        }

        //experimental
        //bool ignoreDrop = false;

        IEnumerator WaitSync()
        {
            yield return new WaitForSeconds(2f);
            Play();
            Invoke("VerifyFrames", 2);
        }

        IEnumerator PlayNow()
        {
            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                yield return new WaitForSeconds(0);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }

            if (!pauseCalled)
            {
                //dropAlreadyCalled = false;
                //ignoreDrop = true;
                Play();
                StartCoroutine(ReleaseDrop());
            }
            else
            {
                StopCoroutine("PlayNow");
            }
        }

        void CheckIfIsSync()
        {

            //disabled for now, just for test purpouses.
            // if (videoPlayer.frame != audioPlayer.frame && notSeeking && videoPlayer.isPlaying)
            // {
            //     notSeeking = false;
            //     Debug.Log("Out of Sync, trying to sync again");
            //     videoPlayer.Pause();
            //     audioPlayer.Pause();
            //     waitAudioSeek = true;
            //     audioPlayer.frame = videoPlayer.frame;
            // }
        }

        IEnumerator ReleaseDrop()
        {
            yield return new WaitForSeconds(2f);
            //ignoreDrop = false;
        }

        IEnumerator PlayNowFromTime(float time)
        {
            yield return new WaitForSeconds(time);
            startedFromTime = false;
            if (!pauseCalled)
            {
                Play();
            }
            else
            {
                StopCoroutine("PlayNowFromTime");
            }
        }


        //experimental
        //bool dropAlreadyCalled = false;

        private void AudioPlayer_frameDropped(VideoPlayer source)
        {
            //Experimental
            /* if (!Application.isEditor) //Dont run this in editor (too much frame drops..)
         {
             if (!dropAlreadyCalled && !ignoreDrop)
             {
                 dropAlreadyCalled = true;
                 audioPlayer.Pause();
                 videoPlayer.Pause();
                 StartCoroutine(PlayNow());
             }
         }*/
        }

        private void VideoPlayer_frameDropped(VideoPlayer source)
        {
            //Experimental
            /*if (!Application.isEditor) //Dont run this in editor (too much frame drops..)
        {
            if (!dropAlreadyCalled && !ignoreDrop)
            {
                dropAlreadyCalled = true;
                audioPlayer.Pause();
                videoPlayer.Pause();
                StartCoroutine(PlayNow());
            }
        }
        else
        {
            Debug.Log("Frame dropped, skiped for editor!! if this appear too much you can disable this or open and close the editor, it's a bug in the unity video player, we don't know yet what exaclty cause this. This don't happens in builds.");
        }*/
        }
        #endregion

        [HideInInspector]
        public bool checkIfSync = false;


        #region Magic to update the plugin on the go
        private class MagicContent
        {
            public string[] defaultFuncName = {
                @"(?:\b|[^\w$])([\w$]{2})\s*=\s*function\(\s*a\s*\)\s*{\s*a\s*=\s*a\.split\(\s*""""\s*\)",
                @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}",
                @"\b[cs]\s*&&\s*[adf]\.set\([^,]+\s*,\s*encodeURIComponent\s*\(\s*([\w$]+)\("
            };
            public string defaultHtmlJson = @"ytplayer\.config\s*=\s*(\{.+?\});ytplayer";
            public string defaultHtmlPlayerVersion = @"player_ias-(.+?).js";
        }
        private MagicContent magicResult;
        private bool canUpdate = true;
        #endregion
        #endregion

        public void ChacheSystem()
        {
            //Check if have the url generated on cache, if yes check the expire timecode, if it is already expired delete and generate a new one, if no, use this instead to prevent calls to server.
            PlayerPrefs.SetString("id", "url");
            PlayerPrefs.SetInt("id-expire", 1634364304);
        }
    }

    public class YoutubeResultIds
    {
        public string lowQuality;
        public string standardQuality;
        public string mediumQuality;
        public string hdQuality;
        public string fullHdQuality;
        public string ultraHdQuality;
        public string bestFormatWithAudioIncluded;
        public string audioUrl;

    }

    public static class Extensions
    {
        public static T[] Splice<T>(this T[] source, int start)
        {
            var listItems = source.ToList<T>();
            var items = listItems.Skip(start).ToList<T>();
            return items.ToArray<T>();
        }
    }
    
}