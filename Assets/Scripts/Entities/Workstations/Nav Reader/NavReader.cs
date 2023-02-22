/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Entities.Workstations.CubeStationParts;
using UnityEngine;
using Managers;
using Mirror;
using Systems.GameBrain;
using UI.NavScreen;
using Entities.Workstations.NavReaderParts;

namespace Entities.Workstations
{
    /// <summary>
    /// The NavReader workstation, used to select locations, unlock locations, and create the cube.
    /// </summary>
    [RequireComponent(typeof(NavScreenController))]
    [RequireComponent(typeof(NavReaderCubeHandler))]
    public class NavReader : Workstation
    {
        #region Variables
        /// <summary>
        /// The screen controller that moves between screens and cleans them up.
        /// </summary>
        private NavScreenController _screenController;
        /// <summary>
        /// The area where the cube appears when a location is set and trajectories are locked.
        /// </summary>
        private NavReaderCubeHandler _cubeHandler;
        #endregion

        #region Unity Events
        /// <summary>
        /// Gets the cube handler object and the screen controller on the NavReader.
        /// </summary>
        protected override void Awake()
        {
            _cubeHandler = GetComponent<NavReaderCubeHandler>();
            _screenController = GetComponent<NavScreenController>();
            base.Awake();
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Resets the nav screens and resets the NavReader back to its default state.
        /// </summary>
        public override void ResetWorkstation()
        {
            _screenController.ResetNavScreens();
            _screenController.SetToDefaultScreen();
            base.ResetWorkstation();
        }
        #endregion

        #region Ship Command Wrappers
        /// <summary>
        /// Sends a request to the server to confirm the location provided as the destination.
        /// </summary>
        /// <param name="location">The location to set as the destination.</param>
        public void ConfirmLocation(Location location)
        {
            int loc = ShipStateManager.Instance.GetLocationIndex(location);
            CmdConfirmLocation(loc);
        }
        #endregion

        #region Ship Commands
        /// <summary>
        /// Makes a call to the server to send a request to unlock a location, given coordinates.
        /// </summary>
        /// <param name="locationCoords">The coordinates used to try to unlock a location.</param>
        [Command(requiresAuthority = false)]
        public void CmdTryUnlockLocation(string locationCoords)
        {
            ShipStateManager.Instance.ShipGameBrainUpdater.SendUnlockLocationRequest(locationCoords);
        }

        /// <summary>
        /// Makes a call to the server to set the target location to one at the specified index.
        /// </summary>
        /// <param name="index">The index correlating with a location in the list.</param>
        [Command(requiresAuthority = false)]
        public void CmdConfirmLocation(int index)
        {
            ShipStateManager.Instance.SetLocation(index);
        }

        /// <summary>
        /// Ejects the cube from the NavReader. This is called on the red triangular button to dispense the cube on the NavReader.
        /// </summary>
        public void EjectCube()
        {
            // If a player is holding the cube and the cube is within the NavReader, eject the cube
            if (!ShipStateManager.Instance.PlayerIsHoldingCube(playerAtWorkstation) && ShipStateManager.Instance.CubeState == CubeState.InNavReader)
            {
                // Have the player at the workstation pick up the cube and eject it from the NavReader
                ShipStateManager.Instance.PickUpCube(playerAtWorkstation);
                _cubeHandler.EjectCube();
            }
        }
        #endregion

        #region Workstation Methods
        /// <summary>
        /// Enters the workstation and displays the NavReader screen.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();
            _screenController.RefreshNavScreen();
        }
        #endregion

    }
}

