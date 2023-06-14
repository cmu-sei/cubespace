using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;

public class PointsTooltip : Singleton<PointsTooltip>
{
    [SerializeField]
    private Image pointsTooltipBorderImage;
    [SerializeField]
    private Image pointsTooltipArrowBorderImage;
    [SerializeField]
    private Image rightPointsTooltipArrowBorderImage;
    [SerializeField]
    private GameObject pointsWrapper;
    [SerializeField]
    private GameObject scoredWrapper;
    [SerializeField]
    private TextMeshProUGUI fullText;
    [SerializeField]
    private TextMeshProUGUI bonusText;
    [SerializeField]
    private TextMeshProUGUI scoredText;

    public int index = -1;
    [HideInInspector]
    public RectTransform rect;

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