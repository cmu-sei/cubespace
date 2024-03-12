/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using Cinemachine;
using Managers;
using Mirror;
using Systems.GameBrain;

namespace Entities.Workstations.SensorStationParts
{
    /// <summary>
    /// The SensorStation workstation, used to scan for a video at a workstation.
    /// </summary>
    [RequireComponent(typeof(SensorStationVideoSystem))]
    [RequireComponent(typeof(SensorStationScreenController))]
    public class SensorStation : Workstation
    {
        #region Variables
        /// <summary>
        /// Whether there is an incoming transmission event.
        /// </summary>
        [SyncVar(hook = nameof(OnIncomingTransmissionChangeHook))]
        private bool incomingTransmission = false;
        /// <summary>
        /// Whether first contact has been completed at this location.
        /// </summary>
        [SyncVar]
        private bool firstContactComplete;
        /// <summary>
        /// The incoming transmission event (if there is one).
        /// </summary>
        [SyncVar]
        private CommEvent incomingTransmissionEvent;
        /// <summary>
        /// Whether scanning is in progress.
        /// </summary>
        [SyncVar]
        private bool scanningInProgress = false;
        /// <summary>
        /// Whether the current location has been scanned. Derives from a private variable.
        /// </summary>
        public bool CurrentLocationScanned => currentLocationScanned;
        /// <summary>
        /// Whether the current location has been scanned.
        /// </summary>
        [SyncVar]
        private bool currentLocationScanned;
        /// <summary>
        /// Extra information about the current location. Derives from a private variable.
        /// </summary>
        public string CurrentLocationSurroundings => currentLocationSurroundings;
        /// <summary>
        /// Extra information about the current location.
        /// </summary>
        [SyncVar]
        private string currentLocationSurroundings;

        /// <summary>
        /// Whether the workstation is able to scan.
        /// </summary>
        [SyncVar]
        private bool ableToScan = true;
        /// <summary>
        /// The response of a scan.
        /// </summary>
        private ScanLocationResponse scanResponse = null;
        /// <summary>
        /// The terminal used for the SensorStation, used to enable/disable a transmission icon.
        /// </summary>
        private SensorStationTerminal _sensorStationTerminal = null;
        /// <summary>
        /// The total time it takes to scan.
        /// </summary>
        private const float SCANNING_TIME = 4f;
        /// <summary>
        /// The video system.
        /// </summary>
        private SensorStationVideoSystem _videoSystem;
        /// <summary>
        /// The component controlling switching screens at this station.
        /// </summary>
        private SensorStationScreenController _screenController;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that grabs the screen controller and video system components.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _screenController = GetComponent<SensorStationScreenController>();
            _videoSystem = GetComponent<SensorStationVideoSystem>();
        }

        /// <summary>
        /// Unity event function that subscribes to different actions on the ShipStateManager.
        /// </summary>
        private void OnEnable()
        {
            ShipGameBrainUpdater.OnShipDataReceived += OnShipDataReceived;
            ShipGameBrainUpdater.OnTryScanLocation += ScanResponse;
            ShipGameBrainUpdater.OnTryCompleteCommEventResponse += TryCompleteCommEventResponse;
        }

        /// <summary>
        /// Unity event function that unsubscribes from different actions on the ShipStateManager.
        /// </summary>
        private void OnDisable()
        {
            ShipGameBrainUpdater.OnShipDataReceived -= OnShipDataReceived;
            ShipGameBrainUpdater.OnTryScanLocation -= ScanResponse;
            ShipGameBrainUpdater.OnTryCompleteCommEventResponse -= TryCompleteCommEventResponse;
        }
        #endregion

