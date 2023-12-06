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
using Managers;
using Mirror;

namespace Entities.Workstations
{
    /// <summary>
    /// A class defining the basic structure and behavior of a workstation.
    /// </summary>
    public class Workstation : NetworkBehaviour
    {
        #region Variables
        /// <summary>
        /// The WorkstationManager, which registers this object once it starts.
        /// </summary>
        [Header("GameObject References")]
        [SerializeField]
        protected WorkstationManager _workstationManager;
        /// <summary>
        /// The source where SFX should play.
        /// </summary>
        [SerializeField]
        private Transform soundSource;

        /// <summary>
        /// The WorkstationID assigned to this workstation. Derives from a private variable.
        /// </summary>
        public WorkstationID StationID => stationID;
        /// <summary>
        /// The WorkstationID assigned to this workstation. Configurable in the Inspector.
        /// </summary>
        [Header("Workstation Variables")]
        [SerializeField]
        private WorkstationID stationID;
        /// <summary>
        /// Whether this workstation is used in launch mode. Derives from a private variable.
        /// </summary>
        public bool UsedInLaunchMode => usedInLaunchMode;
        /// <summary>
        /// Whether this workstation is used in launch mode. Configurable in the Inspector.
        /// </summary>
        [SerializeField]
        private bool usedInLaunchMode;
        /// <summary>
        /// Whether this workstation is used in exploration mode. Derives from a private variable.
        /// </summary>
        public bool UsedInExplorationMode => usedInExplorationMode;
        /// <summary>
        /// Whether this workstation is used in launch mode. Configurable in the Inspector.
        /// </summary>
        [SerializeField]
        private bool usedInExplorationMode;
        /// <summary>
        /// The amount of seconds to zoom in and zoom out of the workstation view from the main view.
        /// </summary>
        [SerializeField]
        private float timeToZoom = 1.5f;
        
        /// <summary>
        /// Whether this workstation is always powered on, no matter what. Derives from a private variable.
        /// </summary>
        public bool AlwaysHasPower => alwaysHasPower;
        /// <summary>
        /// Whether this workstation is always powered on, no matter what.
        /// </summary>
        [SerializeField]
        private bool alwaysHasPower = false;

        /// <summary>
        /// Whether this workstation is currently powered. Derives from a private variable.
        /// </summary>
        public bool IsPowered => isPowered;
        /// <summary>
        /// Whether this workstation is currently powered.
        /// </summary>
        [SyncVar]
        private bool isPowered;

        /// <summary>
        /// An action called when the workstation powers on, used to power on levers, switches, etc.
        /// </summary>
        public event Action OnPowerOn;
        /// <summary>
        /// An action called when the workstation powers off, used to power off levers, switches, etc.
        /// </summary>
        public event Action OnPowerOff;
        /// <summary>
        /// An action called when the workstation's state should be reset, used to power off levers, switches, etc.
        /// This event is fired by the ResetWorkstation function, which should be overriden as needed.
        /// </summary>
        public event Action OnResetState;
        //Events upon leaving and entering - primarily for SFX
        /// <summary>
        /// An action called when a player enters the workstation.
        /// </summary>
        public event Action OnEnter;
        /// <summary>
        /// An action called when a player exits the workstation.
        /// </summary>
        public event Action OnExit;
        
        /// <summary>
        /// Whether there is a player at this workstation.
        /// </summary>
        [HideInInspector]
        public Player playerAtWorkstation = null;
        /// <summary>
        /// Whether the player is in the process of exiting this workstation.
        /// </summary>
        private bool isExiting = false;

        /// <summary>
        /// The camera assigned to this workstation, contained within one of its children.
        /// </summary>
        private CinemachineVirtualCamera cam;
        /// <summary>
        /// The dolly on which the camera travels.
        /// </summary>
        private CinemachineTrackedDolly camDolly;
        /// <summary>
        /// The overlay used by the camera.
        /// </summary>
        private CinemachineStoryboard camStoryboard;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that registers this workstation to the WorkstationManager.
        /// </summary>
        protected virtual void Awake()
        {
            _workstationManager.RegisterWorkstation(stationID, this);
        }

