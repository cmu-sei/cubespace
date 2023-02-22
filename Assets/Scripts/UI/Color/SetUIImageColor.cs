/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ColorPalettes
{
    /// <summary>
    /// Sets the color of an image.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SetUIImageColor : MonoBehaviour
    {
        /// <summary>
        /// The image to set the color of.
        /// </summary>
        private Image Image;
        /// <summary>
        /// An alternative color to use on this image.
        /// </summary>
        [SerializeField]
        private bool overridePaletteColor;
        /// <summary>
        /// The override color used for this text.
        /// </summary>
        [ShowIf("overridePaletteColor")]
        [SerializeField]
        private Color overrideColor;
        /// <summary>
        /// The palette color 
        /// </summary>
        [HideIf("overridePaletteColor")]
        [SerializeField]
        private PaletteColor color;

        /// <summary>
        /// The color to use for this image, based on whether to use the override color.
        /// </summary>
        private Color selectedColor => overridePaletteColor ? overrideColor : ColorPalette.GetColor(color);

        /// <summary>
        /// Gets the image component on this object to set the color of.
        /// </summary>
        private void Awake()
        {
            Image = GetComponent<Image>();
        }

        /// <summary>
        /// Sets the color of the image to the selected color.
        /// </summary>
        void SetTextColor()
        {
            Image.color = selectedColor;
        }

        /// <summary>
        /// Sets the color of an image in-editor.
        /// </summary>
        [Button("Set Now")]
        void EditorSetTextColor()
        {
            Image = GetComponent<Image>();
            SetTextColor();
        }
    }
}
