using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LightShaft.Scripts
{
    public class YoutubePlayer : YoutubeSettings
    {
        public override void Start()
        {
            base.Start();
            //Register Events
            if (!playUsingInternalDevicePlayer)
            {
                _events.OnYoutubeUrlAreReady.AddListener(UrlReadyToUse);
                _events.OnVideoFinished.AddListener(OnVideoPlayerFinished);
                _events.OnVideoReadyToStart.AddListener(OnVideoLoaded);
            }
        }

        ///<summary>This function is callback only, only will be called when the on url are ready to use.</summary>
        private void UrlReadyToUse(string urlToUse)
        {
            if (loadYoutubeUrlsOnly)
            {
                Debug.Log("Here you can call your external video player if you want, passing that two variables:");
                if (videoQuality != YoutubeVideoQuality.STANDARD)
                {
                    Debug.Log("Your video Url: " + urlToUse);
                    Debug.Log("Your audio video Url: " + audioUrl);
                }
                else
                {
                    Debug.Log("You video Url:" + urlToUse);
                }
            }
        }

        ///<summary>Get the video title, but it need to be loaded first.</summary>
        public string GetVideoTitle()
        {
            return videoTitle;
        }

        ///<summary>Load the url only, dont play!.</summary>
        public void LoadUrl(string url)
        {
            logTest = "Getting URL";
            Stop();
            loadYoutubeUrlsOnly = true;
            PlayYoutubeVideo(url);
        }

        ///<summary>Load the video without play, good for when you want just to prepare the video to play later.</summary>
        public void PreLoadVideo(string url)
        {
            logTest = "Getting URL";
            Stop();
            prepareVideoToPlayLater = true;
            autoPlayOnStart = false;
            PlayYoutubeVideo(url);

        }

        ///<summary>Play the loaded video from time.</summary>
        public void Play(int startTime)
        {
            startFromSecond = true;
            startFromSecondTime = startTime;
            DisableThumbnailObject();
            pauseCalled = false;
            _events.OnVideoStarted.Invoke();
            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.Play();
                if (!noAudioAtacched)
                    audioPlayer.Play();
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

        }

        ///<summary>Load and Play the video from youtube.</summary>
        public void Play(string url)
        {
            logTest = "Getting URL";
            Stop();
            PlayYoutubeVideo(url);
        }

        ///<summary>Load and Play a custom playlist.</summary>
        public void Play(string[] playlistUrls)
        {
            logTest = "Getting URL";
            Stop();
            customPlaylist = true;
            youtubeUrls = playlistUrls;
            PlayYoutubeVideo(playlistUrls[currentUrlIndex]);
        }

        ///<summary>Play the loaded video.</summary>
        public override void Play()
        {
            base.Play();
            _events.OnVideoStarted.Invoke();
            DisableThumbnailObject();
            pauseCalled = false;
            if (videoQuality == YoutubeVideoQuality.STANDARD)
            {
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.Play();
                if (!noAudioAtacched)
                {
                    //audioPlayer.Play(); //TODO check other unity versions
                    StartCoroutine(DelayPlay());
                }
            }
        }

        ///<summary>Load and Play the video from youtube, starting from desired second.</summary>
        public void Play(string url, int startFrom)
        {
            logTest = "Getting URL";
            startFromSecond = true;
            startFromSecondTime = startFrom;
            Stop();
            PlayYoutubeVideo(url);
        }

        ///<summary>Play or Pause the active videoplayer.</summary>
        public void PlayPause()
        {
            if (youtubeUrlReady && videoPlayer.isPrepared)
            {
                if (!pauseCalled)
                {
                    _events.OnVideoPaused.Invoke();
                    Pause();
                }
                else
                {
                    //resume
                    _events.OnVideoResumed.Invoke();
                    Play();
                }
            }
        }

        ///<summary>Change the video rendering to fullscreen or back to material renderer.</summary>
        public void ToogleFullsScreenMode()
        {
            fullscreenModeEnabled = !fullscreenModeEnabled;

            if (!fullscreenModeEnabled)
            {
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                if (videoPlayer.targetCamera == null)
                {
                    videoPlayer.targetCamera = mainCamera;
                }
            }
            else
            {
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            }
        }

        ///<summary>Called when the video end.</summary>
        public void OnVideoPlayerFinished()
        {
            if (!finishedCalled)
            {
                Debug.Log(("video finished..."));
                finishedCalled = true;
                StartCoroutine(PreventFinishToBeCalledTwoTimes());
                if (!loadYoutubeUrlsOnly)
                {
                    if (videoPlayer.isPrepared)
                    {
                        if (debug)
                            Debug.Log("Finished");
                        if (videoPlayer.isLooping)
                        {
                            videoPlayer.time = 0;
                            videoPlayer.frame = 0;
                            audioPlayer.time = 0;
                            audioPlayer.frame = 0;
                            videoPlayer.Play();
                            if (!noAudioAtacched)
                                audioPlayer.Play();
                        }
                        CancelInvoke("CheckIfIsSync");
                        _events.OnVideoFinished.Invoke();

                        if (customPlaylist && autoPlayNextVideo)
                        {
                            Debug.Log("Calling next video of playlist");
                            CallNextUrl();
                        }
                    }
                }
                else
                {
                    if (playUsingInternalDevicePlayer)
                    {
                        CancelInvoke("CheckIfIsSync");
                        _events.OnVideoFinished.Invoke();
                    }
                }
            }
        }

        ///<summary>Just a simple callback function to know when the video is loaded and ready to hit play(), you can use the unity events too.</summary>
        private void OnVideoLoaded()
        {
            if (_controller.useSliderToProgressVideo)
            {
                if (_controller.playbackSlider == null)
                {
                    _controller.showPlayerControl = false;  //Disable player controller because there is not playback controller attached;
                }else
                    _controller.playbackSlider.maxValue = Mathf.RoundToInt(videoPlayer.frameCount / videoPlayer.frameRate);
            }

            if (_events != null)
            {
                if (_events.videoTimeEvents.Length > 0)
                {
                    foreach (var ev in _events.videoTimeEvents)
                    {
                        ev.Called = false; //reset timed events if we have it.
                    }
                }
            }
            Debug.Log("The video is ready to play");
        }

        ///<summary>Call the next url of the playlist.</summary>
        public void CallNextUrl()
        {
            if (!customPlaylist)
                return;
            if ((currentUrlIndex + 1) < youtubeUrls.Length)
            {
                currentUrlIndex++;
            }
            else
            {
                //reset
                currentUrlIndex = 0;
            }

            PlayYoutubeVideo(youtubeUrls[currentUrlIndex]);
        }

        public void CallPreviousUrl()
        {
            if (!customPlaylist)
                return;
            if ((currentUrlIndex - 1) > 0)
            {
                currentUrlIndex--;
            }
            else
            {
                currentUrlIndex = 0;
            }
            PlayYoutubeVideo(youtubeUrls[currentUrlIndex]);
        }

        //A workaround for mobile bugs.
        private void OnApplicationPause(bool pause)
        {
            if (!playUsingInternalDevicePlayer && !loadYoutubeUrlsOnly)
            {
                if (videoPlayer.isPrepared)
                {
                    if (audioPlayer != null)
                        audioPlayer.Pause();

                    videoPlayer.Pause();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (!playUsingInternalDevicePlayer)
            {
                _events.OnYoutubeUrlAreReady.RemoveListener(UrlReadyToUse);
                _events.OnVideoFinished.RemoveListener(OnVideoPlayerFinished);
                _events.OnVideoReadyToStart.RemoveListener(OnVideoLoaded);
            }
        }

        //A workaround for mobile bugs.
        //private void OnApplicationFocus(bool focus)
        //{
        //    if (focus == true)
        //    {
        //        if (!playUsingInternalDevicePlayer && !loadYoutubeUrlsOnly && !pauseCalled)
        //        {
        //            if (videoPlayer.isPrepared)
        //            {
        //                if (audioPlayer != null)
        //                {
        //                    if (!noAudioAtacched && (videoQuality != YoutubeVideoQuality.STANDARD))
        //                        audioPlayer.Play();
        //                }
        //                videoPlayer.Play();
        //            }
        //        }
        //    }
        //}
        //A workaround for mobile bugs.
        private void OnEnable()
        {
            if (autoPlayOnEnable && !pauseCalled)
            {
                StartCoroutine(WaitThingsGetDone());
            }
        }
        //A workaround for mobile bugs.
        private IEnumerator WaitThingsGetDone()
        {
            yield return new WaitForSeconds(1);
            if (youtubeUrlReady && videoPlayer.isPrepared)
            {
                Play();
            }
            else
            {
                if (!youtubeUrlReady)
                    Play(youtubeUrl);

            }

        }
    }
    
}