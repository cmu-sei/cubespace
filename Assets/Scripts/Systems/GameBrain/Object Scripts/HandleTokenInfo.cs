/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Runtime.InteropServices;
using UnityEngine;
using Managers;
using Mirror;
using Systems;

/// <summary>
/// Calls a plugin function to retrieve token information and the link to the game server.
/// </summary>
public class HandleTokenInfo : MonoBehaviour
{
    // A reference to the plugin function to retrieve and use a client's token
    [DllImport("__Internal")]
    private static extern void GetTokenInfo();
    // The CustomNetworkManager component reference
    private CustomNetworkManager networkManager;

    /// <summary>
    /// Unity event function that gets the CustomNetworkManager within the game and retrieves the token info and game server link (through GetTokenInfo.jslib) if the game is running as a WebGL instance.
    /// </summary>
    void Start()
    {
        // Get the CustomNetworkManager component of the main network manager
        networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
        
        // If running as a WebGL instance, retrieve the token information and game server link
        #if UNITY_WEBGL
        GetTokenInfo();
        #endif
    }

    /// <summary>
    /// Function externally called to pass client token information into the LoadingSystem.
    /// This is necessary so that the client can pass its token information to the server if needed.
    /// </summary>
    /// <param name="_tokenInfo">The token information as a string.</param>
    public void RecieveTokenInfo(string _tokenInfo)
    {
        if (networkManager && networkManager.isInDebugMode)
        {
            //Debug.Log("Info Recieved: " + _tokenInfo);
        }

        if (_tokenInfo == null)
        {
            Debug.LogError("No credentials found");
            return;
        }

        // Set the token in the loading system to be the one retrieved
        // This will get grabbed on a new client in Player.cs to send to the server (CmdSendClientToken)
        LoadingSystem.Instance.token = _tokenInfo;
    }

    /// <summary>
    /// Function externally called to pass the game server link into the LoadingSystem.
    /// This is necessary so that the client can locate and connect to the server.
    /// </summary>
    /// <param name="_serverLink">The link to the game server.</param>
    public void ReceiveServerLink(string _serverLink)
    {
        if (networkManager && networkManager.isInDebugMode)
        {
            //Debug.Log("Server IP Received: " + _serverLink);
        }

        if (_serverLink == null)
        {
            Debug.LogError("No server link found");
            return;
        }

        // Set the game server link in the loading system to be the one retrieved (NOTE: this var isn't actually used for anything)
        LoadingSystem.Instance.serverLink = _serverLink;
        // Set the link to the server directly in the NetworkManager, TODO: this should be retrieved from loading system for consistency's sake
        NetworkManager.singleton.networkAddress = _serverLink;
    }
}

