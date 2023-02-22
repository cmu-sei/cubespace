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

namespace Entities.Workstations 
{
    /// <summary>
    /// A component for a switch used on a workstation.
    /// </summary>
    public class WorkstationSwitch : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Whether the switch is interactable.
        /// </summary>
        public bool interactable = true;
        /// <summary>
        /// Whether the switch is activated.
        /// </summary>
        public bool activated = false;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Switches the activation state of this switch when the mouse clicks on it.
        /// </summary>
        protected virtual void OnMouseDown() 
        {
            if (Player.LocalCanInput)
            {
                activated = !activated;
            }
        }
        #endregion

        #region Interaction/activation functions
        /// <summary>
        /// Enables interactivity when this switch is powered on.
        /// </summary>
        protected virtual void OnPowerOn() 
        {
            interactable = true;
        }

        /// <summary>
        /// Disables interactivity when this switch is powered off.
        /// </summary>
        protected virtual void OnPowerOff() 
        {
            interactable = false;
        }

        /// <summary>
        /// Resets the activation state of the switch.
        /// </summary>
        protected virtual void ResetWorkstationSwitch() 
        {
            activated = false;
        }
        #endregion
    }
}


