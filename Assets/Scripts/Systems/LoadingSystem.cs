/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

namespace Systems
{
    /// <summary>
    /// A system that fades screens in and out while loading the game and holds actions to call upon loading events.
    /// </summary>
    public class LoadingSystem : Singleton<LoadingSystem>
    {
        [Header("Loading Transition Times")]
        // How long to wait before fading in the loading screen
        [SerializeField]
        private float secondsToWaitBeforeFade = 0.0f;
        // The time it takes to totally fade in the loading screen
        [SerializeField]
        private float timeToFadeIn = 1.0f;
        // The time it takes to totally fade out the loading screen
        [SerializeField]
        private float timeToFadeOut = 1.0f;

        [Header("Loading Objects and Components")]
        // The message displayed on the loading screen
        [SerializeField]
        private TextMeshProUGUI loadingMessageTMP;
        // The group whose alpha value is modified for fading the loading screen in and out
        [SerializeField]
        private CanvasGroup loadingGroup;

        // Whether the game has loaded past the first scene
        [HideInInspector]
        public bool loadedPastInitialScene;
        // Whather the game is loading scenes while as a server instance
        [HideInInspector]
        public bool calledAsServer;

        // An action that fires when the load event is first called
        public static Action OnLoadCalled;
        // An action that fires when loading begins
        public static Action OnLoadStarted;
        // An action that fires when loading finishes successfully
        public static Action OnLoadFinished;
        // An action that fires when loading fails
        public static Action OnLoadFailed;

        // The token to pass to ShipStateManager
        public string token = null;
        // The link to the server the client will connect to
        public string serverLink = null;

        #region Public loading methods
        /// <summary>
        /// Calls an event to trigger actions when first loading the game, then begins fading in the loading screen.
        /// </summary>
        public void BeginLoad()
        {
            if (OnLoadCalled != null)
            {
                OnLoadCalled.Invoke();
            }
            StartCoroutine(Load());
        }

        /// <summary>
        /// Stops loading processes and fades out the loading screen.
        /// </summary>
        public void EndLoad()
        {
            UpdateLoadingMessage("Astralizing Viewport...");
            StopAllCoroutines();
            StartCoroutine(FinishLoad());
        }

        /// <summary>
        /// Updates the message displayed while loading.
        /// </summary>
        /// <param name="loadingMessage">The message to set on the loading screen while loading.</param>
        public void UpdateLoadingMessage(string loadingMessage)
        {
            loadingMessageTMP.text = loadingMessage;
        }
        #endregion

        #region Loading coroutines
        /// <summary>
        /// Fades in the loading screen and invokes loading actions.
        /// </summary>
        /// <returns>A yield return while trying to fade out the loading screen.</returns>
        private IEnumerator Load()
        {
            // Make the loading screen block mouse input
            loadingGroup.interactable = true;
            loadingGroup.blocksRaycasts = true;

            // Wait for a time before starting the fade
            yield return new WaitForSeconds(secondsToWaitBeforeFade);

            // If we've previously joined the game, wait an additional time
            if (loadedPastInitialScene)
            {
                yield return new WaitForSeconds(timeToFadeIn);
            }
            // Otherwise, start fading in the loading screen
            else
            {
                // Adjust alpha value
                while (loadingGroup.alpha < 1.0f)
                {
                    float interval = Time.deltaTime / timeToFadeIn;
                    loadingGroup.alpha += interval;
                    yield return null;
                }
            }

            // Update the loading message
            UpdateLoadingMessage("Matricizing Trajectories...");

            // Invoke an action for when the load starts and then wait until we have either succeeded or failed
            if (OnLoadStarted != null)
            {
                OnLoadStarted.Invoke();
                if (!loadedPastInitialScene)
                {
                    loadingGroup.alpha = 1.0f;
                }
                StartCoroutine(ProcessLoadingRoutine());
            }
        }

        /// <summary>
        /// Coroutine to wait for and process loading success or failure.
        /// </summary>
        /// <returns>A yield return while trying to connect to the online scene.</returns>
        private IEnumerator ProcessLoadingRoutine()
        {
            // We have two given conditions: we're trying to connect, and we're on the starting screen
            bool connecting = NetworkManager.singleton.mode != NetworkManagerMode.Offline;
            bool onInitialScene = SceneManager.GetActiveScene().buildIndex == 0;
            // Wait until one of the conditions is false
            while (connecting && onInitialScene)
            {
                yield return null;
                connecting = NetworkManager.singleton.mode != NetworkManagerMode.Offline;
                onInitialScene = SceneManager.GetActiveScene().buildIndex == 0;
            }
            // If we're not on the initial scene anymore, we've successfully connected
            if (!onInitialScene)
            {
                loadingGroup.alpha = 1.0f;
                loadedPastInitialScene = true;
            }
            // Otherwise, loading has failed
            else
            {
                CatchLoadingFailure();
            }
        }

        /// <summary>
        /// Resets variables and invokes an action called when loading fails.
        /// </summary>
        private void CatchLoadingFailure()
        {
            // Stop any remaining attempt to load the game
            StopAllCoroutines();

            // Disable loading group items
            loadingGroup.alpha = 0.0f;
            loadingGroup.interactable = false;
            loadingGroup.blocksRaycasts = false;

            // Invoke an action when loading fails
            if (OnLoadFailed != null)
            {
                OnLoadFailed.Invoke();
            }
            Debug.Log("Loading failed; either the given address is wrong, or the server is not running.");
        }

        /// <summary>
        /// Fades out the loading screen, disables interaction, and invokes an action at the end of loading.
        /// </summary>
        /// <returns>A yield return while trying to fade out the black screen.</returns>
        private IEnumerator FinishLoad()
        {
            // Adjust the Canvas Group's alpha value
            while (loadingGroup.alpha > 0.0f)
            {
                float interval = Time.deltaTime / timeToFadeOut;
                loadingGroup.alpha -= interval;
                yield return null;
            }

            // Disable interactability and raycast blocking on the Canvas Group
            loadingGroup.interactable = false;
            loadingGroup.blocksRaycasts = false;

            // Invoke the action to call when loading finishes
            if (OnLoadFinished != null)
            {
                OnLoadFinished.Invoke();
            }
        }
        #endregion
    }
}

