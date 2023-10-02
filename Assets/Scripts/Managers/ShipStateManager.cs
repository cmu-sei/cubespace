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
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using Mirror;
using Entities.Workstations;
using Entities.Workstations.CubeStationParts;
using Systems;
using Systems.GameBrain;
using Systems.CredentialRequests.Models;
using Entities;
using UI.HUD;

namespace Managers
{
    /// <summary>
    /// Manager for the state of the ship.
    /// </summary>
    [RequireComponent(typeof(ShipGameBrainUpdater))]
    public partial class ShipStateManager : NetworkedSingleton<ShipStateManager>
    {
        #region Variables
        /// <summary>
        /// The ScriptableObject tracking all workstations.
        /// </summary>
        [SerializeField]
        private WorkstationManager _workstationManager;

        /// <summary>
        /// The time to wait before resetting the ship.
        /// </summary>
        [SerializeField]
        private float delayResetTime = 2.0f;

        /// <summary>
        /// The updater object that requests changes through the interface.
        /// Note that the calls to interface methods are wrapped within commands in relevant workstations (this class has a call
        /// as well because of variables within this class).
        /// </summary>
        public ShipGameBrainUpdater ShipGameBrainUpdater => shipGameBrainUpdater;
        /// <summary>
        /// The reference to the Gamebrain updater used on this object.
        /// </summary>
        private ShipGameBrainUpdater shipGameBrainUpdater;

        // Host/client actions
        /// <summary>
        /// An action called when the state of the cube changes.
        /// </summary>
        public static Action<CubeState> OnCubeStateChange;
        /// <summary>
        /// An action called when the status of SetLocation changes, with the boolean marking a success or fail.
        /// </summary>
        public static Action<bool> OnSetLocationChange;
        /// <summary>
        /// An action called when trajectories are locked, with the boolean marking locked or unlocked.
        /// </summary>
        public static Action<bool> OnTrajectoryLockUpdate;
        /// <summary>
        /// An action called when the ship is ready to launch.
        /// </summary>
        public static Action OnLaunchableChange;
        
        // Client-side network update actions
        /// <summary>
        /// An action called when a location is unlocked.
        /// </summary>
        public static Action<LocationUnlockResponse> OnTryLocationUnlockResponse;
        /// <summary>
        /// An action called when the list of unlocked locations changes.
        /// </summary>
        public static Action<Location[]> UnlockedLocationsChangedHook;
        /// <summary>
        /// An action called when the current location changes.
        /// </summary>
        public static Action<Location> OnCurrentLocationChange;
        /// <summary>
        /// An action called when the list of mission data changes from Gamebrain.
        /// </summary>
        public static Action<List<MissionData>> OnMissionDataChange;

        // Server side only actions
        /// <summary>
        /// An action called when the ship's current location changes.
        /// </summary>
        public static Action<Location> ServerOnCurrentLocationChange;
        
        /// <summary>
        /// The state of the cube, synchronized across clients. This derives from a private variable.
        /// </summary>
        public CubeState CubeState => _cubeState;
        /// <summary>
        /// The state of the cube, synchronized across clients. Used within this class.
        /// </summary>
        [SyncVar(hook = nameof(OnCubeStateChangeHook))]
        private CubeState _cubeState;

        /// <summary>
        /// The identity of the player holding the cube; null if no one else is holding the cube.
        /// </summary>
        [SyncVar]
        private NetworkIdentity _playerHoldingCube = null;

        /// <summary>
        /// The location of the ship.
        /// </summary>
        [SyncVar(hook = nameof(CurrentLocationChangeHook))]
        private Location currentLocation;

        /// <summary>
        /// Data concerning this team's play session, including the name, the codex count, and the jump URL. Derives from a private variable.
        /// </summary>
        public Session Session => session;
        /// <summary>
        /// The core session data.
        /// </summary>
        [SyncVar]
        private Session session = new Session();

        /// <summary>
        /// Used to determine whether or not map button should be displayed. Also contained in `session`, duplicated here so that hook is only called when this actually changes, not just anything in session.
        /// </summary>
        [SyncVar(hook = nameof(UseGalaxyMapHook))][HideInInspector]
        public bool useGalaxyMap = false;

        /// <summary>
        /// The list of thrusters with their original flipped states.
        /// </summary>
        public readonly SyncList<bool> thrusters = new SyncList<bool>() { false, false, false, false };

