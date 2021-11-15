using System;
using UnityEngine;
using UnityEngine.Events;

namespace LightShaft.Scripts
{
    public class YoutubeVideoEvents : MonoBehaviour
    {
        private YoutubePlayer _player;
        [Header("Custom Events To use with video player only")]
        
        [Tooltip("When the url's are loaded")]
        public UrlLoadEvent OnYoutubeUrlAreReady;
        [Tooltip("When the videos are ready to play")]
        public UnityEvent OnVideoReadyToStart;
        [Tooltip("When the video start playing")]
        public UnityEvent OnVideoStarted;
        [Tooltip("When the video resume playing")]
        public UnityEvent OnVideoResumed;
        [Tooltip("When the video pause")]
        public UnityEvent OnVideoPaused;
        [Tooltip("When the video finish")]
        public UnityEvent OnVideoFinished;

        [Header("Event called on desired video time")]
        [Tooltip("If you want to call a custom function on desired video time.")]
        public YoutubeTimedEvent[] videoTimeEvents;


        private void Awake()
        {
            _player = GetComponent<YoutubePlayer>();
        }

        void FixedUpdate()
        {
            if (_player.playUsingInternalDevicePlayer || _player.loadYoutubeUrlsOnly) return;
            if (_player.videoPlayer.frame <= 0 || !_player.videoPlayer.isPlaying) return;
            foreach (var ev in videoTimeEvents)
            {
                if (!ev.Called)
                {
                    if (ev.time <= _player.videoPlayer.time && ev.time > (_player.videoPlayer.time-2))
                    {
                        ev.Called = true;
                        if(ev.pauseVideo)
                            _player.Pause();
                        ev.timeEvent.Invoke();
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class YoutubeTimedEvent
{
    public int time;
    public bool pauseVideo = false;
    public UnityEvent timeEvent;
    
    private bool called;

    public bool Called
    {
        get => called;
        set => called = value;
    }
}

[System.Serializable]
public class UrlLoadEvent : UnityEvent<string>
{
}