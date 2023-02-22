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
using Systems.GameBrain;
using TMPro;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// A comm event screen that displays when a response is received for scanning at the sensor screen.
    /// </summary>
    public class SensorScreenScanResponse : CommEventScreen
    {
        /// <summary>
        /// The transmission view string.
        /// </summary>
        private const string VIEW_WORKSTATION_STRING = "TRANSLATE TO VIEW TRANSMISSION";

        /// <summary>
        /// The text mesh identifying the scan.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI textMesh;
        /// <summary>
        /// The comm event received.
        /// </summary>
        private CommEvent commEvent;

        /// <summary>
        /// Activates this screen and sets properties from the comm event.
        /// </summary>
        /// <param name="commEvent">The comm event.</param>
        public override void ActivateFromCommEvent(CommEvent commEvent)
        {
            this.commEvent = commEvent;
            textMesh.text = commEvent.scanInfoMessage;
            buttonText = VIEW_WORKSTATION_STRING;

            base.ActivateFromCommEvent(commEvent);
        }

        /// <summary>
        /// Sets the translation screen from the comm event received.
        /// </summary>
        public override void OnButtonClick()
        {
            _sensorScreenController.SetTranslationScreenFromCommEvent(commEvent);
        }
    }
}
