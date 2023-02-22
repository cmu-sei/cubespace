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
using TMPro;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// Class that controls the modal window and sets up its colors according to the SensorStationScreenController.
    /// </summary>
    public class SensorModalWindow : MonoBehaviour
    {
        /// <summary>
        /// The color scheme for this sensor modal.
        /// </summary>
        public enum ColorScheme { Green, Red, White };
        
        /// <summary>
        /// The button displayed on the sensor screen, with a different interaction depending on the screen type.
        /// </summary>
        [SerializeField]
        private Button button;
        /// <summary>
        /// The background of the window.
        /// </summary>
        [SerializeField]
        private GameObject background;

        /// <summary>
        /// The text of the button.
        /// </summary>
        private TextMeshProUGUI buttonText;
        /// <summary>
        /// The background images to choose from.
        /// </summary>
        private Image[] bgImages;

        /// <summary>
        /// Unity event function that gets the button text and background images.
        /// </summary>
        void Awake()
        {
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            bgImages = background.GetComponentsInChildren<Image>();
        }

        /// <summary>
        /// Sets the modal window using the screen provided and sets up its button behavior.
        /// </summary>
        /// <param name="screen">The screen to use as the modal window.</param>
        public void SetModalWindow(SensorScreen screen)
        {
            if (!screen)
            {
                button.gameObject.SetActive(false);
                background.gameObject.SetActive(false);
                return;
            }

            Color color;
            switch (screen.colorScheme)
            {
                default:
                case ColorScheme.Green:
                    color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.NavItemComplete);
                    break;
                case ColorScheme.Red:
                    color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.NavItemSelected);
                    break;
                case ColorScheme.White:
                    color = Color.white;
                    break;
            }

            button.gameObject.SetActive(screen.hasButton);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(screen.OnButtonClick);
            button.GetComponent<Image>().color = color;
            buttonText.text = screen.buttonText;

            background.SetActive(screen.hasBackground);
            foreach (Image image in bgImages)
            {
                image.color = color;
            }
        }
    }
}

