using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UI.HUD;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace Managers
{
    // Wrapper for Unity's video player that handles buffering, audio desync, and other problems
    // Used by the cutscene system for jump videos and mission log videos and sensor station for transmissions
    public class VideoPlayerManager : ConnectedSingleton<VideoPlayerManager>
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VideoControls videoControls;

        [SerializeField] private float videoTimeout = 6.0f;

        private Coroutine currentVideoCoroutine = null;
        private bool autoPlayVideoOnPrepare = false;
        private bool videoPlayerInitialized = false;
        [HideInInspector] public string preparedVideoURL = "";

        public static Action<string, bool> OnVideoCompleted; // (url, didVideoFinish?)

        private bool paused = false;

        private void OnEnable()
        {
            videoPlayer.errorReceived += OnErrorReceived;
            videoPlayer.prepareCompleted += OnPlayerPrepared;
            videoControls.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            videoPlayer.errorReceived -= OnErrorReceived;
            videoPlayer.prepareCompleted -= OnPlayerPrepared;
        }
 
        public bool InitializeVideo(string url, RenderTexture tex, bool playAfterPrepared, bool showControls = false)
        {
            // For testing:
            // url = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

            if (videoPlayer == null)
            {
                Debug.LogError("No video player set for VideoPlayerManager!");
                return false;
            }
            else if (tex == null)
            {
                Debug.LogError("No render texture provided for InitializeVideo call");
                return false;
            }
            else if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("No URL provided for InitializeVideo call");
                return false;
            }
            else if (currentVideoCoroutine != null)
            {
                // Stop an existing cutscene and the video player
                Debug.LogWarning("Trying to prepare video while another video is playing! Stopping the previous video.");
                StopVideo();
            }

            // Check if URL is invalid
            Uri uriResult;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
            {
                Debug.LogError($"URL provided is not a valid URL. URL provided: {url}");
                return false;
            }
            // URL does not use HTTPS or HTTP
            else if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError($"URL provided does not use HTTP or HTTPS. URL provided: {url}");
                return false;
            }
            // URL is not a video
            else if (!Regex.IsMatch(url, ".*\\.(avi|mpg|rm|mov|wav|asf|3gp|mkv|rmvb|mp4|ogg|mp3|oga|aac|mpeg|webm)", RegexOptions.IgnoreCase))
            {
                Debug.LogError($"URL provided is not a video. URL provided: {url}");
                return false;
            }

            videoControls.gameObject.SetActive(showControls);
            paused = false;
            videoPlayer.playbackSpeed = 1.0f;

            // Resets texture to black, maybe not the right way to do this
            tex.Release();

            videoPlayer.targetTexture = tex;
            videoPlayer.url = url;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayerInitialized = true;
            preparedVideoURL = url;

            autoPlayVideoOnPrepare = playAfterPrepared;
            videoPlayer.Prepare();

            return true;
        }

        // Will play the most recently prepared video to the most recently provided render texture
        // Use preparedVideoURL to check and make sure you're about to play the right thing
        public void PlayVideo()
        {
            if (!videoPlayerInitialized)
            {
                Debug.LogError("Tried to play a video that has not been prepared");
                return;
            }
            currentVideoCoroutine = StartCoroutine(PlayVideoCoroutine());
        }

        private IEnumerator PlayVideoCoroutine()
        {
            // Wait until video has finished preparing if needed
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(true);

            /*
            while (videoPlayer.isPlaying || paused)
            {
                yield return null;
            }
            */

            /*
            UnityWebRequest webRequest = UnityWebRequest.Head(videoPlayer.url);
            webRequest.Send();
            while (!webRequest.isDone)
            {
                yield return null;
            }
            Debug.Log("Diagnostic info for video at: " + videoPlayer.url);
            Debug.Log("File size: " + webRequest.GetResponseHeader("Content-Length") + " bytes");
            Debug.Log("Frame count: " + videoPlayer.frameCount);
            Debug.Log("Frame rate: " + videoPlayer.frameRate);
            Debug.Log("Length: " + videoPlayer.length + " seconds");
            */

            int prevFrame = -1;
            float timeSinceLastNewFrame = 0.0f;
            float warningSecond = videoTimeout / 2.0f;
            
            AudioSource videoAudioSource = videoPlayer.GetTargetAudioSource(0);
            videoPlayer.frameDropped += (_) => { Debug.LogError("Video player dropped a frame!"); };

            while (videoPlayer.isPlaying || paused)
            {
                if (videoPlayer.frame == prevFrame)
                {
                    timeSinceLastNewFrame += Time.deltaTime;
                    if (timeSinceLastNewFrame > videoTimeout)
                    {
                        // TODO: display this log statement to player in game and provide an exit/restart button as a failsafe
                        Debug.LogError("Video player timed out on frame" + prevFrame + " of " + videoPlayer.frameCount + " after " + timeSinceLastNewFrame + " seconds");
                        StopVideo();
                    }
                    else if (timeSinceLastNewFrame > warningSecond)
                    {
                        Debug.LogWarning("Video player has received 0 new frames in " + warningSecond + " seconds. Video will timeout and exit after " + (videoTimeout - warningSecond) + " more seconds.");
                    }
                }
                else
                {
                    timeSinceLastNewFrame = 0.0f;
                    prevFrame = (int)videoPlayer.frame;
                }

                if (MathF.Abs((float)videoPlayer.time - videoAudioSource.time) > 0.25f)
                {
                    Debug.LogError("Audio out of sync!");
                }

                yield return null;
            }

            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);
            OnVideoCompleted.Invoke(videoPlayer.url, true);
            currentVideoCoroutine = null;

            videoPlayer.playbackSpeed = 1.0f;
            paused = false;
            videoControls.gameObject.SetActive(false);
        }

        public void StopVideo()
        {
            if (currentVideoCoroutine != null)
            {
                StopCoroutine(currentVideoCoroutine);
                currentVideoCoroutine = null;

                Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);

                videoPlayer.Stop();

                videoPlayer.playbackSpeed = 1.0f;
                paused = false;
                videoControls.gameObject.SetActive(false);

                OnVideoCompleted.Invoke(videoPlayer.url, false);
            }
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            if (currentVideoCoroutine != null)
            {
                StopVideo();
            }
        }

        private void OnPlayerPrepared(VideoPlayer source)
        {
            if (autoPlayVideoOnPrepare)
            {
                PlayVideo();
            }
        }

        public void SetPause(bool shouldPause)
        {
            if (currentVideoCoroutine == null)
            {
                return;
            }

            if (shouldPause && !paused)
            {
                paused = true;
                SetPlaybackSpeed(0.0f);
            }
            else if (!shouldPause && paused)
            {
                paused = false;
                SetPlaybackSpeed(1.0f);
            }
        }

        public void SetPlaybackSpeed(float speed)
        {
            if (currentVideoCoroutine == null)
            {
                return;
            }

            videoPlayer.playbackSpeed = speed;
        }

        public void Rewind(float seconds)
        {
            if (currentVideoCoroutine == null)
            {
                return;
            }

            if (seconds > videoPlayer.time)
            {
                videoPlayer.time = 0;
            }
            else
            {
                videoPlayer.time -= seconds;
            }
        }
    }
}

