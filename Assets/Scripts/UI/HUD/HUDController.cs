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
using System.Collections.Generic;
using Systems.GameBrain;
using UnityEngine.UI;

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
        public UIHudDisplayToggleButton MissionLogButton => missionLogButton;
        /// <summary>
        /// The button used to open the settigns panel.
        /// </summary>
        [SerializeField]
        private UIHudDisplayToggleButton settingsButton;
        /// <summary>
        /// The button used to switch off the galaxy map panel.
        /// </summary>
        [SerializeField]
        private UIHudDisplayToggleButton galaxyPanelCloseButton;
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
        /// The panel used to display the galaxy map.
        /// </summary>
        [SerializeField]
        private GameObject galaxyMapPanel;
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
        /// All systems displayed in the galaxy panel.
        /// </summary>
        /// public List<NavReaderGalaxySystem> systems;
        [Header("Prefabs")]
        [SerializeField]
        private GameObject systemPrefab;
        [SerializeField]
        private GameObject linePrefab;
        [SerializeField]
        private GameObject targetPointPrefab;

        /// <summary>
        /// The parent of the system objects.
        /// </summary>
        [SerializeField]
        private Transform systemParent;
        /// <summary>
        /// The parent of the target objects.
        /// </summary>
        [SerializeField]
        private Transform targetParent;
        /// <summary>
        /// The parent of the line objects.
        /// </summary>
        public Transform lineParent;
        /// <summary>
        /// The IDs of systems to system components.
        /// </summary>
        public Dictionary<string, NavReaderGalaxySystem> idsToSystems;

        /// <summary>
        /// The custom NetworkManager object used.
        /// </summary>
        private CustomNetworkManager networkManager;

        [Header("Galaxy System State Highlights")]
        public Color incompleteHighlightColor;
        public Color partiallyCompletedHighlightColor;
        public Color completedHighlightColor;

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
            // No open function for the button on the galaxy panel map panel
            galaxyPanelCloseButton.controllerCloseFunction.AddListener(CloseGalaxyMap);

            cubeIcon.SetCube(false);
            missionLogPanel.SetActive(false);
            settingsPanel.SetActive(false);
            galaxyMapPanel.SetActive(false);
            raycastingBlocker.SetActive(false);

            idsToSystems = new Dictionary<string, NavReaderGalaxySystem>();
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

            if (galaxyMapPanel.activeInHierarchy)
            {
                galaxyPanelCloseButton.OnClick();
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
        /// Opens the galaxy map window.
        /// </summary>
        public void OpenGalaxyMap()
        {
            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(true);
            Entities.Player.LockLocalPlayerInput();
            galaxyMapPanel.SetActive(true);
            raycastingBlocker.SetActive(true);
            IsPanelOpen = true;
        }

        /// <summary>
        /// Closes the galaxy map window.
        /// </summary>
        public void CloseGalaxyMap()
        {
            UIExitWorkstationButton.Instance.SetHiddenByHudPanel(false);
            Entities.Player.UnlockLocalPlayerInput();
            galaxyMapPanel.SetActive(false);
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

        /// <summary>
        /// Adds the system to the dictionary and galaxy map and sets it up, or changes its attributes if it already exists.
        /// </summary>
        /// <param name="md">The incoming mission data.</param>
        /// <param name="index">The index of the mission in the mission log corresponding to this system.</param>
        public void AddSystemOrSetData(MissionData md, int index)
        {
            // Update system attributes if it already exists
            if (idsToSystems.ContainsKey(md.missionID))
            {
                idsToSystems[md.missionID].SetSystemMission(md, index);
            }
            // Set up the system if it doesn't
            else
            {
                // Create a new system on the map from prefabs
                GameObject systemObj = Instantiate(systemPrefab, systemParent);
                GameObject lineObj = Instantiate(linePrefab, lineParent);
                GameObject targetObj = Instantiate(targetPointPrefab, targetParent);

                // Get the system script
                NavReaderGalaxySystem system = systemObj.GetComponent<NavReaderGalaxySystem>();

                // Get the image components
                Image lineImage = lineObj.transform.GetComponent<Image>();
                Image targetImage = targetObj.transform.GetChild(0).GetComponent<Image>();

                // Add the system to the dictionary and set its mission information
                idsToSystems.Add(md.missionID, system);
                system.SetSystemMission(md, index, lineImage, targetImage);

                // Set the position of the system
                systemObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapXPos, md.galaxyMapYPos);
                targetObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapTargetXPos, md.galaxyMapTargetYPos);

                // Get TectTransform references
                RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                RectTransform coreDisplayTransform = system.CoreDisplayRect;
                RectTransform targetRectTransform = targetObj.GetComponent<RectTransform>();

                // Get the positions of the RectTransforms
                Vector2 coreDisplayPosition = coreDisplayTransform.position;
                Vector2 targetPosition = targetRectTransform.position;
                Vector2 coreDisplayLocalPosition = coreDisplayTransform.parent.localPosition - coreDisplayTransform.localPosition;
                Vector2 targetLocalPosition = targetRectTransform.localPosition - targetRectTransform.parent.localPosition;

                // Calculate the distance between the system
                Vector2 midpoint = (coreDisplayPosition + targetPosition) / 2;
                float distance = Vector2.Distance(coreDisplayLocalPosition, targetLocalPosition);

                // Draw the line between the system and its target
                lineRect.position = midpoint;
                lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, distance);
                float z = 90 + Mathf.Atan2(targetPosition.y - coreDisplayPosition.y, targetPosition.x - coreDisplayPosition.x) * 180 / Mathf.PI;
                lineRect.rotation = Quaternion.Euler(0, 0, z);
            }
        }
    }
}

