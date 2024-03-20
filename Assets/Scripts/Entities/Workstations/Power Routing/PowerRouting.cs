/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using Managers;

namespace Entities.Workstations.PowerRouting
{
    public class PowerRouting : Workstation
    {
        #region Variables
        [SyncVar(hook = nameof(OnChangePoweredStations))]
        private int poweredStations = 0;
        [SyncVar]
        private PoweredState curPowerMode = PoweredState.Standby;

        public int TotalPower => totalPower;
        [SerializeField]
        private int totalPower = 5;
        [SerializeField]
        private List<WorkstationPipe> outerPipes;
        [SerializeField]
        private float fadeOpacity = 0.0627451f;

        private readonly SyncDictionary<WorkstationID, bool> systemIDPowerStates = new SyncDictionary<WorkstationID, bool>();

        public WorkstationLight[] powerLights;
        public WorkstationLight launchModeLight;
        public WorkstationLight explorationModeLight;
        public PowerRoutingButton[] workstationButtons;

        public Dictionary<WorkstationID, PowerRoutingButton> workstationButtonDict;

        private CustomNetworkManager networkManager;

        private NetworkIdentity networkIdentity;
        #endregion

        #region Unity event functions
        protected override void Awake()
        {
            if (!AlwaysHasPower)
            {
                Debug.LogError("Power station must have power. Set AlwaysHasPower to true for powerRouting workstation", this);
            }

            base.Awake();
            workstationButtonDict = new Dictionary<WorkstationID, PowerRoutingButton>();
            foreach (PowerRoutingButton tri in workstationButtons)
            {
                if (!workstationButtonDict.ContainsKey(tri.WorkstationID))
                {
                    workstationButtonDict.Add(tri.WorkstationID, tri);
                }
                else
                {
                    Debug.LogWarning("Workstation Power Button Dictionary already contains key for " + tri.WorkstationID);
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
            networkIdentity = GetComponent<NetworkIdentity>();
        }
        #endregion

        #region Mirror methods
        public override void OnStartClient()
        {
            systemIDPowerStates.Callback += OnSystemPowerStateChange;

            foreach (KeyValuePair<WorkstationID, bool> initial in systemIDPowerStates)
            {
                OnSystemPowerStateChange(SyncDictionary<WorkstationID, bool>.Operation.OP_ADD, initial.Key, initial.Value);
            }
            ChangePoweredLightStrip(GetPowerRemaining());

            OnChangePoweredStations(GetPowerRemaining(), GetPowerRemaining());

            bool launch = GetAllPoweredForLaunch();
            bool exploration = GetAllPoweredForExploration();

            launchModeLight.Lit = launch;
            explorationModeLight.Lit = exploration;

            if (launch || exploration) outerPipes.ForEach(p => p.SetEmissionPower(1f));
            else outerPipes.ForEach(p => p.SetEmissionPower(0f));

            if (exploration)
                curPowerMode = PoweredState.ExplorationMode;
            else if (launch)
                curPowerMode = PoweredState.LaunchMode;
            else
                curPowerMode = PoweredState.Standby;

            base.OnStartClient();
        }

        public override void OnStartServer()
        {
            foreach (Workstation station in _workstationManager.GetWorkstations())
            {
                // If a station always has power, we should not attempt to track its power state
                if (!station.AlwaysHasPower)
                {
                    systemIDPowerStates.Add(station.StationID, false);
                }
            }

            // Change the light strip based on the powered workstations
            ChangePoweredLightStrip(poweredStations);
        }
        #endregion

        #region SyncVar hooks
        // Callback function that occurs when the power remaining amount is changed. Updates power remaining indicators
        private void OnChangePoweredStations(int oldPower, int newPower)
        {
            if (newPower < 0 || newPower > totalPower)
            {
                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.LogError("Power managed to get out of range!");
                }
            }
            ChangePoweredLightStrip(GetPowerRemaining());
        }
        #endregion

        #region Callback methods
        /// <summary>
        /// Callback function that occurs when a change is made to the dictionary holding the system power states.
        /// </summary>
        /// <param name="op">The operation - add, set, remove, or clear.</param>
        /// <param name="key">The GameObject stored as a key within the SyncDictionary.</param>
        /// <param name="value">The power state of the system - true if on, false if off.</param>
        void OnSystemPowerStateChange(SyncDictionary<WorkstationID, bool>.Operation op, WorkstationID key, bool value)
        {
            switch (op)
            {
                case SyncDictionary<WorkstationID, bool>.Operation.OP_ADD:
                    break;
                // Setting a key's value changes the power
                case SyncDictionary<WorkstationID, bool>.Operation.OP_SET:
                    _workstationManager.GetWorkstation(key).ChangePower(value);

                    // Set lights
                    bool launch = GetAllPoweredForLaunch();
                    bool exploration = GetAllPoweredForExploration();

                    launchModeLight.Lit = launch;
                    explorationModeLight.Lit = exploration;

                    if (launch || exploration) outerPipes.ForEach(p => p.SetEmissionPower(1f));
                    else outerPipes.ForEach(p => p.SetEmissionPower(0f));
                    break;
                case SyncDictionary<WorkstationID, bool>.Operation.OP_REMOVE:
                    break;
                case SyncDictionary<WorkstationID, bool>.Operation.OP_CLEAR:
                    break;
            }
        }
        #endregion

        #region Workstation methods
        protected override void Enter()
        {
            base.Enter();
            foreach (PowerRoutingButton tri in workstationButtons)
            {
                TogglePowerStateRoutingUI(tri.WorkstationID, GetPowerStateForWorkstation(tri.WorkstationID));
            }

            // Make the text transparent so that the "Available Power" sign is visible
            if (UICurrentShipLocationText.Instance)
            {
                UICurrentShipLocationText.Instance.AdjustOpacity(fadeOpacity);
            }
        }

        protected override void Exit()
        {
            base.Exit();
            foreach (PowerRoutingButton tri in workstationButtons)
            {
                Audio.AudioPlayer.Instance.PowerRoutingTubeOff(tri.WorkstationID);
            }

            // Make the text opaque again
            if (UICurrentShipLocationText.Instance)
            {
                UICurrentShipLocationText.Instance.AdjustOpacity(1);
            }
        }

        // Turns off the power to all workstations.
        public override void ResetWorkstation()
        {
            // Loop using button dict because it avoids touching unmodifiable power states like VMs
            foreach (KeyValuePair<WorkstationID, PowerRoutingButton> pair in workstationButtonDict)
            {
                if (systemIDPowerStates[pair.Value.WorkstationID])
                {
                    TogglePowerState(pair.Key);
                }
            }

            base.ResetWorkstation();
        }
        #endregion

        #region Power helper methods
        /// <summary>
        /// Checks if there is power available ot allocate.
        /// </summary>
        /// <returns>Whether there is remaining power available.</returns>
        public bool PowerIsAvailable()
        {
            return GetPowerRemaining() > 0;
        }

        /// <summary>
        /// Gets the total amount of power remaining.
        /// </summary>
        /// <returns>The amount of power that remains which can be allocated to different stations.</returns>
        public int GetPowerRemaining()
        {
            return totalPower - poweredStations;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Try to change the power state stored on the server if possible.
        /// </summary>
        /// <param name="workstationID">The workstation whose power state should chang.</param>
        /// <param name="state">Resulting powered state of this operation.</param>
        [Command(requiresAuthority = false)]
        public void CmdTryChangeSystemPowerState(NetworkIdentity client, WorkstationID workstationID, bool state)
        {
            // Trying to turn something on while there's no power
            if ((!PowerIsAvailable() && state))
            {
                Debug.LogWarning("Tried to power a workstation on while there was no power remaining! Reverting client's power state");
                TargetClientRevertLocalWorkstationPowerState(client.connectionToClient, workstationID, state);
                return;
            }

            // Update the workstation to be powered
            systemIDPowerStates[workstationID] = state;
            // Get the number of powered workstations
            poweredStations = systemIDPowerStates.Count(x => x.Value);

            // Get the appropriate power mode based on which workstations are powered
            bool launch = GetAllPoweredForLaunch();
            bool exploration = GetAllPoweredForExploration();
            if (launch && exploration)
            {
                Debug.LogError("We should not be able to be in both launch and exploration mode!");
                return;
            }

            // Set a power mode to send; if we have no power mode, set it as standby
            PoweredState poweredState = PoweredState.Standby;
            if (launch)
            {
                poweredState = PoweredState.LaunchMode;
            }
            else if (exploration)
            {
                poweredState = PoweredState.ExplorationMode;
            }
            
            if (poweredState != curPowerMode)
            {
                // Ask Gamebrain to update the power state
                curPowerMode = poweredState;
                ShipStateManager.Instance.ShipGameBrainUpdater.TrySetPowerMode(poweredState);
            }
        }

        /// <summary>
        /// Forcefully sets power to given mode all at once on the server. Standby turns everything off
        /// Handling this all in one call prevents problems with checking for power in the middle of a batch operation
        /// </summary>
        /// <param name="workstationID">The workstation whose power state should chang.</param>
        /// <param name="state">Resulting powered state of this operation.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetSystemPowerStateToMode(NetworkIdentity client, PoweredState targetState)
        {
            // Set all power states
            foreach (Workstation w in _workstationManager.GetWorkstations())
            {
                // Ignore all stations that are always powered
                if (w.AlwaysHasPower) continue;

                // For standby mode, turn everything off
                if (targetState == PoweredState.Standby)
                {
                    systemIDPowerStates[w.StationID] = false;
                }
                else if (targetState == PoweredState.ExplorationMode)
                {
                    systemIDPowerStates[w.StationID] = w.UsedInExplorationMode;
                }
                else
                {
                    systemIDPowerStates[w.StationID] = w.UsedInLaunchMode;
                }
            }

            // Get the number of powered workstations (hook on this var updates lights and things)
            poweredStations = systemIDPowerStates.Count(x => x.Value);

            if (targetState != curPowerMode)
            {
                curPowerMode = targetState;
                ShipStateManager.Instance.ShipGameBrainUpdater.TrySetPowerMode(targetState);
            }
        }
        #endregion

        /// <summary>
        /// Reverts clients local UI and any other immediate state changes from changing power
        /// </summary>
        [TargetRpc]
        public void TargetClientRevertLocalWorkstationPowerState(NetworkConnection target, WorkstationID workstationID, bool stateFailedToChangeTo)
        {
            TogglePowerStateRoutingUI(workstationID, !stateFailedToChangeTo);
        }

        #region Power status methods
        public void ChangePoweredLightStrip(int litLightCount)
        {
            for (int i = 0; i < powerLights.Length; i++)
            {
                powerLights[i].Lit = i < litLightCount;
            }

            if (litLightCount > powerLights.Length)
            {
                Debug.LogWarning("More power available than lights to show. Add more lights?", this);
            }
        }

        /// <summary>
        /// Sends a request to the server to change the power state of a workstation and changes the UI.
        /// If the power state cannot be changed, an audio sound is played.
        /// </summary>
        /// <param name="workstationID">The ID of the workstation to switch the power state of.</param>
        public void TogglePowerState(WorkstationID workstationID)
        {
            bool workstationPower = GetPowerStateForWorkstation(workstationID);

            // There is available power to "spend" and this system is currently unpowered || This system is currently powered
            //   This check happens locally, if succesful a command is sent to the server to actually change the power state
            //   On the server this check happens again against the server's state, if that check is succesful, it changes the power state.
            //   Without the check on the server, the following bug occurs:
            //   Player hits launch mode button -> check passes command sent to server -> player hits exploration mode button immediatly after ->
            //   -> check passes again because first command hasn't reached server so actual state hasn't changed -> second command get's sent to server ->
            //   -> server recieves first command and powers up launch stations -> server recieves second command and powers up exploration stations -> every station is on :(
            if ((PowerIsAvailable() && !workstationPower) || workstationPower)
            {
                // Switch the state to be powered or unpowered based on its previous state
                CmdTryChangeSystemPowerState(netIdentity, workstationID, !workstationPower);
                // Toggle the button UI locally, immediatly. Get's reverted by TargetRPC function if necessary 
                TogglePowerStateRoutingUI(workstationID, !workstationPower);
            }
            else
            {
                Debug.Log("Failed to toggle power for " + workstationID);
                workstationButtonDict[workstationID].OnPowerFail();
            }
        }

        // TODO: Rewrite PowerRouting with better handling of client/server communication and caching
        // This a bit of a hack to make the exploration/launch buttons work by skipping checks to make sure power is avaible, which is fine since we know the end state of this call is allowable
        // Calling this with targetState == Standby will turn all stations off
        public void SetPowerStateToMode(PoweredState targetState)
        {
            // This updates the local UI for everything
            foreach (Workstation w in _workstationManager.GetWorkstations())
            {
                // Ignore all stations that are always powered
                if (w.AlwaysHasPower) continue;

                // For standby mode, turn everything off
                if (targetState == PoweredState.Standby)
                {
                    // Set the button UI locally, immediatly. Actually power state updated later
                    TogglePowerStateRoutingUI(w.StationID, false);
                }
                else if (targetState == PoweredState.ExplorationMode)
                {
                    TogglePowerStateRoutingUI(w.StationID, w.UsedInExplorationMode);
                }
                else
                {
                    TogglePowerStateRoutingUI(w.StationID, w.UsedInLaunchMode);
                }
            }

            // Actually updates power on the server
            CmdSetSystemPowerStateToMode(netIdentity, targetState);
        }

        /// <summary>
        /// Switches the UI of a button associated with a workstation depending on its new power state.
        /// </summary>
        /// <param name="stationToToggleID">The ID of the workstation to toggle.</param>
        /// <param name="isPowered">Whether the workstation is powered.</param>
        public void TogglePowerStateRoutingUI(WorkstationID stationToToggleID, bool isPowered)
        {
            var stationToToggle = _workstationManager.GetWorkstation(stationToToggleID);
            workstationButtonDict[stationToToggleID].TogglePower(stationToToggle, isPowered);
        }
        #endregion

        #region Power check methods
        /// <summary>
        /// Checks if all launch workstations are powered.
        /// </summary>
        /// <returns>Whether all launch workstations are powered.</returns>
        public bool GetAllPoweredForLaunch()
        {
            if (!_workstationManager.HasWorkstations)
            {
                return false;
            }

            return systemIDPowerStates.Where(x=> _workstationManager.GetWorkstation(x.Key).UsedInLaunchMode).All(x => x.Value);
        }

        /// <summary>
        /// Checks if all exploration workstations are powered.
        /// </summary>
        /// <returns>Whether all exploration workstations are powered.</returns>
        public bool GetAllPoweredForExploration()
        {
            if (!_workstationManager.HasWorkstations)
            {
                return false;
            }

            return systemIDPowerStates.Where(x=> _workstationManager.GetWorkstation(x.Key).UsedInExplorationMode).All(x => x.Value);
        }

        /// <summary>
        /// Checks if the workstation with the provided ID is powered.
        /// </summary>
        /// <param name="workstationID">The ID of the workstation whose power state should be checked.</param>
        /// <returns></returns>
        public bool GetPowerStateForWorkstation(WorkstationID workstationID)
        {
            if (systemIDPowerStates.TryGetValue(workstationID, out var powerStatus))
            {
                return powerStatus;
            }
            else
            {
                // In editor, running as Client+Server, workstations register themselves in this dictionary in Awake and OnStartServer get's called in Awake, meaning that
                // when they go to check their power state in OnStartServer, they may not have been registered yet, resulting in this case, which is not a problem since it should return false anyways.
                // (See Workstation.cs line 193)
                // Shouldn't be a problem in real build since OnStartServer will be called after Awake and if they're called at the same time every station that calls this starts powered off anyways
                // Debug.LogError($"Can't get power state for {workstationID}. Ensure that it exists in the dictionary.");
                return false;
            }
        }
        #endregion
    }
}

