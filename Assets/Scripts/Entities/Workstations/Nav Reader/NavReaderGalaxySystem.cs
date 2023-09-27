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
    /// The Image component of the line connecting this system to the target point.
    /// </summary>
    private Image lineImage;
    /// <summary>
    /// The Image component of the target point.
    /// </summary>
    private Image targetImage;

    /// <summary>
    /// The index of the mission associated with this system.
    /// </summary>
    [HideInInspector]
    public int index = -1;

    [SerializeField] private ColorPalette _palette;


    /// <summary>
    /// Unity event function that simply updates the visual state every frame.
    /// </summary>
    void Update()
    {
        // TODO: This is bad and unnecessary. Subscribe to event that fires when mission data changes instead of doing this. Low priority since it only runs when the maps open but still
        UpdateVisualState();
    }

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
                UIHudMissionManager.Instance.SelectMission(index,true);
                // HUDController.Instance.MissionLogButton.OnClick();//I thiiiiink this was to match the visual button state to the menu open state? but I fixed that disconnect, now HUDController manages state in one place - Smokey
            }

            yield return null;
        }
    }

    /// <summary>
    /// Updates the visual state of this system based on its completion state.
    /// </summary>
    public void UpdateVisualState()
    {
        Color setColor = _palette.incompleteHighlightColor;

        if (ShipStateManager.Instance && index >= 0 && index < ShipStateManager.Instance.MissionData.Count)
        {
            MissionData mission = ShipStateManager.Instance.MissionData[index];
            int currentScore = mission.currentScore;

            // If incomplete and not started
            if (!mission.complete && currentScore == 0)
            {
                pointsInnerBacking.color = Color.white;
                pointsText.color = Color.black;
            }
            // If partially solved
            else if (!mission.complete && currentScore > 0)
            {
                pointsInnerBacking.color = Color.white;
                pointsText.color = Color.black;
                setColor = _palette.partiallyCompletedHighlightColor;
            }
            // If fully solved
            else if (mission.complete)
            {
                // Check to see if all associated missions have been completed
                bool cacheComplete = true;
                foreach (AssociatedChallengeData associatedChallenge in ShipStateManager.Instance.MissionData[index].associatedChallenges)
                {
                    foreach (MissionData associatedMission in ShipStateManager.Instance.MissionData)
                    {
                        if (associatedMission.missionID == associatedChallenge.missionID)
                        {
                            cacheComplete &= associatedMission.complete;
                        }
                    }
                }

                if (cacheComplete)
                {
                    pointsInnerBacking.color = Color.magenta;
                    pointsText.color = Color.white;
                    setColor = Color.blue;
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

            if (DisplayTooltip.Instance.index == index)
            {
                DisplayTooltip.Instance.SetPropertiesFromIndex(index, flip);
            }

            if (PointsTooltip.Instance.index == index)
            {
                PointsTooltip.Instance.SetPropertiesFromIndex(index, flip);
            }

            if (SolveCountTooltip.Instance.index == index)
            {
                SolveCountTooltip.Instance.SetPropertiesFromIndex(index, flip);
            }
        }

        // Update colors
        lineImage.color = setColor;
        targetImage.color = setColor;
        displayBorderImage.color = setColor;
        solveCountBorderImage.color = setColor;
        pointsBorderImage.color = setColor;
    }

    /// <summary>
    /// Sets the information of the system based on the mission provided and links it to a mission's place in the Mission Log.
    /// </summary>
    /// <param name="md">The mission data to populate this system with.</param>
    /// <param name="index">The index of the mission in the mission log.</param>
    /// <param name="line">The line drawn between the system and its target point.</param>
    /// <param name="target">The target point.</param>
    public void SetSystemMission(MissionData md, int index, Image line = null, Image target = null)
    {
        if (line && target)
        {
            lineImage = line;
            targetImage = target;
        }

        int currentScore = md.currentScore;

        // Display the correct score according to what was received
        if (!md.complete)
        {
            //For missions that have been started, but not finished, display this score.
            pointsText.text = (md.baseSolveValue + md.bonusRemaining).ToString();
        }
        else
        {
            //for completed missions, show the score we got.
            pointsText.text = currentScore.ToString();
        }

        // Update the solve count and image
        solveCountText.text = $"{md.solveTeams}/{md.totalTeams}";
        spriteImage.sprite = imageMap.GetImage(md.missionIcon);

        this.index = index;

        ToggleState(md.visible);
    }

    /// <summary>
    /// Sets whether this mission is visible.
    /// </summary>
    /// <param name="isVisible">Whether this mission is visible.</param>
    public void ToggleState(bool isVisible)
    {
        gameObject.SetActive(isVisible);
        if (lineImage && targetImage)
        {
            lineImage.gameObject.SetActive(isVisible);
            targetImage.gameObject.SetActive(isVisible);
        }
    }
}
