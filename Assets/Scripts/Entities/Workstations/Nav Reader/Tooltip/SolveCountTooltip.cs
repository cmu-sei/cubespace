using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.HUD;

public class SolveCountTooltip : Singleton<SolveCountTooltip>
{
    [SerializeField]
    private Image solveTooltipBorderImage;
    [SerializeField]
    private Image solveTooltipArrowBorderImage;
    [SerializeField]
    private Image rightSolveTooltipArrowBorderImage;
    [SerializeField]
    private TextMeshProUGUI teamsCompletedText;

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
    public void SetPropertiesFromIndex(int index, int currentScore, bool placeLeft=false)
    {
        this.index = index;
        MissionData md = ShipStateManager.Instance.MissionData[index];
        Color setColor = HUDController.Instance.incompleteHighlightColor;
        if (currentScore == 0)
        {
            // Nothing here
        }
        else if (currentScore < md.baseSolveValue)
        {
            setColor = HUDController.Instance.partiallyCompletedHighlightColor;
        }
        else if (currentScore >= md.baseSolveValue)
        {
            setColor = HUDController.Instance.completedHighlightColor;
        }

        solveTooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightSolveTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        teamsCompletedText.text = $"{md.solveTeams} team{(md.solveTeams == 1 ? " has" : "s have")} solved this challenge";
        solveTooltipBorderImage.color = setColor;
        solveTooltipArrowBorderImage.color = setColor;
        rightSolveTooltipArrowBorderImage.color = setColor;
    }
}
