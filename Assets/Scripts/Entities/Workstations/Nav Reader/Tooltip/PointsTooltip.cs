using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;

/// <summary>
/// A tooltip displayed when the points UI of a system on the galaxy map is hovered over.
/// </summary>
public class PointsTooltip : Singleton<PointsTooltip>
{
    /// <summary>
    /// The image used as the border of the tooltip.
    /// </summary>
    [SerializeField]
    private Image pointsTooltipBorderImage;
    /// <summary>
    /// The image used as the border of the left facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image pointsTooltipArrowBorderImage;
    /// <summary>
    /// The image used as the border of the right facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image rightPointsTooltipArrowBorderImage;
    /// <summary>
    /// The object containing all objects showing possible points. This is only enabled if the challenge is not complete.
    /// </summary>
    [SerializeField]
    private GameObject pointsWrapper;
    /// <summary>
    /// The object containing all objects showing already scored points. This is only enabled if the challenge is complete.
    /// </summary>
    [SerializeField]
    private GameObject scoredWrapper;
    /// <summary>
    /// The text showing the full number of points that can be scored. This only renders if the challenge is not complete.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI fullText;
    /// <summary>
    /// The text showing the number of bonus points that can be scored. This only renders if the challenge is not complete.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI bonusText;
    /// <summary>
    /// The text showing the number of points that have been scored. This only renders if the challenge is complete.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI scoredText;

    /// <summary>
    /// The index of the mission.
    /// </summary>
    [HideInInspector]
    public int index = -1;
    /// <summary>
    /// The rect transform on this tooltip.
    /// </summary>
    [HideInInspector]
    public RectTransform rect;

    /// <summary>
    /// Unity event function that gets the rect transform on this tooltip.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Sets the information and visual state of this tooltip.
    /// </summary>
    /// <param name="index">The index of the mission the system references.</param>
    /// <param name="currentScore">The current score of the mission referenced.</param>
    /// <param name="placeLeft">Whether to place this tooltip to the left of the system.</param>
    public void SetPropertiesFromIndex(int index, int currentScore, bool placeLeft = false)
    {
        this.index = index;
        MissionData md = ShipStateManager.Instance.MissionData[index];
        Color setColor = HUDController.Instance.incompleteHighlightColor;
        if (currentScore == 0)
        {
            pointsWrapper.SetActive(true);
            scoredWrapper.SetActive(false);
        }
        else if (currentScore < md.baseSolveValue)
        {
            pointsWrapper.SetActive(true);
            scoredWrapper.SetActive(false);
            setColor = HUDController.Instance.partiallyCompletedHighlightColor;
        }
        else if (currentScore >= md.baseSolveValue)
        {
            pointsWrapper.SetActive(false);
            scoredWrapper.SetActive(true);
            scoredText.text = $"You have scored {currentScore} PTS";
            setColor = HUDController.Instance.completedHighlightColor;
        }

        pointsTooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightPointsTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        pointsTooltipBorderImage.color = setColor;
        pointsTooltipArrowBorderImage.color = setColor;
        rightPointsTooltipArrowBorderImage.color = setColor;
        fullText.text = $"{md.baseSolveValue} PTS";
        bonusText.text = $"{md.bonusRemaining} PTS";
    }
}