        #region Event callback methods
        /// <summary>
        /// Sets transmission and location information of the SensorStation workstation when new GameData is received from Gamebrain.
        /// </summary>
        /// <param name="hasChanges">Whether this GameData object has changes.</param>
        /// <param name="data">The GameData object received from Gamebrain.</param>
        [Server]
        private void OnShipDataReceived(bool hasChanges, GameData data)
        {
            firstContactComplete = data.currentStatus.firstContactComplete;
            incomingTransmission = data.currentStatus.incomingTransmission;
            currentLocationScanned = data.currentStatus.currentLocationScanned;
            currentLocationSurroundings = data.currentStatus.currentLocationSurroundings;

            if (data.currentStatus.incomingTransmissionObject == null)
            {
                incomingTransmissionEvent = null;
            }
            else
            {
                if (incomingTransmissionEvent == null || !incomingTransmissionEvent.IsEquivalentTo(data.currentStatus.incomingTransmissionObject))
                {
                    incomingTransmissionEvent = data.currentStatus.incomingTransmissionObject;

                    if (string.IsNullOrEmpty(incomingTransmissionEvent.videoURL)) 
                        Debug.LogError("Recieved incoming transmission event with a null or empty videoURL! ID is: " + incomingTransmissionEvent.commID);

                    RpcTryReadyVideo(data.currentStatus.incomingTransmissionObject.videoURL);
                    if (isClient) // For host + client
                    {
                        TryReadyVideo(data.currentStatus.incomingTransmissionObject.videoURL);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the scan response and pushes out an update to the scanning status to all clients.
        /// </summary>
        /// <param name="response">The response received when scanning a location.</param>
        [Server]
        public void ScanResponse(ScanLocationResponse response)
        {
            scanningInProgress = false;
            RpcSetScanningStatus(false, response);
            scanResponse = response;
        }

        /// <summary>
        /// Sets the screen transmission across all clients.
        /// </summary>
        /// <param name="response">The repsonse received from a complete comm event attempt.</param>
        private void TryCompleteCommEventResponse(GenericResponse response)
        {
            if (isServer)
            {
                RpcSetScreenTransmissionComplete();
            }
        }
        #endregion

        #region TryGet methods
        /// <summary>
        /// Tries to get the terminal attached to the SensorStation.
        /// </summary>
        /// <returns>Whether the sensor station terminal is not null.</returns>
        private bool TryGetSensorStationTerminal()
        {
            _sensorStationTerminal = _workstationManager.GetTerminal(WorkstationID.Sensor) as SensorStationTerminal;
            return _sensorStationTerminal != null;
        }

        /// <summary>
        /// Tries to set the transmission icon to that of the incoming transmission.
        /// </summary>
        private void TrySetTransmissionIcon()
        {
            if (_sensorStationTerminal == null)
            {
                if (TryGetSensorStationTerminal())
                {
                    _sensorStationTerminal.SetIncomingTransmissionIcon(incomingTransmission);
                }
            }
            else
            {
                _sensorStationTerminal.SetIncomingTransmissionIcon(incomingTransmission);
            }
        }

        /// <summary>
        /// Tries to preload the video at the sensor station if there is an incoming transmission
        /// </summary>
        private void TryReadyVideo(string url)
        {
            if (incomingTransmission && _videoSystem != null)
            {
                _videoSystem.ReadyVideo(url);
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Asks the server to set the workstation view when the player enters the SensorStation.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdSetWorkstationViewOnEnter()
        {
            SetWorkstationViewOnEnter();
        }

        /// <summary>
        /// Asks the server to try to scan the location.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdTryScan()
        {
            ShipStateManager.Instance.ShipGameBrainUpdater.TryScan();
        }

        /// <summary>
        /// Sets incoming transmission information on the server and prepares to echo that back to all clients.
        /// </summary>
        /// <param name="response">The response from scanning a location.</param>
        [Command(requiresAuthority = false)]
        private void CmdFinishScan(ScanLocationResponse response)
        {
            FinishScan(response);
        }
        
        /// <summary>
        /// Tries to complete a communication event on the server.
        /// </summary>
        [Command(requiresAuthority = false)]
        private void CmdCompleteCommEvent()
        {
            if (!incomingTransmission || incomingTransmissionEvent == null)
            {
                Debug.Log("can't complete null event");
                return;
            }

            ShipStateManager.Instance.ShipGameBrainUpdater.TryCompleteCommEvent();
        }
        #endregion

        #region Server methods
        /// <summary>
        /// Sets the workstation view on the server and updates the view and scan information across clients.
        /// </summary>
        [Server]
        public void SetWorkstationViewOnEnter()
        {
            if (scanningInProgress)
            {
                RpcSetScreenScanning();
                return;
            }

            if (incomingTransmission)
            {
                ableToScan = false;
                RpcSetScanFromCommEvent(incomingTransmission, incomingTransmissionEvent);
                return;
            }
            else
            {
                if (currentLocationScanned)
                {
                    ableToScan = true;
                    RpcSetScreenRefreshScan();
                    return;
                }
                else
                {
                    ableToScan = true;
                    RpcSetScreenReadyToScan();
                    return;
                }

            }
        }

        /// <summary>
        /// Sets incoming transmission information and sends information back to all clients.
        /// </summary>
        /// <param name="response">The response from scanning a location.</param>
        [Server]
        private void FinishScan(ScanLocationResponse response)
        {
            incomingTransmission = response.eventWaiting;
            incomingTransmissionEvent = response.incomingTransmission;
            ableToScan = true;

            if (response.eventWaiting)
            {
                RpcSetScanFromCommEvent(response.eventWaiting, response.incomingTransmission);
            }
            else
            {
                RpcSetScreenNoResults(response.incomingTransmission);
            }
        }
        #endregion

        #region Scanning methods
        /// <summary>
        /// Asks the server to scan the current location and starts the scanning UI routine.
        /// </summary>
        public void Scan()
        {
            if (!ableToScan)
            {
                Debug.Log("unable to scan");
                scanningInProgress = false;
                return;
            }

            if (scanningInProgress)
            {
                Debug.Log("already scanning.");
                return;
            }

            scanningInProgress = true;
            ableToScan = false;
            StartCoroutine(ScanningScreenCoroutine());
            CmdTryScan();
        }

        /// <summary>
        /// Sets the active scanning screen, waits to progress, and then asks the server to finish the scan.
        /// </summary>
        /// <returns>A yield return while the scan attempt is still in progress.</returns>
        private IEnumerator ScanningScreenCoroutine()
        {
            _screenController.SetScanningScreen();
            yield return new WaitForSeconds(SCANNING_TIME);

            // If we have a scan response, call the server to finish the scan, but if not, keep looping
            while (scanningInProgress)
            {
                yield return null;
            }

            if (scanResponse == null)
            {
                Debug.LogWarning("Scan response null!");
            }
            CmdFinishScan(scanResponse);
        }
        #endregion

        #region Client methods
        /// <summary>
        /// Plays the incoming transmission event at its video screen.
        /// </summary>
        [Client]
        public void PlayCurrentVideo()
        {
            _screenController.SetVideoScreen(incomingTransmissionEvent);
        }

        /// <summary>
        /// Asks the server to complete the comm event it has received when a video finishes playing.
        /// </summary>
        /// <param name="urlOfVideoFinished">The URL of the video finished.</param>
        [Client]
        public void OnVideoFinished(string urlOfVideoFinished)
        {
            if (incomingTransmissionEvent == null || urlOfVideoFinished != incomingTransmissionEvent.videoURL)
            {
                Debug.Log("Can't complete null event.");
                return;
            }

            CmdCompleteCommEvent();
        }

        /// <summary>
        /// Asks the server to complete the comm event it has received when translating the comm event fails.
        /// </summary>
        public void OnTranslationError()
        {
            if (incomingTransmissionEvent == null)
            {
                Debug.Log("Can't complete null event.");
                return;
            }

            CmdCompleteCommEvent();
        }
        #endregion

        #region RPC methods
        /// <summary>
        /// Tries to set the incoming transmission icon across all clients.
        /// </summary>
        [ClientRpc]
        private void RpcTrySetIncomingTransmissionIcon()
        {
            TrySetTransmissionIcon();
        }

        /// <summary>
        /// Tries to prepare a new transmission video across all clients.
        /// </summary>
        [ClientRpc]
        private void RpcTryReadyVideo(string url)
        {
            TryReadyVideo(url);
        }

        /// <summary>
        /// Calls the controller to complete the the transmission logic across all clients.
        /// </summary>
        [ClientRpc]
        private void RpcSetScreenTransmissionComplete()
        {
            _screenController.OnTransmissionComplete();
        }

        /// <summary>
        /// Sets the scanning screen across all clients.
        /// </summary>
        [ClientRpc]
        private void RpcSetScreenScanning()
        {
            _screenController.SetScanningScreen();
        }

        /// <summary>
        /// Sets the scan screen based on the comm event across all clients.
        /// </summary>
        /// <param name="hasEvent">Whether the comm event exists.</param>
        /// <param name="commEvent">The comm event received on the server.</param>
        [ClientRpc]
        private void RpcSetScanFromCommEvent(bool hasEvent, CommEvent commEvent)
        {
            if (hasEvent && commEvent == null)
            {
                Debug.Log("indicated we have comm event, but no event object.");
            }
            
            if (commEvent != null)
            {
                _screenController.SetScanFromCommEvent(commEvent);
            }
        }

        /// <summary>
        /// Refreshes the scan screen across all clients.
        /// </summary>
        [ClientRpc]
        private void RpcSetScreenRefreshScan()
        {
            _screenController.SetScanScreen(currentLocationScanned, currentLocationSurroundings);
        }

        /// <summary>
        /// Sets the scan screen scross all clients.
        /// </summary>
        [ClientRpc]
        private void RpcSetScreenReadyToScan()
        {
            _screenController.SetScanScreen(currentLocationScanned, currentLocationSurroundings);
        }
        
        /// <summary>
        /// Sets attributes about the progress of the scan across all clients.
        /// </summary>
        /// <param name="status">Whether scanning is in progress or not.</param>
        /// <param name="response">The response received from scanning a location.</param>
        [ClientRpc]
        private void RpcSetScanningStatus(bool status, ScanLocationResponse response)
        {
            scanningInProgress = status;
            scanResponse = response;
            ableToScan = !status;
        }

        /// <summary>
        /// Asks all clients to set the scan screen from the received comm event.
        /// </summary>
        /// <param name="commEvent">The comm event received on the server.</param>
        [ClientRpc]
        private void RpcSetScreenNoResults(CommEvent commEvent)
        {
            _screenController.SetScanFromCommEvent(commEvent);
        }
        #endregion

        #region SyncVar hooks
        /// <summary>
        /// Updates the transmission icon when incomingTransmission changes
        /// </summary>
        private void OnIncomingTransmissionChangeHook(bool oldVal, bool newVal)
        {
            TrySetTransmissionIcon();
        }
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the video system and screen controller components.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _videoSystem = GetComponent<SensorStationVideoSystem>();
            _screenController = GetComponent<SensorStationScreenController>();
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Gives the player authority over this workstation when they enter it and also sets the view.
        /// </summary>
        /// <param name="player">The lcal player activating this workstation.</param>
        /// <param name="currentCam">The current camera.</param>
        public override void Activate(Player player, CinemachineVirtualCamera currentCam)
        {
            base.Activate(player, currentCam);
            if (IsPowered) 
            {
                CmdSetWorkstationViewOnEnter();
            }
        }

        /// <summary>
        /// Sets the workstation view displayed when the power is changed.
        /// </summary>
        /// <param name="isPowered">Whether this workstation is powered.</param>
        public override void ChangePower(bool isPowered)
        {
            base.ChangePower(isPowered);
            if (isPowered)
            {
                CmdSetWorkstationViewOnEnter();
            }
            else
            {
                _videoSystem.InterruptVideo();
                _screenController.PowerOff();
            }
        }

        /// <summary>
        /// Stops the transmission alert sound when the workstation is exited.
        /// </summary>
        protected override void Exit()
        {
            base.Exit();
            _screenController.StopTransmissionAlertSFX();
        }
        #endregion
    }
}

