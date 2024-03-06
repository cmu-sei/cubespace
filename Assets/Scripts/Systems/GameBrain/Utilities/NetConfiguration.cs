/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

namespace Systems.GameBrain
{
	/// <summary>
	/// Static class used to hold configuration settings and create URIs for talking to a remote Gamebrain resource.
	/// </summary>
	internal static class NetConfiguration
	{
		// The link to the Gamebrain server, used to connect to Gamebrain
		public static string gamebrainURI;
		// The link to the Identity server, used to retrieve a token
		public static string idURI;

        #region Constructor
        /// <summary>
        /// Sets the base domain URI, Gamebrain URI, and identity URI.
        /// </summary>
        /// <param name="_gamebrainURI">The link for the Gamebrain server, needed to send and receive game data remotely</param>
        /// <param name="_idURI">The link to the identity server, needed to retrieve a token for the server.</param>
        internal static void SetURIs(string _gamebrainURI, string _idURI)
		{
			gamebrainURI = TrimURI(_gamebrainURI);
			idURI = TrimURI(_idURI);
		}
        #endregion

        #region Helper methods
        /// <summary>
        /// Helper method that removes an ending / character on the given URI (if it ends with a / character) and returns it.
        /// </summary>
        /// <param name="uri">The URI to trim.</param>
        /// <returns>The URI without an ending / character, if it ended with that character.</returns>
        private static string TrimURI(string uri)
        {
			return uri.EndsWith("/") ? uri.TrimEnd('/') : uri;
		}

		/// <summary>
		/// Helper method that creates a URI using a starting string shared among many URIs ("{gamebrainURI}/GameData/") 
		/// and the given ending of the URI, appended at the end of that starting string.
		/// </summary>
		/// <param name="uriEnd">The ending portion of a URI following /GameData/.</param>
		/// <returns>The complete URI for the endpoint.</returns>
		private static string CreateGameDataURI(string uriEnd)
		{
			return $"{gamebrainURI}/GameData/{uriEnd}";
		}
        #endregion

        #region Common URIs
        /// <summary>
        /// Gets the basic ship data URI used to retrieve general information about the ship.
        /// </summary>
        /// <param name="teamID">The ID of the team. An empty string by default.</param>
        /// <returns>The URI used to retrieve ship data.</returns>
        internal static string GetShipDataURI(string teamID = "")
		{
			return CreateGameDataURI(teamID);
		}

		/// <summary>
		/// Gets the URI used to retrieve whether the team is still active.
		/// </summary>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to retrieve whether the team is still active.</returns>
		internal static string GetTeamActiveURI(string teamID = "")
		{
			return $"{gamebrainURI}/gamestate/team_active/{teamID}";
		}

		/// <summary>
		/// Gets the URI used to attempt to unlock a location.
		/// </summary>
		/// <param name="coordString">The coordinate string used to attempt to unlock a location.</param>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to unlock a location.</returns>
		internal static string GetTryUnlockLocationURI(string coordString, string teamID = "")
		{
			return CreateGameDataURI($"LocationUnlock/{coordString}/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to jump to a location.
		/// </summary>
		/// <param name="locationID">The ID of the location the ship is jumping to.</param>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to jump to a location.</returns>
		internal static string GetTryJumpURI(string locationID, string teamID = "")
		{
			return CreateGameDataURI($"Jump/{locationID}/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to extend the antenna.
		/// </summary>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to extend the antenna.</returns>
		public static string GetTryExtendAntennaURI(string teamID = "")
		{
			return CreateGameDataURI($"ExtendAntenna/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to retract the antenna.
		/// </summary>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to retract the antenna.</returns>
		public static string GetTryRetractAntennaURI(string teamID = "")
		{
			return CreateGameDataURI($"RetractAntenna/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to scan the current location.
		/// </summary>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to scan the current location.</returns>
		public static string GetTryScanLocationURI(string teamID = "")
		{
			return CreateGameDataURI($"ScanLocation/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to complete a comm event at the sensor station.
		/// </summary>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to complete a comm event at the sensor station.</returns>
		public static string GetTryCommEventCompleteURI(string teamID = "")
		{
			return CreateGameDataURI($"CommEventCompleted/{teamID}");
		}

		/// <summary>
		/// Gets the URI used to attempt to set the power mode of the ship.
		/// </summary>
		/// <param name="powerMode">The power mode (exploration, launch, or standby) to set the ship to have.</param>
		/// <param name="teamID">The ID of the team. An empty string by default, but necessary for the request using this URI to work.</param>
		/// <returns>The URI used to try to update the ship's current power mode.</returns>
		public static string GetTryUpdatePowerModeURI(CurrentLocationGameplayData.PoweredState powerMode, string teamID = "")
		{
			return CreateGameDataURI($"PowerMode/{CurrentLocationGameplayData.PoweredStateAsString(powerMode)}/{teamID}");
		}
        #endregion
    }
}
