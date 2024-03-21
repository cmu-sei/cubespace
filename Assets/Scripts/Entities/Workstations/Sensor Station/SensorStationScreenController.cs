/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;
using UI.SensorScreen.SensorScreenComponents;
using Systems.GameBrain;
using Managers;
using Mirror;

namespace Entities.Workstations.SensorStationParts
{
    /// <summary>
    /// The controller which dictates behavior on the SensorStation.
    /// </summary>
    public class SensorStationScreenController : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The full list of screens displayed on the sensor station.
        /// </summary>
        [SerializeField]
        private List<SensorScreen> screens;
        /// <summary>
        /// The modal window that shows screens on the SensorStation.
        /// </summary>
        [SerializeField]
        private SensorModalWindow modalWindow;

        /// <summary>
        /// The screen displayed when a probe can be deployed.
        /// </summary>
        [SerializeField]
        private SensorScreenProbe probeScreen;
        /// <summary>
        /// The screen displayed following a scan.
        /// </summary>
        [SerializeField]
        private SensorScreenScanResponse scanResponseScreen;
        /// <summary>
        /// The screen displayed when an incoming transmission is present.
        /// </summary>
        [SerializeField]
        private SensorScreenIncomingTransmission incomingTransmissionScreen;
        /// <summary>
        /// The screen displayed when a scan fails.
        /// </summary>
        [SerializeField]
        private SensorScreenFailedScan failedScanScreen;
        /// <summary>
        /// The screen displayed before scanning.
        /// </summary>
        [SerializeField]
        private SensorScreenScanScreen scanScreen;
        /// <summary>
        /// The animator on the sensor station that gradually fills in consecutive pie pieces during scanning.
        /// </summary>
        [SerializeField]
        private SensorStationInfillAnimator scanningAnimationScreen;
        /// <summary>
        /// The screen shown when a video is playing.
        /// </summary>
        [SerializeField]
        private SensorStationVideoScreen videoScreen;
        /// <summary>
        /// The screen shown when a transmission is complete.
        /// </summary>
        [SerializeField]
        private SensorScreen transmissionCompleteScreen;

        /// <summary>
        /// The SensorStation workstation.
        /// </summary>
        public SensorStation sensorStation;

        /// <summary>
        /// The current screen displayed on the SensorStation.
        /// </summary>
        private SensorScreen currentScreen;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that deactivates the screens on the SensorScreen workstation.
        /// </summary>
        void Start() 
        {
            DeactivateAll();
            sensorStation = GetComponent<SensorStation>();

            if (!sensorStation)
            {
                if (((CustomNetworkManager)NetworkManager.singleton).isInDebugMode)
                    Debug.LogError("No Sensor Station component found for screen controller!");
            }
        }
        #endregion

        #region Set screen methods
        /// <summary>
        /// Activates or deactivates common screen elements, depending on screen properties.
        /// </summary>
        /// <param name="commEvent">The communication event at the SensorStation.</param>
        public void SetScanFromCommEvent(CommEvent commEvent) 
        {
            CommEventScreen screen;

            switch (commEvent.template)
            {
                case (CommEvent.CommEventTemplate.Probe): 
                {
                    // Play the SFX of the screen coming up
                    if (sensorStation.playerAtWorkstation && sensorStation.playerAtWorkstation.isLocalPlayer)
                       Audio.AudioPlayer.Instance.ScanComplete(transform);

                    screen = probeScreen;
                    break;
                }
                case (CommEvent.CommEventTemplate.None):
                {
                    // Play the SFX of the screen coming up
                    if (sensorStation.playerAtWorkstation && sensorStation.playerAtWorkstation.isLocalPlayer)
                        Audio.AudioPlayer.Instance.ScanComplete(transform);

                    screen = failedScanScreen;
                    break;
                }
                case (CommEvent.CommEventTemplate.BadTranslation):
                {
                    // Play the SFX of the screen coming up
                    if (sensorStation.playerAtWorkstation && sensorStation.playerAtWorkstation.isLocalPlayer)
                        Audio.AudioPlayer.Instance.ScanComplete(transform);

                    sensorStation.OnTranslationError();
                    screen = scanResponseScreen;
                    break;
                }
                case (CommEvent.CommEventTemplate.Incoming): 
                {
                    // Play the SFX of the screen coming up
                    if (sensorStation.playerAtWorkstation && sensorStation.playerAtWorkstation.isLocalPlayer)
                        Audio.AudioPlayer.Instance.TransmissionAlert(); 

                    screen = scanResponseScreen;
                    break;
                }
                default:
                {
                    if (((CustomNetworkManager)NetworkManager.singleton).isInDebugMode)
                        Debug.LogError("Screen from comm event had bad enum!");
                    screen = scanResponseScreen;
                    break;
                }
            }

            SwitchScreen(screen);
            screen.ActivateFromCommEvent(commEvent);
        }

