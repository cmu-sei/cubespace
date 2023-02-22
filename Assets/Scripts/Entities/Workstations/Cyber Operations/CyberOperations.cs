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

namespace Entities.Workstations.CyberOperationsParts
{
    /// <summary>
    /// The workstation used for opening a VM via a URL, through a confirmation window.
    /// </summary>
    [RequireComponent(typeof(VMWindowController))]
    public class CyberOperations : VMWorkstation
    {
        #region Variables
        /// <summary>
        /// The button used to open the confirmation window.
        /// </summary>
        [SerializeField]
        private Button _openWindowButton;
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
            _openWindowButton.onClick.AddListener(OpenConfirmationWindow);
        }
        #endregion

        #region VMWorkstation methods
        /// <summary>
        /// Makes the button to open a window interactable or non-interactable.
        /// </summary>
        /// <param name="state">Whether the button is interactable.</param>
        protected override void SetAccessUIState(bool state) 
        {
            _openWindowButton.interactable = state;
        }

        /// <summary>
        /// Sets this workstation's URL for its VM from the ship data received.
        /// </summary>
        /// <param name="hasChanged">Whether the ship data received has changed.</param>
        /// <param name="data">The ship data received.</param>
        protected override void OnShipDataReceived(bool hasChanged, GameData data)
        {
            if (hasChanged || string.IsNullOrEmpty(_vmURL))
            {
                _vmURL = data.ship.GetURLForStation(StationID);
            }
        }
        #endregion
    }
}

