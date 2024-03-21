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
    /// An option the player can select to use as their icon.
    /// </summary>
	public class PlayerIconSelectionOption : MonoBehaviour
	{
        /// <summary>
        /// The button to select the icon.
        /// </summary>
        [Header("Component Setup")]
        [SerializeField]
        private Button chooseIconButton;
        /// <summary>
        /// The actual icon image.
        /// </summary>
        [SerializeField]
        private Image iconDisplayImage;
        /// <summary>
        /// The list of customization options.
        /// </summary>
        [Header("Asset Setup")]
        [SerializeField]
        private CustomizationOptions _options;

        /// <summary>
        /// The icon choice selected.
        /// </summary>
        private IconChoice _choice;
        /// <summary>
        /// The larger panel this option is a part of.
        /// </summary>
        private PlayerIconSelectionPanel _panel;

        /// <summary>
        /// Sets the icon option and the display image and sprite.
        /// </summary>
        /// <param name="icon"></param>
        public void SetIconOption(IconChoice icon)
        {
            _choice = icon;
            if (iconDisplayImage)
            {
                iconDisplayImage.enabled = true;
                iconDisplayImage.sprite = icon.Icon;
            }
        }

        /// <summary>
        /// Selects the icon selected on the panel to be this one.
        /// </summary>
        public void IconSelected()
        {
            if (_panel == null)
            {
                Debug.LogWarning("Button is not part of a panel!", this);
                return;
            }

            _panel.IconSelected(_choice);
        }

        /// <summary>
        /// Injects the panel reference.
        /// </summary>
        /// <param name="playerColorSelectionPanel">The panel used to select this icon.</param>
        public void SetSelectionPanel(PlayerIconSelectionPanel playerColorSelectionPanel)
        {
            _panel = playerColorSelectionPanel;
        }
	}
}
