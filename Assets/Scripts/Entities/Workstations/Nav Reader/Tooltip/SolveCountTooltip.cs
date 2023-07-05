using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;
using System.Reflection;

/// <summary>
/// A tooltip displayed when the solve count UI of a system on the galaxy map is hovered over.
/// </summary>
public class SolveCountTooltip : Singleton<SolveCountTooltip>
{
    /// <summary>
    /// The image used as the border of the tooltip.
    /// </summary>
    [SerializeField]
    private Image solveTooltipBorderImage;
    /// <summary>
    /// The image used as the border of the left facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image solveTooltipArrowBorderImage;
    /// <summary>
    /// The image used as the border of the right facing arrow on the tooltip.
    /// </summary>
    [SerializeField]
    private Image rightSolveTooltipArrowBorderImage;
    /// <summary>
    /// The text used to display the number of teams who have solved the system's mission.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI teamsCompletedText;

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
    public void SetPropertiesFromIndex(int index, bool placeLeft=false)
    {
        this.index = index;
        MissionData mission = ShipStateManager.Instance.MissionData[index];
        Color setColor = HUDController.Instance.incompleteHighlightColor;
        if (!mission.complete && mission.currentScore == 0)
        {
            // Nothing here
        }
        else if (!mission.complete && mission.currentScore > 0)
        {
            setColor = HUDController.Instance.partiallyCompletedHighlightColor;
        }
        else if (mission.complete)
        {
            setColor = HUDController.Instance.completedHighlightColor;
        }

        solveTooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightSolveTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        teamsCompletedText.text = $"{mission.solveTeams} team{(mission.solveTeams == 1 ? " has" : "s have")} solved this challenge";
        solveTooltipBorderImage.color = setColor;
        solveTooltipArrowBorderImage.color = setColor;
        rightSolveTooltipArrowBorderImage.color = setColor;
    }
}
