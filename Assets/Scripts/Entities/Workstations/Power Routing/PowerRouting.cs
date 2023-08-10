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
using Systems.GameBrain;
using System.Collections;

namespace Entities.Workstations.PowerRouting
{
    /// <summary>
    /// Defines the PowerRouting workstation, used to turn workstations on or off.
    /// </summary>
    public class PowerRouting : Workstation
    {
        #region Variables
        /// <summary>
        /// The power that clients can allocate to systems. When the power value is changed by one client, a callback function is called across all clients.
        /// </summary>
        [SyncVar(hook = nameof(OnChangePoweredStations))]
        private int poweredStations = 0;

        /// <summary>
        /// The total power remaining that can be allocated. Derives from a private variable.
        /// </summary>
        public int TotalPower => totalPower;
        /// <summary>
        /// The total power remaining that can be allocated.
        /// </summary>
        [SerializeField]
        private int totalPower = 5;
        /// <summary>
        /// The pipes on the outermost of the PowerRouting workstation.
        /// </summary>
        [SerializeField]
        private List<WorkstationPipe> outerPipes;
        /// <summary>
        /// The opacity to fade the Available Power text to.
        /// </summary>
        [SerializeField]
        private float fadeOpacity = 0.0627451f;

        /// <summary>
        /// The power states of all systems in the ship, synchronized across clients.
        /// </summary>
        private readonly SyncDictionary<WorkstationID, bool> systemIDPowerStates = new SyncDictionary<WorkstationID, bool>();

        /// <summary>
        /// The lights used to show the total amount of remaining power.
        /// </summary>
        public WorkstationLight[] powerLights;
        /// <summary>
        /// The light used to show if the PowerRouting is in Launch Mode.
        /// </summary>
        public WorkstationLight launchModeLight;
        /// <summary>
        /// The light used to show if the PowerRouting is in the Exploration mode.
        /// </summary>
        public WorkstationLight explorationModeLight;
        /// <summary>
        /// The list of buttons on the PowerRouting workstation.
        /// </summary>
        public PowerRoutingButton[] workstationButtons;

        /// <summary>
        /// A dictionary mapping WorkstationIDs to buttons on the PowerRouting workstation.
        /// </summary>
        public Dictionary<WorkstationID, PowerRoutingButton> workstationButtonDict;
        /// <summary>
        /// The text showing the name of the location.
        /// </summary>
        private UICurrentShipLocationText locationText;

        /// <summary>
        /// The CustomNetworkManager component of the main NetworkManager singleton.
        /// </summary>
        private CustomNetworkManager networkManager;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that adds workstation IDs and buttons to a dictionary.
        /// </summary>
        protected override void Awake()
        {
            if (!AlwaysHasPower)
            {
                Debug.Log("Power station must have power. Set AlwaysHasPower to true for powerRouting workstation", this);
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
                    // Debug.LogWarning("Workstation Power Button Dictionary already contains key for " + tri.WorkstationID);
                }
            }
        }