        // The index in the location list for the location selected
        /// <summary>
        /// The index in the location list for the location selected. Derives from a public variable.
        /// </summary>
        public int CurrentSetLocationIndex => currentSetLocationIndex;
        /// <summary>
        /// The index in the location list for the location selected.
        /// </summary>
        [SyncVar]
        private int currentSetLocationIndex = 0;

        /// <summary>
        /// Whether a location has been locked in at the NavReader. Derives from a private variable.
        /// </summary>
        public bool LocationSet => locationSet;
        /// <summary>
        /// Whether a location has been locked in at the NavReader.
        /// </summary>
        [SyncVar(hook = nameof(OnSetLocationHook))]
        private bool locationSet = false;

        // Whether the trajectories have been locked

        /// <summary>
        /// Whether the trajectories have been locked. Derives from a private variable.
        /// </summary>
        public bool TrajectoriesLocked => trajectoriesLocked;
        /// <summary>
        /// Whether the trajectories have been locked.
        /// </summary>
        [SyncVar]
        private bool trajectoriesLocked = false;

        /// <summary>
        /// The name used when connecting to a remote network via the antenna system. Derives from a private variable.
        /// </summary>
        public string CurrentNetworkName => currentNetworkName;
        /// <summary>
        /// The name used when connecting to a remote network via the antenna system.
        /// </summary>
        [SyncVar]
        private string currentNetworkName = "REMOTE SYSTEM";

        /// <summary>
        /// The list of locations available to the team so far.
        /// </summary>
        public readonly SyncList<Location> unlockedLocations = new SyncList<Location>();
        /// <summary>
        /// The list of missions the team can attempt.
        /// </summary>
        public readonly List<MissionData> MissionData = new List<MissionData>();

        /// <summary>
        /// The token of the team, used by the server to make requests to Gamebrain.
        /// </summary>
        [HideInInspector]
        public string token = null;
        /// <summary>
        /// A hexadecimal string representing the team ID (i.e. "053820f008e741a29010f658e80592fe")
        /// </summary>
        [HideInInspector]
        public string teamID = "";

        /// <summary>
        /// The list of workstations with accessible VMs.
        /// </summary>
        [HideInInspector]
        public List<VMWorkstation> vmWorkstations = new List<VMWorkstation>();

        /// <summary>
        /// Whether first contact has been made. Derives from a private variable.
        /// </summary>
        public bool FirstContactEstablished => firstContactEstablished;
        /// <summary>
        /// Whether first contact has been made.
        /// </summary>
        [SyncVar]
        private bool firstContactEstablished;

        /// <summary>
        /// An action called when the ship launched.
        /// </summary>
        public static Action OnLaunch;

        /// <summary>
        /// The CustomNetworkManager component of the main NetworkManager singleton.
        /// </summary>
        private CustomNetworkManager networkManager;
        #endregion

        #region Unity Event Functions
        /// <summary>
        /// Unity event function that gets the reference to the GameBrain updater object.
        /// </summary>
        public override void Awake()
        {
            shipGameBrainUpdater = GetComponent<ShipGameBrainUpdater>();
            LoadingSystem.Instance.UpdateLoadingMessage("Acquiring Alien Intelligence...");
            base.Awake();
        }

        /// <summary>
        /// Unity event function that gets the CustomNetworkManager component of the main NetworkManager object and gets all VM workstations.
        /// </summary>
        private void Start()
        {
            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

            if (isServer)
            {
                // Add all VMs to the dictionary
                foreach (VMWorkstation w in FindObjectsOfType<VMWorkstation>())
                {
                    vmWorkstations.Add(w);
                }
            }
        }

        /// <summary>
        /// Unity event function that checks for dev hotkeys if enabled.
        /// </summary>
        private void Update()
        {
            // If hotkeys are enabled and this is a client
            if (networkManager && networkManager.isInDevMode && isClient)
            {
                // Jump if the hotkey is pressed
                if (Input.GetKeyDown(networkManager.jumpKeyCode) && locationSet)
                {
                    CmdJump();
                }

                // Abort the jump preparation process if the hotkey is pressed
                if (Input.GetKeyDown(networkManager.abortKeyCode))
                {
                    CmdResetShip();
                }
            }
        }

