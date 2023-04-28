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
using System.Collections.Generic;
using Mirror;
using Managers;
using UnityEngine;
using Systems.CredentialRequests;
using Systems.CredentialRequests.Models;

namespace Systems.GameBrain
{
	/// <summary>
	/// Gamebrain structure which makes calls to a remote resource to update game information.
	/// </summary>
	public class NetworkGameBrainInterface : GameBrainInterface
	{
		/// <summary>
		/// The network manager object used to manage the game.
		/// </summary>
		CustomNetworkManager networkManager;

		/// <summary>
		/// Unity event function that grabs the command line functions provided and makes a request for an access token on the server.
		/// </summary>
		void Start()
		{
			GrabURIsOnCommandArguments();

			networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

			// Ask for an access token if running the server
			#if UNITY_SERVER
            ClientCredentialSender.Instance.GetServerToken();
			#endif
		}

		#region Session data methods
		/// <summary>
		/// Retrieves ship data stored on Gamebrain.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void GetShipData(DataCallback<GameData> callback)
		{
			string uri = NetConfiguration.GetShipDataURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Retrieves whether the team tracked by this server is still playing the game.
		/// </summary>
		/// <param name="teamID">The ID of the team whose status should be checked.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void GetTeamActive(string teamID, DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTeamActiveURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}
		#endregion

		#region Player action methods
		/// <summary>
		/// Tries to unlock a location with the given coordinates.
		/// </summary>
		/// <param name="coords">The coordinates of the location a team is trying to unlock.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryUnlockLocation(string coords, DataCallback<LocationUnlockResponse> callback)
		{
			string uri = NetConfiguration.GetTryUnlockLocationURI(coords, ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to jump to a new location with the given location ID.
		/// </summary>
		/// <param name="locationID">The ID of the location a team is trying to jump to.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryJump(string locationID, DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTryJumpURI(locationID, ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to scan the current location.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryScanLocation(DataCallback<ScanLocationResponse> callback)
		{
			string uri = NetConfiguration.GetTryScanLocationURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to complete a comm event at the sensor station.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryCompleteCommEvent(DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTryCommEventCompleteURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to extend the antenna.
		/// </summary>
		/// <param name="callback"></param>
		public override void TryExtendAntenna(DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTryExtendAntennaURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to retract the antenna.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryRetractAntenna(DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTryRetractAntennaURI(ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to update the power mode of the ship to exploration, launch, or standby.
		/// </summary>
		/// <param name="powerMode">The power state of the ship based on its current power configuration.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TryUpdatePowerMode(CurrentLocationGameplayData.PoweredState powerMode, DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTryUpdatePowerModeURI(powerMode, ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}

		/// <summary>
		/// Tries to set the power state of the codex station.
		/// </summary>
		/// <param name="isPowered">Whether the codex station is powered.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		public override void TrySetCodexPower(bool isPowered, DataCallback<GenericResponse> callback)
		{
			string uri = NetConfiguration.GetTrySetCodexPowerURI(isPowered, ShipStateManager.Instance.teamID);
			MakeGetRequest(uri, callback, ShipStateManager.Instance.token);
		}
        #endregion

        #region Token methods
        /// <summary>
        /// Makes a request to get an authorization token for the game server from the Identity server so the server can begin making calls to Gamebrain.
        /// </summary>
        /// <param name="requestURI">The URI of the token </param>
        /// <param name="callback">The function to call after this method completes.</param>
        /// <param name="tokenRequestBody">The complete body for the request to get an authorization token.</param>
        public void GetServerToken(string requestURI, DataCallback<TokenResponse> callback, string tokenRequestBody)
		{
			MakePostRequest(requestURI, callback, null, tokenRequestBody, "application/x-www-form-urlencoded");
		}

		/// <summary>
		/// Sends a token from a client instance and the server's authorization token to Gamebrain to retrieve the team ID.
		/// </summary>
		/// <param name="callback">The function to call after this method completes.</param>
		/// <param name="clientToken">The token of the client connecting to the server.</param>
		/// <param name="serverToken">The token of the server.</param>
		public void SendClientToken(DataCallback<TeamID> callback, string clientToken, string serverToken = null)
		{
			// Get the URI that is called to get the team this server is assigned to and the client token
			string[] tokenInfo = ClientCredentialSender.Instance.MakeTokenURIAndJSON(clientToken);
			string uri = tokenInfo[0];
			string requestBody = tokenInfo[1];
			MakePostRequest(uri, callback, serverToken, requestBody, "application/json");
		}
        #endregion

        #region Network Requests
        /// <summary>
        /// Makes a GET request to Gamebrain. Most calls should use this.
        /// </summary>
        /// <typeparam name="T">The type to receive the response as.</typeparam>
        /// <param name="uri">The URI to access in the web request.</param>
        /// <param name="callback">The function to call after this method completes.</param>
        /// <param name="token">The token to send in this request.</param>
        /// <param name="requestBody">The body of the request.</param>
        /// <param name="contentType">The type of content to receive.</param>
        private void MakeGetRequest<T>(string uri, DataCallback<T> callback, string token = null, string requestBody = null, string contentType = null) where T : IDeserializableFromJSON<T>
		{
			StartCoroutine(NetworkRoutine(uri, callback, NetUtility.HTTPMethod.GET, token, requestBody, contentType));
		}

		/// <summary>
		/// Makes a POST request to Gamebrain. Calls involving getting tokens should use this.
		/// </summary>
		/// <typeparam name="T">The type to receive the response as.</typeparam>
		/// <param name="uri">The URI to access in the web request.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		/// <param name="token">The token to send in this request.</param>
		/// <param name="requestBody">The body of the request.</param>
		/// <param name="contentType">The type of content to receive.</param>
		/// <returns>A yield return while waiting for the web request to finish.</returns>
		private void MakePostRequest<T>(string uri, DataCallback<T> callback, string token = null, string requestBody = null, string contentType = null) where T : IDeserializableFromJSON<T>
        {
			StartCoroutine(NetworkRoutine(uri, callback, NetUtility.HTTPMethod.POST, token, requestBody, contentType));
		}

		/// <summary>
		/// Makes a web request to Gamebrain using the HTTP method provided.
		/// </summary>
		/// <typeparam name="T">The type to receive the response as.</typeparam>
		/// <param name="uri">The URI to access in the web request.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		/// <param name="httpMethod">The HTTP method to use in this request.</param>
		/// <param name="token">The token to send in this request.</param>
		/// <param name="requestBody">The body of the request.</param>
		/// <param name="contentType">The type of content to receive.</param>
		/// <returns>A yield return while waiting for the web request to finish.</returns>
		private IEnumerator NetworkRoutine<T>(string uri, DataCallback<T> callback, NetUtility.HTTPMethod httpMethod, string token = null, string requestBody = null, string contentType = null) where T : IDeserializableFromJSON<T>
		{
			if (networkManager && networkManager.isInDebugMode)
            {
				Debug.Log($"Attempting to access {uri}");
            }
			string response = "{}";

			// Make the web request and wait for the result
			yield return StartCoroutine(NetUtility.WebRequest(uri, (x) => response = x, httpMethod, token, requestBody, contentType));

			// If the URI is meant to check if the team is still active, we need to call a funciton that specially converts the response to make it more parsable
			if (uri.Contains("team_active"))
			{
				response = ConvertTeamActiveResponse(response);
			}

			// Handle the response received
			HandleNetworkResult(response, callback);
		}
        #endregion

        #region Helper methods
        /// <summary>
        /// Converts the response of a team active call to be a string structured as a GenericResponse JSON so it can be interpreted and used.
        /// This call is necessary because the team active call returns a string of a boolean, rather than a GenericResponse structure.
        /// </summary>
        /// <param name="response">The response received from a team active call.</param>
        /// <returns>The response as JSON.</returns>
        private string ConvertTeamActiveResponse(string response)
		{
			if (networkManager && networkManager.isInDebugMode)
            {
				Debug.Log($"Team active response: {response}");
            }
			switch (response)
			{
				case "true":
					response = JsonUtility.ToJson(new GenericResponse { success = true, message = response });
					break;
				case "false":
					response = JsonUtility.ToJson(new GenericResponse { success = false, message = response });
					break;
				default:
					Debug.Log($"Received unexpected response from team_active. Received: {response}. Aborting generic response conversion.");
					break;
			}

			return response;
		}

		/// <summary>
		/// Handles the result of a network call.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the JSON received as.</typeparam>
		/// <param name="response">The response received from the earlier networking call.</param>
		/// <param name="callback">The function to call after this method completes.</param>
		private void HandleNetworkResult<T>(string response, DataCallback<T> callback) where T : IDeserializableFromJSON<T>
		{
			// As long as the response exists, convert the response to JSON and invoke the callback with the converted response
			if (response != "" && response != "null")
			{
				try
				{
					T lr = IDeserializableFromJSON<T>.CreateFromJSON(response);
					lr.Initiate(); // This builds dictionaries, converts strings to enums, performs and other actions we might do in a constructor if we weren't deserializing JSON
					callback.Invoke(lr);
				}
				catch (ArgumentException e)
				{
					if (networkManager && networkManager.isInDebugMode)
                    {
						Debug.Log($"Encountered an exception while trying to parse JSON. Exception: {e}");
						Debug.Log($"Response received: {response}");
                    }
				}
			}
		}
        #endregion

        #region Command line argument methods
        /// <summary>
        /// Sets the URIs used by the server.
        /// </summary>
        private static void GrabURIsOnCommandArguments()
		{
			// Create a dictionary to hold the arguments provided
            Dictionary<string, string> providedArguments = ParseCommandLineArgs();
			// Create variables with defaults to store the URIs we may or may not find
			string baseURI = "https://foundry.local";
			string gamebrainURI = "https://foundry.local/gamebrain";
			string identityURI = "https://foundry.local/identity";

			// If the base URI was provided, set it
			if (providedArguments.TryGetValue("uriBase", out string _uriBase) && !string.IsNullOrEmpty(_uriBase))
			{
				baseURI = _uriBase;
			}

			// If the Gamebrain URI was provided, set it
			if (providedArguments.TryGetValue("gamebrainURI", out string _gamebrainURI) && !string.IsNullOrEmpty(_gamebrainURI))
			{
				gamebrainURI = _gamebrainURI;
			}

			// If the Identity URI was provided, set it
			if (providedArguments.TryGetValue("identityURI", out string _identityURI) && !string.IsNullOrEmpty(_identityURI))
			{
				identityURI = _identityURI;
			}

			// If the client ID is provided, set it
			if (providedArguments.TryGetValue("clientID", out string _clientID) && !string.IsNullOrEmpty(_clientID))
			{
				ClientCredentialSender.SetClientID(_clientID);
			}

			// If the client secret is provided, set it
			if (providedArguments.TryGetValue("clientSecret", out string _clientSecret) && !string.IsNullOrEmpty(_clientSecret))
			{
				ClientCredentialSender.SetClientSecret(_clientSecret);
			}

			// Print the URIs supplied
			Debug.Log($"Base URI: {baseURI}");
			Debug.Log($"Gamebrain URI: {gamebrainURI}");
			Debug.Log($"Identity URI: {identityURI}");

			// Set the URIs elsewhere for later use and construct the server token and team endpoints
			NetConfiguration.SetURIs(baseURI, gamebrainURI, identityURI);
			ClientCredentialSender.SetServerEndpointURIs();

			// Get the CustomNetworkManager to set global parameters if needed
			CustomNetworkManager manager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();

			// Set the global dev parameter
			if (providedArguments.TryGetValue("dev", out string dev))
			{
				Debug.Log("Running the game in dev mode.");
				manager.isInDevMode = true;
			}

			// Set the global debug parameter
			if (providedArguments.TryGetValue("debug", out string debug))
			{
				Debug.Log("Running the game in debug mode.");
				// Set the global debug parameter
				manager.isInDebugMode = true;
			}
		}

		/// <summary>
		/// Parses through the provided command line arguments.
		/// </summary>
		/// <returns>The dictionary with the provided command line arguments.</returns>
		private static Dictionary<string, string> ParseCommandLineArgs()
		{
			// Prepare a dictionary of the provided arguments
			Dictionary<string, string> providedArguments = new Dictionary<string, string>();
			// Retrieve the provided command line arguments
			string[] args = Environment.GetCommandLineArgs();

			// Loop through the provided arguments
			for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse the command line flag
                bool isFlag = args[current].StartsWith("-");
				if (!isFlag)
				{
					continue;
				}
                string flag = args[current].TrimStart('-');

                // Parse the optional value following the flag
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
				// Add the argument into the dictionary
                providedArguments.Add(flag, value);
            }

			return providedArguments;
		}
		#endregion
	}
}
