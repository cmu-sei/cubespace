/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Systems.GameBrain;
using TMPro;
using UI.HUD;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The list of tasks in the mission log.
/// </summary>
public class UIHudTaskList : MonoBehaviour
{
    /// <summary>
    /// The prefab used to populate the task items within the list.
    /// </summary>
    [SerializeField]
    private UIHudTaskListItem taskItemPrefab;
    /// <summary>
    /// The larger parent object of a task item.
    /// </summary>
    [SerializeField]
    private Transform taskItemParent;
    /// <summary>
    /// The additional information text displayed.
    /// </summary>
    [SerializeField]
    private TMP_Text additionalInfoText;
    /// <summary>
    /// The additional information panel.
    /// </summary>
    [SerializeField]
    private GameObject additionalInfoPanel;
    /// <summary>
    /// Text displayed in additional info box for a complete task
    /// </summary>
    [SerializeField]
    private string additionalInfoTaskCompleteMessage = "This task has been completed!";

    /// <summary>
    /// The task list viewable by the player.
    /// </summary>
    private TaskData[] visibleTaskList;
    /// <summary>
    /// The additional info for the task.
    /// </summary>
    private TaskData additionalInfoTask = null;

    /// <summary>
    /// The button which closes the additional info display.
    /// </summary>
    [SerializeField]
    private Button additionalInfoCloseButton;

    /// <summary>
    /// Unity event function that adds a listener function to the info close button to close the additional info panel.
    /// </summary>
    private void Awake()
    {
        additionalInfoCloseButton.onClick.AddListener(CloseAdditionalInfo);
        CloseAdditionalInfo();
        additionalInfoText.text = "";

        // This should be handled by the mission log panel but just disabling the panel everytime the menu state changes works well enough
        HUDController.Instance.OnMenuStateChange += (_) => { CloseAdditionalInfo(); }; // Disables itself anytime the menu state changes
    }

    /// <summary>
    /// Sets the task list from the task list array provided.
    /// </summary>
    /// <param name="taskList">The list of tasks for a mission.</param>
    public void SetTaskList(TaskData[] taskList)
    {
        // Determine if the task list actually changed any since the last poll
        bool needsChanged = false;
        if (visibleTaskList == null || taskList == null || taskList.Length != visibleTaskList.Length)
        {
            needsChanged = true;
        }
        else
        {
            for (int i = 0; i < taskList.Length; i++)
            {
                // Check to make sure it's the same task, if a new video is available for the task, if it's been completed, or if new additional info is available
                TaskData curTask = visibleTaskList[i];
                TaskData newTask = taskList[i];
                if (newTask.taskID != curTask.taskID || newTask.videoPresent != curTask.videoPresent || newTask.complete != curTask.complete || newTask.infoPresent != curTask.infoPresent)
                {
                    needsChanged = true;
                }
            }
        }
        // Don't need to do anything if the task list hasn't changed
        if (!needsChanged)
        {
            return;
        }

        foreach (Transform child in taskItemParent)
        {
            Destroy(child.gameObject);
        }
        if (taskList != null)
        {
            for (int i = 0; i < taskList.Length; i++)
            {
                var item = Instantiate(taskItemPrefab, taskItemParent);
                item.SetTaskList(this);//DI
                item.SetTaskItem(taskList[i]);
            }
        }
        visibleTaskList = taskList;

        // Close the additional info window if it was open for a task from a previous task list
        if (additionalInfoTask != null)
        {
            CloseAdditionalInfo();
        }
    }

    /// <summary>
    /// Turns on the additional info panel.
    /// </summary>
    private void OpenAdditionalInfo()
    {
        additionalInfoPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the additional info panel.
    /// </summary>
    public void CloseAdditionalInfo()
    {
        additionalInfoPanel.SetActive(false);
        additionalInfoTask = null;
    }

    /// <summary>
    /// Displays or hides additional info based on the task data.
    /// </summary>
    /// <param name="data">The TaskData used in the additional info panel.</param>
    public void SelectAdditionalInfo(TaskData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Tried to open additional info panel with null data!");
            CloseAdditionalInfo();
            return;
        }

        // We're already showing additional info and it is the same as what we're being told to show, so player clicked the additional info button twice for the same task and we'll close it for them
        if (additionalInfoTask != null && data.taskID == additionalInfoTask.taskID && data.infoText == additionalInfoTask.infoText)
        {
            CloseAdditionalInfo();
        }
        else
        {
            OpenAdditionalInfo();
            additionalInfoTask = data;
            if (data.complete)
            {
                // If the task is marked as complete, ignore whatever gamebrain is saying and display this message
                // Ideally gamebrain would handle this to remove the edge case and ensure that cubespace is respecting whatever gamebrain says
                additionalInfoText.text = additionalInfoTaskCompleteMessage;
            }
            else
            {
                additionalInfoText.text = data.infoText;
            }
        }
    }
}