        /// <summary>
        /// Unity event function that subscribes to GameBrain actions.
        /// </summary>
        private void OnEnable()
        {
            // Macro that sets the token from the LoadingSystem if this is a WebGL build (if it's the server build, we have to wait to get the token).
            #if UNITY_WEBGL
                if (LoadingSystem.Instance) token = LoadingSystem.Instance.token;
                else Debug.Log("LoadingSystem does not exist! Therefore we cannot retrieve the JSON web token.");
            #endif

            // Subscribe to GameBrain actions
            ShipGameBrainUpdater.OnShipDataReceived += OnShipDataReceived;
            ShipGameBrainUpdater.OnLocationUnlockResponse += OnLocationUnlockResponse;
            ShipGameBrainUpdater.OnJumpResponse += OnJumpResponse;
        }

        /// <summary>
        /// Unity event function that unsubscribes from GameBrain actions.
        /// </summary>
        private void OnDisable()
        {
            // Unsubscribe from GameBrain actions
            ShipGameBrainUpdater.OnShipDataReceived -= OnShipDataReceived;
            ShipGameBrainUpdater.OnLocationUnlockResponse -= OnLocationUnlockResponse;
            ShipGameBrainUpdater.OnJumpResponse -= OnJumpResponse;
        }
        #endregion

        #region Action functions called on server
        /// <summary>
        /// Super-function that updates ship and session data when new data is recieved from GameBrain.
        /// <para>
        /// This is used to subscribe to the OnShipDataReceived action within the ShipGameBrainUpdater.
        /// </para>
        /// </summary>
        /// <param name="updatedSinceLastPoll">Whether the ship data has been updated since the last request for it, or if it is different.</param>
        /// <param name="data">The ship data received from the poll.</param>
        [Server]
        private void OnShipDataReceived(bool updatedSinceLastPoll, GameData data)
        {
            // If the data is null or has no current location, do not continue
            if (data == null || data.currentStatus.currentLocation == "")
            {
                if (data == null)
                {
                    Debug.LogError("Recieved null data!");
                }
                else
                {
                    Debug.LogError("Current location is an empty string");
                }
                return;
            }

            // Mark a default for whether we need to reset the ship at the end of this data handling
            bool doReset = false;

            // Update the team's session data to whatever was received
            session = data.session;

            // Update useGalaxyMap if it's changed. Will enable/disable map button via callbacks
            if (data.session.useGalaxyDisplayMap != useGalaxyMap)
            {
                useGalaxyMap = data.session.useGalaxyDisplayMap;
            }

            // Mark whether first contact was completed
            firstContactEstablished = data.currentStatus.firstContactComplete;
            // Update the network name to what's included in the data
            currentNetworkName = data.currentStatus.networkName;

            // If jumping, mark that we need to do a full ship reset
            if (currentLocation != null && currentLocation.locationID != "" && currentLocation.locationID != data.currentStatus.currentLocation)
            {
                doReset = true;
            }

            // Update the ship's unlocked location list
            MergeLocationList(data);
            if (data.ship != null && data.currentStatus.currentLocation != null)
            {
                // Try to find the current location in the location dictionary
                if (data.locationMap != null && data.locationMap.TryGetValue(data.currentStatus.currentLocation, out var loc))
                {
                    // Mark the current location as the one found in the dictionary; this calls currentLocation's SyncVar Hook on clients
                    currentLocation = loc;
                    // Invoke changes on the server when the location changes as well
                    ServerOnCurrentLocationChange?.Invoke(currentLocation);
                }
                // If the current location received is not in the dictionary, we should not set a current location
                else
                {
                    currentLocation = null;
                }
            }

            // Update mission log/task list to what's included in the data
            MergeMissionDataList(data);

            // Check if the ship is supposed to reset (like after a jump)
            if (doReset)
            {
                StartCoroutine(DelayShipReset(delayResetTime));
            }
        }

        /// <summary>
        /// Performs actions across clients via RPC when a location is unlocked.
        /// <para>
        /// This is used to subscribe to the OnLocationUnlockResponse action within the ShipGameBrainUpdater.
        /// </para>
        /// </summary>
        /// <param name="response">The repsonse received after unlocking a location.</param>
        [Server]
        private void OnLocationUnlockResponse(LocationUnlockResponse response)
        {
            RpcOnLocationUnlockResponse(response);
        }

        /// <summary>
        /// Resets the ship state and performs actions on clients when launching the ship.
        /// <para>
        /// This is used to subscribe to the OnJumpResponse action within the ShipGameBrainUpdater.
        /// </para>
        /// </summary>
        /// <param name="response">The repsonse received after jumping.</param>
        [Server]
        public void OnJumpResponse(GenericResponse response)
        {
            if (response.success)
            {
                // Reset the state of the ship back to its default
                ResetShip();
                // Perform actions across clients when launching the ship
                RpcLaunch();
            }
        }
        #endregion

