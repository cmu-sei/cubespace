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
using Systems.GameBrain;
using TMPro;
using UnityEngine;
using Systems;

/// <summary>
/// The text used to display a team name.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class UITeamNameText : NetworkBehaviour
{
    /// <summary>
    /// The text component used to display the team name.
    /// </summary>
    private TMP_Text text;
    /// <summary>
    /// The team name.
    /// </summary>
    [SyncVar(hook = nameof(SetText))]
    private string teamName;

    /// <summary>
    /// Unity event function that gets the text component to display the team name.
    /// </summary>
    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Sets the team name as soon as the client starts up.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        SetText("", teamName);
    }

    /// <summary>
    /// Unity event function that subscribes to the OnShipDataReceived action.
    /// </summary>
    private void OnEnable()
    {
        ShipGameBrainUpdater.OnShipDataReceived += OnShipDataReceived;
    }

    /// <summary>
    /// Unity event function that unsubscribes from the OnShipDataReceived action.
    /// </summary>
    private void OnDisable()
    {
        ShipGameBrainUpdater.OnShipDataReceived -= OnShipDataReceived;
    }

    /// <summary>
    /// Updates the team name when ShipData is received.
    /// </summary>
    /// <param name="changes">Whether the data has changed.</param>
    /// <param name="data">The data received.</param>
    [Server]
    private void OnShipDataReceived(bool changes, GameData data)
    {
        if (isServer && changes)
        {
            teamName = data.session.teamInfoName;
            
            SetText("", teamName);
        }
    }

    /// <summary>
    /// Sets the name text. This acts as an overload for use as a SyncVar hook.
    /// </summary>
    /// <param name="oldName">The previous team name.</param>
    /// <param name="newName">The new team name.</param>
    void SetText(string oldName, string newName)
    {
        text.text = teamName;

        if (isClient)
        {
            LoadingSystem.Instance.EndLoad();
        }
    }

}

