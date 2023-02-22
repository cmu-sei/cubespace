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
using System;
using Managers;
using Systems.GameBrain;
using UI.HUD;

/// <summary>
/// The component which populates mission UI from the Unity server and is responsible for selecting missions.
/// </summary>
public class UIHudMissionManager : MonoBehaviour
{
    /// <summary>
    /// The map of missions to icons.
    /// </summary>
    [SerializeField]
    private IDToImageMap missionIconMap;
    /// <summary>
    /// The list of mission items, which should be stored in numeric order.
    /// </summary>
    [SerializeField]
    private UIHudMissionItem[] missionItems;
    /// <summary>
    /// A reference to the scripted slider for highlighting items.
    /// </summary>
    [SerializeField]
    private UIHudMissionSlider missionSlider;

    /// <summary>
    /// The details of each mission.
    /// </summary>
    [SerializeField]
    private UIHudMissionDetailsPanel missionDetails;
    /// <summary>
    /// The vignette used in the back of the mission log to obfuscate the rest of the game.
    /// </summary>
    [SerializeField]
    private GameObject vignetteObject;

    /// <summary>
    /// The last selected mission's index.
    /// </summary>
    private int lastSelectedIndex = 0;

    /// <summary>
    /// Unity event function that initiates the mission and icon mapping.
    /// </summary>
    private void Awake()
    {
        missionIconMap.InitiateDictionary();
    }

    /// <summary>
    /// Unity event function that subscribes to the event fired when mission data changes and selects the correct mission.
    /// </summary>
    public void OnEnable()
    {
        ShipStateManager.OnMissionDataChange += OnMissionDataChange;
        if (vignetteObject)
        {
            vignetteObject.SetActive(true);
        }

        if (ShipStateManager.Instance)
        {
            SetFromMissionData(ShipStateManager.Instance.MissionData);
        }

        if (missionItems != null && missionItems.Length > 0)
        {
            SelectMission(missionItems[lastSelectedIndex]);
        }
    }

    /// <summary>
    /// Unity event function that unsubscribes from the event fired when mission data changes and hides the vignette.
    /// </summary>
    private void OnDisable()
    {
        ShipStateManager.OnMissionDataChange -= OnMissionDataChange;
        if (vignetteObject)
        {
            vignetteObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the object from received mission data.
    /// </summary>
    /// <param name="data">The mission data received.</param>
    private void OnMissionDataChange(List<MissionData> data)
    {
        SetFromMissionData(data);
    }

    /// <summary>
    /// Sets the missions displayed from the mission data.
    /// </summary>
    /// <param name="missionData">The mission data received.</param>
    public void SetFromMissionData(List<MissionData> missionData)
    {
        for (int i = 0; i < missionData.Count; i++)
        {
            if (missionItems.Length > i && missionData[i].visible)
            {
                missionItems[i].gameObject.SetActive(true);
                missionItems[i].SetMissionData(missionData[i]);
            }
            else
            {
                missionItems[i].gameObject.SetActive(false);
            }
        
            if (i == lastSelectedIndex)
            {
                missionDetails.SetMissionData(missionItems[i].VisibleMissionData);
            }
        }
    }

    /// <summary>
    /// Selects a given mission within the UI.
    /// </summary>
    /// <param name="item">The UI piece for a mission.</param>
    public void SelectMission(UIHudMissionItem item)
    {
        var index = Array.IndexOf(missionItems, item);
        lastSelectedIndex = index;

        // Set the status as selected on every item in the array
        for (int i = 0; i < missionItems.Length; i++)
        {
            missionItems[i].SetSelected(i == index);
        }
        missionDetails.SetMissionData(item.VisibleMissionData);
        missionSlider.SetPosition(item);
    }
}

