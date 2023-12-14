using Managers;
using System.Collections;
using Systems.GameBrain;
using TMPro;
using UI.ColorPalettes;
using UI.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A system displayed on the galaxy map.
/// </summary>
public class NavReaderGalaxySystem : TooltipControl
{
    /// <summary>
    /// The largest display rect of the system.
    /// </summary>
    [Header("Core Display References")]
    [SerializeField]
    private RectTransform coreDisplayRect;
    public RectTransform CoreDisplayRect => coreDisplayRect;
    /// <summary>
    /// The image acting as a border for this object.
    /// </summary>
    [SerializeField]
    private Image displayBorderImage;
    /// <summary>
    /// The image representing this system's icon.
    /// </summary>
    [SerializeField]
    private Image spriteImage;

    /// <summary>
    /// The image acting as a border on the system's solve count rect.
    /// </summary>
    [Header("Solve Count References")]
    [SerializeField]
    private Image solveCountBorderImage;

    /// <summary>
    /// The image acting as a border on the system's points rect.
    /// </summary>
    [Header("Points References")]
    [SerializeField]
    private Image pointsBorderImage;
    /// <summary>
    /// The image acting as a border on the system's points rect.
    /// </summary>
    [SerializeField]
    private Image pointsInnerBacking;

    /// <summary>
    /// The text object showing the point value.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI pointsText;
    /// <summary>
    /// The text object showing the number of teams who have solved the mission.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI solveCountText;

    [Header("Configuration Variables")]
    /// <summary>
    /// The image map with IDs corresponding to system images.
    /// </summary>
    [SerializeField]
    private IDToImageMap imageMap;
    /// <summary>
    /// The x coordinate where tooltips should flip to the other side.
    /// </summary>
    public float flipTooltipXThreshold = 200.0f;
    /// <summary>
    /// The horizontal distance tooltips should be from the system.
    /// </summary>
    public float tooltipHorizontalBuffer = 40.0f;

    /// <summary>
    /// The line connecting this system to the target point.
    /// </summary>
    private GameObject lineObj;
    private Image lineImage;
    /// <summary>
    /// The Image component of the target point.
    /// </summary>
    private GameObject targetObj;
    private Image targetImage;

    /// <summary>
    /// The index of the mission associated with this system.
    /// </summary>
    [HideInInspector]
    public int index = -1; // TODO: This is only used for selection missions. Should rework all that to use IDs instead and get rid of all referencing to missions by index which is fragile AF
    /// <summary>
    /// The mission associated with this system, cached when updated mission data is sent from the ShipStateManager
    /// </summary>
    [HideInInspector]
    public MissionData missionData = null;

    [SerializeField] private ColorPalette _palette;

