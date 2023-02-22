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
using Systems.GameBrain;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// The comm event screen displayed when a scan for comm events fails.
    /// </summary>
    public class SensorScreenFailedScan : CommEventScreen
    {
        /// <summary>
        /// Activates a screen from the comm event.
        /// </summary>
        /// <param name="commEvent">The comm event to activate the screen with.</param>
        public override void ActivateFromCommEvent(CommEvent commEvent)
        {
            base.ActivateFromCommEvent(commEvent);
            StartCoroutine(DelayReturnToScan());
        }

        /// <summary>
        /// Delays returning to the scan screen.
        /// </summary>
        /// <returns>A yield statement while setting the scan screen on the controller.</returns>
        IEnumerator DelayReturnToScan()
        {
            yield return new WaitForSeconds(TRANSITION_TIME);
            _sensorScreenController.SetScanScreen();          
        }
    }
}

