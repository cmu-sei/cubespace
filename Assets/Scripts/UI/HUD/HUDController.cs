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
        public Action<MenuState> OnMenuStateChange;
        public enum MenuState
        {
            None,
            MissionLog, 
            Settings,
            GalaxyMap
        }
        
        /// <summary>
        /// Whether a panel is open or not.
        /// </summary>
        public static bool IsPanelOpen => Instance._menuState != MenuState.None;

        private MenuState _menuState;
        

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
        private UIHudMissionManager missionManager;
        /// <summary>
        /// The panel used to display the galaxy map.
        /// </summary>
        [SerializeField]
        private GameObject galaxyMapPanel;

        /// <summary>
        /// The quit button, used to exit the game.
        /// </summary>
        [SerializeField]
        private GameObject quitButton;
        /// <summary>
        /// The CanvasGroup used to make the HUD visible or invisible.
        /// </summary>
        [SerializeField]
        private CanvasGroup group;

        [SerializeField] private UIHudDisplayMenuButton _mapButton;

        [SerializeField] private UIMissionPogContainer pogContainer;

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
            missionManager = missionLogPanel.GetComponent<UIHudMissionManager>();

            LoadingSystem.Instance.UpdateLoadingMessage("Reticulating Splines...");

            cubeIcon.SetCube(false);
            
            SetMenuState(MenuState.None);

            if (ShipStateManager.Instance)
            {
                UpdateMapButtonVisibility(ShipStateManager.Instance.useGalaxyMap);
            }
            else
            {
                UpdateMapButtonVisibility(false);
            }

            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

            #if UNITY_EDITOR
            quitButton.SetActive(true);
            #endif

            // TODO: The placement of this is somewhat arbitrary. Loading sequence should be better scripted
            LoadingSystem.Instance.EndLoad();
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

        public void ToggleMenuState(MenuState toggleState)
        {
            if (toggleState == MenuState.None)
            {
                //can't toggle between off and off.
                return;
            }
            else if (_menuState == MenuState.None || _menuState != toggleState)
            {
                SetMenuState(toggleState);
            }
            else if (_menuState == toggleState)
            {
                SetMenuState(MenuState.None);
            }
        }

        public void SetMenuState(MenuState newState)
        {
            missionLogPanel.SetActive(newState == MenuState.MissionLog);
            if (newState == MenuState.MissionLog) missionManager.OnOpen(); // This is to prevent an order of operations issue with just using OnEnable, which works for the other panels
            settingsPanel.SetActive(newState == MenuState.Settings);
            galaxyMapPanel.SetActive(newState == MenuState.GalaxyMap);

            if (newState != MenuState.None)
            {
                Entities.Player.LockLocalPlayerInput();
                UIExitWorkstationButton.Instance.SetHiddenByHudPanel(true);
            }
            else
            {
                Entities.Player.UnlockLocalPlayerInput();
                UIExitWorkstationButton.Instance.SetHiddenByHudPanel(false);
            }

            //should check for going from state to same, but then would have to deal with init flow.
            _menuState = newState;
            OnMenuStateChange?.Invoke(_menuState);
        }

        /// <summary>
        /// Convenience wrapper for opening the mission log.
        /// It just calls SetMenuState(MenuState.MissionLog)
        /// </summary>
        public void OpenMissionLog()
        {
            SetMenuState(MenuState.MissionLog);
        }

        /// <summary>
        /// Closes the Mission Log, Galaxy Map, or Settings panel.
        /// Convenience wrapper to SetMenuState(MenuState.None) for easy use with UnityEvents (ie: buttons)
        /// </summary>
        public void CloseAnyPanel()
        {
            SetMenuState(MenuState.None);
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

        public void UpdateMapButtonVisibility(bool mapButtonEnabled)
        {
            if (mapButtonEnabled && _mapButton.enabled == false)
            {
                _mapButton.enabled = true;
                _mapButton.SetVisable(true);
            }
            else if (!mapButtonEnabled && _mapButton.enabled == true)
            {
                _mapButton.SetVisable(false);
                _mapButton.enabled = false;
            }
        }

        public void UpdatePogVisibility(bool pogsEnabled)
        {
            pogContainer.enabled = pogsEnabled;
        }
    }
}

