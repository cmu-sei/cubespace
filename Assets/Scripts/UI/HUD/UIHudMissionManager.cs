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
    [SerializeField]
    private IDToImageMap missionIconMap; // b&w hex mission icons
    [SerializeField]
    private Dictionary<string, UIHudMissionItem> missionItems = new Dictionary<string, UIHudMissionItem>();
    [SerializeField]
    private UIHudMissionSlider missionSlider;
    [SerializeField]
    private Transform missionListParent;
    [SerializeField]
    private GameObject missionListingItemPrefab;
    [SerializeField]
    private UIHudMissionDetailsPanel missionDetailsPanel;

    private string lastSelectedId = "";


    public override void Awake()
    {
        base.Awake();

        missionIconMap.InitiateDictionary();
    }

    public void OnEnable()
    {
        ShipStateManager.OnMissionDatasChange += OnMissionDataChange;
    }

    private void OnDisable()
    {
        ShipStateManager.OnMissionDatasChange -= OnMissionDataChange;
    }

    private void OnMissionDataChange(List<MissionData> data)
    {
        SetMissionItemsFromMissionData(data);
        UpdateDetailsForSelectedMission(data);
    }

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
        UIHudMissionItem prevSelectedItem;
        missionItems.TryGetValue(lastSelectedId, out prevSelectedItem);

        if (prevSelectedItem != null && prevSelectedItem.CachedMissionData.visible)
        {
            SelectMission(prevSelectedItem, true);
        }
        // Else default to first item we can find that isn't hidden
        else if (missionItems.Count > 0)
        {
            foreach (UIHudMissionItem item in missionItems.Values)
            {
                if (item.CachedMissionData.visible)
                {
                    SelectMission(item, true);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Sets the missions displayed from the mission data. Called when you open the mission log and when OnMissionsChanged is fired while the log is open
    /// </summary>
    /// <param name="missionData">The mission data received.</param>
    public void SetMissionItemsFromMissionData(List<MissionData> missionData)
    {
        // Check to see if any missions we currently have items for have been removed from the data coming in
        if (missionItems.Count > 0)
        {
            List<string> keysToBeRemoved = new List<string>();

            foreach (string id in missionItems.Keys)
            {
                if (!missionData.Exists((o) => { return o.missionID == id; }))
                {
                    keysToBeRemoved.Add(id);
                }
            }

            foreach (string key in keysToBeRemoved)
            {
                Destroy(missionItems[key].gameObject);
                missionItems.Remove(key);
            }
        }
        
        foreach (MissionData md in missionData)
        {
            if (!missionItems.ContainsKey(md.missionID))
            {
                GameObject missionListing = Instantiate(missionListingItemPrefab, missionListParent);
                missionItems.Add(md.missionID, missionListing.GetComponent<UIHudMissionItem>());
            }

            UIHudMissionItem item;
            missionItems.TryGetValue(md.missionID, out item);
            item.SetMissionData(md);
            item.gameObject.SetActive(md.visible);
        }
    }

    // Updates the details panel for the currently selected mission when new mission data is recieved
    private void UpdateDetailsForSelectedMission(List<MissionData> missionData)
    {
        UIHudMissionItem selectedItem;
        missionItems.TryGetValue(lastSelectedId, out selectedItem);

        if (selectedItem != null && selectedItem.CachedMissionData.visible)
        {
            missionDetailsPanel.SetMissionDetailsData(selectedItem.CachedMissionData); // safe because the items in missionItems all get updated before this is called so the cached mission will be up to date
            SelectMission(selectedItem);
        }
        else if (missionItems.Count > 0)
        {
            // previously selected mission either doesn't exist or is now hidden so choose another random mission to select
            foreach (UIHudMissionItem item in missionItems.Values)
            {
                if (item.CachedMissionData.visible)
                {
                    SelectMission(item, true);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Selects a given mission within the UI.
    /// </summary>
    /// <param name="item">The UI piece for a mission.</param>
    public void SelectMission(UIHudMissionItem item, bool jumpToPosition = false)
    {
        if (!item.CachedMissionData.visible)
        {
            Debug.LogWarning("Tried to select hidden mission");
            return;
        }

        string id = item.CachedMissionData.missionID;
        lastSelectedId = id;

        // Set the status as selected on every item in the array
        foreach (UIHudMissionItem i in missionItems.Values)
        {
            if (i.CachedMissionData.visible)
            {
                i.SetSelected(i.CachedMissionData.missionID == id);
            }
        }
        missionDetailsPanel.SetMissionDetailsData(item.CachedMissionData);

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

    public void SelectMission(string missionID, bool jumpToPosition = false )
    {
        // Even if the mission isn't found in our list we set this ID as the previously selected mission,
        // which handles the case where we try to select a mission before the log has been initialized OnEnable (from a galaxy system for example)
        lastSelectedId = missionID; 

        UIHudMissionItem item;
        missionItems.TryGetValue(missionID, out item);
        if (item != null)
        {
            SelectMission(item, jumpToPosition);
        }
    }
}

