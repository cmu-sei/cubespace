/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using Systems.GameBrain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Systems;

/// <summary>
/// A task list item displayed within the mission log.
/// </summary>
public class UIHudTaskListItem : MonoBehaviour
{
    /// <summary>
    /// The icon to use when a task is complete.
    /// </summary>
    [SerializeField]
    private Image completeIcon;
    /// <summary>
    /// The icon to use when a task is incomplete.
    /// </summary>
    [SerializeField]
    private Image incompleteIcon;
    /// <summary>
    /// The description of a task.
    /// </summary>
    [SerializeField]
    private TMP_Text taskDescription;

    /// <summary>
    /// The button to click next to a task to get additional information.
    /// </summary>
    [SerializeField]
    private Button additionalInfoButton;
    /// <summary>
    /// Audio to use when the additional info button is clicked.
    /// </summary>
    [SerializeField]
    private Audio.ButtonAudio additionalInfoButtonAudio;
    /// <summary>
    /// The icon for when there is additional information.
    /// </summary>
    [SerializeField]
    private Image additionalInfoIcon;
    /// <summary>
    /// The icon for when there is no additional information.
    /// </summary>
    [SerializeField]
    private Image noAdditionalInfoIcon;
    /// <summary>
    /// The button next to a task which plays a video.
    /// </summary>
    [SerializeField]
    private Button videoButton;
    /// <summary>
    /// The icon of the button used to play a video.
    /// </summary>
    [SerializeField]
    private Image videoIcon;
    /// <summary>
    /// The alpha value to use when the task is completed.
    /// </summary>
    [SerializeField, Range(0, 1)]
    private float textAlphaWhenCompleted;
    /// <summary>
    /// The data object used to populate the data of the task.
    /// </summary>
    private TaskData data;
    /// <summary>
    /// The full task list.
    /// </summary>
    private UIHudTaskList taskList;

    /// <summary>
    /// Unity event function that adds listeners to buttons and activates icons.
    /// </summary>
    private void Awake()
    {
        additionalInfoButton.onClick.AddListener(OnAdditionalInfoButtonClick);
        videoButton.onClick.AddListener(OnVideoButtonClick);

        completeIcon.gameObject.SetActive(true);
        incompleteIcon.gameObject.SetActive(true);
        additionalInfoIcon.gameObject.SetActive(true);
        noAdditionalInfoIcon.gameObject.SetActive(true);
        videoIcon.gameObject.SetActive(true);

        taskDescription.text = "";
        completeIcon.enabled = false;
        incompleteIcon.enabled = false;
        additionalInfoIcon.enabled = false;
        noAdditionalInfoIcon.enabled = false;
        additionalInfoButton.enabled = false;
        additionalInfoButtonAudio.enabled = false;
        videoIcon.enabled = false;
        videoButton.enabled = false;
    }

    /// <summary>
    /// Sets the parent task list.
    /// </summary>
    /// <param name="list">The list where the task list item resides.</param>
    public void SetTaskList(UIHudTaskList list)
    {
        taskList = list;
    }

    /// <summary>
    /// Sets up the information of the task list item.
    /// </summary>
    /// <param name="taskData">The task data used to populate the task information.</param>
    public void SetTaskItem(TaskData taskData)
    {
        data = taskData;
        
        completeIcon.enabled = taskData.complete;
        incompleteIcon.enabled = !taskData.complete;

        additionalInfoIcon.enabled = taskData.infoPresent;
        noAdditionalInfoIcon.enabled = !taskData.infoPresent;
        additionalInfoButton.enabled = taskData.infoPresent;
        additionalInfoButtonAudio.enabled = taskData.infoPresent;

        taskDescription.text = taskData.descriptionText;

        if (taskData.complete)
        {
            taskDescription.alpha = textAlphaWhenCompleted;

            // Only show the video button AFTER a player watches the video and completes the task
            videoIcon.enabled = taskData.videoPresent;
            videoButton.enabled = taskData.videoPresent;
        }
        else
        {
            taskDescription.alpha = 1;
            videoIcon.enabled = false;
            videoButton.enabled = false;
        }
    }

    /// <summary>
    /// Selects additional information when the info button is clicked.
    /// </summary>
    public void OnAdditionalInfoButtonClick()
    {
        if (taskList != null && data != null)
        {
            taskList.SelectAdditionalInfo(data);
        }
    }

    /// <summary>
    /// Prepares the web cutscene with the given URL when the video button is clicked.
    /// </summary>
    public void OnVideoButtonClick()
    {
        CutsceneSystem.Instance.PrepareWebCutscene(data.videoURL, true, true);
    }
}

