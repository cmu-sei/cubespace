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
using Mirror;
using Entities.Workstations.CyberOperationsParts;
using Systems.GameBrain;
using Audio;
using UI;

namespace Entities.Workstations 
{
    /// <summary>
    /// A workstation used to access a virtual machine. Cyber ops previously inhireted from this, codex still does. This should just be assimilated into codex and gotten rid of
    /// </summary>
    [RequireComponent(typeof(VMWindowController))]
    public abstract class VMWorkstation : Workstation
    {
        #region Variables
        /// <summary>
        /// The namer of the VM, set in the Inspector.
        /// </summary>
        [Header("VM Variables")]
        [SerializeField]
        protected string _vmName;
        /// <summary>
        /// Whether to automatically upgrade the url set to this workstation.
        /// </summary>
        [SerializeField]
        private bool autoUpgradeUrlToHttps = true;

        /// <summary>
        /// The URL of the VM.
        /// </summary>
        [SyncVar]
        protected string _vmURL;
        /// <summary>
        /// The confirmation screen, which displays options on opening the VM.
        /// </summary>
        protected ModalWindowContent _confirmationScreenContent;
        /// <summary>
        /// The controller of the window.
        /// </summary>
        protected VMWindowController _windowController;
        /// <summary>
        /// Whether a confirmation window is currently open.
        /// </summary>
        protected bool _confirmationWindowOpen = false;
        /// <summary>
        /// The string to display on the confirmation window when trying to access the VM.
        /// </summary>
        protected readonly string confirmationText = "Do you want to launch this VM in a new tab<br>or embedded into this page?";
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets a reference to the VM window and confirmation screen.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _windowController = GetComponent<VMWindowController>();
            _confirmationScreenContent = new ModalWindowContent(_vmName, confirmationText, "New Tab", "Embedded Window", OpenVMWindowNewTab, OpenVMWindowEmbedded, CloseConfirmationWindow);
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
            SetAccessUIState(true);
            base.Activate(player, currentCam);
        }

        /// <summary>
        /// Disables the UI objects needed to access the workstation and gives the player authority over the workstation.
        /// </summary>
        public override void Deactivate()
        {
            SetAccessUIState(false);
            // Unmute the game in case a separate tab was open
            AudioPlayer.Instance.SetMuteSnapshot(false);
            base.Deactivate();
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
                // This function has been commented out and will not actually do anything (see the method itself for details)
                _windowController.CloseVM(StationID);
            }
        }

        /// <summary>
        /// Closes the VM window (if open) and resets the workstation.
        /// </summary>
        public override void ResetWorkstation()
        {
            _windowController.CloseVM(StationID); // This function has been commented out and will not actually do anything (see the method itself for details)
            base.ResetWorkstation();
        }

        /// <summary>
        /// Closes the window prompting the player to open the VM window.
        /// </summary>
        private void CloseConfirmationWindow()
        {
            if (_confirmationWindowOpen)
            {
                _confirmationWindowOpen = false;
                SetAccessUIState(true);
            }
        }
        #endregion

        #region Event methods
        /// <summary>
        /// Callback method to perform logic when ship data is received. Override this method.
        /// </summary>
        /// <param name="hasChanged">Whether the data receved has changed.</param>
        /// <param name="data">The GameData received.</param>
        protected abstract void OnShipDataReceived(bool hasChanged, GameData data);
        #endregion

        #region VM window methods
        /// <summary>
        /// Opens the window to prompt the user to open the VM.
        /// </summary>
        public void OpenConfirmationWindow()
        {
            if (!_confirmationWindowOpen && _vmURL != null && _vmURL != "")
            {
                _confirmationWindowOpen = true;
                SetAccessUIState(false);
                ModalPanel.Instance.OpenWindow(_confirmationScreenContent);
            }
        }

        /// <summary>
        /// Opens an embedded VM window in the game at the workstation. The JavaScript code will close any previously opened window beforehand.
        /// </summary>
        protected virtual void OpenVMWindowEmbedded()
        {
            SetAccessUIState(true);
            _confirmationWindowOpen = false;
            if (autoUpgradeUrlToHttps) _vmURL = _vmURL.Replace("http://", "https://");
            _windowController.OpenWindowInFrame(_vmURL, StationID);
            AudioPlayer.Instance.SetMuteSnapshot(true);
        }

        /// <summary>
        /// Opens a VM window in a new tab. The JavaScript code will close any previously opened window beforehand.
        /// </summary>
        protected virtual void OpenVMWindowNewTab()
        {
            SetAccessUIState(true);
            _confirmationWindowOpen = false;
            _windowController.OpenWindowInTab(_vmURL, StationID, _vmName);
            AudioPlayer.Instance.SetMuteSnapshot(true);
        }

        /// <summary>
        /// Mutes the game when a VM window closes.
        /// </summary>
        public virtual void OnCloseVMWindow()
        {
            AudioPlayer.Instance.SetMuteSnapshot(false);
        }
        #endregion

        #region Access methods
        /// <summary>
        /// General method useful for turning on/off buttons or other UI objects used to access the workstation.
        /// This method must be overridden in any script deriving from this one.
        /// </summary>
        /// <param name="state">Whether the UI can be accessed.</param>
        protected abstract void SetAccessUIState(bool state);
        #endregion
    }
}

