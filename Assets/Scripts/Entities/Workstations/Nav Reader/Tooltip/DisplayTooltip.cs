using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;
using Systems.GameBrain;
using UI.ColorPalettes;
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

    [SerializeField] private ColorPalette _palette;


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
        MissionData mission = ShipStateManager.Instance.MissionData[index];
        Color setColor = _palette.incompleteHighlightColor;
        if (!mission.complete && mission.currentScore == 0)
        {
            tooltipText.text = "Incomplete";
        }
        else if (!mission.complete && mission.currentScore > 0)
        {
            tooltipText.text = "Partially completed";
            setColor = _palette.partiallyCompletedHighlightColor;
        }
        else if (mission.complete)
        {
            tooltipText.text = "Completed";
            setColor = _palette.completedHighlightColor;

            // Display coordinates for cache missions once main mission is complete
            if (mission.associatedChallenges != null && mission.associatedChallenges.Length > 0)
            {
                if (mission.associatedChallenges.Length == 1 && mission.associatedChallenges[0].unlockCode != null && mission.associatedChallenges[0].unlockCode != "")
                {
                    tooltipText.text = "Cache at: " + mission.associatedChallenges[0].unlockCode;
                }
                else
                {
                    bool foundNonNullCoord = false;
                    
                    for (int i = 0; i < mission.associatedChallenges.Length; i++)
                    {
                        if (mission.associatedChallenges[i].unlockCode != null && mission.associatedChallenges[i].unlockCode != "")
                        {
                            if (!foundNonNullCoord)
                            {
                                tooltipText.text = "Caches at: " + mission.associatedChallenges[i].unlockCode;
                                foundNonNullCoord = true;
                            }
                            else
                            {
                                tooltipText.text += ", " + mission.associatedChallenges[i].unlockCode;
                            }
                        }
                    }
                }
            }
        }

        tooltipArrowBorderImage.gameObject.SetActive(!placeLeft);
        rightTooltipArrowBorderImage.gameObject.SetActive(placeLeft);

        tooltipBorderImage.color = setColor;
        tooltipArrowBorderImage.color = setColor;
        rightTooltipArrowBorderImage.color = setColor;

        titleText.text = mission.title;
    }
}
