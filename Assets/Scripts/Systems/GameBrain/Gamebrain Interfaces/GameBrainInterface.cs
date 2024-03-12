/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Mirror;

namespace Systems.GameBrain
{
	/// <summary>
	/// General interface for all Gamebrain functions. Used for both local file GameBrain calls and networked GameBrain calls.
	/// </summary>
	public abstract class GameBrainInterface : NetworkBehaviour
	{
		// The GameData received
		public GameData GameData => gameData;
		protected GameData gameData;

		/// <summary>
		/// Generic function used to represent a real callback function called at the end of a GameBrain method.
		/// </summary>
		/// <typeparam name="T">The type of the function.</typeparam>
		/// <param name="data">The function parameter.</param>
		public delegate void DataCallback<T>(T data);

        #region Session data methods
        /// <summary>
        /// Abstract function used to retrieve a ship data JSON.
        /// </summary>
        /// <param name="callback">The function to call after this method completes.</param>
        public abstract void GetShipData(DataCallback<GameData> callback);

		/// <summary>
		/// Abstract function used to retrieve the status of the team tracked by the server.
		/// </summary>
		/// <param name="teamID">The ID of the team whose status should be checked.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void GetTeamActive(string teamID, DataCallback<GenericResponse> callback);
		#endregion

		#region Player action methods
		/// <summary>
		/// Abstract function used to unlock a specific location.
		/// </summary>
		/// <param name="coords">The coordinates of the location a team is trying to unlock.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryUnlockLocation(string coords, DataCallback<LocationUnlockResponse> callback);

		/// <summary>
		/// Abstract function used to attempt jumping to a different location.
		/// </summary>
		/// <param name="locationID">The ID of the location a team is trying to jump to.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryJump(string locationID, DataCallback<GenericResponse> callback);

		/// <summary>
		/// Abstract function used to scan at the sensor station at the current location.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryScanLocation(DataCallback<ScanLocationResponse> callback);

		/// <summary>
		/// Abstract function used to complete a comm event at the current location.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryCompleteCommEvent(DataCallback<GenericResponse> callback);

		/// <summary>
		/// Abstract function used to extend the antenna at the current location.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryExtendAntenna(DataCallback<GenericResponse> callback);

		/// <summary>
		/// Abstract function used to retract the antenna at the current location.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryRetractAntenna(DataCallback<GenericResponse> callback);

		/// <summary>
		/// Abstract function used to change the power mode of the ship stored.
		/// </summary>
		/// <param name="powerMode">The power state of the ship based on its current power configuration.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public abstract void TryUpdatePowerMode(CurrentLocationGameplayData.PoweredState powerMode, DataCallback<GenericResponse> callback);
		#endregion
    }
}
