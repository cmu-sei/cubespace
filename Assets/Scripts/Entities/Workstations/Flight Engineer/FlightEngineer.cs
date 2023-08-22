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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Entities.Workstations.CubeStationParts;
using Entities.Workstations.FlightEngineerParts;
using Mirror;
using Managers;
using System.Collections.Generic;

namespace Entities.Workstations
{
    /// <summary>
    /// A class for the flight engineer workstation, used to lock in and jump to a location.
    /// </summary>
    public class FlightEngineer : Workstation
    {
        #region Variables
        /// <summary>
        /// The launch angle dial.
        /// </summary>
        [SerializeField]
        private TrajectoryDial launchDial;
        /// <summary>
        /// The correction angle dial.
        /// </summary>
        [SerializeField]
        private TrajectoryDial correctionDial;
        /// <summary>
        /// The cube space trajectory dial.
        /// </summary>
        [SerializeField]
        private TrajectoryDial cubeSpaceTrajectoryDial;
        /// <summary>
        /// The lock button, used by the player after all target angles have been reached.
        /// </summary>
        [SerializeField]
        private LockButton lockButton;
        /// <summary>
        /// The button on top of the lock button object.
        /// </summary>
        [SerializeField]
        private Button lockButtonButtonOverlay;

        /// <summary>
        /// The current state of the dials - whether they are locked or not.
        /// Derives from a private variable.
        /// </summary>
        public bool DialsLocked => _dialsLocked;
        /// <summary>
        /// Whether the dials have been locked.
        /// </summary>
        [SyncVar(hook = nameof(OnDialsLocked))]
        private bool _dialsLocked = false;

        /// <summary>
        /// The default value and target settings for a dial.
        /// </summary>
        [SerializeField]
        private DialInfo defaultDialInfo = new DialInfo(0, -1);

        /// <summary>
        /// Action called when the dials, trajectory flippers, and cube have moved to the
        /// correct (true) or incorrect (false) positions.
        /// </summary>
        public static Action<bool> OnLaunchableChange;
        /// <summary>
        /// The action triggered when locking in the dial angles.
        /// </summary>
        public static Action OnLock;

        /// <summary>
        /// A dictionary associating a dial on the Flight Engineer to a representative DialID.
        /// </summary>
        private Dictionary<DialID, TrajectoryDial> dialToIDPairs = new Dictionary<DialID, TrajectoryDial>();
        /// <summary>
        /// A dictionary synchronizing the info of each DialID.
        /// </summary>
        private readonly SyncDictionary<DialID, DialInfo> dialIDInfoPairs = new SyncDictionary<DialID, DialInfo>();
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that initially disables the lock button and adds all dials
        /// and their angles to a dictionary that tracks them.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // Disable the lock button
            DisableLockButton();

            // Instantiate the dial to ID pair dictionary with all dial types and their defaults
            dialToIDPairs.Add(DialID.launchAngle, launchDial);
            dialToIDPairs.Add(DialID.correctionAngle, correctionDial);
            dialToIDPairs.Add(DialID.cubeSpaceTrajectoryAngle, cubeSpaceTrajectoryDial);

            if (isServer)
            {
                ResetDialPairs();
            }
        }

        /// <summary>
        /// Unity event function that subscribes to ShipStateManager actions.
        /// </summary>
        private void OnEnable()
        {
            ShipStateManager.OnCubeStateChange += OnCubeStateChange;
            ShipStateManager.OnLaunchableChange += CallLaunchableChangeCallback;
        }

        /// <summary>
        /// Unity event function that unsubscribes from ShipStateManager actions.
        /// </summary>
        private void OnDisable()
        {
            ShipStateManager.OnCubeStateChange -= OnCubeStateChange;
            ShipStateManager.OnLaunchableChange -= CallLaunchableChangeCallback;
        }
        #endregion

        #region Mirror methods
        /// <summary>
        /// Resets the dial pairs and subscribes a method to the dictionary of pairs, 
        /// called when the dial pair dictionary changes.
        /// </summary>
        public override void OnStartServer()
        {
            Debug.Log("Flight eng OnStartServer, adding OnDialPairsChange to dialIDInfoPairs.Callback");
            dialIDInfoPairs.Callback += OnDialPairsChange;
            base.OnStartServer();
        }
        #endregion

