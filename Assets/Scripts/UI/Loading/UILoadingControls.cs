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
using TMPro;
using Mirror;
using Systems;

/// <summary>
/// Sets the UI loading controls.
/// </summary>
public class UILoadingControls : MonoBehaviour
{
    /// <summary>
    /// The IP field, with text entered by the client to connect to the server. Only used in-editor.
    /// </summary>
    [SerializeField]
    private TMP_InputField ipField;
    /// <summary>
    /// The button used to run a host instance. Only used in-editor.
    /// </summary>
    [SerializeField]
    private GameObject hostButton;
    /// <summary>
    /// The box used in the IP field.
    /// </summary>
    [SerializeField]
    private GameObject ipBox;
    /// <summary>
    /// The button used to connect the client to the server.
    /// </summary>
    [SerializeField]
    private GameObject clientButton;
    /// <summary>
    /// The button used to activate the server. This is hidden in production.
    /// </summary>
    [SerializeField]
    private GameObject serverButton;
    /// <summary>
    /// The button shown in production to connect a client to the server.
    /// </summary>
    [SerializeField]
    private GameObject productionClientButton;

    /// <summary>
    /// Unity event function that sets whether or not some buttons are active based on the production version.
    /// </summary>
    private void Start()
    {
        if (ipField)
        {
            ipField.text = NetworkManager.singleton.networkAddress;
        }

        if (hostButton)
        {
            hostButton.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }

        if (serverButton)
        {
            serverButton.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }
        
        if (clientButton && productionClientButton)
        {
            clientButton.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            productionClientButton.SetActive(Application.platform == RuntimePlatform.WebGLPlayer);
        }

        #if !UNITY_EDITOR
        ipField.gameObject.SetActive(false);
        #endif
    }

    /// <summary>
    /// Launches only the server.
    /// </summary>
    public void LaunchServerOnly()
    {
        NetworkManager.singleton.StartServer();
        LoadingSystem.Instance.calledAsServer = true;
    }

    /// <summary>
    /// Launches only the client.
    /// </summary>
    public void LaunchClientOnly()
    {
        if (LoadingSystem.Instance)
        {
            // NetworkManager.singleton.networkAddress = LoadingSystem.Instance.serverLink;
            LoadingSystem.OnLoadStarted += NetworkManager.singleton.StartClient;
            LoadingSystem.Instance.BeginLoad();
        }
    }

    /// <summary>
    /// Launches the game in host mode.
    /// </summary>
    public void LaunchServerAndClient()
    {
        if (LoadingSystem.Instance)
        {
            LoadingSystem.OnLoadStarted += NetworkManager.singleton.StartHost;
            LoadingSystem.Instance.calledAsServer = true;
            LoadingSystem.Instance.BeginLoad();
        }
    }

    /// <summary>
    /// Unity event function that unsubscribes from networking methods.
    /// </summary>
    private void OnDestroy()
    {
        try
        {
            LoadingSystem.OnLoadStarted -= NetworkManager.singleton.StartClient;
            LoadingSystem.OnLoadStarted -= NetworkManager.singleton.StartHost;
        }
        catch
        {
            // Not an error, just caught an exception when destroying the LoadingSystem
        }
    }
}

