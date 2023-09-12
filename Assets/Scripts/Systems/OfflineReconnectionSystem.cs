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
using TMPro;
using UnityEngine;
using Mirror;

namespace Systems
{
    /// <summary>
    /// A general class used for automatically and manually reconnecting to the server after a disconnect.
    /// </summary>
    public class OfflineReconnectionSystem : MonoBehaviour
    {
        // The UI piece saying we're attempting to reconnect and set its text
        [SerializeField]
        private GameObject reconnectingBlock;
        // The text component of the above piece used in reconnecting
        [SerializeField]
        private TextMeshProUGUI reconnectingBlockText;
        // The UI piece saying the reconnect attempt failed
        [SerializeField]
        private GameObject reconnectingFailedBlock;
        // The time to wait between modifying the reconnecting text
        [SerializeField]
        private float timeToWaitBetweenEllipses = 0.5f;
        // The original text used when trying to reconnect to the server
        private string originalReconnectingBlockText;

        #region Unity event functions
        /// <summary>
        /// Unity event function that enables and disables UI pieces and subscribes to loading actions.
        /// </summary>
        void Start()
        {
            // Set the original reconnecting block text
            originalReconnectingBlockText = reconnectingBlockText.text;
            // If the LoadingSystem exists, enable/disable different UI pieces
            if (LoadingSystem.Instance)
            {
                // If we are still on the offline scene, this object should be disabled
                if (!LoadingSystem.Instance.loadedPastInitialScene)
                {
                    gameObject.SetActive(false);
                }
                // Otherwise, try to reconnect to the server
                else
                {
                    DisplayReconnectUI();
                    // Disable this object on the server
                    if (LoadingSystem.Instance.calledAsServer)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    // Try to connect to the server on the client once its loading action is called
                    else
                    {
                        LoadingSystem.OnLoadStarted += NetworkManager.singleton.StartClient;
                    }
                    // Subscribe to an action called when the client fails to reconnect to the server
                    LoadingSystem.OnLoadFailed += DisplayReconnectFailureUI;
                    // Begin loading the game
                    LoadingSystem.Instance.BeginLoad();
                }
            }
        }

        /// <summary>
        /// Unity event that unsubscribes from actions when this object is destroyed (AKA, when loading into the main game).
        /// </summary>
        private void OnDestroy()
        {
            if (LoadingSystem.Instance == null) Debug.LogError("No loading system!");

            // If we've loaded past the initial scene, unsubscribe from the loading failed action
            if (LoadingSystem.Instance.loadedPastInitialScene)
            {
                LoadingSystem.OnLoadFailed -= DisplayReconnectFailureUI;
                // If this is the server, unsubscribe from the loading action
                if (!LoadingSystem.Instance.calledAsServer)
                {
                    LoadingSystem.OnLoadStarted -= NetworkManager.singleton.StartClient;
                }
            }
        }
        #endregion

        #region Reconnection UI methods
        /// <summary>
        /// Enables and disables UI and attempts to reconnect a client to the server after a disconnect.
        /// </summary>
        public void DisplayReconnectUI()
        {
            // Enable the UI piece saying we're attempting to reconnect and set its text
            reconnectingBlock.SetActive(true);
            // Disable the UI piece saying the reconnect attempt failed
            reconnectingFailedBlock.SetActive(false);
            // Attempt to reconnect
            StartCoroutine(AttemptReconnectUI());
        }

        /// <summary>
        /// Enables and disables UI upon a failure to reconnect to the server.
        /// </summary>
        public void DisplayReconnectFailureUI()
        {
            // Disable the UI piece saying we're attempting to reconnect and set its text
            reconnectingBlock.SetActive(false);
            reconnectingBlockText.text = originalReconnectingBlockText;
            // Enable the UI piece saying the reconnect attempt failed
            reconnectingFailedBlock.SetActive(true);
            // Stop any attempt to reconnect
            StopAllCoroutines();
        }

        /// <summary>
        /// Coroutine that updates the UI while trying to reconnect to the server.
        /// </summary>
        /// <returns>Yield returns between changes to the reconnecting block.</returns>
        private IEnumerator AttemptReconnectUI()
        {
            // While trying to reconnect, update the ellipses UI
            while (true)
            {
                reconnectingBlockText.text = originalReconnectingBlockText + ".";
                yield return new WaitForSeconds(timeToWaitBetweenEllipses);
                reconnectingBlockText.text = originalReconnectingBlockText + "..";
                yield return new WaitForSeconds(timeToWaitBetweenEllipses);
                reconnectingBlockText.text = originalReconnectingBlockText + "...";
                yield return new WaitForSeconds(timeToWaitBetweenEllipses);
            }
        }
        #endregion
    }

}