        #region SyncVar hooks and actions
        /// <summary>
        /// Asks clients to update a dial's rotation and check the lock button state when a dial ID and its information 
        /// are added to, edited in, or cleared from the dictionary. This function runs on the server.
        /// </summary>
        /// <param name="op">The operation called - add, set, remove, or clear (remove doesn't do anything).</param>
        /// <param name="id">The ID of the dial added to, edited in, or cleared from the dictionary.</param>
        /// <param name="info">The current angle of the dial and the target the dial should be set to by the player.</param>
        private void OnDialPairsChange(SyncDictionary<DialID, DialInfo>.Operation op, DialID id, DialInfo info)
        {
            Debug.Log("OnDialPairsChange: client updating dial info");
            switch (op)
            {
                case SyncIDictionary<DialID, DialInfo>.Operation.OP_ADD:
                    RpcSetDialInfo(id, info);
                    break;
                case SyncIDictionary<DialID, DialInfo>.Operation.OP_SET:
                    RpcSetDialInfo(id, info);
                    break;
                case SyncIDictionary<DialID, DialInfo>.Operation.OP_REMOVE:
                    break;
                case SyncIDictionary<DialID, DialInfo>.Operation.OP_CLEAR:
                    RpcSetDialInfo(id, info);
                    break;
            }
        }

        /// <summary>
        /// Updates the appearance of a dial and checks if the lock button is lockable across clients.
        /// </summary>
        /// <param name="id">The ID of the dial whose appearance should be updated.</param>
        /// <param name="info">The information </param>
        [ClientRpc]
        private void RpcSetDialInfo(DialID id, DialInfo info)
        {
            dialToIDPairs[id].UpdateDialAppearance(info);
            lockButton.SetVisualState();
        }

        /// <summary>
        /// Calls launchable change logic when the cube's state changes.
        /// </summary>
        /// <param name="cubeState">The new state of the cube.</param>
        private void OnCubeStateChange(CubeState cubeState)
        {
            CallLaunchableChangeCallback();
        }

        /// <summary>
        /// Performs logic when the dials are all locked.
        /// </summary>
        /// <param name="oldVal">The previous lock state of the dial.</param>
        /// <param name="newVal">The new lock state of the dial.</param>
        private void OnDialsLocked(bool oldVal, bool newVal)
        {
            // No logic needed here
        }
        #endregion

        #region Launchable/Unlaunchable Callbacks
        /// <summary>
        /// Activates or deactivates the engine sound effect and invokes events which have subscribed to
        /// the OnLaunchableChange action when the ship's ability to launch changes.
        /// </summary>
        private void CallLaunchableChangeCallback()
        {
            // If we can jump to another location, start the engine sound effect if the player is there
            if (IsLaunchable())
            {
                if (playerAtWorkstation && playerAtWorkstation.isLocalPlayer)
                {
                    Audio.AudioPlayer.Instance.FlightEngineerStartEngine();
                }
            }
            // Otherwise, stop the engine sound effect
            else
            {
                Audio.AudioPlayer.Instance.FlightEngineerStopEngine();
            }

            // Invoke the launchable change event with whether the game is launchable
            OnLaunchableChange?.Invoke(IsLaunchable());
        }
        #endregion

