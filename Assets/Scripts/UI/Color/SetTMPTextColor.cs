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
using TMPro;
using UnityEngine;

namespace UI.ColorPalettes
{
    /// <summary>
    /// Sets the text color of the TextMeshPro object.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class SetTMPTextColor : MonoBehaviour
    {
        /// <summary>
        /// The text component for this button.
        /// </summary>
        private TMP_Text Text;
        [SerializeField] private bool overridePaletteColor;
        /// <summary>
        /// The alternative color to use on this text, if specified.
        /// </summary>
        [ShowIf("overridePaletteColor")]
        [SerializeField]
        private Color overrideColor;
        /// <summary>
        /// The palette color to use on this text.
        /// </summary>
        [HideIf("overridePaletteColor")]
        [SerializeField] private PaletteColor color;

        private Color selectedColor => overridePaletteColor ? overrideColor : ColorPalette.GetColor(color);

        /// <summary>
        /// Gets the text component.
        /// </summary>
        private void Awake()
        {
            Text = GetComponent<TMP_Text>();
        }

        /// <summary>
        /// Sets the text color to the color it should have when selected.
        /// </summary>
        void SetTextColor()
        {
            Text.color = selectedColor;
        }

        /// <summary>
        /// An editor function to manually set the text color.
        /// </summary>
        [Button("Set Now")]
        void EditorSetTextColor()
        {
            Text = GetComponent<TMP_Text>();
            SetTextColor();
        }
    }
}
