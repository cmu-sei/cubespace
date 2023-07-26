/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Entities;
using UnityEngine.Video;
using Managers;
using System.Text.RegularExpressions;
using Mirror;
using UnityEngine.UI;

namespace Systems
{
    /// <summary>
    /// Class that plays videos, either as on-demand playback from a player or because the ship has jumped.
    /// </summary>
    public class CutsceneSystem : ConnectedSingleton<CutsceneSystem>
    {
        [Header("GameObject References")]
        // The video player where playback occurs
        [SerializeField]
        private VideoPlayer _videoPlayer;
        // The GameObject holding the Canvas containing the video
        [SerializeField]
        private GameObject canvasObject;
        // The Canvas object that contains the visual components of the video
        [SerializeField]
        private Canvas _videoCanvas;
        // The Canvas object that contains pause, rewind, and fast forward controls for the video
        [SerializeField]
        private Canvas _videoControls;
        // The camera that renders the cutscenes on the screen
        [SerializeField]
        private CinemachineVirtualCamera _cutsceneCam;

        [Header("Images on Video Controls")]
        // The image displayed on the pause button, swapping between pause and play icons depending on playback state
        [SerializeField]
        private Image pauseImage;
        // The specific pause icon
        [SerializeField]
        private Sprite pauseSprite;
        // The specific play icon
        [SerializeField]
        private Sprite playSprite;

        [Header("")]
        // The video screen shown while watching a previous video stored in the Mission Log
        [SerializeField]
        private GameObject archiveVideoScreen;
        // The video screen shown while jumping from one location to another
        [SerializeField]
        private GameObject jumpVideoScreen;

        // The URL of the jump cutscene
        private string _cutsceneURL = "";
        // Whether to play the cutscene immediately after preparing it; false until a video is received
        private bool autoplayAfterPrepare = false;
        // Whether playback is currently paused
        private bool paused = false;
        // The cutscene currently playing, if any
        private Coroutine _cutsceneCoroutine = null;
        // A reference to the main network manager
        private CustomNetworkManager networkManager;

        // An action called when beginning to play a cutscene
        public static Action OnCutsceneStart;
        // An action called when exiting a cutscene
        public static Action OnCutsceneExit;
        // An action called when a cutscene finishes playing
        public static Action OnCutsceneFinish;

        #region Unity event methods
        /// <summary>
        /// Unity event function that subscribes to the video player's error action and gets a reference to the CustomNetworkManager.
        /// </summary>
        private void OnEnable()
        {
            // Subscribe to the error action called by the video player when it gets an error
            _videoPlayer.errorReceived += CloseVideoItems;
            // Set a new loading message
            LoadingSystem.Instance.UpdateLoadingMessage("Ocularizing Information...");
            // Wait until the ShipStateManager exists
            StartCoroutine(WaitForShipStateManager());
            // Get a reference to the CustomNetworkManager
            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
        }

        /// <summary>
        /// Unity event function that unsubscribes from the video player's preparation action and the global launch action.
        /// </summary>
        private void OnDisable()
        {
            _videoPlayer.prepareCompleted -= VideoPlayerOnPrepareCompleted;
            ShipStateManager.OnLaunch -= PlayLaunchCutscene;
        }