        /// <summary>
        /// Sets the translation screen based on the communication event received.
        /// </summary>
        /// <param name="commEvent">The communication event to use for the translation event.</param>
        public void SetTranslationScreenFromCommEvent(CommEvent commEvent)
        {
            SwitchScreen(incomingTransmissionScreen);
            incomingTransmissionScreen.ActivateFromCommEvent(commEvent);
        }

        /// <summary>
        /// Sets the modal window to use the provided screen.
        /// </summary>
        /// <param name="screen">The screen to set at the modal window.</param>
        public void SetModalWindow(SensorScreen screen) 
        {
            modalWindow.SetModalWindow(screen);
        }

        /// <summary>
        /// Switches the screen used and activates the scan screen based on whether the current location has been scanned and its surroundings.
        /// </summary>
        /// <param name="currentLocationScanned">Whether the current location has been scanned.</param>
        /// <param name="currentLocationSurroundings">The surroundings on the location.</param>
        public void SetScanScreen(bool currentLocationScanned, string currentLocationSurroundings)
        {
            SwitchScreen(scanScreen);
            modalWindow.SetModalWindow(scanScreen);
            scanScreen.ActivateScanScreen(currentLocationScanned, currentLocationSurroundings);
        }

        /// <summary>
        /// Acts as an overload for screens calling this which can't access the SensorStation.
        /// </summary>
        public void SetScanScreen()
        {
            SetScanScreen(sensorStation.CurrentLocationScanned, sensorStation.CurrentLocationSurroundings);
        }

        /// <summary>
        /// Sets the current screen to the animation screen.
        /// </summary>
        public void SetScanningScreen()
        {
            SwitchScreen(scanningAnimationScreen);
            scanningAnimationScreen.Activate();
        }

        /// <summary>
        /// Switches to the video screen to play a video.
        /// </summary>
        /// <param name="commEvent"></param>
        public void SetVideoScreen(CommEvent commEvent)
        {
            SwitchScreen(videoScreen);
            videoScreen.ActivateVideoScreen(commEvent);

            // Stop the transmission SFX when the video starts playing
            StopTransmissionAlertSFX();
        }

        /// <summary>
        /// Turns the current screen off and sets a new current screen.
        /// </summary>
        /// <param name="newScreen">The screen to switch to.</param>
        private void SwitchScreen(SensorScreen newScreen)
        {
            if (currentScreen != null)
            {
                currentScreen.Deactivate();
            }
            currentScreen = newScreen;
        }
        #endregion

        #region Deactivation/End methods
        /// <summary>
        /// Turns all sensor screens off.
        /// </summary>
        public void DeactivateAll()
        {
            screens.ForEach(s => s.Deactivate());
            modalWindow.SetModalWindow(null);
        }

        /// <summary>
        /// Powers off the current screen and sets the current modal window as null.
        /// </summary>
        public void PowerOff() 
        {
            if (currentScreen != null)
            {
                currentScreen.Deactivate();
                currentScreen = null;
            }

            StopTransmissionAlertSFX();
            modalWindow.SetModalWindow(null);
        }

        /// <summary>
        /// Switches the screen to the transmission complete screen and activates it.
        /// </summary>
        public void OnTransmissionComplete()
        {
            // Check if it was a bad translation and if not then do this
            if (currentScreen == videoScreen)
            {
                // Play the SFX of it being complete
                Audio.AudioPlayer.Instance.TransmissionEnded(transform);

                SwitchScreen(transmissionCompleteScreen);
                transmissionCompleteScreen.Activate();
            }
        }

        /// <summary>
        /// Stops playing the transmission alert sound effect.
        /// </summary>
        public void StopTransmissionAlertSFX()
        {
            Audio.AudioPlayer.Instance.StopTransmissionAlert();
        }
        #endregion
    }
}