        #region SyncVar hooks
        /// <summary>
        /// A function that runs on clients as a SyncVar hook when the ship's current location changes.
        /// </summary>
        /// <param name="oldLocation">The previous location of the ship. This is unused but necessary to include.</param>
        /// <param name="location">The ship's new current location.</param>
        private void CurrentLocationChangeHook(Location oldLocation, Location location)
        {
            OnCurrentLocationChange?.Invoke(location);
        }

        /// <summary>
        /// A function that runs on clients as a SyncVar hook when the cube's state changes.
        /// </summary>
        /// <param name="oldCubeState">The previous state of the cube. This is unused but necessary to include.</param>
        /// <param name="newCubeState">The cube's new state.</param>
        private void OnCubeStateChangeHook(CubeState oldCubeState, CubeState newCubeState)
        {
            OnCubeStateChange?.Invoke(newCubeState);
        }

        /// <summary>
        /// A function that runs on clients as a SyncVar hook when a location has been set or unset in the NavReader.
        /// </summary>
        /// <param name="prevState">Whether a location was set previously.</param>
        /// <param name="newState">Whether a location has now been set.</param>
        private void OnSetLocationHook(bool prevState, bool newState)
        {
            OnSetLocationChange?.Invoke(newState);
        }

        /// <summary>
        /// A function that runs on clients as a SyncVar hook when useGalaxyMap has been changed by GameBrain.
        /// </summary>
        private void UseGalaxyMapHook(bool prevState, bool newState)
        {
            if (HUDController.Instance)
            {
                HUDController.Instance.UpdateMapButtonVisibility(newState);
            }
        }
        #endregion

        #region SyncList callbacks
        /// <summary>
        /// A function that runs on clients as a callback when the SyncList of unlocked locations is modified.
        /// </summary>
        /// <param name="op">The operation performed on the SyncList (add, clear, insert, remove, or set).</param>
        /// <param name="index">The index of the Location affected within the SyncList.</param>
        /// <param name="oldItem">The previous Location data at the index given.</param>
        /// <param name="newItem">The new Location data at the index given.</param>
        private void UnlockedLocationsOnCallback(SyncList<Location>.Operation op, int index, Location oldItem, Location newItem)
        {
            UnlockedLocationsChangedHook?.Invoke(unlockedLocations.ToArray());
        }

        /// <summary>
        /// A function that runs on clients as a callback when the SyncList of thruster states is modified.
        /// </summary>
        /// <param name="op">The operation performed on the SyncList (add, clear, insert, remove, or set).</param>
        /// <param name="index">The index of the thruster affected within the SyncList.</param>
        /// <param name="oldValue">The previous state of the thruster at the index given.</param>
        /// <param name="newValue">The new state of the thruster at the index given.</param>
        void OnThrusterValueChange(SyncList<bool>.Operation op, int index, bool oldValue, bool newValue)
        {
            // Because we have a fixed number of thrusters, we only care about performing an action when setting, not adding, inserting, removing, or clearing the list
            switch (op)
            {
                case SyncList<bool>.Operation.OP_ADD:
                    break;
                case SyncList<bool>.Operation.OP_INSERT:
                    break;
                case SyncList<bool>.Operation.OP_REMOVEAT:
                    break;
                case SyncList<bool>.Operation.OP_SET:
                    OnLaunchableChange?.Invoke();
                    break;
                case SyncList<bool>.Operation.OP_CLEAR:
                    break;
            }
        }
        #endregion

        #region Mirror networking methods
        /// <summary>
        /// Instantiates callbacks when a client joins.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            // Force an update for everyone when a client joins, so that the client doesn't have to wait until next poll cycle
            shipGameBrainUpdater.PollForShipData();
            
            // Add methods to SyncLists
            thrusters.Callback += OnThrusterValueChange;
            unlockedLocations.Callback += UnlockedLocationsOnCallback;

            // Call different SyncVarHooks
            OnSetLocationHook(!locationSet, locationSet);
            OnCubeStateChangeHook(CubeState.InPlayerHands, _cubeState);

