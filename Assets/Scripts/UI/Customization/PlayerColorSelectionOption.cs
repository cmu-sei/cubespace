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
using TMPro;

namespace UI.Customization
{
    /// <summary>
    /// An option within the color selection panel, used by the player to set their color.
    /// </summary>
    public class PlayerColorSelectionOption : MonoBehaviour
    {
        /// <summary>
        /// The button on this color the player clicks to set the color.
        /// </summary>
        [Header("Component Setup")]
        [SerializeField]
        private Button chooseColorButton;
        /// <summary>
        /// The image which displays the color on this option.
        /// </summary>
        [SerializeField]
        private Image colorDisplayImage;
        /// <summary>
        /// The text on the name component.
        /// </summary>
        [SerializeField]
        private TMP_Text colorNameText;

        /// <summary>
        /// The color choice selected.
        /// </summary>
        private ColorChoice _choice;
        /// <summary>
        /// The panel used to select colors.
        /// </summary>
        private PlayerColorSelectionPanel _panel;

        /// <summary>
        /// Sets the color option used by the player.
        /// </summary>
        /// <param name="color">The color selected.</param>
        public void SetColorOption(ColorChoice color)
        {
            // Force no transparency
            _choice = color;
            if (colorDisplayImage && colorNameText)
            {
                colorDisplayImage.color = color.color;
                colorNameText.text = color.colorName;
            }
        }

        /// <summary>
        /// Calls the button object
        /// </summary>
        public void ColorSelected()
        {
            if (_panel == null)
            {
                Debug.LogWarning("Button is not part of a panel!",this);
                return;
            }

            _panel.ColorSelected(_choice);
        }

        //Injected on creation.
        /// <summary>
        /// Injects the wider color selection panel into this choice object so it can be referenced.
        /// </summary>
        /// <param name="playerColorSelectionPanel">The panel where this color is selected.</param>
        public void SetSelectionPanel(PlayerColorSelectionPanel playerColorSelectionPanel)
        {
            _panel = playerColorSelectionPanel;
        }
    }
}
