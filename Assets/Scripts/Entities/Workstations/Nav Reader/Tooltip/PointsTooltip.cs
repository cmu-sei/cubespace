using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;
using System.Reflection;
using UI.ColorPalettes;

/// <summary>
/// A tooltip displayed when the points UI of a system on the galaxy map is hovered over.
/// </summary>
public class PointsTooltip : Singleton<PointsTooltip>
{
    [SerializeField] private ColorPalette _palette;
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
    /// Text showing the number of points that the player currently has.
    /// </summary>
    [SerializeField] private TextMeshProUGUI currentText;

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
        rect = transform as RectTransform;
    }

    /// <summary>
    /// Sets the information and visual state of this tooltip.
    /// </summary>
    /// <param name="index">The index of the mission the system references.</param>
    /// <param name="placeLeft">Whether to place this tooltip to the left of the system.</param>
    public void SetPropertiesFromIndex(int index, bool placeLeft = false)
    {
        this.index = index;
        MissionData mission = ShipStateManager.Instance.missionDatas[index];
        Color setColor = _palette.incompleteHighlightColor;
        if (!mission.complete && mission.currentScore == 0)
        {
            pointsWrapper.SetActive(true);
            scoredWrapper.SetActive(false);
        }
        else if (!mission.complete && mission.currentScore > 0)
        {
            pointsWrapper.SetActive(true);
            scoredWrapper.SetActive(false);
            setColor = _palette.partiallyCompletedHighlightColor;
        }
        else if (mission.complete)
        {
            pointsWrapper.SetActive(false);
            scoredWrapper.SetActive(true);
            scoredText.text = $"You have scored {mission.currentScore} PTS";
            setColor = _palette.completedHighlightColor;
        }

        pointsTooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightPointsTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        pointsTooltipBorderImage.color = setColor;
        pointsTooltipArrowBorderImage.color = setColor;
        rightPointsTooltipArrowBorderImage.color = setColor;
        fullText.text = $"{mission.baseSolveValue} PTS";
        bonusText.text = $"{mission.bonusRemaining} PTS";
        currentText.text = $"{mission.currentScore} PTS";
    }
}