        /// <summary>
        /// Unity event function that gets references to Cinemachine objects and initializes camera variables.
        /// </summary>
        protected virtual void Start()
        {
            cam = GetComponentInChildren<CinemachineVirtualCamera>();
            camDolly = cam.GetCinemachineComponent<CinemachineTrackedDolly>();
            camStoryboard = cam.gameObject.GetComponent<CinemachineStoryboard>();

            camDolly.m_PathPosition = camDolly.m_Path.MinPos;
            camStoryboard.m_Alpha = 1f;
        }

        /// <summary>
        /// Unity event function that changes the power of this workstation if in the editor and a hotkey is pressed.
        /// </summary>
        protected virtual void Update()
        {
            #if UNITY_EDITOR
            if (playerAtWorkstation && playerAtWorkstation.isLocalPlayer && Input.GetKeyDown(KeyCode.P))
            {
                ChangePower(!IsPowered);
            }
            #endif
        }

        protected void OnDestroy()
        {
            _workstationManager.DeregisterWorkstation(stationID, this);
        }
        

        #endregion

        #region Mirror networking methods
        /// <summary>
        /// Sets the power state of this workstation based on the power state stored in PowerRouting and whether this object should always be powered.
        /// </summary>
        public override void OnStartServer()
        {
            if (_workstationManager.HasWorkstations)
            {
                PowerRouting.PowerRouting powerRouting = _workstationManager.GetWorkstation(WorkstationID.PowerRouting) as PowerRouting.PowerRouting;
                if (powerRouting != null)
                {
                    isPowered = AlwaysHasPower || powerRouting.GetPowerStateForWorkstation(stationID);
                }
            }
            else if (((CustomNetworkManager) NetworkManager.singleton).isInDebugMode)
            {
                Debug.Log("No workstations.");
            }

            base.OnStartServer();
        }
        #endregion

        #region General workstation methods
        /// <summary>
        /// Enables SFX and fires a client-side event when the player exits the workstation.
        /// </summary>
        protected virtual void Enter()
        {
            if (isPowered)
            {
                Audio.AudioPlayer.Instance.WorkstationHumOn(soundSource);
            }
            if (OnEnter != null)
            {
                OnEnter();
            }
        }

        /// <summary>
        /// Disables SFX and fires a client-side event when the player exits the workstation.
        /// </summary>
        protected virtual void Exit()
        {
            if (isPowered)
            {
                Audio.AudioPlayer.Instance.WorkstationHumOff();
            }

            if (OnExit != null)
            {
                OnExit();
            }
        }

        /// <summary>
        /// Switches a workstation's authority to a player when that player enters it and the Cinemachine camera zooms into it.
        /// </summary>
        public virtual void Activate(Player player, CinemachineVirtualCamera currentCam)
        {
            // Assigns authority over this workstation to the player entering this workstation
            CmdSetWorkstationAuthorityToPlayer(player, true);
            // Set the given player to be the one currently at this workstation
            playerAtWorkstation = player;

            // Zoom the player into the workstation and switch the camera on the player's client
            player.EnterWorkstationView(stationID);
            currentCam.enabled = false;
            cam.enabled = true;

            // Play SFX
            Audio.AudioPlayer.Instance.SetListenerLocation(Camera.main.gameObject, true);

            // Zoom the player into this workstation
            StartCoroutine(EnterWorkstationView());
        }

        /// <summary>
        /// Removes authority over this workstation from the player and zooms the player out of the workstation.
        /// </summary>
        public virtual void Deactivate()
        {
            CmdSetWorkstationAuthorityToPlayer(playerAtWorkstation, false);
            Deactivate(false);
        }

        /// <summary>
        /// Zooms the player out of the workstation and possibly resets it.
        /// </summary>
        /// <param name="willReset">Whether to reset the workstation's state while exiting the view.</param>
        public virtual void Deactivate(bool willReset = false)
        {
            if (!isExiting)
            {
                StartCoroutine(ExitWorkstationView(CameraManager.Instance.mainVCam, willReset));
                UI.HUD.UIExitWorkstationButton.Instance.DisableButton();
                UI.HUD.UICurrentWorkstationPowerStatusDisplay.ExitWorkstation();
            }
        }

