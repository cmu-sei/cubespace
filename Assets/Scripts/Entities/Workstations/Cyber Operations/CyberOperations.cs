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
using UnityEngine.UI;
using Systems.GameBrain;
using Mirror;
using UI;
using Audio;

namespace Entities.Workstations.CyberOperationsParts
{
    /// <summary>
    /// The workstation used for opening a VM via a URL, through a confirmation window.
    /// </summary>
    [RequireComponent(typeof(VMWindowController))]
    public class CyberOperations : Workstation
    {
        #region Variables
        [SerializeField]
        [Header("VM Variables")]
        /// <summary>
        /// The controller of the window.
        /// </summary>
        protected VMWindowController windowController;
        /// <summary>
        /// The controller of the screen
        /// </summary>
        [SerializeField] 
        private CyberOperationsScreenController screenController;

        /// <summary>
        /// Whether to automatically upgrade the url set to this workstation.
        /// </summary>
        [SerializeField]
        private bool autoUpgradeUrlToHttps = true;
        /// <summary>
        /// The URL set vm to be opened
        /// </summary>
        public string vmURL;
        /// <summary>
        /// The name used for the confirmation window if using the old structure
        /// </summary>
        public string staticVmName = "Cyber Operations";

        // True if someone is at the station
        private bool inUse;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Adds a listener for the open window button.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            if (!AlwaysHasPower)
            {
                Debug.LogWarning("CyberOperations must always have power! Please check 'always has power' box for this component.", this);
            }
        }

        /// <summary>
        /// Unity event function that subscribes to the action called when ship data is received.
        /// </summary>
        private void OnEnable()
        {
            ShipGameBrainUpdater.OnShipDataReceived += OnShipDataReceived;
        }

        /// <summary>
        /// Unity event function that unsubscribes from the action called when ship data is received.
        /// </summary>
        private void OnDisable()
        {
            ShipGameBrainUpdater.OnShipDataReceived -= OnShipDataReceived;
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Enables the UI objects needed to access the workstation and gives the player authority over the workstation.
        /// </summary>
        /// <param name="player">The player activating the workstation.</param>
        /// <param name="currentCam">The camera to zoom into the workstation.</param>
        public override void Activate(Player player, Cinemachine.CinemachineVirtualCamera currentCam)
        {
            base.Activate(player, currentCam);
            inUse = true;
            screenController.ResetState();
        }

        /// <summary>
        /// Disables the UI objects needed to access the workstation and gives the player authority over the workstation.
        /// </summary>
        public override void Deactivate()
        {
            // Unmute the game in case a separate tab was open
            AudioPlayer.Instance.SetMuteSnapshot(false);
            base.Deactivate();
            inUse = false;
            screenController.ResetState();
        }

        /// <summary>
        /// Changes the power state on the workstation and closes any open VM at this station.
        /// </summary>
        /// <param name="isPowered">Whether this workstation is powered.</param>
        public override void ChangePower(bool isPowered)
        {
            base.ChangePower(isPowered);
            if (!isPowered)
            {
                windowController.CloseVM(StationID);
            }
        }

        /// <summary>
        /// Closes the VM window (if open) and resets the workstation.
        /// </summary>
        public override void ResetWorkstation()
        {
            windowController.CloseVM(StationID);
            base.ResetWorkstation();
        }

        /// <summary>
        /// Sets this workstation's URL for its VM from the ship data received.
        /// </summary>
        /// <param name="hasChanged">Whether the ship data received has changed.</param>
        /// <param name="data">The ship data received.</param>
        private void OnShipDataReceived(bool hasChanged, GameData data)
        {   
            bool usingNewStructure = data.ship.IsMissionVMsStructureInUse();

            if (!screenController.usingOldStructure != usingNewStructure)
            {
                screenController.usingOldStructure = !usingNewStructure;
            }

            if (!usingNewStructure)
            {
                if (hasChanged || string.IsNullOrEmpty(vmURL))
                {
                    vmURL = data.ship.GetURLForStation(StationID);
                }
            }
            else if (hasChanged) // TODO: hasChanged is not accurate here and this will destroy and recreate the buttons every 2 seconds; BAD
            {
                screenController.OnShipDataReceived(data);
            }
        }

        public void OnMouseModelClick()
        {
            screenController.OnMouseModelClick();
        }
        #endregion

        #region VM Window Functions

        /// <summary>
        /// Opens an embedded VM window in the game at the workstation. The JavaScript code will close any previously opened window beforehand.
        /// </summary>
        public void OpenVMWindowEmbedded()
        {
            if (!string.IsNullOrEmpty(vmURL))
            {
                if (autoUpgradeUrlToHttps) vmURL = vmURL.Replace("http://", "https://");
                windowController.OpenWindowInFrame(vmURL, StationID);
                AudioPlayer.Instance.SetMuteSnapshot(true);
            }
        }

        /// <summary>
        /// Opens a VM window in a new tab. The JavaScript code will close any previously opened window beforehand.
        /// </summary>
        public void OpenVMWindowNewTab()
        {
            if (!string.IsNullOrEmpty(vmURL))
            {
                windowController.OpenWindowInTab(vmURL, StationID, staticVmName);
                AudioPlayer.Instance.SetMuteSnapshot(true);
            }
        }

        public void OnCloseVMWindow()
        {
            AudioPlayer.Instance.SetMuteSnapshot(false);
        }
        #endregion
    }
}

