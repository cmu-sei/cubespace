/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Swaps the color of an image on hover, selection, or deselection.
/// </summary>
public class SimpleButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// The image that should be colored on hover.
    /// </summary>
    [SerializeField]
    private Image imageToColor;
    /// <summary>
    /// The color of the image when selected.
    /// </summary>
    [SerializeField]
    private Color colorOnSelect = Color.white;
    /// <summary>
    /// The color of the image when hovered over.
    /// </summary>
    [SerializeField]
    private Color colorOnHover = Color.gray;

    /// <summary>
    /// The base color of the image.
    /// </summary>
    private Color baseColor;

    /// <summary>
    /// Unity event function that initializes the base color of the image.
    /// </summary>
    void Start()
    {
        baseColor = imageToColor.color;
    }

    /// <summary>
    /// Sets the color of the image to its color on selection.
    /// </summary>
    public void OnSelectOption()
    {
        imageToColor.color = colorOnSelect;
    }

    /// <summary>
    /// Resets the color of the image back to its base color.
    /// </summary>
    public void OnDeselectOption()
    {
        imageToColor.color = baseColor;
    }

    /// <summary>
    /// Colors the image to a color on hover when the mouse enters this object.
    /// </summary>
    /// <param name="eventData">The data of the mouse hover.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        imageToColor.color = colorOnHover;
    }
    
    /// <summary>
    /// Colors the image back to its base color when the mouse exits this object.
    /// </summary>
    /// <param name="eventData">The data of the mouse hover.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        imageToColor.color = baseColor;
    }
}

