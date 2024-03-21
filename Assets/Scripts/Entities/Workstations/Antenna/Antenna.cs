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
using Managers;
using Mirror;
using Systems.GameBrain;
using UnityEngine;
using Entities.Workstations.AntennaParts;

namespace Entities.Workstations
{
    /// <summary>
    /// A class for the antenna workstation, used to connect to and from different networks.
    /// </summary>
    public class Antenna : Workstation
    {
        #region Variables
        /// <summary>
        /// The current state of the antenna - whether it is connected to a network, disconnected, 
        /// trying to connect, or trying to disconnect. Derives from a private variable.
        /// </summary>
        public AntennaState ConnectionState => connectionState;
        /// <summary>
        /// The current state of the antenna. This should only be set within Antenna.cs.
        /// </summary>
        [SyncVar(hook = nameof(SetVisualState))]
        private AntennaState connectionState = AntennaState.Disconnected;

        /// <summary>
        /// The animator used on the antenna.
        /// </summary>
        [Header("Antenna References")]
        [SerializeField]
        private Animator antennaAnimator;
        /// <summary>
        /// The animator used to move the connection arrow depending on the connection state.
        /// </summary>
        [SerializeField]
        private Animator connectionArrowAnimator;
        /// <summary>
        /// The controller directing which antenna screen to display at a given time.
        /// </summary>
        [SerializeField]
        private AntennaScreenController screenController;
        /// <summary>
        /// The strip of lights that light up once the antenna is ready to be extended.
        /// </summary>
        [SerializeField]
        private WorkstationLEDStrip LEDStrip;
        /// <summary>
        /// The pipes on the antenna that light up as an additional effect upon extension.
        /// </summary>
        [SerializeField]
        private WorkstationPipe[] pipes;

        /// <summary>
        /// The time to wait before aborting a connection attempt.
        /// </summary>
        [Header("Connection Variables")]
        [SerializeField]
        private float connectionTimeout = 30.0f;
        /// <summary>
        /// The time between a connection attempt's command to change the antenna's state to connecting and the loop waiting to set it to be connected.
        /// </summary>
        [SerializeField]
        private float connectionAttemptBufferTime = 5.0f;
        /// <summary>
        /// The time between a connection attempt's command to change the antenna's state to connected and visual updates.
        /// </summary>
        [SerializeField]
        private float connectionEstablishedBufferTime = 2.0f;

        /// <summary>
        /// The time to wait before aborting a disconnection attempt.
        /// </summary>
        [Header("Disconnection Variables")]
        [SerializeField]
        private float disconnectionTimeout = 30.0f;
        /// <summary>
        /// The time between a disconnection attempt's command to change the antenna's state to disconnecting and the loop waiting to set it to be disconnected.
        /// </summary>
        [SerializeField]
        private float disconnectionAttemptBufferTime = 3.0f;

        /// <summary>
        /// The current connection or disconnection attempt.
        /// </summary>
        private Coroutine connectionAttempt;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that subscribes to ship data and antenna actions.
        /// </summary>
        private void OnEnable()
        {
            ShipGameBrainUpdater.OnTryExtendAntennaResponse += ExtendAntennaResponse;
            ShipGameBrainUpdater.OnTryRetractAntennaResponse += RetractAntennaResponse;
        }

        /// <summary>
        /// Unity event function that unsubscribes from ship data and antenna actions.
        /// </summary>
        private void OnDisable()
        {
            ShipGameBrainUpdater.OnTryExtendAntennaResponse -= ExtendAntennaResponse;
            ShipGameBrainUpdater.OnTryRetractAntennaResponse -= RetractAntennaResponse;
        }
        #endregion

        #region Action functions
        /// <summary>
        /// Sets the antenna to be connected upon a successful extension, or prints an error message on a failed extension.
        /// </summary>
        /// <param name="response">The response received from Gamebrain.</param>
        public void ExtendAntennaResponse(GenericResponse response)
        {
            if (response.success)
            {
                connectionState = AntennaState.Connected;
            }
            else
            {
                Debug.LogWarning("Could not extend antenna.");
            }
        }

