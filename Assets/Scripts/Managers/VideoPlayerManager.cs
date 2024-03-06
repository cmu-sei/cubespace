using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace Managers
{
    // Wrapper for Unity's video player that handles buffering, audio desync, and other problems
    // Used by the cutscene system for jump videos and mission log videos and sensor station for transmissions
    public class VideoPlayerManager : ConnectedSingleton<VideoPlayerManager>
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VideoControls videoControls;

        [SerializeField] AudioMixerGroup transmissionsMixerGroup;
        private AudioSource audioSource;

        [SerializeField] private float videoTimeout = 6.0f;

        private Coroutine currentVideoCoroutine = null;
        private bool autoPlayVideoOnPrepare = false;
        private bool videoPlayerInitialized = false;
        [HideInInspector] public string preparedVideoURL = "";

        public static Action<string, bool> OnVideoCompleted; // (url, didVideoFinish?)

        private bool paused = false;

        public override void Start()
        {
            base.Start();

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = transmissionsMixerGroup;

            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.playOnAwake = false;
            audioSource.mute = false;
            audioSource.spatialBlend = 0;
            audioSource.volume = 1;
            audioSource.pitch = 1;
            audioSource.bypassReverbZones = false;

            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

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
                // This message was getting printed after videos succesuflly played and I can't see why it would be. Commenting out for now until something starts breaking
                //Debug.LogError("No URL provided for InitializeVideo call");
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
                Debug.LogError($"URL provided is not a valid URL. URL provided: '{url}'");
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

            int prevFrame = -1;
            float timeSinceLastNewFrame = 0.0f;
            
            int droppedFrames = 0;
            videoPlayer.frameDropped += (_) => { droppedFrames += 1; };

            while (videoPlayer.isPlaying || paused)
            {
                if (videoPlayer.frame == prevFrame)
                {
                    timeSinceLastNewFrame += Time.deltaTime;
                    if (timeSinceLastNewFrame >= videoTimeout)
                    {
                        Debug.LogError("Video player timed out on frame" + prevFrame + " of " + videoPlayer.frameCount + " after " + timeSinceLastNewFrame + " seconds");
                        StopVideo();
                    }

                    // Streamed videos in webgl like to get stuck on the last few frames so just cut those off if that happens
                    if ((int)videoPlayer.frameCount - videoPlayer.frame <= 6 && timeSinceLastNewFrame > 0.2f)
                    {
                        break;
                    }
                }
                else
                {
                    timeSinceLastNewFrame = 0.0f;
                    prevFrame = (int)videoPlayer.frame;
                }

                yield return null;
            }
            
            if (droppedFrames > 10)
            {
                // The count this is reporting is unreliable and almost certainly incorrect most of the time so I'm not going to report it anymore
                //Debug.LogWarning("Video dropped " + droppedFrames + " frames!");
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

        // Should only be used for WebGL, where it is necessary since video player audio can't be routed through the main mixer in WebGL
        public void SetVolume(float val)
        {
            videoPlayer.SetDirectAudioVolume(0, val);
        }
    }
}

