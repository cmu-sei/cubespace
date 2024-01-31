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
using Entities.Workstations.CubeStationParts;
using TMPro;
using UI.ColorPalettes;

namespace UI.NavScreen.NavScreenComponents
{
    /// <summary>
    /// The text to show on the cube status object.
    /// </summary>
    public class CubeStatusText : MonoBehaviour
    {
        /// <summary>
        /// The text reflecting whether the cube has been encoded.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI cubeEncoderText;
        /// <summary>
        /// The text reflecting whether the cube is in the cube drive.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI cubeDriveText;

        /// <summary>
        /// Displays the state of the cube on this screen.
        /// </summary>
        /// <param name="cubeState">The cube state to display.</param>
        public void SetText(CubeState cubeState)
        {
            switch (cubeState)
            {
                case (CubeState.InCubeDrive):
                    cubeEncoderText.text = "Cube Status - Ejected";
                    cubeEncoderText.color = ColorPalette.GetColor(PaletteColor.NavItemComplete);
                    cubeDriveText.text = "Engaged";
                    cubeDriveText.color = ColorPalette.GetColor(PaletteColor.NavItemComplete);
                    break;
                case (CubeState.InNavReader):
                    cubeEncoderText.text = "Cube Status - Please Eject";
                    cubeEncoderText.color = ColorPalette.GetColor(PaletteColor.NavButtonBase);
                    cubeDriveText.text = "Awaiting Data Cube";
                    cubeDriveText.color = ColorPalette.GetColor(PaletteColor.NavItemSelected);
                    break;
                case (CubeState.InPlayerHands):
                    cubeEncoderText.text = "Cube Status - Ejected";
                    cubeEncoderText.color = ColorPalette.GetColor(PaletteColor.NavItemComplete);
                    cubeDriveText.text = "Awaiting Data Cube";
                    cubeDriveText.color = ColorPalette.GetColor(PaletteColor.NavItemSelected);
                    break;
                case (CubeState.NotAvailable):
                    cubeEncoderText.text = "Cube Status - Not Encoded";
                    cubeEncoderText.color = ColorPalette.GetColor(PaletteColor.NavItemSelected);
                    cubeDriveText.text = "Awaiting Data Cube";
                    cubeDriveText.color = ColorPalette.GetColor(PaletteColor.NavItemSelected);
                    break;
            }
        }
    }
}