        #region Boolean Functions
        /// <summary>
        /// Checks if the ship is launchable based on if the dials have been locked, the thrusters have been flipped, and the cube is inserted.
        /// </summary>
        /// <returns>Whether the ship can jump to a new location.</returns>
        public bool IsLaunchable()
        {
            // If not all dials are activated or the cube isn't in the cube drive, return false
            if (!AreDialsActivated() || ShipStateManager.Instance.CubeState != CubeState.InCubeDrive)
            {
                return false;
            }
            
            // Make sure all thrusters have been activated
            foreach (bool thruster in ShipStateManager.Instance.thrusters)
            {
                if (!thruster)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Returns true when all dials are set to their neceessary target angles.
        /// </summary>
        public bool AreDialsActivated()
        {
            return dialToIDPairs.Values.All(d => d.targetAngle == d.GetAngle());
        }
        #endregion

        #region Dial info
        /// <summary>
        /// Tries to look up the current angle and target angle of a given dial.
        /// </summary>
        /// <param name="id">The ID of the dial to look up.</param>
        /// <returns>
        /// The launch angle and target angle of the given dial ID, or defaults 
        /// of 0 and -1 respectively if an ID cannot be found.
        /// </returns>
        public DialInfo GetDialInfo(DialID id)
        {
            // Try to look up the provided ID in the dictionary and return its dial's information
            try
            {
                return dialIDInfoPairs[id];
            }
            // If that fails, return default values and give an error message
            catch
            {
                if (((CustomNetworkManager) NetworkManager.singleton).isInDebugMode)
                {
                    Debug.LogWarning("Invalid DialID provided to Flight Engineer: " + id);
                }
                return new DialInfo(0, -1);
            }
        }

        /// <summary>
        /// Sets the current angle and target angle.
        /// </summary>
        /// <param name="id">The ID of the dial whose information should be set.</param>
        /// <param name="val">The current value of the dial.</param>
        /// <param name="target">The target value of the dial.</param>
        public void SetDialInfo(DialID id, int val, int target)
        {
            dialIDInfoPairs[id] = new DialInfo(val, target);
        }
        #endregion

        #region GameObject methods
        /// <summary>
        /// Locks trajectories on the server and change UI and audio on the client.
        /// <para>
        /// This is called by the FlightEngineer object; it should not be used elsewhere.
        /// </para>
        /// </summary>
        public void LockTrajectories()
        {
            // Make UI and audio changes
            OnLock?.Invoke();

            // Lock trajectories on the server
            CmdLockTrajectories();

            CallLaunchableChangeCallback();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Sets the dial information on the server.
        /// </summary>
        /// <param name="id">The ID of the dial whose information should be set.</param>
        /// <param name="val">The current value of the dial.</param>
        /// <param name="target">The target value of the dial.</param>
        [Command(requiresAuthority = false)]
        public void CmdSetDialInfo(DialID id, int val, int target)
        {
            SetDialInfo(id, val, target);
        }

        /// <summary>
        /// Locks the trajectories on the server.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdLockTrajectories()
        {
            // Lock the trajectories
            _dialsLocked = true;
            // Call the lock change
            ShipStateManager.Instance.TrajectoryLockChangeCallback(true);

            //todo: refactor this and ShipStateManager.cs "OnSetLocation"
            // If the location is set and the cube is not in the NavReader, place it there
            if (ShipStateManager.Instance.LocationSet && ShipStateManager.Instance.CubeState != CubeState.InNavReader)
            {
                ShipStateManager.Instance.SetCubeState(CubeState.InNavReader);
            }
        }

        /// <summary>
        /// Unlocks the trajectories on the server.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdUnlockTrajectories()
        {
            _dialsLocked = false;
        }

        /// <summary>
        /// Resets the targets of the dials on the server.
        /// </summary>
        [Command(requiresAuthority = false)]
        private void CmdResetDialPairs()
        {
            ResetDialPairs();
        }
        #endregion

        #region External calls
        /// <summary>
        /// Changes the current angle of the dial with the given ID to the given angle.
        /// </summary>
        /// <param name="id">The ID of the dial whose current angle should be updated.</param>
        /// <param name="angle">The current angle of the dial.</param>
        /// <param name="resetTarget">Whether to reset the dial target to -1.</param>
        public void ChangeDialAngle(DialID id, int angle, bool resetTarget=false) 
        {
            CmdSetDialInfo(id, angle, resetTarget ? -1 : GetDialInfo(id).target);
        }

        /// <summary>
        /// Sets all dial targets to their respective given angles.
        /// </summary>
        /// <param name="launchAngle">The angle to set the launch dial to target.</param>
        /// <param name="correctionAngle">The angle to set the correction dial to target.</param>
        /// <param name="cubeAngle">The angle to set the cubespace trajectory dial to target.</param>
        public void SetAllDialTargets(int launchAngle, int correctionAngle, int cubeAngle)
        {
            SetDialInfo(DialID.launchAngle, GetDialInfo(DialID.launchAngle).value, launchAngle);
            SetDialInfo(DialID.correctionAngle, GetDialInfo(DialID.correctionAngle).value, correctionAngle);
            SetDialInfo(DialID.cubeSpaceTrajectoryAngle, GetDialInfo(DialID.cubeSpaceTrajectoryAngle).value, cubeAngle);
        }

        /// <summary>
        /// Changes the state of a given thruster.
        /// </summary>
        /// <param name="index">The index of the thruster whose state should be set.</param>
        /// <param name="isActivated">Whether the thruter is activated.</param>
        public void ChangeSwitchState(int index, bool isActivated) 
        {
            ShipStateManager.Instance.CmdSetThruster(index, isActivated);
            CallLaunchableChangeCallback();
        }
        #endregion

        #region SFX methods
        /// <summary>
        /// Activates or deactivates the SFX of the given thruster.
        /// </summary>
        /// <param name="index">The index of the thruster whose sound should be activated or deactivated.</param>
        /// <param name="isActivated">Whether the thruster has been activated.</param>
        /// <param name="transform">The transform where this sound should be played.</param>
        /// <param name="ignorePowerUp">Whether to not play a sound effect for powering on a thruster.</param>
        public void SetThrusterSFX(int index, bool isActivated, Transform transform, bool ignorePowerUp = false) 
        {
            int numThrusters = 0;
            foreach (bool thruster in ShipStateManager.Instance.thrusters)
            {
                if (thruster)
                {
                    numThrusters++;
                }
            }

            /*
            // Set the number of thrusters enabled
            int numThrusters = ShipStateManager.Instance.thrusters.Where(on => on).Count();
            */

            // Sets the intensity of the hum of the thrusters based on the number of those activated
            Audio.AudioPlayer.Instance.FlightEngineerSetThrusterHumNumber(numThrusters);

            // If the lever is in the launch position, turn the thruster pulsing sound effect on
            if (isActivated) 
            {
                if (!ignorePowerUp) 
                {
                    Audio.AudioPlayer.Instance.FlightEngineerThrusterOn();
                }
                Audio.AudioPlayer.Instance.FlightEngineerThrusterPulseOn(index, transform);
            }
            // Otherwise, turn the thruster pulsing sound effect off
            else 
            {
                Audio.AudioPlayer.Instance.FlightEngineerThrusterPulseOff(index);
            }
        }
        #endregion

        #region Workstation Methods
        /// <summary>
        /// Plays the start engine sound if the ship is launchable on entering the Flight Engineer.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();
            if (IsLaunchable()) 
            {
                Audio.AudioPlayer.Instance.FlightEngineerStartEngine();
            }
        }

        /// <summary>
        /// Stops the engine sound effect and turns off the thruster sound effects on exiting the Flight Engineer.
        /// </summary>
        protected override void Exit()
        {
            base.Exit();
            for (int i = 0; i < ShipStateManager.Instance.thrusters.Count; i++) 
            {
                Audio.AudioPlayer.Instance.FlightEngineerThrusterPulseOff(i);
            }
            Audio.AudioPlayer.Instance.FlightEngineerSetThrusterHumNumber(0);
            Audio.AudioPlayer.Instance.FlightEngineerStopEngine();
        }

        /// <summary>
        /// Asks the server to reset dial pairs, thruster states, and trajectory locks, and disables the lock button on the front end.
        /// </summary>
        public override void ResetWorkstation()
        {
            // Ask the server to reset the dials
            CmdResetDialPairs();

            // Disable the lock button
            DisableLockButton();

            // Ask the server to reset the thruster states (unflipped)
            ShipStateManager.Instance.CmdSetAllThrusters(false);
            // Ask the server to unlock the trajectories
            CmdUnlockTrajectories();

            base.ResetWorkstation();
        }

        /// <summary>
        /// Resets the dictionary of DialID to DialInfo pairs to defaults.
        /// </summary>
        private void ResetDialPairs()
        {
            // The default value and target
            int defaultValue = defaultDialInfo.value;
            int defaultTarget = defaultDialInfo.target;

            // Set the dictionary entries to have their default targets
            SetDialInfo(DialID.launchAngle, defaultValue, defaultTarget);
            SetDialInfo(DialID.correctionAngle, defaultValue, defaultTarget);
            SetDialInfo(DialID.cubeSpaceTrajectoryAngle, defaultValue, defaultTarget);
        }
        #endregion

        #region Lock button methods
        /// <summary>
        /// Allows the lock button to be pressed.
        /// </summary>
        public void EnableLockButton()
        {
            lockButtonButtonOverlay.enabled = true;
        }

        /// <summary>
        /// Prevents the unlock button from being pressed.
        /// </summary>
        public void DisableLockButton()
        {
            lockButtonButtonOverlay.enabled = false;
        }
        #endregion
    }
}


