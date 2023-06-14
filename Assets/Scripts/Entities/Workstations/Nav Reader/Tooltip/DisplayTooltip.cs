using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;

public class DisplayTooltip : Singleton<DisplayTooltip>
{
    [SerializeField]
    private Image tooltipBorderImage;
    [SerializeField]
    private Image tooltipArrowBorderImage;
    [SerializeField]
    private Image rightTooltipArrowBorderImage;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI tooltipText;

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
            tooltipText.text = "Incomplete";
        }
        else if (currentScore < md.baseSolveValue)
        {
            tooltipText.text = "Partially completed";
            setColor = HUDController.Instance.partiallyCompletedHighlightColor;
        }
        else if (currentScore >= md.baseSolveValue)
        {
            tooltipText.text = "Completed";
            setColor = HUDController.Instance.completedHighlightColor;
        }

        tooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        tooltipBorderImage.color = setColor;
        tooltipArrowBorderImage.color = setColor;
        rightTooltipArrowBorderImage.color = setColor;

        titleText.text = md.title;
    }
}