        /// <summary>
        /// Sets the antenna to be disconnected upon a successful retraction, or prints an error message on a failed retraction.
        /// </summary>
        /// <param name="response">The response received from Gamebrain.</param>
        public void RetractAntennaResponse(GenericResponse response)
        {
            if (response.success)
            {
                connectionState = AntennaState.Disconnected;
            }
            else
            {
                Debug.LogWarning("Could not retract antenna.");
            }
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Enables or disables visual artifacts when entering the antenna.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();

            // If the antenna is turned on already, show the player the antenna's visual artifacts
            if (IsPowered)
            {
                SetVisualState(connectionState);
            }
            // Otherwise, turn off the screen, pipes, and lights
            else
            {
                screenController.DisableCurrentScreen();
                SetPipes(false);
                LEDStrip.DeactivateAll();
                return;
            }
        }

        /// <summary>
        /// Disables visual artifacts on the antenna depending on the given power state and sets its state to be disconnected.
        /// </summary>
        /// <param name="isPowered">Whether this workstation is being powered on.</param>
        public override void ChangePower(bool isPowered)
        {
            base.ChangePower(isPowered);

            // If the antenna is being turned on, deactivate the lights
            if (isPowered)
            {
                LEDStrip.DeactivateAll();
            }
            // Otherewise, deactivate the pipes and turn off the screen
            else
            {
                SetPipes(false);
                screenController.DisableCurrentScreen();
            }

            // Set the antenna state to be disconnected
            if (NetworkClient.active)
            {
                CmdSetAntennaState(AntennaState.Disconnected);
            }
        }

        /// <summary>
        /// Sets the antenna state to be disconnected and pushes the player out of the workstation, if one is present.
        /// </summary>
        public override void ResetWorkstation()
        {
            base.ResetWorkstation();
            CmdSetAntennaState(AntennaState.Disconnected);
        }
        #endregion

        #region Connection/disconnection methods
        /// <summary>
        /// Initiates a new connection or disconnection attempt (by starting a new coroutine).
        /// </summary>
        /// <param name="newState">The new state of the antenna.</param>
        public void TrySetAntenna(AntennaState newState)
        {
            if (newState != connectionState)
            {
                // Attempt to connect
                if (newState == AntennaState.Connected || newState == AntennaState.Connecting)
                {
                    AttemptConnection(ConnectAnim());
                }
                // Attempt to disconnect
                else
                {
                    AttemptConnection(DisconnectAnim());
                }
            }
        }

        /// <summary>
        /// Stops an existing coroutine attempting to connect or disconnect and starts a new attempt.
        /// </summary>
        /// <param name="newConnectionAttempt">The new connection or disconnection attempt to start.</param>
        private void AttemptConnection(IEnumerator newConnectionAttempt)
        {
            if (connectionAttempt != null)
            {
                StopCoroutine(connectionAttempt);
            }
            connectionAttempt = StartCoroutine(newConnectionAttempt);
        }
        #endregion

