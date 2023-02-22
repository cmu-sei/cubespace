/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;
using UI.ColorPalettes;
using Systems;
using Managers;
using Mirror;

namespace UI.HUD
{
    /// <summary>
    /// The master controller object used to control UI behavior.
    /// </summary>
    public class HUDController : ConnectedSingleton<HUDController>
    {
        /// <summary>
        /// Whether a panel is open or not.
        /// </summary>
        public static bool IsPanelOpen = false;
        
        /// <summary>
        /// The button used to exit the workstation.
        /// </summary>
        [SerializeField]
        private UIExitWorkstationButton exitWorkstationButton;
        /// <summary>
        /// The button used to open the mission log.
        /// </summary>
        [SerializeField]
        private UIHudDisplayToggleButton missionLogButton;
        /// <summary>
        /// The button used to open the settigns panel.
        /// </summary>
        [SerializeField]
        private UIHudDisplayToggleButton settingsButton;
        /// <summary>
        /// The UI component of the mission log button that flashes if the player has not clicked it yet.
        /// </summary>
        [SerializeField]
        private FlashBox missionLogButtonFlashBox;

        /// <summary>
        /// The list of tasks for the player.
        /// </summary>
        [SerializeField]
        private UIHudTaskList taskList;
        /// <summary>
        /// The cube icon displayed in the HUD.
        /// </summary>
        [SerializeField]
        private UIHudCube cubeIcon;

        /// <summary>
        /// The larger parent container for the HUD object.
        /// </summary>
        [SerializeField]
        private GameObject hudParentObject;
        /// <summary>
        /// The settings panel activated by this controller.
        /// </summary>
        [SerializeField]
        private GameObject settingsPanel;
        /// <summary>
        /// The panel used to display the mission log.
        /// </summary>
        [SerializeField]
        private GameObject missionLogPanel;
        /// <summary>
        /// The object preventing the player from clicking on anything while the mission log or settings panel is open.
        /// </summary>
        [SerializeField]
        private GameObject raycastingBlocker;
        /// <summary>
        /// The quit button, used to exit the game.
        /// </summary>
        [SerializeField]
        private GameObject quitButton;
        /// <summary>
        /// The color palette used to set colors.
        /// </summary>
        [SerializeField]
        private ColorPalette palette;
        /// <summary>
        /// The CanvasGroup used to make the HUD visible or invisible.
        /// </summary>
        [SerializeField]
        private CanvasGroup group;

        /// <summary>
        /// The custom NetworkManager object used.
        /// </summary>
        private CustomNetworkManager networkManager;

        /// <summary>
        /// Unity event function that adds listeners to the open/close functions and disables some UI objects.
        /// </summary>
        public override void Start()
        {
            base.Start();
            LoadingSystem.Instance.UpdateLoadingMessage("Reticulating Splines...");

            missionLogButton.controllerOpenFunction.AddListener(OpenMissionLog);
            missionLogButton.controllerCloseFunction.AddListener(CloseMissionLog);
            settingsButton.controllerCloseFunction.AddListener(CloseSettings);
            settingsButton.controllerOpenFunction.AddListener(OpenSettings);

            cubeIcon.SetCube(false);
            missionLogPanel.SetActive(false);
            settingsPanel.SetActive(false);
            raycastingBlocker.SetActive(false);

            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

            #if UNITY_EDITOR
            quitButton.SetActive(true);
            #else
            quitButton.SetActive(networkManager && networkManager.isInDevMode);
            #endif
        }

        /// <summary>
        /// Hides the mission log, or toggles the HUD as invisible.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                missionLogButton.OnClick();
            }

            if (networkManager.isInDevMode && Input.GetKeyDown(networkManager.hideHUDKeyCode))
            {
                ToggleHUDVisibility();
            }
        }

        /// <summary>
        /// Makes the HUD either visible or invisible, depending on its current state.
        /// </summary>
        public void ToggleHUDVisibility()
        {
            group.alpha = group.alpha == 0 ? 1 : 0;
        }

        /// <summary>
        /// Hides the HUD. Unused.
        /// </summary>
        public void HideHUD()
        {
            hudParentObject.SetActive(false);
        }

        /// <summary>
        /// Displays the HUD. Unused.
        /// </summary>
        public void RevealHUD()
        {
            hudParentObject.SetActive(true);
        }

        /// <summary>
        /// Shows or hides the cube sprite.
        /// </summary>
        /// <param name="enabled">Whether to show the cube sprite.</param>
        public void SetCubeSprite(bool enabled)
        {
            cubeIcon.SetCube(enabled);
        }

        /// <summary>
        /// Opens the mission log.
        /// </summary>
        public void OpenMissionLog()
        {
            if (missionLogButtonFlashBox) missionLogButtonFlashBox.stopFlashing = true;

            if (settingsPanel.activeInHierarchy)
            {
                settingsButton.OnClick();
            }
            
            Audio.AudioPlayer.Instance.SetMissionLogSnapshot(true);
            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(true);
            Entities.Player.LockLocalPlayerInput();
            missionLogPanel.SetActive(true);
            raycastingBlocker.SetActive(true);
            IsPanelOpen = true;
        }

        /// <summary>
        /// Closes the mission log.
        /// </summary>
        public void CloseMissionLog()
        {
			Audio.AudioPlayer.Instance.SetMissionLogSnapshot(false);
            taskList.CloseAdditionalInfo();
            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(false);
            Entities.Player.UnlockLocalPlayerInput();
            missionLogPanel.SetActive(false);
            raycastingBlocker.SetActive(false);
            IsPanelOpen = false;
        }

        /// <summary>
        /// Opens the settings window.
        /// </summary>
        public void OpenSettings()
        {
            if (missionLogPanel.activeInHierarchy)
            {
                missionLogButton.OnClick();
            }

            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(true);
            Entities.Player.LockLocalPlayerInput();

            settingsPanel.SetActive(true);
            raycastingBlocker.SetActive(true);
            IsPanelOpen = true;
        }

        /// <summary>
        /// Closes the settings window.
        /// </summary>
        public void CloseSettings()
        {
            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(false);
            Entities.Player.UnlockLocalPlayerInput();
            settingsPanel.SetActive(false);
            raycastingBlocker.SetActive(false);
            IsPanelOpen = false;
        }

        /// <summary>
        /// Quits the game. This is unused.
        /// </summary>
        public void QuitGame()
        {
            // Stop the host if in host mode
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            // Stop the client if client-only
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            // Stop the server if server-only
            else if (NetworkServer.active)
            {
                NetworkManager.singleton.StopServer();
            }
        }
    }
}

