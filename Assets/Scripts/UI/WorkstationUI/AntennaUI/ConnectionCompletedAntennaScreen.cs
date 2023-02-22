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
using Managers;
using TMPro;

namespace UI.AntennaScreen.AntennaScreenComponents
{
    /// <summary>
    /// An extended antenna screen that sets UI displaying the current network.
    /// </summary>
    public class ConnectionCompletedAntennaScreen : AntennaScreen
    {
        #region Variables
        /// <summary>
        /// Whether the antenna should use a default name when connecting to a remote network.
        /// </summary>
        [SerializeField]
        private bool useDefaultConnectedNetworkName = true;
        /// <summary>
        /// The default name to use if useDefaultConnectedNetworkName is true.
        /// </summary>
        [SerializeField]
        private string defaultConnectedNetworkName = "REMOTE SYSTEM";

        /// <summary>
        /// The TextMeshPro object that displays text.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI connectedToText;
        #endregion

        #region AntennaScreen methods
        /// <summary>
        /// Displays what network the antenna is connected to once this screen is activated.
        /// </summary>
        public override void Activate()
        {
            string connectedString;
            // Use the default connection name if specified
            if (useDefaultConnectedNetworkName)
            {
                connectedString = defaultConnectedNetworkName;
            }
            // Otherwise, use the actual current network name
            else
            {
                connectedString = ShipStateManager.Instance.CurrentNetworkName;
            }
            connectedToText.text = $"CONNECTED TO:<br>{connectedString}";

            base.Activate();
        }
        #endregion
    }
}

