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
    [SerializeField]
    private Image solveCountInnerBacking;
    /// <summary>
    /// The text object showing the number of teams who have solved the mission.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI solveCountText;

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
    // Affects width of highlight circle on systems
    [SerializeField] 
    private Image backingImageMask;

    /// <summary>
    /// The text object showing the point value.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI pointsText;

    [Header("Configuration Variables")]
    /// <summary>
    /// The image map with IDs corresponding to system images.
    /// </summary>
    [SerializeField]
    private IDToImageMap imageMap; // non-hex mission icons
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
    /// The mission associated with this system, cached when updated mission data is sent from the ShipStateManager
    /// </summary>
    [HideInInspector]
    public MissionData missionData = null;
    
    // Used for highlighting cache complete missions
    [SerializeField] private Sprite goldBorderSprite;
    [SerializeField] private Sprite defaultBorderSprite;

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
                // Disable any tooltips that might be open
                if (DisplayTooltip.Instance) DisplayTooltip.Instance.gameObject.SetActive(false);
                if (PointsTooltip.Instance) PointsTooltip.Instance.gameObject.SetActive(false);
                if (SolveCountTooltip.Instance) SolveCountTooltip.Instance.gameObject.SetActive(false);

                // Open mission log and select the mission that correlates to this system
                HUDController.Instance.OpenMissionLog();
                UIHudMissionManager.Instance.SelectMission(missionData.missionID, true);

                // Break out of loop and stop the coroutine
                break;
            }
            yield return null;
        }
    }

    public void InitializeSystem(MissionData md, GameObject line, GameObject target)
    {
        lineObj = line;
        targetObj = target;
        lineImage = line.transform.GetComponent<Image>();
        targetImage = target.transform.GetChild(0).GetComponent<Image>();

        UpdateSystem(md);
    }

    /// <summary>
    /// Sets the information of the system based on the mission provided and links it to a mission's place in the Mission Log. Then updates the visual state based on completion.
    /// </summary>
    /// <param name="md">The mission data to populate this system with.</param>
    /// <param name="line">The line drawn between the system and its target point.</param>
    /// <param name="target">The target point.</param>
    public void UpdateSystem(MissionData md)
    {
        if (lineObj == null || targetObj == null)
        {
            Debug.LogError("Tried to update a galaxy map system without initializing it first!");
            return;
        }
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
            spriteImage.sprite = imageMap.GetImage(missionData.missionIcon, false);

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
        if (m.missionID == "mission_a")
        {
            Debug.Log("Updating visuals for mission_a!");
        }

        // If incomplete and not started
        if (!m.complete && m.currentScore == 0)
        {
            SetHighlightColors(defaultBorderSprite, 1.0f, ColorPalette.GetColor(PaletteColor.incompleteHighlight), Color.white, Color.black, Color.white, Color.black);
        }
        // If partially solved
        else if (!m.complete && m.currentScore > 0)
        {
            SetHighlightColors(defaultBorderSprite, 1.0f, ColorPalette.GetColor(PaletteColor.partiallyCompletedHighlight), Color.white, Color.black, Color.white, Color.black);
            if (m.missionID == "mission_a")
            {
                Debug.Log("mission_a is marked as partially solved (!m.complete && m.currentScore > 0)");
            }
        }
        // If fully solved
        else if (m.complete)
        {
            if (IsMissionCacheComplete(m))
            {
                SetHighlightColors(goldBorderSprite, 0.95f, ColorPalette.GetColor(PaletteColor.cacheCompleteHighlight), Color.black, Color.white, Color.black, Color.white, goldBorderSprite);

                //repeat for 3 tooltips
                // Set line image to gold, color to white
                // Set target point to gold image
                if (m.missionID == "mission_a")
                {
                    Debug.Log("mission_a is marked as cache complete");
                }
            }
            else
            {
                SetHighlightColors(defaultBorderSprite, 1.0f, ColorPalette.GetColor(PaletteColor.completedHighlight), Color.black, Color.white, Color.black, Color.white);
                if (m.missionID == "mission_a")
                {
                    Debug.Log("mission_a is marked as normal complete");
                }
            }
        }

        // Flip the display, points, and solve count tooltips if specified
        bool flip = GetComponent<RectTransform>().localPosition.x > flipTooltipXThreshold;
        // Check if the tooltips are being displayed for this mission, if so update those tooltips with this new data
        if (!string.IsNullOrEmpty(DisplayTooltip.Instance.id) && DisplayTooltip.Instance.id == m.missionID)
        {
            DisplayTooltip.Instance.SetProperties(m, flip);
        }
        if (!string.IsNullOrEmpty(PointsTooltip.Instance.id) && PointsTooltip.Instance.id == m.missionID)
        {
            PointsTooltip.Instance.SetProperties(m, flip);
        }
        if (!string.IsNullOrEmpty(SolveCountTooltip.Instance.id) && SolveCountTooltip.Instance.id == m.missionID)
        {
            SolveCountTooltip.Instance.SetProperties(m, flip);
        }
    }

    private void SetHighlightColors(Sprite borderSprite, float backingScale, Color highlightCol, Color pointsBackingCol, Color pointsTextCol, Color solveCountBackingCol, Color solveCountTextCol, Sprite lineSprite = null)
    {
        lineImage.color = highlightCol;
        targetImage.color = highlightCol;

        displayBorderImage.sprite = borderSprite;
        displayBorderImage.color = highlightCol;

        solveCountInnerBacking.color = solveCountBackingCol;
        solveCountText.color = solveCountTextCol;
        solveCountBorderImage.sprite = borderSprite;
        solveCountBorderImage.color = highlightCol;

        pointsInnerBacking.color = pointsBackingCol;
        pointsText.color = pointsTextCol;
        pointsBorderImage.sprite = borderSprite;
        pointsBorderImage.color = highlightCol;

        backingImageMask.transform.localScale = new Vector2(backingScale, backingScale);

        lineImage.sprite = lineSprite;
        lineImage.color = highlightCol;
        targetImage.sprite = borderSprite;
        targetImage.color = highlightCol;
    }

    private bool IsMissionCacheComplete(MissionData mission)
    {
        bool cacheComplete = true;

        // Check to see if all associated missions have been completed
        foreach (AssociatedChallengeData associatedChallenge in mission.associatedChallenges)
        {
            foreach (MissionData associatedMission in ShipStateManager.Instance.MissionDatas)
            {
                if (associatedMission.missionID == associatedChallenge.missionID)
                {
                    cacheComplete &= associatedMission.complete;
                }
            }
        }
        return cacheComplete;
    }

    // Sets the position of the system, line, and target on the galaxy map
    private void UpdatePosition(MissionData md)
    {
        //Allowable x range: [-540, 540]. Allowable y range: [-320, 320]. Give each a circle with diameter 125 to avoid overlap
        GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapXPos, md.galaxyMapYPos);
        targetObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapTargetXPos, md.galaxyMapTargetYPos);

        // Get RectTransform references
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
