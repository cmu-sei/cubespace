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
using Managers;
using Entities.Workstations;
using Mirror;
using Audio;
using UI;

namespace Entities
{
    /// <summary>
    /// A basic interactable object without workstation functionality. These are used in the Main.unity scene.
    /// </summary>
    public class Terminal : Interactable
    {
        #region Variables
        /// <summary>
        /// The WorkstationManager ScriptableObject that manages all Terminals.
        /// </summary>
        [SerializeField]
        private WorkstationManager _workstationManager;
        /// <summary>
        /// The icon displayed on mouse over, also showing whether this Terminal is in use.
        /// </summary>
        [SerializeField]
        private WorkstationIcon icon;
        /// <summary>
        /// The ID of the workstation this Terminal is associated with. Derives from a private variable.
        /// </summary>
        public WorkstationID StationID => stationID;
        /// <summary>
        /// The ID of the workstation this Terminal is associated with.
        /// </summary>
        [SerializeField]
        private WorkstationID stationID;

        /// <summary>
        /// Whether this Terminal is being used by a player.
        /// </summary>
        [HideInInspector]
        [SyncVar(hook = nameof(OnChangeStatus))]
        private bool inUse = false;
        /// <summary>
        /// The coroutine playing Terminal ambiance.
        /// </summary>
        private Coroutine audioCoroutine;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that registers this Terminal in the WorkstationManager and starts playing ambiance.
        /// </summary>
        private void Start()
        {
            if (stationID != WorkstationID.NULL)
            {
                _workstationManager.RegisterTerminal(stationID, this);
            }

            audioCoroutine = StartCoroutine(AudioPlayer.Instance.TerminalAmbience(transform));
        }

        /// <summary>
        /// Unity event function that enables the workstation's icon when local player mouses over it.
        /// </summary>
        void OnMouseEnter()
        {
            if (Player.LocalCanInput)
            {
                icon.EnableIcon();
            }
        }

        /// <summary>
        /// Unity event function that disables the workstation's icon when local player mouses out of it.
        /// </summary>
        void OnMouseExit()
        {
            icon.DisableIcon();
        }
        #endregion

        #region SyncVar hooks
        /// <summary>
        /// Changes the occupation status display of the workstation icon based on whether this Terminal is in use.
        /// </summary>
        /// <param name="oldVal">The old status of whether this Terminal is in use.</param>
        /// <param name="newVal">The new status of whether this Terminal is in use.</param>
        private void OnChangeStatus(bool oldVal, bool newVal)
        {
            if (oldVal != newVal)
            {
                icon.SetOccupied(newVal);
            }
        }
        #endregion

        #region Interaction/access methods
        /// <summary>
        /// Places this Terminal's workstation under the player's authority if possible and moves the player to the Terminal.
        /// </summary>
        /// <param name="player">The Player object interacting with this Terminal.</param>
        public override void OnInteract(Player player)
        {
            base.OnInteract(player);

            // If the player is in range when they try to interact and this Terminal is free, access the Terminal's workstation.
            if (isInRange && !inUse && _workstationManager.CheckForWorkstation(stationID))
            {
                AccessWorkstation(player);
                SetInUse(true);
            }

            // Sets this Terminal's interaction point as the player's destination
            player.SetNavAgentDestination(interactionPoint.position);
        }

        /// <summary>
        /// Places the workstation associated with this Terminal under the player's authority.
        /// </summary>
        /// <param name="player">The Player object trying to access this Terminal's workstation.</param>
        public void AccessWorkstation(Player player)
        {
            _workstationManager.AccessWorkstation(player, stationID);
        }

        /// <summary>
        /// Marks the Terminal as not in use. Used when a player stops interacting with this Terminal.
        /// </summary>
        public void ExitTerminal()
        {
            SetInUse(false);
        }
        #endregion

        #region In use methods
        /// <summary>
        /// Sets this Terminal object as in use.
        /// </summary>
        /// <param name="inUse">Whether this Terminal is in use.</param>
        private void SetInUse(bool inUse)
        {
            if (NetworkClient.active)
            {
                CmdSetInUse(inUse);
                return;
            }

            if (isServer)
            {
                this.inUse = inUse;
                return;
            }

            Debug.LogWarning("Tried to mark a Terminal in use on the client.");
        }

        /// <summary>
        /// Sets this Terminal object as in use.
        /// </summary>
        /// <param name="inUse">Whether this Terminal is in use.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetInUse(bool inUse)
        {
            this.inUse = inUse;
        }
        #endregion

        #region Entry/Exit methods
        /// <summary>
        /// Enables the icon displayed when a player enters the range of the Terminal.
        /// </summary>
        /// <param name="player">The Player object entering the range of the Terminal.</param>
        public override void LocalPlayerEnters(Player player)
        {
            base.LocalPlayerEnters(player);
            icon.SetInRange(isInRange);
        }

        /// <summary>
        /// Disables the icon displayed when a player exits the range of the Terminal.
        /// </summary>
        /// <param name="player">The Player object exiting the range of the Terminal.</param>
        public override void LocalPlayerExits(Player player)
        {
            base.LocalPlayerExits(player);
            icon.SetInRange(isInRange);
        }
        #endregion
    }
}