        #region Connection/disconnection coroutines
        /// <summary>
        /// Changes the antenna state on the server, sends a connection request to Gamebrain, and updates the visual state to match.
        /// </summary>
        /// <returns>A yield while waiting for the antenna to try to connect.</returns>
        private IEnumerator ConnectAnim()
        {
            // Set the antenna state to be connecting and update its visuals accordingly
            float timeElapsed = 0.0f;
            CmdSetAntennaState(AntennaState.Connecting);
            yield return new WaitForSeconds(connectionAttemptBufferTime);

            // Wait to make sure the antenna state is set to be connecting on the server
            while (timeElapsed < connectionTimeout)
            {
                // When the antenna state is set to be connecting, make the antenna state be connected
                if (IsPowered && connectionState == AntennaState.Connecting)
                {
                    CmdSetAntennaState(AntennaState.Connected);
                    yield return new WaitForSeconds(connectionEstablishedBufferTime);
                    break;
                }
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Loop while the connection attempt is still valid
            timeElapsed = 0.0f;
            while (timeElapsed < connectionTimeout)
            {
                if (IsPowered && (connectionState == AntennaState.Connected))
                {
                    break;
                }
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Set the antenna's visual state upon connection
            SetVisualState(connectionState);
        }

        /// <summary>
        /// Changes the antenna state on the server, sends a disconnection request to Gamebrain, and updates the visual state to match.
        /// </summary>
        /// <returns>A yield while waiting for the antenna to try to disconnect.</returns>
        private IEnumerator DisconnectAnim()
        {
            // Set the antenna state to be disconnecting and update its visuals accordingly
            float timeElapsed = 0.0f;
            CmdSetAntennaState(AntennaState.Disconnecting);
            yield return new WaitForSeconds(disconnectionAttemptBufferTime);

            // Wait to make sure the antenna state is set to be disconnecting on the server
            while (timeElapsed < disconnectionTimeout)
            {
                // When the antenna state is set to be disconnecting, make the antenna state be disconnected
                if (IsPowered && (connectionState == AntennaState.Disconnecting))
                {
                    CmdSetAntennaState(AntennaState.Disconnected);
                    yield break;
                }
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command to set the state of the antenna.
        /// </summary>
        /// <param name="newState">The new state of the antenna.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetAntennaState(AntennaState newState)
        {
            // If trying to connect
            if (connectionState != newState && newState == AntennaState.Connected)
            {
                ShipStateManager.Instance.ShipGameBrainUpdater.TryExtendAntenna();
            }
            // If trying to disconnect
            else if (connectionState != newState && newState == AntennaState.Disconnected)
            {
                ShipStateManager.Instance.ShipGameBrainUpdater.TryRetractAntenna();
            }
            // If trying to set the state to be between connected or disconnected
            else
            {
                connectionState = newState;
            }
        }
        #endregion

        #region Visual methods
        /// <summary>
        /// Sets the visual state of the antenna. This acts as a wrapper function which can be used as a SyncVar hook.
        /// </summary>
        /// <param name="oldState">The previous state of the antenna.</param>
        /// <param name="newState">The current state of the antenna.</param>
        private void SetVisualState(AntennaState oldState, AntennaState newState)
        {
            SetVisualState(newState);
        }

        /// <summary>
        /// Sets the visual state of the antenna by toggling the screen displayed, changing animations, enabling pipe emissions, and turning on or off the lights.
        /// </summary>
        /// <param name="newState">The new state of the antenna (disconnected, disconnecting, connecting, or connected).</param>
        private void SetVisualState(AntennaState newState)
        {
            if (IsPowered)
            {
                // Set the correct screen
                screenController.ToggleScreen(newState);
                switch (newState)
                {
                    // If the antenna will be connected
                    case AntennaState.Connected:
                        SetAnimators(true);
                        SetPipes(true);
                        LEDStrip.ActivateAll();
                        break;
                    // If the antenna will be disconnected
                    case AntennaState.Disconnected:
                        SetAnimators(false);
                        SetPipes(false);
                        LEDStrip.ActivateAnimation();
                        break;
                    // If the antenna will be connecting
                    case AntennaState.Connecting:
                        SetAnimators(true);
                        SetPipes(true);
                        LEDStrip.ActivateAll();
                        break;
                    // If the antenna will be disconnecting
                    case AntennaState.Disconnecting:
                        SetAnimators(false);
                        SetPipes(false);
                        LEDStrip.DeactivateAll();
                        break;
                }
            }
        }

        /// <summary>
        /// Turns on or off the emission of the pipes depending on if the antenna is connected.
        /// </summary>
        /// <param name="isConnected">Whether the antenna is connected.</param>
        private void SetPipes(bool isConnected)
        {
            foreach (WorkstationPipe pipe in pipes)
            {
                if (isConnected)
                {
                    pipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
                }
                else
                {
                    pipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
                }
            }
        }

        /// <summary>
        /// Sets the state of the antenna animator and connection arrow animator depending on if the antenna is connected.
        /// </summary>
        /// <param name="isConnected">Whether the antenna is connected.</param>
        private void SetAnimators(bool isConnected)
        {
            antennaAnimator.SetBool("isConnected", isConnected);
            connectionArrowAnimator.SetBool("isConnected", isConnected);
        }
        #endregion
    }
}