        /// <summary>
        /// General function used to script specific system behavior. This fires an event that will be recieved by each individual part in the workstation.
        /// In new Workstation scripts, override this function, implement custom logic, and call base.ChangePower.
        /// </summary>
        /// <param name="isPowered">Whether this system is now powered.</param>
        public virtual void ChangePower(bool isPowered)
        {
            // Ask the server to change the workstation's power
            CmdChangePower(isPowered);

            // If the workstation is powered, if possible, fire an event and/or play SFX
            if (isPowered)
            {
                if (OnPowerOn != null)
                {
                    OnPowerOn();
                }

                if (playerAtWorkstation != null && playerAtWorkstation.isLocalPlayer)
                {
                    Audio.AudioPlayer.Instance.WorkstationPowerOn(soundSource);
                    Audio.AudioPlayer.Instance.WorkstationHumOn(soundSource);
                }
            }
            // Otherwise, if possible, fire a different event and/or play SFX
            else
            {
                if (OnPowerOff != null)
                {
                    OnPowerOff();
                }
                if (playerAtWorkstation != null && playerAtWorkstation.isLocalPlayer)
                {
                    Audio.AudioPlayer.Instance.WorkstationPowerOff(soundSource);
                    Audio.AudioPlayer.Instance.WorkstationHumOff();
                }
            }
        }

