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
using System.Collections;
using Managers;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.GameBrain
{
	/// <summary>
	/// A behavior which lives between the ShipStateManager (which does not care where data comes from and just gets ShipData)
	/// and the NetworkManager (which deals with the network connection, user authentication, and miscellaneous operations).
	/// <para>
	/// This behavior handles two primary complications:
	/// <list type="bullet">
	/// <item>Going from callbacks with a specific method signature to whatever else is needed, using callbacks and references to the ShipStateManager</item>
	/// <item>Adding a ship state update in after entering coordinates successfully, before finishing the location entry response (so we can enter a correct coordinate, then return to the location selection screen populated, and already set to that coordinate)</item>
	/// </list>
	/// </para>
	/// </summary>
	public class ShipGameBrainUpdater : NetworkBehaviour
	{
		// The interface uesd for updates
		[SerializeField]
		private GameBrainInterface _gameBrainInterface;
		// How long to wait before polling data again
		[SerializeField, Min(0.25f)]
		private float pollingDelay;

		// The bool in this Action is the known difference, while GameData is the ship data
		public static Action<bool, GameData> OnShipDataReceived;
		public static Action<LocationUnlockResponse> OnLocationUnlockResponse;
		public static Action<GenericResponse> OnJumpResponse;
		public static Action<GenericResponse> OnTryExtendAntennaResponse;
		public static Action<GenericResponse> OnTryRetractAntennaResponse;
		public static Action<GenericResponse> OnTrySetPowerModeResponse;
		public static Action<ScanLocationResponse> OnTryScanLocation;
		public static Action<GenericResponse> OnTryCompleteCommEventResponse;
		public static Action<GenericResponse> OnTrySetCodexPower;

		// The last ship data received, used only for debugging in inspector
		[FormerlySerializedAs("lastShipData")]
		[SerializeField]
		private GameData lastGameData;

		private bool checkIfTeamIsActive = true;
		private int lastShipDataHash;

		private CustomNetworkManager networkManager;

		/// <summary>
		/// Starts polling for new ship data.
		/// </summary>
		void Start()
        {
			networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

			StartCoroutine(Poll());
        }

		/// <summary>
		/// Polls for new ship data after a specified delay
		/// </summary>
		/// <returns>A yield while waiting on the delay to end.</returns>
		private IEnumerator Poll()
        {
			while (true)
            {
				yield return new WaitForSeconds(pollingDelay);
				PollForShipData();
            }
        }

        #region Requests
        /// <summary>
        /// Forcefully polls ship data.
        /// </summary>
        public void PollForShipData()
		{
			if (isServer)
			{
                Debug.LogWarning("?DEBUGGING?: ShipGameBrainUpdater.cs:92\nServer is polling for data like it does ever 2 seconds");
                // If the server is tracking a team (and it should be), make a call to check if they're still active
                if (checkIfTeamIsActive && ShipStateManager.Instance.teamID != "")
				{
                    Debug.LogWarning("?DEBUGGING?: ShipGameBrainUpdater.cs:97\nServer has a teamID == " + ShipStateManager.Instance.teamID + " so it will hit the team_active endpoint");
                    _gameBrainInterface.GetTeamActive(ShipStateManager.Instance.teamID, GetTeamActiveCallback);
                }
				else
				{
                    Debug.LogWarning("?DEBUGGING?: ShipGameBrainUpdater.cs:101\nServer does NOT have a teamID, so it will not hit the team_active endpoint");
                }

				// Do the forceful update
				_gameBrainInterface.GetShipData(PollShipDataCallback);
			}
		}

		/// <summary>
		/// Sends a request to jump to a different location.
		/// </summary>
		/// <param name="locationID">The ID of the location to jump to.</param>
		public void SendJumpRequest(string locationID)
		{
			if (isServer)
			{
				_gameBrainInterface.TryJump(locationID, JumpCallback);
			}
		}

		/// <summary>
		/// Sends a request to unlock a location with given coordinates.
		/// </summary>
		/// <param name="coords">The coordinates entered to try to unlock the location.</param>
		public void SendUnlockLocationRequest(string coords)
		{
			// Return if we're trying to call this on the client
			if (!isServer)
			{
				return;
			}

			// Try to unlock the location with the coordinates provided
			_gameBrainInterface.TryUnlockLocation(coords, UnlockLocationDataCallback);
		}

		/// <summary>
		/// Sends a request to extend the antenna.
		/// </summary>
		public void TryExtendAntenna()
		{
			_gameBrainInterface.TryExtendAntenna(TryExtendAntennaCallback);
		}

		/// <summary>
		/// Sends a request to retract the antenna.
		/// </summary>
		public void TryRetractAntenna()
		{
			_gameBrainInterface.TryRetractAntenna(TryRetractAntennaCallback);
		}

		/// <summary>
		/// Sends a request to set the state of the ship's power.
		/// </summary>
		/// <param name="state">The type of state the ship has with its current power configuration.</param>
		public void TrySetPowerMode(CurrentLocationGameplayData.PoweredState state)
		{
			_gameBrainInterface.TryUpdatePowerMode(state, TrySetPowerModeCallback);
		}

		/// <summary>
		/// Sends a request to scan the current location.
		/// </summary>
		public void TryScan()
		{
			_gameBrainInterface.TryScanLocation(TryScanCallback);
		}

		/// <summary>
		/// Sends a request to set the codex station's power state, used for decoding the codex.
		/// </summary>
		/// <param name="isPowered">Whether the codex station is powered.</param>
		public void TrySetCodexPower(bool isPowered)
		{
			_gameBrainInterface.TrySetCodexPower(isPowered, TrySetCodexPowerCallback);
		}

		/// <summary>
		/// Sends a request to complete the comm event at the current location.
		/// </summary>
		public void TryCompleteCommEvent()
		{
			_gameBrainInterface.TryCompleteCommEvent(TryCompleteCommEventCallback);
		}
		#endregion

		#region Callbacks
		/// <summary>
		/// Handles the response from jumping, received from the GameBrain server after a jump's success.
		/// </summary>
		/// <param name="response">The response from a succesful jump.</param>
		private void JumpCallback(GenericResponse response)
		{
			// If the jump succeeds, force update the game and call the callback function
			if (response.success)
			{
				PollForShipData();
				OnJumpResponse?.Invoke(response);
			}
			// If the jump fails, write a message to the server
			else if (networkManager && networkManager.isInDebugMode)
			{
				Debug.LogWarning($"Jump Failed! if jump was attempted in earnest, it should not have failed. Message: {response.message}");
			}
		}

		/// <summary>
		/// Handles the response from a ship data poll.
		/// </summary>
		/// <param name="data">The game data received from GameBrain.</param>
		private void PollShipDataCallback(GameData data)
		{
			// Set the last ship data to be the ship data we just got
			lastGameData = data;

			// If this data is the same as the last one, invoke the response while marking the data as new
			var hash = data.GetHashCode();
			if (hash != lastShipDataHash)
			{
				OnShipDataReceived?.Invoke(true, data);
			}
			// Otherwise, the data appears to be the same
			else
			{
				OnShipDataReceived?.Invoke(false, data);
			}
		}

		/// <summary>
		/// Handles the response from a request for whether the team tracked by the server is still active, and shuts the server down if not.
		/// </summary>
		/// <param name="isTeamActiveResponse">The team response data received from GameBrain.</param>
		private void GetTeamActiveCallback(GenericResponse isTeamActiveResponse)
		{
            Debug.LogWarning("?DEBUGGING?: ShipGameBrainUpdater.cs:236\nServer received a response from the team_active endpoint and fired the GetTeamActiveCallback callback\nisTeamActiveResponse.success == " + isTeamActiveResponse.success + "\nShipStateManager.Instance.teamID == " + ShipStateManager.Instance.teamID);

            // Shut the server down if everyone's gone and the team isn't active - it should automatically get restarted by Kubernetes
            if (ShipStateManager.Instance != null && !isTeamActiveResponse.success && ShipStateManager.Instance.teamID != "")
			{
				if (networkManager && networkManager.isInDebugMode)
                {
					Debug.Log("Restarting server");
                }
                Debug.LogWarning("?DEBUGGING?: ShipGameBrainUpdater.cs:245\nResponse from team_active endpoint was unsuccessful and ShipStateManager.Instance.teamID == " + ShipStateManager.Instance.teamID + "which is not an empty string, so the server will nuke itself");
                Application.Quit();
			}
		}

		/// <summary>
		/// Handles the response from an attempt to unlock a location. This could be a failure response or a success response.
		/// </summary>
		/// <param name="response">The location unlock response.</param>
		private void UnlockLocationDataCallback(LocationUnlockResponse response)
		{
			OnLocationUnlockResponse?.Invoke(response);
		}

		/// <summary>
		/// Handles the response from an attempt to extend the antenna.
		/// </summary>
		/// <param name="response">The response indicating whether the antenna extension was successful or not.</param>
		private void TryExtendAntennaCallback(GenericResponse response)
		{
			OnTryExtendAntennaResponse?.Invoke(response);
		}

		/// <summary>
		/// Handles the response from an attempt to retract the antenna.
		/// </summary>
		/// <param name="response">The response indicating whether the antenna retraction was successful or not.</param>
		private void TryRetractAntennaCallback(GenericResponse response)
		{
			OnTryRetractAntennaResponse?.Invoke(response);
		}

		/// <summary>
		/// Handles the response from an attempt to change the power mode resulting from the power configuration.
		/// </summary>
		/// <param name="response">The response indicating whether the power mode switch was successful or not.</param>
		private void TrySetPowerModeCallback(GenericResponse response)
		{
			// This does nothing, but it should do nothing, because power should be controlled purely internally
			OnTrySetPowerModeResponse?.Invoke(response);
		}

		/// <summary>
		/// Handles the response from an attempt to scan the current location.
		/// </summary>
		/// <param name="response">The response indicating whether there is a comm event waiting at this location and information on the incoming transmission.</param>
		private void TryScanCallback(ScanLocationResponse response)
		{
			if (response.success)
			{
				OnTryScanLocation?.Invoke(response);
			}
		}

		/// <summary>
		/// Handles the response from an attempt to complete a comm event.
		/// </summary>
		/// <param name="response">The response indicating whether the comm event was successfully completed.</param>
		private void TryCompleteCommEventCallback(GenericResponse response)
		{
			if (response.success)
			{
				OnTryCompleteCommEventResponse?.Invoke(response);
			}
		}

		/// <summary>
		/// Handles the response from an attempt to set the power on the codex decoder workstation.
		/// </summary>
		/// <param name="response">The response indicating whether the attempt to set the codex decoder workstation's power was successful.</param>
		private void TrySetCodexPowerCallback(GenericResponse response)
		{
			if (response.success)
			{
				OnTrySetCodexPower?.Invoke(response);
			}
		}
		#endregion
	}
}
