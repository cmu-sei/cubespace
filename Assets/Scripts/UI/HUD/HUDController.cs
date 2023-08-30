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
        /// The color palette used to set colors.
        /// </summary>
        [SerializeField]
        private ColorPalette palette;
        /// <summary>
        /// The CanvasGroup used to make the HUD visible or invisible.
        /// </summary>
        [SerializeField]
        private CanvasGroup group;

        [SerializeField] private UIHudGalaxyPanelManager _galaxyPanelManager;

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

            cubeIcon.SetCube(false);
            
            SetMenuState(MenuState.None);

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
                ToggleMenuState(MenuState.MissionLog);
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
            if (cubeIcon == null)
            {
                Debug.LogError("Null cubeIcon [HUDController.cs:172]");
            }
            cubeIcon.SetCube(enabled);
        }

        public void ToggleMenuState(MenuState toggleState)
        {
            if (toggleState == MenuState.None)
            {
                //can't toggle between off and off.
                return;
            }

            if (_menuState == MenuState.None)
            {
                //turn on when off
                SetMenuState(toggleState);
            }
            else
            {
                //close the open state.
                if (_menuState == toggleState)
                {
                    SetMenuState(MenuState.None);
                }
                else
                {
                    //switch directly to a different state. ie: from missionLog even when settings is open.
                    SetMenuState(toggleState);
                }
            }
        }
        public void SetMenuState(MenuState newState)
        {
            //change state
            //this can be done with 'UIMenuPanelActiveWithState', but that only works when gameobjects begin active; which would be a change in how we have treated them, and unexpected.
            //to fix, we could switch the above script to work on an empty parent object that's always active to enable and disable children.
            missionLogPanel.SetActive(newState == MenuState.MissionLog);
            settingsPanel.SetActive(newState == MenuState.Settings);
            galaxyMapPanel.SetActive(newState == MenuState.GalaxyMap);

            if (newState != MenuState.None)
            {
                //any state open
                Entities.Player.LockLocalPlayerInput();
                UIExitWorkstationButton.Instance.SetHiddenByHudPanel(true);
            }
            else
            {
                //any menu close
                Entities.Player.UnlockLocalPlayerInput();
                UIExitWorkstationButton.Instance.SetHiddenByHudPanel(false);

                //close the mission log specific case
                if (_menuState == MenuState.MissionLog)
                {
                    taskList.CloseAdditionalInfo();
                }
                
            }

            //audio config
            Audio.AudioPlayer.Instance.SetMissionLogSnapshot(newState == MenuState.MissionLog);
            
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

        /// <summary>
        /// Adds the system to the dictionary and galaxy map and sets it up, or changes its attributes if it already exists.
        /// </summary>
        /// <param name="md">The incoming mission data.</param>
        /// <param name="index">The index of the mission in the mission log corresponding to this system.</param>
        public void AddSystemOrSetData(MissionData md, int index)
        {
            _galaxyPanelManager.AddSystemOrSetData(md,index);
        }
    }
}