        /// <summary>
        /// Resets the state of the workstation, zooms the local player out of the workstation, and removes a player's authority from it.
        /// </summary>
        public virtual void ResetWorkstation()
        {
            if (OnResetState != null)
            {
                OnResetState();
            }

            if (playerAtWorkstation != null)
            {
                Deactivate();
            }
            else
            {
                CmdSetWorkstationAuthorityToPlayer(playerAtWorkstation, false);
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Gradually zooms the camera (and player) into this workstation.
        /// </summary>
        /// <returns>A yield while waiting to zoom the player out of the workstation.</returns>
        private IEnumerator EnterWorkstationView()
        {
            // Make the layer masks visible and set the dolly back to its resting position
            CameraUtility.SetLayerMaskVisible(_workstationManager.WorkstationViewLayerMask, true);
            CameraUtility.SetLayerMaskVisible(_workstationManager.MainShipViewLayerMask, false);
            camDolly.m_PathPosition = camDolly.m_Path.MinPos;

            // Fire entry events
            Enter();

            // Slowly zoom the camera into the workstation so the player can use it
            float timer = 0f;
            while (timer < timeToZoom)
            {
                timer += Time.deltaTime;
                camDolly.m_PathPosition = Mathf.Lerp(camDolly.m_Path.MinPos, camDolly.m_Path.MaxPos, timer / timeToZoom);
                camStoryboard.m_Alpha = Mathf.Lerp(1f, 0f, camDolly.m_PathPosition / (camDolly.m_Path.MaxPos - 0.4f));
                yield return null;
            }

            // Set the camera position and enable the exit button
            camDolly.m_PathPosition = camDolly.m_Path.MaxPos;
            UI.HUD.UIExitWorkstationButton.Instance.EnableButton(this);
            UI.HUD.UICurrentWorkstationPowerStatusDisplay.EnterWorkstation(this);
        }

        /// <summary>
        /// Gradually zooms the camera (and player) out of this workstation.
        /// </summary>
        /// <param name="newCam">The camera used after zooming out of this workstation.</param>
        /// <param name="willReset">Whether to reset the workstation following the player's entry.</param>
        /// <returns>A yield while waiting to zoom the player out of the workstation.</returns>
        private IEnumerator ExitWorkstationView(CinemachineVirtualCamera newCam, bool willReset = false)
        {
            // Mark that the player is exiting the workstation, so nothing interrupts the exit
            isExiting = true;

            // Gradually zoom the player out of the workstation
            float timer = 0f;
            while (timer < timeToZoom)
            {
                timer += Time.deltaTime;
                camDolly.m_PathPosition = Mathf.Lerp(camDolly.m_Path.MaxPos, camDolly.m_Path.MinPos, timer / timeToZoom);
                camStoryboard.m_Alpha = Mathf.Lerp(0f, 1f, 1f - (camDolly.m_PathPosition / camDolly.m_Path.MaxPos));
                yield return null;
            }

            // Set the dolly back to its default position
            camDolly.m_PathPosition = camDolly.m_Path.MinPos;

            // Fire exit events
            Exit();

            // Switch the camera
            cam.enabled = false;
            CameraUtility.SetLayerMaskVisible(_workstationManager.MainShipViewLayerMask, true);
            newCam.enabled = true;

            // Remove the player's associations with the workstation
            _workstationManager.ExitTerminal(stationID);
            playerAtWorkstation.ExitWorkstationView();
            playerAtWorkstation = null;
            if (willReset)
            {
                ResetWorkstation();
            }
            isExiting = false;

            // Reset the AudioListener and hide the view of the workstation
            Audio.AudioPlayer.Instance.ResetListener();
            CameraUtility.SetLayerMaskVisible(_workstationManager.WorkstationViewLayerMask, false);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Assigns or unassigns authority to or from a player.
        /// </summary>
        /// <param name="player">The player to assign authority to or remove authority from.</param>
        /// <param name="authority">Whether to assign authority to the player.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetWorkstationAuthorityToPlayer(Player player, bool authority)
        {
            if (authority)
            {
                netIdentity.AssignClientAuthority(player.connectionToClient);
            }
            else
            {
                netIdentity.RemoveClientAuthority();
            }
        }

        /// <summary>
        /// Updates the power state of this workstation.
        /// </summary>
        /// <param name="isPowered">Whether this workstation is powered.</param>
        [Command(requiresAuthority = false)]
        private void CmdChangePower(bool isPowered)
        {
            this.isPowered = isPowered;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Resets all workstations used in launch mode (other than the thrusters).
        /// </summary>
        public void ResetLaunchWorkstations() 
        {
            foreach (Workstation launchWorkstation in _workstationManager.GetLaunchWorkstations())
            {
                if (launchWorkstation.StationID != WorkstationID.ThrustersAB && launchWorkstation.StationID != WorkstationID.ThrustersCD)
                {
                    TakeAuthorityOfWorkstation(launchWorkstation.StationID);
                }
            }

            // Reset the ship as a whole
            ShipStateManager.Instance.CmdResetShip();
        }

        /// <summary>
        /// Takes authority of a workstation with the given ID, if the workstation has a player there. This is used in resetting launch workstations.
        /// </summary>
        /// <param name="workstationID">The ID of the workstation for the player to take authority of.</param>
        protected void TakeAuthorityOfWorkstation(WorkstationID workstationID) 
        {
            if (_workstationManager.GetWorkstation(workstationID).playerAtWorkstation)
            {
                _workstationManager.GetWorkstation(workstationID).CmdSetWorkstationAuthorityToPlayer(playerAtWorkstation, true);
            }
        }

        /// <summary>
        /// Converts the given Workstation ID to a prettified name of the workstation.
        /// </summary>
        /// <param name="id">The ID of the workstation.</param>
        /// <returns>The ID prettified to a string.</returns>
        public static string GetPrettyName(WorkstationID id)
        {
            switch (id)
            {
                case WorkstationID.FlightEngineer:
                    return "Flight Engineer";
                case WorkstationID.NavReader:
                    return "Navigation";
                case WorkstationID.PowerRouting:
                    return "Power Routing";
                case WorkstationID.ThrustersAB:
                    return "Thrusters AB";
                case WorkstationID.ThrustersCD:
                    return "Thrusters CD";
                case WorkstationID.CubeStation:
                    return "Cube Station";
                default:
                    return id.ToString();
            }
        }
        #endregion
    }
}


