/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using TMPro;
using Systems.GameBrain;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// The screen displayed at the sensor station when there is an incoming transmission.
    /// </summary>
    public class SensorScreenIncomingTransmission : CommEventScreen
    {
        /// <summary>
        /// The message displayed when trying to translate a message.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI translationStatusMessage;
        /// <summary>
        /// The message displayed on a successful scan.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI successMessage;
        /// <summary>
        /// The message displayed on a failed scan.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI failureMessage;

        /// <summary>
        /// Activates a specific screen from a comm event.
        /// </summary>
        /// <param name="commEvent">The comm event to use to activate the screen.</param>
        public override void ActivateFromCommEvent(CommEvent commEvent)
        {
            switch (commEvent.template) 
            {
                case CommEvent.CommEventTemplate.Incoming:
                    translationStatusMessage.text = commEvent.translationMessage;
                    successMessage.enabled = true;
                    failureMessage.enabled = false;
                    colorScheme = SensorModalWindow.ColorScheme.Green;
                    break;
                case CommEvent.CommEventTemplate.BadTranslation:
                    translationStatusMessage.text = commEvent.translationMessage;
                    failureMessage.enabled = true;
                    successMessage.enabled = false;
                    colorScheme = SensorModalWindow.ColorScheme.Red;
                    break;
                default:
                    Debug.LogWarning("Incoming Transmission page activated with incorrect template " + commEvent.template.ToString());
                    break;
            }
            base.ActivateFromCommEvent(commEvent);

            if (commEvent.template == CommEvent.CommEventTemplate.Incoming) 
            {
                StartCoroutine(DelayVideoPlay(commEvent));
            } 
        }

        /// <summary>
        /// Delays video playback until after a set time has elapsed.
        /// </summary>
        /// <param name="commEvent">The comm event used to play the video.</param>
        /// <returns>A yield while waiting to set the video screen using the received comm event.</returns>
        IEnumerator DelayVideoPlay(CommEvent commEvent) 
        {
            yield return new WaitForSeconds(TRANSITION_TIME);
            _sensorScreenController.SetVideoScreen(commEvent);
        }

    }
}

