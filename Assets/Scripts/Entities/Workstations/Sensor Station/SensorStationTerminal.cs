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
using UI;

namespace Entities.Workstations.SensorStationParts
{
    /// <summary>
    /// The terminal used at the SensorStation. This is used to enable an icon in case of an incoming transmission.
    /// </summary>
    public class SensorStationTerminal : Terminal
    {
        #region Variables
        /// <summary>
        /// The icon used for an incoming transmission.
        /// </summary>
        [SerializeField]
        private WorkstationIcon incomingTransmissionIcon;
        #endregion

        #region Main methods
        /// <summary>
        /// Enables or disables the incoming transmission icon.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable the incoming transmission icon.</param>
        public void SetIncomingTransmissionIcon(bool enabled)
        {
            if (enabled)
            {
                incomingTransmissionIcon.EnableIcon();
            }
            else
            {
                incomingTransmissionIcon.DisableIcon();
            }
        }
        #endregion
    }
}

