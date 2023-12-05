using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UI.HUD;
using UnityEngine;
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

            // URL is invalid
            Uri uriResult;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
            {
                Debug.LogError($"URL provided is not a valid URL. URL provided: {url}");
                return false;
            }
            // URL does not use HTTPS
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

            while (videoPlayer.isPlaying || paused)
            {
                yield return null;
            }

            /*
            int prevFrame = -1;
            float timeBuffering = 0.0f;
            int attemptsToRestart = 0;

            // Video player was cutting the video short instead of buffering when it didn't get frames quickly enough so this is the hack to get around it
            while (_videoPlayer.isPlaying || (int)_videoPlayer.frame < (int)_videoPlayer.frameCount - 2) // to prevent hanging on last frame??
            {
                if (networkManager && networkManager.isInDevMode && Input.GetKeyDown(networkManager.skipVideoKeyCode))
                {
                    break;
                }

                // Manually check for buffering, if we get stuck for longer than the timeout attempt to restart or interupt the video and print an error
                // This will prevent the callback from firing, which means that the video will not register as completed and the player should be able to rewatch it
                if ((int)_videoPlayer.frame == prevFrame)
                {
                    timeBuffering += Time.deltaTime;
                    if (timeBuffering > videoTimeout)
                    {
                        // TODO: display these log statement to player
                        Debug.LogError("Sensor station video system timed out on frame" + prevFrame + " of " + _videoPlayer.frameCount + " after " + videoTimeout + " seconds");

                        // TODO: This should really be checking how long since the last timeout instead of just using an arbitrary fixed number of attempts
                        //       if it immediatly fails to get new frames a few times in a row, quit trying, if it works for a while then hangs, keep restarting
                        if (attemptsToRestart <= maximumAttemptsToRestartVideo)
                        {
                            Debug.LogError("Attempting to restart video...");
                            attemptsToRestart++;

                            // Attempt to restart the video from where it timed out
                            _videoPlayer.Stop();
                            yield return null; // give it a frame to Stop

                            _videoPlayer.frame = prevFrame;
                            _videoPlayer.Prepare();
                            while (!_videoPlayer.isPrepared)
                            {

                                yield return null;
                            }

                            _videoPlayer.Play();
                            timeBuffering = 0.0f;
                        }
                        else
                        {
                            InterruptVideo(); // this call will stop the coroutine
                        }
                    }
                }
                else
                {
                    timeBuffering = 0.0f;
                    prevFrame = (int)_videoPlayer.frame;
                }

                yield return null;
            }
            */

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

