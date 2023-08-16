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
        ShipStateManager.OnMissionDataChange += OnMissionDataChange;

        if (ShipStateManager.Instance)
        {
            SetFromMissionData(ShipStateManager.Instance.MissionData);
        }

        if (missionItems != null && missionItems.Count > 0)
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
            if (i >= missionItems.Count)
            {
                missionItems.Add(Instantiate(missionListingItemPrefab, missionListParent).GetComponent<UIHudMissionItem>());
            }

            if (missionData.Count > i && missionData[i].visible)
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
    public void SelectMission(UIHudMissionItem item, bool jumpToPosition = false)
    {
        var index = Array.IndexOf(missionItems.ToArray(), item);
        lastSelectedIndex = index;

        // Set the status as selected on every item in the array
        for (int i = 0; i < missionItems.Count; i++)
        {
            missionItems[i].SetSelected(i == index);
        }
        missionDetails.SetMissionData(item.VisibleMissionData);
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
        UIHudMissionItem item = missionItems[index];
        SelectMission(item, jumpToPosition);
    }
    
}

