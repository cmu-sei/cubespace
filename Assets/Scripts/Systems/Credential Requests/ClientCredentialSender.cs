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
using UnityEngine;
using Systems.GameBrain;
using Systems.CredentialRequests.Models;

namespace Systems.CredentialRequests
{
    /// <summary>
    /// Class used within the Workstations scene to obtain authorization tokens and information for the server, through communication with a client and Gamebrain.
    /// </summary>
    public class ClientCredentialSender : Singleton<ClientCredentialSender>
    {
        // OAuth 2.0 client configuration variables
        // The ID of the game server 
        private static string clientID = "game-server";
        // The server's secret key that allows it to get an authorization token
        private static string clientSecret = "1bdebf134cdd457a9585d719d8415750";
        // The audience of the token; this should likely remain "gamestate-api"
        private const string audience = "gamestate-api";
        // The structure of the token 
        private const string authTokenRequestStructure = "grant_type=client_credentials&client_id={0}&client_secret={1}&audience={2}";
        // URIs to get the token and get the team ID
        private static string tokenEndpointURI, getTeamIDEndpointURI;

        // An extra buffer that multiplies the time to wait before refreshing the server token
        [SerializeField]
        private float tokenRefreshMultiplier = 0.9f;

        // The Gamebrain interface used within the scene
        [SerializeField]
        private NetworkGameBrainInterface gameBrainInterface;

        #region Authoriation token attribute methods (set externally)
        /// <summary>
        /// Sets the server's secret key so it can get an authorization token.
        /// </summary>
        /// <param name="_clientSecret">The client secret to assign to the server.</param>
        public static void SetClientSecret(string _clientSecret)
        {
            clientSecret = _clientSecret;
        }

        /// <summary>
        /// Sets the server's client ID so it can get an authorization token.
        /// </summary>
        /// <param name="_clientID">The client ID to assign to the server.</param>
        public static void SetClientID(string _clientID)
        {
            clientID = _clientID;
        }

        /// <summary>
        /// Sets the URIs needed for the server to get an authorization token and the team ID.
        /// </summary>
        public static void SetServerEndpointURIs()
        {
            tokenEndpointURI = $"{NetConfiguration.idURI}/connect/token";
            getTeamIDEndpointURI = $"{NetConfiguration.gamebrainURI}/privileged/get_team";
        }
        #endregion

        #region Server token methods
        /// <summary>
        /// Gets the token used on the server.
        /// </summary>
        public void GetServerToken()
        {
            // Create the OAuth 2.0 authorization request for client credentials
            string authorizationRequestBody = string.Format(authTokenRequestStructure, clientID, clientSecret, audience);

            // Retrieve an authorization token for the game server
            gameBrainInterface.GetServerToken(tokenEndpointURI, SetToken, authorizationRequestBody);
        }

        /// <summary>
        /// Sets the server token received and waits to refresh the token.
        /// </summary>
        /// <param name="response">The response received from a call to get a token.</param>
        private void SetToken(TokenResponse response)
        {
            // Set the access token used by the server to be the one received in the response
            ShipStateManager.Instance.token = response.access_token;

            // Wait for the token to almost expire, then ask for credentials again
            StartCoroutine(RefreshServerToken(response.expires_in * tokenRefreshMultiplier));
        }

        /// <summary>
        /// Waits for a short time, then gets a new token for the server via a request.
        /// </summary>
        /// <param name="delay">The time to wait before getting a new token.</param>
        /// <returns>A yield while waiting for a time before the server token is retrieved again.</returns>
        private IEnumerator RefreshServerToken(float delay)
        {
            yield return new WaitForSeconds(delay);
            GetServerToken();
        }
        #endregion

        #region Client token methods
        /// <summary>
        /// Method that returns an array holding the URI needed to get a team ID for the server and a client token used with that URI.
        /// <para>
        /// The URI is in the form of "...gamebrain/privileged/get_team" and the token is in the form of "eyjalkasdvlnasdv..."
        /// </para>
        /// </summary>
        /// <param name="clientToken">The token of a client joining the server.</param>
        /// <returns>An array containing the endpoint to get the team ID and the JSON of the client token and machine name.</returns>
        public string[] MakeTokenURIAndJSON(string clientToken)
        {
            string json = JsonUtility.ToJson(new PostData(clientToken));
            return new string[] { getTeamIDEndpointURI, json };
        }

        /// <summary>
        /// Sends the token of the player using the WebGL client to Gamebrain. The server calls this method; the client token is sent to the server before this method is called.
        /// </summary>
        /// <param name="clientToken">The token for the player using a WebGL client.</param>
        public void SendClientToken(string clientToken)
        {
            ClientToken token = JsonUtility.FromJson<ClientToken>(clientToken);
            Debug.LogWarning("?DEBUGGING?: ClientCredentialSender.cs:133\nSending new clients access token: " + token.access_token + " with my server token: " + ShipStateManager.Instance.token);
            gameBrainInterface.SendClientToken(ShipStateManager.Instance.SetTeamID, token.access_token, ShipStateManager.Instance.token);
        }
        #endregion
    }

}

