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
    /// A middleman component that enables another specified component based on whether the workstation specified is powered.
    /// </summary>
    public class PoweredComponent : MonoBehaviour
    {
        // The component to enable when the given workstation is powered
        [SerializeField] protected Behaviour component;
        // The workstation whose power state determines the component's status
        [SerializeField] protected Workstation workstation;

        /// <summary>
        /// Unity event that enables the component if the workstation always has power or is currently powered.
        /// </summary>
        protected virtual void Start()
        {
            if (component != null)
            {
                component.enabled = workstation.AlwaysHasPower || workstation.IsPowered;
            }
        }

        /// <summary>
        /// Unity event that subscribes to powering on/off actions.
        /// </summary>
        void OnEnable() 
        {
            workstation.OnPowerOn += TurnOn;
            workstation.OnPowerOff += TurnOff;
        }

        /// <summary>
        /// Unity event that unsubscribes from powering on/off actions.
        /// </summary>
        void OnDisable() 
        {
            workstation.OnPowerOn -= TurnOn;
            workstation.OnPowerOff -= TurnOff;
        }

        /// <summary>
        /// Enables the given component. Called when the workstation is powered on.
        /// </summary>
        protected virtual void TurnOn() 
        {
            component.enabled = true;
        }

        /// <summary>
        /// Disables the given component. Called when the workstation is powered off.
        /// </summary>
        protected virtual void TurnOff() 
        {
            component.enabled = false;
        }
    }
}

