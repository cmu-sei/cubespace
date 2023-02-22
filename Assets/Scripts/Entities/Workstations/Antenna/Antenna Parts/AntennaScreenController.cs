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
using TMPro;
using UI.AntennaScreen.AntennaScreenComponents;

namespace Entities.Workstations.AntennaParts
{
    /// <summary>
    /// A component that should be attached to the Antenna workstation which controls the displayed antenna screen.
    /// </summary>
    public class AntennaScreenController : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The screen displayed while trying to connect to a network.
        /// </summary>
        [Header("UI Screens")]
        [SerializeField]
        private AntennaScreen connectingScreen;
        /// <summary>
        /// The screen displayed when trying to disconnect from a network.
        /// </summary>
        [SerializeField]
        private AntennaScreen disconnectingScreen;
        /// <summary>
        /// The screen displayed when not connected to a network
        /// </summary>
        [SerializeField]
        private AntennaScreen noConnectionScreen;
        /// <summary>
        /// The screen displayed when first connected to a network.
        /// </summary>
        [SerializeField]
        private AntennaScreen establishedScreen1;
        /// <summary>
        /// The screen displayed after being connected to a network for a short time.
        /// </summary>
        [SerializeField]
        private ConnectionCompletedAntennaScreen establishedScreen2;
        /// <summary>
        /// The text object displaying what network the antenna is connected to.
        /// </summary>
        [SerializeField]
        private TMP_Text connectedToText;

        /// <summary>
        /// The currently active antenna screen.
        /// </summary>
        private AntennaScreen currentScreen;
        #endregion

        #region Screen toggle methods
        /// <summary>
        /// Sets the screen displayed on the antenna to match the connection state provided.
        /// </summary>
        /// <param name="connectionState">The connection state that determines which screen is shown.</param>
        public void ToggleScreen(AntennaState connectionState)
        {
            AntennaScreen antennaScreen = null;
            switch (connectionState)
            {
                // There are two types of connection established screens
                case AntennaState.Connected:
                    // If the first screen is currently displayed, the second screen is displayed, or no screen is displayed (on entering the antenna), show the second screen
                    if (currentScreen == establishedScreen1 || currentScreen == establishedScreen2 || currentScreen == null)
                    {
                        antennaScreen = establishedScreen2;
                    }
                    // If there is no screen displayed, show the first screen
                    else if (currentScreen != establishedScreen2)
                    {
                        antennaScreen = establishedScreen1;
                    }
                    break;
                case AntennaState.Disconnected:
                    antennaScreen = noConnectionScreen;
                    break;
                case AntennaState.Connecting:
                    antennaScreen = connectingScreen;
                    break;
                case AntennaState.Disconnecting:
                    antennaScreen = disconnectingScreen;
                    break;
                default:
                    Debug.Log("No valid connectionState provided.");
                    break;
            }

            // Turn the current antenna screen off and activate the new antenna screen
            SwitchScreen(antennaScreen);
            antennaScreen.Activate();
        }

        /// <summary>
        /// Calls the Deactivate method on the current antenna screen and sets the current antenna screen to be the specified one.
        /// </summary>
        /// <param name="newScreen">The new antenna screen to make the current one.</param>
        private void SwitchScreen(AntennaScreen newScreen)
        {
            if (currentScreen != null)
            {
                currentScreen.Deactivate();
            }
            currentScreen = newScreen;
        }

        /// <summary>
        /// Disables the current screen.
        /// </summary>
        public void DisableCurrentScreen()
        {
            SwitchScreen(null);
        }

        public void DisableAllScreens()
        {
            connectingScreen.Deactivate();
            disconnectingScreen.Deactivate();
            noConnectionScreen.Deactivate();
            establishedScreen1.Deactivate();
            establishedScreen2.Deactivate();
        }
        #endregion
    }
}