        /// <summary>
        /// Unity event function that gets a reference to the location text.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
            StartCoroutine(FindLocationLabel());
        }

        private IEnumerator FindLocationLabel()
        {
            while (locationText == null)
            {
                try
                {
                    locationText = GameObject.Find("Text_LocationName").GetComponent<UICurrentShipLocationText>();
                }
                catch
                {
                    // Location text not found, keep looping
                }
                yield return null;
            }
        }
        #endregion

        #region Mirror methods
        /// <summary>
        /// Subscribes to the OnSystemPowerStateChange callback and calls mehtods to change lighting.
        /// </summary>
        public override void OnStartClient()
        {
            systemIDPowerStates.Callback += OnSystemPowerStateChange;

            foreach (KeyValuePair<WorkstationID, bool> initial in systemIDPowerStates)
            {
                OnSystemPowerStateChange(SyncDictionary<WorkstationID, bool>.Operation.OP_ADD, initial.Key, initial.Value);
            }
            ChangePoweredLightStrip(GetPowerRemaining());

            OnChangePoweredStations(GetPowerRemaining(), GetPowerRemaining());
            base.OnStartClient();
        }

        /// <summary>
        /// Sets the light state of each light displaying the remaining amount of power.
        /// </summary>
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
        /// <summary>
        /// Callback function that occurs when the power remaining amount is changed. This should be used to do stuff like change UI.
        /// </summary>
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

            bool launch = GetAllPoweredForLaunch();
            bool exploration = GetAllPoweredForExploration();

            launchModeLight.Lit = launch;
            explorationModeLight.Lit = exploration;

            if (launch || exploration) outerPipes.ForEach(p => p.SetEmissionPower(1f));
            else outerPipes.ForEach(p => p.SetEmissionPower(0f));
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
                    break;
                case SyncDictionary<WorkstationID, bool>.Operation.OP_REMOVE:
                    break;
                case SyncDictionary<WorkstationID, bool>.Operation.OP_CLEAR:
                    break;
            }
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Toggles the UI (based on the power state) when entering the workstation.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();
            foreach (PowerRoutingButton tri in workstationButtons)
            {
                TogglePowerStateRoutingUI(tri.WorkstationID, GetPowerStateForWorkstation(tri.WorkstationID));
            }

            // Make the text transparent so that the "Available Power" sign is visible
            if (locationText)
            {
                locationText.AdjustOpacity(fadeOpacity);
            }
        }

        /// <summary>
        /// Turns off the tubes when exiting the workstation.
        /// </summary>
        protected override void Exit()
        {
            base.Exit();
            foreach (PowerRoutingButton tri in workstationButtons)
            {
                Audio.AudioPlayer.Instance.PowerRoutingTubeOff(tri.WorkstationID);
            }

            // Make the text opaque again
            if (locationText)
            {
                locationText.AdjustOpacity(1);
            }
        }

        /// <summary>
        /// Turns off the power to all workstations.
        /// </summary>
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
        /// Change the power state stored on the server.
        /// </summary>
        /// <param name="workstationID">The workstation whose power state has changed.</param>
        /// <param name="state">Whether the given workstation is powered.</param>
        [Command(requiresAuthority = false)]
        public void CmdChangeSystemPowerState(WorkstationID workstationID, bool state)
        {
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
            CurrentLocationGameplayData.PoweredState poweredState = CurrentLocationGameplayData.PoweredState.Standby;
            if (launch)
            {
                poweredState = CurrentLocationGameplayData.PoweredState.LaunchMode;
            }
            else if (exploration)
            {
                poweredState = CurrentLocationGameplayData.PoweredState.ExplorationMode;
            }

            // Ask Gamebrain to update the power state
            ShipStateManager.Instance.ShipGameBrainUpdater.TrySetPowerMode(poweredState);
        }
        #endregion

        #region Power status methods
        /// <summary>
        /// Turns the lights on the powered light strip on or off.
        /// </summary>
        /// <param name="litLightCount">The number of lights to turn on.</param>
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
            if ((PowerIsAvailable() && !workstationPower) || workstationPower)
            {
                // Switch the state to be powered or unpowered based on its previous state
                CmdChangeSystemPowerState(workstationID, !workstationPower);
                // Toggle the button UI
                TogglePowerStateRoutingUI(workstationID, !workstationPower);

                // Update power routing state locally, will get updated on server in the command
                // Prevents mangled states being created due to lag
                systemIDPowerStates[workstationID] = !workstationPower;
                poweredStations = systemIDPowerStates.Count(x => x.Value);
            }
            else
            {
                workstationButtonDict[workstationID].OnPowerFail();
            }
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
                if (networkManager && networkManager.isInDebugMode)
                {
                    Debug.LogError($"Can't get power state for {workstationID}. Ensure that it exists in the dictionary.");
                }
                return false;
            }
        }
        #endregion
    }
}