        /// <summary>
        /// Unity event function that unsubscribes from the video player's error action.
        /// </summary>
        private void OnDestroy()
        {
            _videoPlayer.errorReceived -= CloseVideoItems;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Waits until the ShipStateManager exists before subscribing to actions and then preparing a cutscene.
        /// </summary>
        /// <returns>A yield while waiting for the ShipStateManager to exist.</returns>
        private IEnumerator WaitForShipStateManager()
        {
            // Wait until the ShipStateManager exists
            yield return new WaitWhile(() => ShipStateManager.Instance == null);

            // If the ShipStateManager is running on a client instance, prepare cutscene items
            if (!ShipStateManager.Instance.isServerOnly)
            {
                // Subscribe to play the jump cutscene when the ship jumps
                ShipStateManager.OnLaunch += PlayLaunchCutscene;

                // Subscribe to possibly play the cutscene after the video player is prepared
                _videoPlayer.prepareCompleted += VideoPlayerOnPrepareCompleted;

                // Set the Canvas to be visible
                canvasObject.SetActive(true);
                // Disable the visibility of all video items
                _videoCanvas.enabled = false;
                _cutsceneCam.enabled = false;
                _videoControls.enabled = false;

                // If the cutscene URL has already been specified, begin preparing it (but don't autoplay it)
                if (_cutsceneURL != "")
                {
                    PrepareWebCutscene(_cutsceneURL, false);
                }
            }
            // Otherwise, just disable video items, because videos should not be played on the server
            else
            {
                canvasObject.SetActive(false);
            }
        }

        /// <summary>
        /// Prepares the video player to play back a video from a given URL.
        /// </summary>
        /// <param name="videoURL">The web URL where the video is located.</param>
        /// <param name="playAfterPrepared">Whether to autoplay this video after it is finished preparing.</param>
        /// <param name="hasOverlay">Whether this video should have an overlay. False by default.</param>
        /// <returns>Whether the preparation was successful.</returns>
        public bool PrepareWebCutscene(string videoURL, bool playAfterPrepared = true, bool hasOverlay = false)
        {
            Debug.Log("Preparing cutscene at this url: " + videoURL);

            // No video player set
            if (!_videoPlayer || !_videoPlayer.targetTexture)
            {
                Debug.LogError("No video player or render texture set for CutsceneSystem!");
                return false;
            }
            // Already trying to play another video
            else if (_cutsceneCoroutine != null)
            {
                // Stop an existing cutscene and the video player
                Debug.LogWarning("Trying to prepare video while another cutscene is playing! Cancelling the previous cutscene.");
                StopCoroutine(_cutsceneCoroutine);
                _videoPlayer.Stop();
                // Disable some video controls
                _videoCanvas.enabled = false;
                _cutsceneCam.enabled = false;
            }

            // No URL provided
            if (string.IsNullOrEmpty(videoURL))
            {
                Debug.LogError("no URL, can't play video");
                return false;
            }

            // URL is invalid
            Uri uriResult;
            if (!Uri.TryCreate(videoURL, UriKind.Absolute, out uriResult))
            {
                Debug.LogError($"URL provided is not a valid URL. URL provided: {videoURL}");
                return false;
            }

            // URL does not use HTTPS
            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError($"URL provided does not use HTTP or HTTPS. URL provided: {videoURL}");
                return false;
            }

            // URL is not a video
            if (!Regex.IsMatch(videoURL, ".*\\.(avi|mpg|rm|mov|wav|asf|3gp|mkv|rmvb|mp4|ogg|mp3|oga|aac|mpeg|webm)", RegexOptions.IgnoreCase))
            {
                Debug.LogError($"URL provided is not a video. URL provided: {videoURL}");
                return false;
            }

            // Enable the correct screen based on whether the cutscene should use an overlay
            archiveVideoScreen.SetActive(hasOverlay);
            jumpVideoScreen.SetActive(!hasOverlay);

            // Enalbe the video controls based on whether the cutscene should use an overlay
            _videoControls.enabled = hasOverlay;

            // Set attributes of the video player
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.isLooping = false;
            _videoPlayer.targetTexture.Release();
            _videoPlayer.url = videoURL;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _videoPlayer.EnableAudioTrack(0, true);
            autoplayAfterPrepare = playAfterPrepared;

            // Prepare the video
            Debug.Log("Right before calling prepare");
            _videoPlayer.prepareCompleted += (_) => { Debug.Log("Cutscene system finished prepping~~~"); };
            _videoPlayer.Prepare();
            

            return true;
        }
        #endregion

        #region Action methods
        /// <summary>
        /// Plays a cutscene when the ship jumps to a location.
        /// </summary>
        private void PlayLaunchCutscene()
        {
            PrepareWebCutscene(ShipStateManager.Instance.Session.jumpCutsceneURL, true, false);
        }

        /// <summary>
        /// Closes and disables video items.
        /// </summary>
        /// <param name="source">The VideoPlayer. Unused, but necessary.</param>
        /// <param name="message">The message from the action. Unused, but necessary.</param>
        void CloseVideoItems(VideoPlayer source, string message)
        {
            canvasObject.SetActive(true);

            _videoCanvas.enabled = false;
            _cutsceneCam.enabled = false;
            _videoControls.enabled = false;
        }

        /// <summary>
        /// Plays a cutscene when the video player finishes preparing, if set to autoplay.
        /// </summary>
        /// <param name="source">The VideoPlayer. Unused, but necessary.</param>
        private void VideoPlayerOnPrepareCompleted(VideoPlayer source)
        {
            if (autoplayAfterPrepare)
            {
                PlayCutscene();
            }
        }
        #endregion

