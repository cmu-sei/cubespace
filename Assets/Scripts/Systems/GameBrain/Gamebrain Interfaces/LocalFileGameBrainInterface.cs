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

namespace Systems.GameBrain
{
	/// <summary>
	/// An interface that mimics a remote GameBrain resource as a local GameBrain resource.
	/// This structure uses a JSON TextAsset to load ship information and then perform functions.
	/// Because this script is meant to be used for local testing, it largely spoofs functionality.
	/// </summary>
	public class LocalFileGameBrainInterface : GameBrainInterface
	{
		// The local JSON data that represents the full ship data
		[SerializeField]
		private TextAsset localJSONData;

        #region Session data methods
        /// <summary>
        /// Retrieves ship data from the local JSON.
        /// </summary>
        /// <param name="callback">The function to call after this method completes.</param>
        public override void GetShipData(DataCallback<GameData> callback)
		{
			gameData = GameData.CreateFromJSON(localJSONData.text);
			gameData.Initiate();
			callback.Invoke(GameData);
		}

		/// <summary>
		/// Retrieves whether the team tracked by this server is still playing the game. Note that this is never used throughout the Editor; it just needs to be here as part of the interface definition.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="teamID">The team ID. Unused, but necessary.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void GetTeamActive(string teamID, DataCallback<GenericResponse> callback)
        {
			GenericResponse isActiveResponse = new GenericResponse();
			isActiveResponse.success = true;
			callback.Invoke(isActiveResponse);
        }
		#endregion

		#region Player action methods
		/// <summary>
		/// Tries to unlock a location with the given coordinates.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as invalid.
		/// </summary>
		/// <param name="coords">The coordinates of the location a team is trying to unlock.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryUnlockLocation(string coords, DataCallback<LocationUnlockResponse> callback)
		{
			LocationUnlockResponse r = new LocationUnlockResponse
			{
				unlockResult = LocationUnlockResponse.UnlockResult.Invalid
			};

			callback.Invoke(r);
		}

		/// <summary>
		/// Tries to jump to a new location with the given location ID.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="locationID">The ID of the location a team is trying to jump to.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryJump(string locationID, DataCallback<GenericResponse> callback)
		{
			var jumped = new GenericResponse();
			jumped.success = true;
			jumped.message = "testing";
			callback.Invoke(jumped);
		}

		/// <summary>
		/// Tries to scan the current location.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful with no event waiting.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryScanLocation(DataCallback<ScanLocationResponse> callback)
		{
			var response = new ScanLocationResponse();
			response.success = true;
			response.eventWaiting = false;
			callback.Invoke(response);
		}

		/// <summary>
		/// Tries to complete a comm event at the sensor station.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryCompleteCommEvent(DataCallback<GenericResponse> callback)
		{
			var response = new GenericResponse();
			response.success = true;
			callback.Invoke(response);
		}

		/// <summary>
		/// Tries to extend the antenna.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryExtendAntenna(DataCallback<GenericResponse> callback)
		{
			var response = new GenericResponse();
			response.success = true;
			callback.Invoke(response);
		}

		/// <summary>
		/// Tries to retract the antenna.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryRetractAntenna(DataCallback<GenericResponse> callback)
		{
			var response = new GenericResponse();
			response.success = true;
			callback.Invoke(response);
		}

		/// <summary>
		/// Tries to update the power mode of the ship to exploration, launch, or standby.
		/// Because this is a local file interface, this function spoofs a response and always returns a response marking the attempt as successful.
		/// </summary>
		/// <param name="powerMode">The power state of the ship based on its current power configuration.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryUpdatePowerMode(PoweredState powerMode, DataCallback<GenericResponse> callback)
		{
			var response = new GenericResponse();
			response.success = true;
			callback.Invoke(response);
		}
		#endregion
	}
}
