using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;

/// <summary>
/// A tooltip displayed when the main UI of a system on the galaxy map is hovered over.
/// </summary>
public class DisplayTooltip : Singleton<DisplayTooltip>
{
    /// <summary>
    /// The image used as the border of the tooltip.
    /// </summary>
    [SerializeField]
    private Image tooltipBorderImage;
    /// <summary>
    /// The image used as the border of the left facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image tooltipArrowBorderImage;
    /// <summary>
    /// The image used as the border of the right facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image rightTooltipArrowBorderImage;
    /// <summary>
    /// The text showing the title of the mission.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI titleText;
    /// <summary>
    /// The text showing the status of the mission. In the future this status text could (and maybe should) be read from the JSON nad inserted here.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI tooltipText;

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