            // The HUD scene loads before the scene where this object lives, so force an update
            OnMissionDataChange?.Invoke(MissionData);
            OnCurrentLocationChange?.Invoke(currentLocation);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Sends a request to have the ship jump to a different location.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdJump()
        {
            string id = unlockedLocations[currentSetLocationIndex].locationID;
            shipGameBrainUpdater.SendJumpRequest(id);
        }

        /// <summary>
        /// Ejects a cube from the nav reader and gives it to the player.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdEjectCube(Player player)
        {
            // Only set the player holding the cube if the cube is in the Nav Reader
            if (_cubeState == CubeState.InNavReader)
            {
                _playerHoldingCube = player.netIdentity;
            }
            SetCubeState(CubeState.InPlayerHands);
        }

        /// <summary>
        /// Removes the player associated with the cube when it is inserted into the Cube Drive workstation.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdInsertCube()
        {
            _playerHoldingCube = null;
            SetCubeState(CubeState.InCubeDrive);
        }

        /// <summary>
        /// Sets whether a thruster has been flipped on or off.
        /// </summary>
        /// <param name="thrusterIndex">The index of the thruster that has been flipped on or off.</param>
        /// <param name="thrusterOn">Whether the thruster has been flipped.</param>
        [Command(requiresAuthority = false)]
        public void CmdSetThruster(int thrusterIndex, bool thrusterOn)
        {
            thrusters[thrusterIndex] = thrusterOn;
        }

        /// <summary>
        /// Flips all thrusters on the Flight Engineer on or off.
        /// </summary>
        /// <param name="thrusterOn">Whether to flip all thrusters on or off.</param>
        [Command(requiresAuthority = false)]
        public void CmdSetAllThrusters(bool thrusterOn)
        {
            for (int i = 0; i < thrusters.Count; i++)
            {
                thrusters[i] = thrusterOn;
            }
        }

        /// <summary>
        /// Calls the server to reset the state of the ship.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdResetShip()
        {
            ResetShip();
        }
        #endregion

        #region Rpc wrapper functions
        /// <summary>
        /// Sets the synchronized variable of whether the trajectories are locked, and calls an action across clients.
        /// </summary>
        /// <param name="trajectoriesAreCorrect">Whether the trajectories entered are correct.</param>
        public void TrajectoryLockChangeCallback(bool trajectoriesAreCorrect)
        {
            // Set whether the trajectories are locked based on whether they were correct
            trajectoriesLocked = trajectoriesAreCorrect;
            // Call a function across clients
            RpcTrajectoryLockChangeCallback(trajectoriesAreCorrect);
        }

