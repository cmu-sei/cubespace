/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Managers;

/// <summary>
/// Pops up information to the client.
/// </summary>
public class UIPopupInformation : Singleton<UIPopupInformation>
{
    /// <summary>
    /// The queue of information.
    /// </summary>
    private List<string> informationQueue;
    /// <summary>
    /// The object handling how this popup is revealed to the player.
    /// </summary>
    [SerializeField]
    private UIReveal revealHandler;
    /// <summary>
    /// The text object of this popup.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI text;

    /// <summary>
    /// Populates a new list.
    /// </summary>
    public override void Start()
    {
        base.Start();
        informationQueue = new List<string>();
    }

    /// <summary>
    /// Adds a string to the popup object.
    /// </summary>
    /// <param name="newString">The new piece of information to display.</param>
    public void AddString(string newString)
    {
        informationQueue.Add(newString);
    }

    /// <summary>
    /// Displays the latest message within the popup.
    /// </summary>
    public void Update()
    {
        if (revealHandler.fullCycleCompleted && informationQueue.Count != 0)
        {
            text.text = informationQueue[0];
            informationQueue.RemoveAt(0);
            revealHandler.RevealHorizontally();
        }
    }
}

