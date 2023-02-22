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
using Entities.Workstations.SensorStationParts;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// A sensor screen displayed on the SensorScreen workstation.
    /// </summary>
    public class SensorScreen : MonoBehaviour
    {        
        /// <summary>
        /// The controller dictating what screen to show at a given point at the SensorStation.
        /// </summary>
        [SerializeField]
        protected SensorStationScreenController _sensorScreenController;

        /// <summary>
        /// Whether this screen has a button attached to it.
        /// </summary>
        public bool hasButton; 
        /// <summary>
        /// The text displayed on the button.
        /// </summary>
        public string buttonText = "";
        /// <summary>
        /// Whether this screen should have a background.
        /// </summary>
        public bool hasBackground;
        /// <summary>
        /// The color scheme.
        /// </summary>
        public SensorModalWindow.ColorScheme colorScheme;

        /// <summary>
        /// The time required to wait between screens which automatically progress from one to the other.
        /// </summary>
        protected const float TRANSITION_TIME = 4f;

        /// <summary>
        /// Activates this object and makes it the active modal window.
        /// </summary>
        public virtual void Activate() 
        {
            gameObject.SetActive(true);
            _sensorScreenController.SetModalWindow(this);
        }

        /// <summary>
        /// Disables this screen.
        /// </summary>
        public virtual void Deactivate() 
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Invokes logic when a button is clicked.
        /// </summary>
        public virtual void OnButtonClick() 
        {
            //Empty for screens with no buttons
        }
    }
}

