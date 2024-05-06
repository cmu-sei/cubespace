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
/// Calls an external function to retrieve client's token and the link to the game server
/// </summary>
public class HandleTokenInfo : MonoBehaviour
{
    // A reference to the javascript plugin function to retrieve a client's token from local storage in the user's browser
    // Javascript file at: Assets/Plugins/GetTokenInfo.jslib
    [DllImport("__Internal")]
    private static extern void GetTokenInfo();
    // The CustomNetworkManager component reference
    private CustomNetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.singleton.GetComponent<CustomNetworkManager>();
        
        // If this is a webgl client, call the external javascript function to retrieve our token and the link to the server from local storage (which is placed there by GameBoard)
        #if UNITY_WEBGL
        GetTokenInfo();
        #endif
    }

    /// <summary>
    /// Function externally called by GetTokenInfo.jslib to pass the token into cubespace from local storage
    /// This is necessary so that the client can pass this token information to the cubespace server
    /// </summary>
    /// <param name="_tokenInfo">The token information as a string.</param>
    public void RecieveTokenInfo(string _tokenInfo)
    {
        if (_tokenInfo == null)
        {
            Debug.LogError("Cubespace client did not receive a token!");
            return;
        }

        // Set the token in the loading system to be the one retrieved
        // This will get grabbed on a new client in Player.cs to send to the server (CmdSendClientToken)
        LoadingSystem.Instance.token = _tokenInfo;
    }

    /// <summary>
    /// Function externally called by GetTokenInfo.jslib to pass the url of the correct cubespace server into cubespace from local storage
    /// This is necessary so that the client can connect to the server
    /// </summary>
    /// <param name="_serverLink">The link to the game server.</param>
    public void ReceiveServerLink(string _serverLink)
    {
        if (_serverLink == null)
        {
            Debug.LogError("Cubespace client did not receive a link to a cubespace server!");
            return;
        }

        // Set the game server link in the loading system to be the one retrieved (NOTE: this var isn't actually used for anything)
        LoadingSystem.Instance.serverLink = _serverLink;
        // Set the link to the server directly in the NetworkManager,
        // TODO: this should be retrieved from loading system or the loading system should be axed and everything should go through the network manager
        NetworkManager.singleton.networkAddress = _serverLink;
    }
}