        #region Playback methods
        /// <summary>
        /// Plays the prepared cutscene.
        /// </summary>
        public void PlayCutscene()
        {
            Debug.Log("PLAYING CUTSCENE");

            // Call an action before the cutscene begins playing
            OnCutsceneStart?.Invoke();
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(true);
            // If the video player is not prepared yet, stop an attempt to play the cutscene
            if (!_videoPlayer.isPrepared)
            {
                Debug.LogError("Cutscene video player not ready!");
                return;
            }
            // Otherwise, if there is already a cutscene playing, stop an attempt to play the cutscene
            else if (_cutsceneCoroutine != null)
            {
                Debug.LogWarning("Cutscene video played while another was playing! Player must be re-prepared. Ignoring request to play a cutscene.");
                return;
            }

            // Play the cutscene
            _cutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine());
        }

        /// <summary>
        /// Plays the cutscene and performs actions necessary while playing the cutscene.
        /// </summary>
        /// <returns>A yield while waiting for a video to stop playing or unpause.</returns>
        private IEnumerator PlayCutsceneCoroutine()
        {
            Debug.Log("COROUTINE STARTED");
            // Lock the local player's input
            Player.LockLocalPlayerInput();
            // Reset the starting frame of the video
            _videoPlayer.frame = 0;
            // Play the video
            _videoPlayer.Play();
            // Enable the video canvas and the cutscene camera
            _videoCanvas.enabled = true;
            _cutsceneCam.enabled = true;
            // Reset the playback speed
            _videoPlayer.playbackSpeed = 1f;

            // Wait while a video is playing
            while (_videoPlayer.isPlaying)
            {
                // If in dev mode and a skip hotkey is pressed, skip the cutscene
                if (networkManager && networkManager.isInDevMode && Input.GetKeyDown(networkManager.skipVideoKeyCode))
                {
                    break;
                }

                yield return null;
            }

            // If the Mission Log is open, lock player input
            if (UI.HUD.HUDController.IsPanelOpen)
            {
                Player.LockLocalPlayerInput();
            }
            // Otherwise, unlock it
            else
            {
                Player.UnlockLocalPlayerInput();
            }

            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);

            // Call an action on the cutscene finishing
            OnCutsceneFinish?.Invoke();

            // Remove the cutscene coroutine
            _cutsceneCoroutine = null;

            // Disable the canvases and camera
            _videoCanvas.enabled = false;
            _cutsceneCam.enabled = false;
            _videoControls.enabled = false;
        }
        #endregion

        #region Video controls methods
        /// <summary>
        /// Sets whether the video is paused.
        /// </summary>
        public void FlipPaused()
        {
            // Set the correct sprite on the pause button image depending on if the video is paused or not
            pauseImage.sprite = paused ? pauseSprite : playSprite;

            // If the video is now paused, physically pause the video
            _videoPlayer.playbackSpeed = paused ? 1.0f : 0.0f;
            paused = !paused;
        }

        /// <summary>
        /// Closes the video window.
        /// </summary>
        public void ExitVideo()
        {
            // Stop the video player and unpause it (if paused)
            _videoPlayer.Stop();
            pauseImage.sprite = pauseSprite;
            _videoPlayer.playbackSpeed = 1f;
            paused = false;
            Audio.AudioPlayer.Instance.SetMuteSFXSnapshot(false);
            // Call an action for exiting the cutscene
            OnCutsceneExit?.Invoke();
            // If the Mission Log is open, re-lock the local player's input
            if (UI.HUD.HUDController.IsPanelOpen)
            {
                Player.LockLocalPlayerInput();
            }
            // Otherwise, unlock the local player's input
            else
            {
                Player.UnlockLocalPlayerInput();
            }
        }

        /// <summary>
        /// Fast forwards the video, or sets it back to its original speed if already fast forwarding.
        /// </summary>
        public void FastForward()
        {
            if (_videoPlayer.playbackSpeed == 2f)
            {
                _videoPlayer.playbackSpeed = 1f;
            }
            else
            {
                _videoPlayer.playbackSpeed = 2f;
            }
        }

        /// <summary>
        /// Performs a basic rewind of the video by skipping back five seconds.
        /// </summary>
        public void Rewind()
        {
            _videoPlayer.time -= 5f;
        }
        #endregion

        #region Volume methods
        /// <summary>
        /// Method that ismply sets the volume of the video player, set through a slider (via another script).
        /// </summary>
        /// <param name="value">The new volume of the video player.</param>
        public void SetVideoVolume(float value)
        {
            _videoPlayer.SetDirectAudioVolume(0, value);
        }
        #endregion
    }
}


