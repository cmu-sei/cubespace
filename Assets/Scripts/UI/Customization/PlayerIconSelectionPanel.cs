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
using UnityEngine.UI;

namespace UI.Customization
{
    /// <summary>
    /// A panel used for icon selection.
    /// </summary>
	public class PlayerIconSelectionPanel : MonoBehaviour
	{
        /// <summary>
        /// The image selected for the player.
        /// </summary>
        [Header("Scene Configuration")]
        [SerializeField]
        private Image playerIconImage;
        /// <summary>
        /// The customization options. Derives from a private variable.
        /// </summary>
        public CustomizationOptions CustomizationOptions => customizationOptions;
        /// <summary>
        /// The customization options.
        /// </summary>
        [Header("Asset Configuration")]
        [SerializeField]
        private CustomizationOptions customizationOptions;
        /// <summary>
        /// A prefab representing an icon selection option.
        /// </summary>
        [SerializeField]
        private PlayerIconSelectionOption iconSelectionOptionPrefab;
        /// <summary>
        /// The box to flash if an icon has not been chosen.
        /// </summary>
        [Header("Scene References")]
        [SerializeField]
        private FlashBox iconFlashBox;

        /// <summary>
        /// Whether the player has selected an icon.
        /// </summary>
        public bool iconSelected;

        /// <summary>
        /// Initiates the icon map.
        /// </summary>
        private void Awake()
        {
            customizationOptions.InitiateIconMap();
        }

        /// <summary>
        /// Generates the possible icon options the player can choose from.
        /// </summary>
        private void OnEnable()
        {
            // Generate the buttons
            GenerateOptions();
        }

        /// <summary>
        /// Clears out the previous set of icons and instantiates new ones.
        /// </summary>
        void GenerateOptions()
        {
            ClearChildren();
            foreach (var icon in customizationOptions.GetIconChoices())
            {
                var option = Instantiate(iconSelectionOptionPrefab, transform);
                option.SetSelectionPanel(this);
                option.SetIconOption(icon);
            }
        }

        /// <summary>
        /// Clears out all icons.
        /// </summary>
        void ClearChildren()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Sets the active icon for the player.
        /// </summary>
        /// <param name="iconChoice">The icon chosen by the player.</param>
        public void IconSelected(IconChoice iconChoice)
        {
            iconSelected = true;
            iconFlashBox.stopFlashing = true;

            if (playerIconImage)
            {
                playerIconImage.enabled = true;
                playerIconImage.sprite = iconChoice.Icon;
            }

            customizationOptions.ChooseLocalIcon(iconChoice);
        }
	}
}
