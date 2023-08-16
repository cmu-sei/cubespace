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
using NaughtyAttributes;

namespace UI.ColorPalettes
{
    /// <summary>
    /// Color Palettes define a manually created list of colors, all set to an enum.
    /// Add colors by editing the various public fields, switch statements, and enums in this class (hypothetically, a design is not very often changing the number of colors it uses...)
    /// In the scene, add a "ColorPaletteSetter" which simply sets the static reference on awake from a serialized field.
    /// This system does not support changing colors during gameplay, but a static action in this object (OnColorUpdated or something) and then adding a listener to all the color setters (SetTMPTextColor, SetUIImageColor) could work dynamically.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Game Data/ColorPalette", order = 0)]
    public class ColorPalette : ScriptableObject
    {
        /// <summary>
        /// The primary active color palette.
        /// </summary>
        public static ColorPalette activeColorPalette;

        // Define our colors here and in the enum
        [Header("Workstation Colors")]
        public Color PrimaryTextColor;
        public Color UnpoweredTextColor;
        public Color PoweredTextColor;
        public Color UnpoweredColor;
        public Color PoweredColor;
        public Color NavItemSelectedColor = Color.red;
        public Color NavItemCompleteColor = Color.green;
        public Color NavSquareBaseColor = Color.blue;
        public Color NavButtonBaseColor = Color.yellow;
        public Color NavButtonWarningColor = Color.red;
        [Header("HUD Elements")]
        public Color UIButtonTextColor = Color.white;
        public Color UIAccentColorOne;
        public Color UIAccentColorTwo;
        public Color UISpecialMissionSelected;
        public Color UISpecialMissionUnseleceted;
        [Header("Galaxy Map System State Highlights")]
        public Color incompleteHighlightColor;

        public Color partiallyCompletedHighlightColor;
        public Color completedHighlightColor;
        [Header("Other")]
        public Color LaunchModePoweredColor = Color.green;
        public Color ExplorationModePoweredColor = Color.blue;
        
        /// <summary>
        /// Gets the active palette color.
        /// </summary>
        /// <param name="c">The palette color to display.</param>
        /// <returns>A color within the palette.</returns>
        public Color GetPaletteColor(PaletteColor c)
        {
            switch (c)
            {
                case PaletteColor.White:
                    return Color.white;
                case PaletteColor.PrimaryText:
                    return PrimaryTextColor;
                case PaletteColor.UIAccentColorOne:
                    return UIAccentColorOne;
                case PaletteColor.UIButtonText:
                    return UIButtonTextColor;
                case PaletteColor.UnpoweredText:
                    return UnpoweredTextColor;
                case PaletteColor.PoweredText:
                    return PoweredTextColor;
                case PaletteColor.Unpowered:
                    return UnpoweredColor;
                case PaletteColor.Powered:
                    return PoweredColor;
                case PaletteColor.NavItemSelected:
                    return NavItemSelectedColor;
                case PaletteColor.NavItemComplete:
                    return NavItemCompleteColor;
                case PaletteColor.NavSquareBase:
                    return NavSquareBaseColor;
                case PaletteColor.NavButtonBase:
                    return NavButtonBaseColor;
                case PaletteColor.NavButtonWarning:
                    return NavButtonWarningColor;
                case PaletteColor.UISpecialMissionSelected:
                    return UISpecialMissionSelected;
                case PaletteColor.UISpecialMissionUnselected:
                    return UISpecialMissionUnseleceted;
            }

            return Color.white;
        }

        /// <summary>
        /// Gets the hex code of a given palette color.
        /// </summary>
        /// <param name="c">The palette color to get the hex code of.</param>
        /// <param name="includeHashtag">Whether to include the hashtag in the hex string returned.</param>
        /// <returns>The final hex string.</returns>
        public static string GetHexCode(PaletteColor c, bool includeHashtag = false)
        {
            Color col = GetColor(c);
            var octothorpe = includeHashtag ? "#" : "";
            return octothorpe + ColorUtility.ToHtmlStringRGB(col);
        }

        /// <summary>
        /// Gets a color within the color palette that corresponds to a given PaletteColor parameter.
        /// </summary>
        /// <param name="c">The palette color to retrieve.</param>
        /// <returns>The color within the palette.</returns>
        public static Color GetColor(PaletteColor c)
        {
            SafeGetActiveColorPalette();
            return activeColorPalette.GetPaletteColor(c);
        }

        /// <summary>
        /// Gets the active color palette.
        /// </summary>
        private static void SafeGetActiveColorPalette()
        {
            if (activeColorPalette == null)
            {
                activeColorPalette = FindObjectOfType<ColorPalette>();
            }
        }

        /// <summary>
        /// Gets the button for a nav color.
        /// </summary>
        /// <param name="isComplete">Whether to render the button as being colored as complete.</param>
        /// <param name="isSelected">Whether the button is on a selected screen.</param>
        /// <returns>The appropriate color for the nav button.</returns>
        public Color GetNavButtonColor(bool isComplete, bool isSelected)
        {
            if (isComplete)
            {
                return NavItemCompleteColor;
            }

            if (isSelected)
            {
                return NavButtonWarningColor;
            }
            else
            {
                return NavButtonBaseColor;
            }
        }
        
        /// <summary>
        /// Makes this the active color palette when this object is enabled.
        /// </summary>
        private void OnEnable()
        {
            MakeActivePalette();
        }

        /// <summary>
        /// Makes this the active color palette.
        /// </summary>
        [Button]
        public void MakeActivePalette()
        {
            activeColorPalette = this;
        }

        /// <summary>
        /// Gets the color of text on the NavReader destination screen.
        /// </summary>
        /// <param name="isReadyForLaunch">Whether the NavReader is ready to launch.</param>
        /// <returns>The color for the destination screen text on the NavReader.</returns>
        public Color GetNavReaderDestinationScreenTextColor(bool isReadyForLaunch)
        {
            return isReadyForLaunch ? NavItemCompleteColor : UIButtonTextColor;
        }
    }

    /// <summary>
    /// A list of possible colors within the palette.
    /// </summary>
    public enum PaletteColor
    {
        White,
        PrimaryText,
        UIButtonText,
        UnpoweredText,
        PoweredText,
        Unpowered,
        Powered,
        NavItemSelected,
        NavItemComplete,
        NavSquareBase,
        NavButtonBase,
        NavButtonWarning,
        UIAccentColorOne,
        UISpecialMissionSelected,
        UISpecialMissionUnselected
    }
}
