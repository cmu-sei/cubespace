/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Customization;
using UnityEngine;


namespace UI.Customization
{
    /// <summary>
    /// The panel used for color selection.
    /// </summary>
    public class PlayerColorSelectionPanel : MonoBehaviour
    {
        /// <summary>
        /// The renderer to display the selected color on.
        /// </summary>
        [Header("Scene Configuration")]
        [SerializeField]
        private Renderer colorRenderer;
        /// <summary>
        /// The customization options available to the player.
        /// </summary>
        [Header("Asset Configuration")]
        [SerializeField]
        private CustomizationOptions customizationOptions;
        /// <summary>
        /// The prefab used as a color selection option.
        /// </summary>
        [SerializeField]
        private PlayerColorSelectionOption colorSelectionOptionPrefab;
        /// <summary>
        /// The box to flash if a color has not been chosen.
        /// </summary>
        [Header("Scene References")]
        [SerializeField]
        private FlashBox colorFlashBox;

        /// <summary>
        /// Whether a color has been selected.
        /// </summary>
        public bool colorSelected;
        
        /// <summary>
        /// Generates the customization option buttons on this GameObject's initialization.
        /// </summary>
        private void OnEnable()
        {
            GenerateOptions();
        }

        /// <summary>
        /// Populates the panel with color choices.
        /// </summary>
        void GenerateOptions()
        {
            ClearChildren();
            foreach (var color in customizationOptions.GetColorChoices())
            {
                var option = Instantiate(colorSelectionOptionPrefab, transform);
                option.SetSelectionPanel(this);
                option.SetColorOption(color);
            }
        }

        /// <summary>
        /// Destroys all options within the panel.
        /// </summary>
        void ClearChildren()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Sets the color selected on the player object.
        /// </summary>
        /// <param name="colorChoice">The color choice used.</param>
        public void ColorSelected(ColorChoice colorChoice)
        {
            colorSelected = true;
            colorFlashBox.stopFlashing = true;

            if (colorRenderer)
            {
                colorRenderer.material.color = colorChoice.color;
            }

            customizationOptions.ChooseLocalColor(colorChoice);
        }
    }
}
