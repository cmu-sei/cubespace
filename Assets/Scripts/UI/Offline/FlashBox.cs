/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The box which flashes when a color or icon is not selected.
/// </summary>
public class FlashBox : MonoBehaviour
{
    /// <summary>
    /// The image to flash.
    /// </summary>
    [Header("Flash Objects")]
    [SerializeField]
    private Image imageToFlash;
    /// <summary>
    /// The text to flash.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI textToFlash;
    /// <summary>
    /// An additional image to flash, if there is one specified.
    /// </summary>
    [SerializeField]
    private Image secondaryImageToFlash;

    /// <summary>
    /// The time required to flash text.
    /// </summary>
    [Header("Configuration")]
    [SerializeField]
    private float timeToFlash = 2.0f;
    /// <summary>
    /// The color to use for the text box flash.
    /// </summary>
    [SerializeField]
    private Color flashColor = Color.green;
    /// <summary>
    /// The secondary color to use in the flash box.
    /// </summary>
    [SerializeField]
    private Color secondaryImageFlashColor = Color.white;
    /// <summary>
    /// Whether to disable the secondary image when the flashing finishes.
    /// </summary>
    [SerializeField]
    private bool disableSecondaryImageOnFinish = false;

    /// <summary>
    /// Whether to stop flashing the text box.
    /// </summary>
    public bool stopFlashing;

    /// <summary>
    /// Whether to flash in reverse.
    /// </summary>
    private bool flashBack;

    /// <summary>
    /// The initial color of the image.
    /// </summary>
    private Color imageStartColor;
    /// <summary>
    /// The initial color of the text.
    /// </summary>
    private Color textStartColor;
    /// <summary>
    /// The initial color of the secondary text object.
    /// </summary>
    private Color secondaryImageStartColor;

    /// <summary>
    /// Unity event function that sets up the variables and starts flashing the flash box.
    /// </summary>
    void Start()
    {
        // Get base colors
        if (imageToFlash)
        {
            imageStartColor = imageToFlash.color;
        }
        if (textToFlash)
        {
            textStartColor = textToFlash.color;
        }
        if (secondaryImageToFlash)
        {
            secondaryImageStartColor = secondaryImageToFlash.color;
        }

        // Start a coroutine to flash the image, text, and secondary image
        StartCoroutine(Flash());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>A yield while waiting for the image to stop flashing.</returns>
    private IEnumerator Flash()
    {
        float timeElapsed = 0.0f;
        while (!stopFlashing)
        {
            // Primary image flash color
            if (imageToFlash)
            {
                Color startColorPrimary = flashBack ? imageStartColor : flashColor;
                Color targetColorPrimary = flashBack ? flashColor : imageStartColor;
                imageToFlash.color = Color.Lerp(startColorPrimary, targetColorPrimary, timeElapsed / timeToFlash);
            }

            // Text flash color
            if (textToFlash)
            {
                Color startColorText = flashBack ? textStartColor : flashColor;
                Color targetColorText = flashBack ? flashColor : textStartColor;
                textToFlash.color = Color.Lerp(startColorText, targetColorText, timeElapsed / timeToFlash);
            }

            // Secondary image flash color
            if (secondaryImageToFlash)
            {
                Color startColorSecondary = flashBack ? secondaryImageStartColor : secondaryImageFlashColor;
                Color targetColorSecondary = flashBack ? secondaryImageFlashColor : secondaryImageStartColor;
                secondaryImageToFlash.color = Color.Lerp(startColorSecondary, targetColorSecondary, timeElapsed / timeToFlash);
            }

            if (timeElapsed >= timeToFlash)
            {
                timeElapsed = 0.0f;
                flashBack = !flashBack;
            }
            else timeElapsed += Time.deltaTime;

            yield return null;
        }

        // Reset to base colors
        if (imageToFlash)
        {
            imageToFlash.color = imageStartColor;
        }

        if (textToFlash)
        {
            textToFlash.color = textStartColor;
        }

        if (secondaryImageToFlash)
        {
            secondaryImageToFlash.color = secondaryImageStartColor;
            secondaryImageToFlash.enabled = !disableSecondaryImageOnFinish;
        }
    }
}

