/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using Systems.GameBrain;
using TMPro;
using UnityEngine;

/// <summary>
/// A text component used to display the ship's current location.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class UICurrentShipLocationText : Singleton<UICurrentShipLocationText>
{
    /// <summary>
    /// The text object which shows the current location.
    /// </summary>
    private TMP_Text text;

    /// <summary>
    /// Unity event function that gets the text component when this object first starts.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        text = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Subscribes to the current location change event.
    /// </summary>
    private void OnEnable()
    {
        ShipStateManager.OnCurrentLocationChange += OnLocationChange;
    }

    /// <summary>
    /// Unsubscribes from the current location change event.
    /// </summary>
    private void OnDisable()
    {
        ShipStateManager.OnCurrentLocationChange -= OnLocationChange;
    }

    /// <summary>
    /// Unity event function that calls the OnLocationChange event with the current location on startup.
    /// </summary>
    public override void Start()
    {
        base.Start();
        OnLocationChange(ShipStateManager.Instance.GetCurrentLocation());
    }

    /// <summary>
    /// Sets the text which displays the location name when invoked.
    /// </summary>
    /// <param name="location">The location the ship has jumped to.</param>
    private void OnLocationChange(Location location)
    {
        if (location != null)
        {
            SetText(location.name);
        }
    }

    /// <summary>
    /// Adjusts the opacity of the text object.
    /// </summary>
    /// <param name="value">The value to set the opacity to. Should be between 0 and 1.</param>
    public void AdjustOpacity(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        text.color = new Color(text.color.r, text.color.g, text.color.b, value);
    }

    /// <summary>
    /// Sets the text to the name of the location provided.
    /// </summary>
    /// <param name="locationName">The name of the location.</param>
    void SetText(string locationName)
    {
        //Remove any break statements that might be used anywhere else.
        if (locationName == null)
        {
            locationName = "";
        }
        locationName = locationName.Replace("<br>", " ");
        text.text = "<font-weight=700>"+ locationName +"</font-weight>";
        text.enabled = locationName != "";
    }
}