    /// <summary>
    /// Unity event function that starts a coroutine when the mouse enters this object.
    /// </summary>
    /// <param name="eventData">The mouse enter data.</param>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        StartCoroutine("WaitForInput");
    }

    /// <summary>
    /// Unity event function that starts a coroutine when the mouse exits this object.
    /// </summary>
    /// <param name="eventData">The mouse exit data.</param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        StopCoroutine("WaitForInput");
    }

    /// <summary>
    /// Watches for input from the player.
    /// </summary>
    /// <returns>A yield statement while waiting for a player to click the system.</returns>
    private IEnumerator WaitForInput()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HUDController.Instance.OpenMissionLog();
                UIHudMissionManager.Instance.SelectMission(index, true); // TODO: Just pass along missionID and get rid of the index altogether
                // HUDController.Instance.MissionLogButton.OnClick();//I thiiiiink this was to match the visual button state to the menu open state? but I fixed that disconnect, now HUDController manages state in one place - Smokey
            }

            yield return null;
        }
    }

    public void InitializeSystem(MissionData md, int missionIndex, GameObject line, GameObject target)
    {
        lineObj = line;
        targetObj = target;
        lineImage = line.transform.GetComponent<Image>();
        targetImage = target.transform.GetChild(0).GetComponent<Image>();

        UpdateSystem(md, missionIndex);
    }

    /// <summary>
    /// Sets the information of the system based on the mission provided and links it to a mission's place in the Mission Log. Then updates the visual state based on completion.
    /// </summary>
    /// <param name="md">The mission data to populate this system with.</param>
    /// <param name="index">The index of the mission in the mission log.</param>
    /// <param name="line">The line drawn between the system and its target point.</param>
    /// <param name="target">The target point.</param>
    public void UpdateSystem(MissionData md, int missionIndex)
    {
        if (lineObj == null || targetObj == null)
        {
            Debug.LogError("Tried to update a galaxy map system without initializing it first!");
            return;
        }

        index = missionIndex;
        missionData = md;

        ToggleState(missionData.visible);
        if (missionData.visible)
        {
            // Display the correct score according to what was received
            if (!missionData.complete)
            {
                //For missions that have been started, but not finished, display this score.
                pointsText.text = (missionData.baseSolveValue + missionData.bonusRemaining).ToString();
            }
            else
            {
                //for completed missions, show the score we got.
                pointsText.text = missionData.currentScore.ToString();
            }

            // Update the solve count and image
            solveCountText.text = $"{missionData.solveTeams}/{missionData.totalTeams}";
            spriteImage.sprite = imageMap.GetImage(missionData.missionIcon);

            UpdateVisualState(missionData);
            UpdatePosition(missionData);
        }
    }

    /// <summary>
    /// Sets whether this mission is visible.
    /// </summary>
    /// <param name="isVisible">Whether this mission is visible.</param>
    private void ToggleState(bool isVisible)
    {
        gameObject.SetActive(isVisible);
        if (lineImage && targetImage)
        {
            lineObj.SetActive(isVisible);
            targetObj.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Updates the visual state of this system based on its completion state.
    /// </summary>
    private void UpdateVisualState(MissionData m)
    {
        Color setColor = _palette.incompleteHighlightColor;

        int currentScore = m.currentScore;

        // If incomplete and not started
        if (!m.complete && currentScore == 0)
        {
            pointsInnerBacking.color = Color.white;
            pointsText.color = Color.black;
        }
        // If partially solved
        else if (!m.complete && currentScore > 0)
        {
            pointsInnerBacking.color = Color.white;
            pointsText.color = Color.black;
            setColor = _palette.partiallyCompletedHighlightColor;
        }
        // If fully solved
        else if (m.complete)
        {
            // Check to see if all associated missions have been completed
            bool cacheComplete = true;
            foreach (AssociatedChallengeData associatedChallenge in m.associatedChallenges)
            {
                foreach (MissionData associatedMission in ShipStateManager.Instance.MissionDatas)
                {
                    if (associatedMission.missionID == associatedChallenge.missionID)
                    {
                        cacheComplete &= associatedMission.complete;
                    }
                }
            }

            if (cacheComplete)
            {
                pointsInnerBacking.color = Color.black;
                pointsText.color = Color.white;
                setColor = _palette.cacheCompleteHighlightColor;
                //scale backing of core display down to 0.96x0.96
                //set image of border to goldMetal
                //set color of border to white
                // repeat for two other circles
                //repeat for 3 tooltips
            }
            else
            {
                pointsInnerBacking.color = Color.black;
                pointsText.color = Color.white;
                setColor = _palette.completedHighlightColor;
            }
        }

        // Flip the display, points, and solve count tooltips if specified
        bool flip = GetComponent<RectTransform>().localPosition.x > flipTooltipXThreshold;

        // Check if the tooltips are being displayed for this mission, if so update those tooltips with this new data
        if (DisplayTooltip.Instance.id == m.missionID)
        {
            DisplayTooltip.Instance.SetProperties(m, flip);
        }

        if (PointsTooltip.Instance.id == m.missionID)
        {
            PointsTooltip.Instance.SetProperties(m, flip);
        }

        if (SolveCountTooltip.Instance.id == m.missionID)
        {
            SolveCountTooltip.Instance.SetProperties(m, flip);
        }

        // Update colors
        lineImage.color = setColor;
        targetImage.color = setColor;
        displayBorderImage.color = setColor;
        solveCountBorderImage.color = setColor;
        pointsBorderImage.color = setColor;
    }

    // Sets the position of the system, line, and target on the galaxy map
    private void UpdatePosition(MissionData md)
    {
        //Allowable x range: [-540, 540]. Allowable y range: [-320, 320]. Give each a circle with diameter 125 to avoid overlap
        GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapXPos, md.galaxyMapYPos);
        targetObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapTargetXPos, md.galaxyMapTargetYPos);

        // Get TectTransform references
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        RectTransform coreDisplayTransform = CoreDisplayRect;
        RectTransform targetRectTransform = targetObj.GetComponent<RectTransform>();

        // Get the positions of the RectTransforms
        Vector2 coreDisplayPosition = coreDisplayTransform.position;
        Vector2 targetPosition = targetRectTransform.position;
        Vector2 coreDisplayLocalPosition = coreDisplayTransform.parent.localPosition - coreDisplayTransform.localPosition;
        Vector2 targetLocalPosition = targetRectTransform.localPosition - targetRectTransform.parent.localPosition;

        // Calculate the distance between the system
        Vector2 midpoint = (coreDisplayPosition + targetPosition) / 2;
        float distance = Vector2.Distance(coreDisplayLocalPosition, targetLocalPosition);

        // Draw the line between the system and its target
        lineRect.position = midpoint;
        lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, distance);
        float z = 90 + Mathf.Atan2(targetPosition.y - coreDisplayPosition.y, targetPosition.x - coreDisplayPosition.x) * 180 / Mathf.PI;
        lineRect.rotation = Quaternion.Euler(0, 0, z);
    }

    public void Delete()
    {
        Destroy(lineObj);
        Destroy(targetObj);
        Destroy(gameObject);
    }
}