        /// <summary>
        /// Sets the target location as the destination for a jump.
        /// </summary>
        /// <param name="locationIndex">The index of the location to set as the destination of a jump.</param>
        public void SetLocation(int locationIndex)
        {
            // Set the current set location index and get the location using it
            FlightEngineer flightEngineer = (FlightEngineer)_workstationManager.GetWorkstation(WorkstationID.FlightEngineer);

            if (locationIndex == -1)
            {
                locationSet = false;
                currentSetLocationIndex = 0;

                // Make the targets of each dial negative
                flightEngineer.SetAllDialTargets(-1, -1, -1);

                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.Log("Location index of -1 received; probably called via a reset.");
                }
            }
            else if (locationIndex < 0 || locationIndex >= unlockedLocations.Count)
            {
                // Return if the index is not within the bounds of the location list
                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.Log("Location index is not within the bounds of the unlocked locations.");
                }
            }
            else
            {
                // Set a variable that the location is set
                locationSet = true;
                currentSetLocationIndex = locationIndex;

                // Set the targets of each dial
                Location loc = unlockedLocations[locationIndex];
                flightEngineer.SetAllDialTargets(loc.trajectoryLaunch, loc.trajectoryCorrection, loc.trajectoryCube);

                if (networkManager.isInDebugMode)
                {
                    Debug.Log($"Location index of {locationIndex} received");
                }
            }
        }
        #endregion

        #region Rpc functions
        /// <summary>
        /// Calls an action across all clients when a location is unlocked.
        /// </summary>
        /// <param name="response">The response containing whether the attempt to unlock was successful or not.</param>
        [ClientRpc]
        private void RpcOnLocationUnlockResponse(LocationUnlockResponse response)
        {
            OnTryLocationUnlockResponse?.Invoke(response);
        }

        /// <summary>
        /// Calls an action across all clients when the mission data changes.
        /// </summary>
        /// <param name="md">The list of missions.</param>
        [ClientRpc]
        private void RpcOnMissionDataChange(List<MissionData> md)
        {
            OnMissionDataChange?.Invoke(md);
        }

        /// <summary>
        /// Calls an action across all clients when the state of whether the trajectories are locked or not changes.
        /// </summary>
        /// <param name="trajectoriesAreCorrect">Whether the trajectories initially provided are correct.</param>
        [ClientRpc]
        private void RpcTrajectoryLockChangeCallback(bool trajectoriesAreCorrect)
        {
            OnTrajectoryLockUpdate?.Invoke(trajectoriesAreCorrect);
        }

        /// <summary>
        /// Calls an action across all clients when the ship jumps.
        /// </summary>
        [ClientRpc]
        private void RpcLaunch()
        {
            OnLaunch?.Invoke();
        }

        /// <summary>
        /// Resets launch workstations on the ship across all clients.
        /// </summary>
        [ClientRpc]
        public void RpcResetLaunchWorkstations()
        {
            // Loop through all launch workstations and call to reset them
            foreach (var workstation in _workstationManager.GetLaunchWorkstations())
            {
                if (workstation != null)
                {
                    workstation.ResetWorkstation();
                }
            }
        }

        /// <summary>
        /// Disables the local display of the cube sprite for the given player (if they have it).
        /// </summary>
        /// <param name="player">The player whose local display of the cube sprite should be disabled.</param>
        [ClientRpc]
        private void RpcDisableCubeSprite(Player player)
        {
            if (player != null && player.isLocalPlayer)
            {
                UI.HUD.HUDController.Instance.SetCubeSprite(false);
            }
        }
        #endregion

        #region Merge methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        [Server]
        private void MergeLocationList(GameData data)
        {
            // Loop through all received locations
            int i = 0;
            for (; i < data.locations.Length; i++)
            {
                // If there's more locations in the data than what's unlocked, add the new location
                if (unlockedLocations.Count == i)
                {
                    unlockedLocations.Add(data.locations[i]);
                }
                // Otherwise, if the unlocked locations currently in-game differs from what's in the data, just update the data
                else if (!unlockedLocations[i].IsEquivalent(data.locations[i]))
                {
                    unlockedLocations[i] = data.locations[i];
                }
            }
            // If there are more locations unlocked than there are locations in the data, remove the extra locations
            for (; i < unlockedLocations.Count; i++)
            {
                unlockedLocations.RemoveAt(i);
            }
        }

        /// <summary>
        /// Merges the list of existing missions
        /// </summary>
        /// <param name="data">The entire chunk of ship data, used to get the missions received.</param>
        [Server]
        private void MergeMissionDataList(GameData data)
        {
            // Cut the size of the current mission list down if it is greater than the size of the new mission list
            if (MissionData.Count > data.missions.Length)
            {
                MissionData.RemoveRange(data.missions.Length, MissionData.Count - data.missions.Length);
            }

            // Loop through all missions in the received data and update the existing list to match it
            for (int i = 0; i < data.missions.Length; i++)
            {
                // Update an existing mission if still within the existing mission data list
                if (i < MissionData.Count)
                {
                    if (!MissionData[i].IsEquivalentTo(data.missions[i]))
                    {
                        MissionData[i] = data.missions[i];
                    }
                }
                // Otherwise, add the new mission to the existing mission list
                else
                {
                    MissionData.Add(data.missions[i]);
                }
            }

            // Call the RPC function after the mission data is set
            RpcOnMissionDataChange(MissionData);
        }
        #endregion

        #region Cube functions
        /// <summary>
        /// Sets the cube state. This should only be called on the server, since it modifies a SyncVar.
        /// </summary>
        /// <param name="cubeState">The state to set the cube as.</param>
        [Server]
        public void SetCubeState(CubeState cubeState)
        {
            _cubeState = cubeState;

            // If the cube will not be held by a player, disable the cube sprite and remove the reference to the player holding a cube (if there is a player)
            if (cubeState != CubeState.InPlayerHands && _playerHoldingCube != null)
            {
                // Disable the sprite of the cube on the screen of the player who had it before
                RpcDisableCubeSprite(_playerHoldingCube.GetComponent<Player>());
                // Remove the NetworkIdentity associated with the cube
                _playerHoldingCube = null;
            }
        }

        /// <summary>
        /// Wrapper function to take the cube from the Nav Reader workstation.
        /// </summary>
        /// <param name="player"></param>
        public void PickUpCube(Player player)
        {
            UI.HUD.HUDController.Instance.SetCubeSprite(true);
            CmdEjectCube(player);
        }

        /// <summary>
        /// Wrapper function to place the cube within the Cube Drive workstation. 
        /// </summary>
        public void InsertCube()
        {
            CmdInsertCube();
        }

        /// <summary>
        /// Checks if a given player is holding the cube.
        /// </summary>
        /// <param name="player">The player to check if they're holding the cube.</param>
        /// <returns>Whether the given player is holding the cube.</returns>
        public bool PlayerIsHoldingCube(Player player)
        {
            return player.netIdentity == _playerHoldingCube;
        }
        #endregion

        #region Ship reset functions
        /// <summary>
        /// Resets the state of the ship.
        /// </summary>
        public void ResetShip()
        {
            // Reset cube state and and player holding cube
            SetCubeState(CubeState.NotAvailable);

            // Reset thrusters
            SetAllThrusters(false);

            // Reset location
            SetLocation(-1);

            // Reset ship workstations
            RpcResetLaunchWorkstations();
        }

        /// <summary>
        /// Short coroutine that delays a one-time ship reset.
        /// </summary>
        /// <param name="delay">The time to wait before resetting the ship on the server.</param>
        /// <returns>A yield return while waiting to reset the ship.</returns>
        private IEnumerator DelayShipReset(float delay)
        {
            yield return new WaitForSeconds(delay);
            CmdResetShip();
        }
        #endregion

        #region Getter methods
        /// <summary>
        /// Gets the current location of the ship.
        /// </summary>
        /// <returns>The current location.</returns>
        public Location GetCurrentLocation()
        {
            if (currentLocation != null)
            {
                return currentLocation;
            }
            else
            {
                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.LogWarning("currentLocation not (yet) set. This is fine if we haven't pulled data yet.");
                }
                return new Location();
            }
        }

        /// <summary>
        /// Gets the index of the provided location within the list of unlocked locations.
        /// </summary>
        /// <param name="location">The location object to find within the list.</param>
        /// <returns>An integer representing the index of the location within the list.</returns>
        public int GetLocationIndex(Location location)
        {
            if (unlockedLocations.Contains(location))
            {
                return unlockedLocations.IndexOf(location);
            }
            else
            {
                return unlockedLocations.Count - 1;
            }
        }

        /// <summary>
        /// Checks whether all thrusters have been flipped on.
        /// </summary>
        /// <returns></returns>
        public bool GetAllThrustersOn()
        {
            return thrusters.All(x => x);
        }

        /// <summary>
        /// Gets whether a thruster at the given index has been flipped on.
        /// </summary>
        /// <param name="thrusterIndex">The index of the thruster object whose state needs to be retrieved.</param>
        /// <returns>The thruster object at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Exception thrown if thrusterIndex is less than 0 or greater than the number of thrusters available.</exception>
        public bool GetThrusterOn(int thrusterIndex)
        {
            if (thrusters.Count > thrusterIndex && thrusterIndex >= 0)
            {
                return thrusters[thrusterIndex];
            }
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Returns the location with the provided location ID.
        /// </summary>
        /// <param name="locationID">The identifier of the location to retrieve.</param>
        /// <returns>The location with the provided ID.</returns>
        public Location GetLocation(string locationID)
        {
            // As long as some locations remain unlocked, we can try to retrieve the location
            if (unlockedLocations.Count > 0)
            {
                var loc = unlockedLocations.First(x => x.locationID == locationID);
                return loc;
            }
            // Otherwise return null
            else
            {
                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.LogError("No unlocked locations exist.");
                }
                return null;
            }
        }
        #endregion

        #region Setter methods
        /// <summary>
        /// Sets the team ID to one received in a response.
        /// </summary>
        /// <param name="response">The team ID wrapped in a JSON layer.</param>
        public void SetTeamID(TeamID response)
        {
            teamID = response.teamID;
        }

        /// <summary>
        /// Flips all thrusters on or off according to the given boolean (true is on, false is off).
        /// </summary>
        /// <param name="thrusterOn">Whether the thrusters should be flipped on or off.</param>
        public void SetAllThrusters(bool thrusterOn)
        {
            // Loop through all thrusters and set their state
            for (int i = 0; i < thrusters.Count; i++)
            {
                thrusters[i] = thrusterOn;
            }
        }
        #endregion
    }
}

