/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UI.HUD;
using Managers;
using Mirror;

namespace Entities.Workstations.SensorStationParts
{
    /// <summary>
    /// The system which dictates what video to play.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class SensorStationVideoSystem : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The event invoked when the video finishes playing.
        /// </summary>
        /// <param name="urlOfVideoFinished">The URL of the video that just finished.</param>
        public delegate void VideoEvent(string urlOfVideoFinished);

        /// <summary>
        /// The video player used to play the video.
        /// </summary>
        private VideoPlayer _videoPlayer;
        /// <summary>
        /// The current video playing.
        /// </summary>
        private Coroutine currentVideoCoroutine;
        /// <summary>
        /// The CustomNetworkManager within the game.
        /// </summary>
        private CustomNetworkManager networkManager;

        private float videoTimeout = 10.0f;

        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the VideoPlayer and CustomNetworkManager and sets attributes of the video player.
        /// </summary>
        private void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _videoPlayer.isLooping = false;
            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

            if (!_videoPlayer || !_videoPlayer.targetTexture || !networkManager)
            {
                Debug.LogError("Error in sensor station video system initialization");
                return;
            }
        }
        #endregion

        #region Video playback methods
        /// <summary>
        /// Prepares, then plays the video.
        /// </summary>
        /// <param name="url">The URL of the video that should play.</param>
        /// <param name="callback">The callback method to invoke within a coroutine upon finishing the video.</param>
        public void PlayVideo(string url, VideoEvent callback)
        {
            _videoPlayer.targetTexture.Release();
            _videoPlayer.url = url;
            _videoPlayer.EnableAudioTrack(0, true);
            _videoPlayer.Prepare();
            currentVideoCoroutine = StartCoroutine(VideoPlayCoroutine(url, callback));
        }

        /// <summary>
        /// Stops playing the video.
        /// </summary>
        public void InterruptVideo()
        {
            if (currentVideoCoroutine != null)
            {
                UIExitWorkstationButton.Instance.SetHiddenByVideo(false);
                StopCoroutine(currentVideoCoroutine);
            }
            if (_videoPlayer.isPlaying)
            {
                UIExitWorkstationButton.Instance.SetHiddenByVideo(false);
                _videoPlayer.Stop();
            }
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);
        }

        /// <summary>
        /// Plays a video and then waits for it to finish.
        /// </summary>
        /// <param name="url">The URL of the video that should play.</param>
        /// <param name="callback">The callback method to invoke upon finishing the video.</param>
        /// <returns>A yield return while playing the video.</returns>
        private IEnumerator VideoPlayCoroutine(string url, VideoEvent callback)
        {
            // can't easily use built-in prepareCompleted callback because we need to pass along the url/callback. This is simpler
            while (!_videoPlayer.isPrepared)
            {
                yield return null;
            }
            // This gets called after prepare has completed, so there should be an accurate count set by now
            //Debug.Log("Playing video with frame count == " + _videoPlayer.frameCount);

            UIExitWorkstationButton.Instance.SetHiddenByVideo(true);

            _videoPlayer.Play();
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(true);

            int prevFrame = 0;
            float timeBuffering = 0.0f;

            // Video player was cutting the video short instead of buffering when it didn't get frames quickly enough so this is the hack to get around it
            while (_videoPlayer.isPlaying || (int)_videoPlayer.frame < (int)_videoPlayer.frameCount - 1)
            {
                // Videos are 24fps, so this should print roughly once a second
                /*
                if (_videoPlayer.frame % 24 == 0)
                {
                    Debug.Log("Current frame: " + _videoPlayer.frame + "\nTotal frames: " + _videoPlayer.frameCount);
                }
                */

                if (networkManager && networkManager.isInDevMode && Input.GetKeyDown(networkManager.skipVideoKeyCode))
                {
                    break;
                }

                // Manually check for buffering, if we get stuck for longer than the timeout interupt the video and print an error
                // This will prevent the callback from firing, which means that the video will not register as completed and the player should be able to rewatch it
                if ((int)_videoPlayer.frame == prevFrame)
                {
                    timeBuffering += Time.deltaTime;
                    if (timeBuffering > videoTimeout)
                    {
                        Debug.LogError("Sensor station video system timed out on frame" + prevFrame + " of " + _videoPlayer.frameCount + " after " + videoTimeout + " seconds");
                        InterruptVideo(); // this call will stop the coroutine
                    }
                }
                else
                {
                    prevFrame = (int)_videoPlayer.frame;
                }

                yield return null;
            }

            UIExitWorkstationButton.Instance.SetHiddenByVideo(false);
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);
            callback.Invoke(url);
        }

        /// <summary>
        /// Sets the volume of the video.
        /// </summary>
        /// <param name="value">The new volume the video should have.</param>
        public void SetVideoVolume(float value)
        {
            _videoPlayer.SetDirectAudioVolume(0, value);
        }
        #endregion
    }
}

