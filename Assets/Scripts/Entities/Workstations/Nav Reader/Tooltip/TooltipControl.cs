using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the behavior of something which should display a tooltip when moused over.
/// </summary>
public class TooltipControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// The tooltip type to display when this is hovered over.
    /// </summary>
    [SerializeField]
    private TooltipType tooltipType;
    /// <summary>
    /// The system to show the tooltip beside.
    /// </summary>
    [SerializeField]
    private NavReaderGalaxySystem system;
    /// <summary>
    /// The RectTransform of the above system.
    /// </summary>
    [SerializeField]
    private RectTransform systemRect;

    /// <summary>
    /// Unity event function that deactivates this object's tooltip based on the set TooltipType.
    /// </summary>
    public virtual void Awake()
    {
        if (tooltipType == TooltipType.Display)
        {
            DisplayTooltip.Instance.gameObject.SetActive(false);
        }
        else if (tooltipType == TooltipType.SolveCount)
        {
            //SolveCountTooltip.Instance.gameObject.SetActive(false);
        }
        else
        {
            PointsTooltip.Instance.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Unity event function that sets the position of this object's tooltip when the mouse enters this object.
    /// </summary>
    /// <param name="eventData">The mouse enter data.</param>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // Store an initial tooltip rect and establish a baseline final position for the tooltip
        RectTransform rect = DisplayTooltip.Instance.rect;
        Vector2 finalPosition = systemRect.localPosition;

        // Decide whether to flip this tooltip
        bool flip = systemRect.localPosition.x >= system.flipTooltipXThreshold;

        // Set the position of the main tooltip
        if (tooltipType == TooltipType.Display)
        {
            DisplayTooltip.Instance.gameObject.SetActive(true);
            DisplayTooltip.Instance.SetProperties(system.missionData, flip);

        }
        // Sets the position of the tooltip displaying the solve count
        else if (tooltipType == TooltipType.SolveCount)
        {
            SolveCountTooltip.Instance.gameObject.SetActive(true);
            SolveCountTooltip.Instance.SetProperties(system.missionData, flip);
            rect = SolveCountTooltip.Instance.rect;
            finalPosition.y -= 30;
        }
        // Sets the position of the tooltip displaying the number of points
        else
        {
            PointsTooltip.Instance.gameObject.SetActive(true);
            PointsTooltip.Instance.SetProperties(system.missionData, flip);
            rect = PointsTooltip.Instance.rect;
            finalPosition.y += 30;
        }

        // Flip the tooltip if necessary
        if (flip)
        {
            finalPosition.x -= (rect.sizeDelta.x / 2 + system.tooltipHorizontalBuffer);
        }
        else
        {
            finalPosition.x += (rect.sizeDelta.x / 2 + system.tooltipHorizontalBuffer);
        }

        // Set the tooltip's position
        rect.localPosition = finalPosition;

        if (tooltipType == TooltipType.SolveCount)
        {
            Debug.Log("Finished OnPointerEnter. SolveCount.Instance == null?: " + (SolveCountTooltip.Instance == null).ToString());
        }
    }

    /// <summary>
    /// Unity event function that deactivates this object's tooltip when the mouse exits this object.
    /// </summary>
    /// <param name="eventData">The mouse exit data.</param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipType == TooltipType.SolveCount) Debug.Log("Starting OnPointerExit. SolveCount.Instance == null?: " + (SolveCountTooltip.Instance == null).ToString());
        if (tooltipType == TooltipType.Display)
        {
            DisplayTooltip.Instance.gameObject.SetActive(false);
        }
        else if (tooltipType == TooltipType.SolveCount)
        {
            Debug.Log("OnPointerExit, setting active to false. SolveCount.Instance == null?: " + (SolveCountTooltip.Instance == null).ToString());
            SolveCountTooltip.Instance.gameObject.SetActive(false);
        }
        else
        {
            PointsTooltip.Instance.gameObject.SetActive(false);
        }
        if (tooltipType == TooltipType.SolveCount) Debug.Log("Finished OnPointerExit. SolveCount.Instance == null?: " + (SolveCountTooltip.Instance == null).ToString());
    }

    /// <summary>
    /// General configuration type for the tooltips. This should not need to be changed once initially set.
    /// </summary>
    private enum TooltipType
    {
        Display,
        SolveCount,
        Points
    }
}
