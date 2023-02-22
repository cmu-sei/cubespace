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
    /// The screen displayed when a probe is visible on the scan screen.
    /// </summary>
    public class SensorScreenProbe : CommEventScreen
    {
        /// <summary>
        /// The string prompting the player to deploy the probe.
        /// </summary>
        private const string DEPLOY_PROBE_STRING = "DEPLOY PROBE?";

        /// <summary>
        /// The text mesh identifying the scan.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI textMesh;
        /// <summary>
        /// The comm event received.
        /// </summary>
        private CommEvent _commEvent;

        /// <summary>
        /// Sets properties of the screen based on the comm event received.
        /// </summary>
        /// <param name="commEvent">The comm event received.</param>
        public override void ActivateFromCommEvent(CommEvent commEvent)
        {
            textMesh.text = commEvent.scanInfoMessage;
            _commEvent = commEvent;
            buttonText = DEPLOY_PROBE_STRING;
            base.ActivateFromCommEvent(commEvent);
        }

        /// <summary>
        /// The video screen set when the button is clicked.
        /// </summary>
        public override void OnButtonClick()
        {
            _sensorScreenController.SetVideoScreen(_commEvent);
        }
    }
}
