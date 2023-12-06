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
    /// Class that plays cutscenes, which currently only include the video that plays when the ship jumps
    /// </summary>
    public class CutsceneSystem : ConnectedSingleton<CutsceneSystem>
    {
        [SerializeField] private GameObject videoCanvas;

        [SerializeField] private CinemachineVirtualCamera cutsceneCam;
        [SerializeField] private RenderTexture cutsceneRenderTex;

        [SerializeField] private GameObject archivesVideoCanvas;
        [SerializeField] private RenderTexture archivesRenderTex;

        public override void Start()
        {
            base.Start();
            videoCanvas.SetActive(false);
            archivesVideoCanvas.SetActive(false);
            cutsceneCam.enabled = false;
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
                cutsceneCam.enabled = true;
                VideoPlayerManager.OnVideoCompleted += OnCutsceneCompleted;
                return true;
            }
            return false;
        }

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

        /// <summary>
        /// Plays a cutscene when the ship jumps to a location.
        /// </summary>
        private void PlayLaunchCutscene()
        {
            PlayCutscene(ShipStateManager.Instance.Session.jumpCutsceneURL);
        }     
    }
}


