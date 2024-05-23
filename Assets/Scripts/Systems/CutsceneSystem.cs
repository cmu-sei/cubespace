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
using Entities;
using Managers;

namespace Systems
{
    /// <summary>
    /// Wrapper for the VideoPlayerManager which handles playing the jump cutscene and replaying sensor station
    /// videos from the mission log 
    /// </summary>
    public class CutsceneSystem : ConnectedSingleton<CutsceneSystem>
    {
        // The canvas used for the jump cutscene
        [SerializeField] private GameObject videoCanvas;
        // The blank render texture assigned to the cutscene canvas
        [SerializeField] private RenderTexture cutsceneRenderTex;

        // The canvas and tex used for playing archived videos
        [SerializeField] private GameObject archivesVideoCanvas;
        [SerializeField] private RenderTexture archivesRenderTex;

        public override void Start()
        {
            base.Start();
            videoCanvas.SetActive(false);
            archivesVideoCanvas.SetActive(false);
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForShipStateManager());
        }

        private void OnDisable()
        {
            ShipStateManager.OnLaunch -= PlayLaunchCutscene;
        }

        private IEnumerator WaitForShipStateManager()
        {
            yield return new WaitWhile(() => ShipStateManager.Instance == null);

            if (!ShipStateManager.Instance.isServerOnly)
            {
                // Subscribe to play the jump cutscene when the ship jumps
                ShipStateManager.OnLaunch += PlayLaunchCutscene;
            }
        }

        // Plays video fullscreen without any further initialization needed. May fail based on the state of the VideoPlayerManager
        // Returns bool the reports whther or not video initialization was successful
        public bool PlayCutscene(string videoURL)
        {
            if (!VideoPlayerManager.Instance)
            {
                Debug.LogError("Couldn't find video player manager!");
                return false;
            }

            if (VideoPlayerManager.Instance.InitializeVideo(videoURL, cutsceneRenderTex, true))
            {
                Player.LockLocalPlayerInput();
                videoCanvas.SetActive(true);
                VideoPlayerManager.OnVideoCompleted += OnCutsceneCompleted;
                return true;
            }
            return false;
        }

        // Plays video in archives octogon window without any further initialization needed. May fail based on the state of the VideoPlayerManager
        // Returns bool the reports whther or not video initialization was successful
        public bool PlayArchivesVideo(string videoURL)
        {
            if (!VideoPlayerManager.Instance)
            {
                Debug.LogError("Couldn't find video player manager!");
                return false;
            }

            if (VideoPlayerManager.Instance.InitializeVideo(videoURL, archivesRenderTex, true, true))
            {
                Player.LockLocalPlayerInput();
                archivesVideoCanvas.SetActive(true);
                VideoPlayerManager.OnVideoCompleted += OnCutsceneCompleted;
                return true;
            }
            return false;
        }

        private void OnCutsceneCompleted(string url, bool videoCompleted)
        {
            VideoPlayerManager.OnVideoCompleted -= OnCutsceneCompleted;
            videoCanvas.SetActive(false);
            archivesVideoCanvas.SetActive(false);

            // If this was an archives video, player input needs to stay locked
            if (UI.HUD.HUDController.IsPanelOpen)
            {
                Player.LockLocalPlayerInput();
            }
            // Otherwise, unlock it
            else
            {
                Player.UnlockLocalPlayerInput();
            }
        }

        /// Plays jump cutscene
        private void PlayLaunchCutscene()
        {
            PlayCutscene(ShipStateManager.Instance.Session.jumpCutsceneURL);
        }     
    }
}


