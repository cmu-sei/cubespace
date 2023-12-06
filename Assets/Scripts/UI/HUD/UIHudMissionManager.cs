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
using System;
using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using UI.HUD;
using UnityEngine;

/// <summary>
/// The component which populates mission UI from the Unity server and is responsible for selecting missions.
/// </summary>
public class UIHudMissionManager : Singleton<UIHudMissionManager>
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
    private List<UIHudMissionItem> missionItems;
    /// <summary>
    /// A reference to the scripted slider for highlighting items.
    /// </summary>
    [SerializeField]
    private UIHudMissionSlider missionSlider;
    /// <summary>
    /// Mission listing prefab to spawn in, in case more items need to be displayed.
    /// </summary>
    [SerializeField]
    private GameObject missionListingItemPrefab;

    /// <summary>
    /// The details of each mission.
    /// </summary>
    [SerializeField]
    private UIHudMissionDetailsPanel missionDetails;

    /// <summary>
    /// Mission list parent object, holding the mission list items.
    /// </summary>
    [SerializeField]
    private Transform missionListParent;

    /// <summary>
    /// The last selected mission's index.
    /// </summary>
    private int lastSelectedIndex = 0;

    /// <summary>
    /// Unity event function that initiates the mission and icon mapping.
    /// </summary>
    public override void Awake()
    {
        base.Awake();

        missionIconMap.InitiateDictionary();
    }

    /// <summary>
    /// Unity event function that subscribes to the event fired when mission data changes and selects the correct mission.
    /// </summary>
    public void OnEnable()
    {
        ShipStateManager.OnMissionDatasChange += OnMissionDataChange;
    }

    /// <summary>
    /// Unity event function that unsubscribes from the event fired when mission data changes and hides the vignette.
    /// </summary>
    private void OnDisable()
    {
        ShipStateManager.OnMissionDatasChange -= OnMissionDataChange;
    }

    /// <summary>
    /// Sets the object from received mission data.
    /// </summary>
    /// <param name="data">The mission data received.</param>
    private void OnMissionDataChange(List<MissionData> data)
    {
        SetMissionItemsFromMissionData(data);
        UpdateDetailsForSelectedMission(data);
    }

    /// <summary>
    /// Called when the mission log is opened to initialize the mission log. Starts a coroutine to wait a frame before initialization
    /// </summary>
    public void OnOpen()
    {
        StartCoroutine(Co_Open());
    }

    // Hack to get around text not getting set properly
    private IEnumerator Co_Open()
    {
        // If you set the text in the details panel without waiting a frame, they don't actually update. Waiting a frame adds a little skip but seems to work reliably
        yield return null;

        SetMissionItemsFromMissionData(ShipStateManager.Instance.MissionDatas);

        // If the last selected mission is still valid, open the log to it
        if (lastSelectedIndex >= 0 && lastSelectedIndex < missionItems.Count && missionItems[lastSelectedIndex].CachedMissionData.visible)
        {
            SelectMission(missionItems[lastSelectedIndex]);
        }
        // Else default to first in the list
        else if (missionItems.Count > 0 && missionItems[0].CachedMissionData.visible)
        {
            SelectMission(missionItems[0]);
        }
        else
        {
            Debug.LogWarning("Mission log failed to open previously selected mission");
        }
    }

    /// <summary>
    /// Sets the missions displayed from the mission data. Called when you open the mission log and when OnMissionsChanged is fired while the log is open
    /// </summary>
    /// <param name="missionData">The mission data received.</param>
    public void SetMissionItemsFromMissionData(List<MissionData> missionData)
    {
        for (int i = 0; i < missionData.Count; i++)
        {
            if (i >= missionItems.Count)
            {
                GameObject missionListing = Instantiate(missionListingItemPrefab, missionListParent);
                missionItems.Add(missionListing.GetComponent<UIHudMissionItem>());
            }

            if (missionData[i].visible)
            {
                missionItems[i].gameObject.SetActive(true);
                missionItems[i].SetMissionData(missionData[i]);
            }
            else
            {
                missionItems[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates the details panel for the currently selected mission when new mission data is recieved
    /// </summary>
    private void UpdateDetailsForSelectedMission(List<MissionData> missionData)
    {
        if (lastSelectedIndex >= 0 && lastSelectedIndex < missionData.Count && missionData[lastSelectedIndex].visible)
        {
            missionDetails.SetMissionDetailsData(missionData[lastSelectedIndex]);
        }
    }

    /// <summary>
    /// Selects a given mission within the UI.
    /// </summary>
    /// <param name="item">The UI piece for a mission.</param>
    public void SelectMission(UIHudMissionItem item, bool jumpToPosition = false)
    {
        var index = Array.IndexOf(missionItems.ToArray(), item);
        lastSelectedIndex = index;

        // Set the status as selected on every item in the array
        for (int i = 0; i < missionItems.Count; i++)
        {
            missionItems[i].SetSelected(i == index);
        }
        missionDetails.SetMissionDetailsData(item.CachedMissionData);
        if (jumpToPosition)
        {
            ScrollToMission(item);
        }

        //update after scrolltomission.
        missionSlider.SetPosition(item, jumpToPosition);
    }

    private void ScrollToMission(UIHudMissionItem item)
    {
        //set missionListParent y position such that the scrollbar updates.
        //the scrollview will clamp/control the y position within bounds and move the scrollbar itself.
        //0 is the top, some positive y value us the bottom.

        var itemTransform = item.transform as RectTransform;
        float offset = itemTransform.anchoredPosition.y;
       
        var missionParent = missionListParent.transform as RectTransform;
        missionParent.anchoredPosition = new Vector3(missionParent.anchoredPosition.x, -offset/2);
    }

    public void SelectMission(int index,bool jumpToPosition =false )
    {
        UIHudMissionItem item = missionItems[index]; // TODO: make this search based on id, not index
        SelectMission(item, jumpToPosition);
    }
}

