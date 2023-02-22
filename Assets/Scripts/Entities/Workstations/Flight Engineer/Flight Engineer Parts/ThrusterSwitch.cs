/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Entities.Workstations.FlightEngineerParts
{
    /// <summary>
    /// A component for a thruster on the Flight Engineer, used as a set that allow launching to a
    /// new location when flipped.
    /// </summary>
    public class ThrusterSwitch : WorkstationSwitch
    {
        #region Variables
        /// <summary>
        /// The WorkstationManager object used to track all workstations in the scene.
        /// </summary>
        [Header("GameObject references")]
        [SerializeField]
        private WorkstationManager workstationManager;
        /// <summary>
        /// A reference to the Flight Engineer workstation.
        /// </summary>
        [SerializeField]
        private FlightEngineer flightEngineer;
        /// <summary>
        /// The UI animation that plays when the thruster switch is flipped.
        /// </summary>
        [SerializeField]
        private Animator animator;

        /// <summary>
        /// The rotation of the switch. For the thrusters, this number is either 0 or 180.
        /// </summary>
        [Header("Variables")]
        [SerializeField]
        private float switchRotation;
        /// <summary>
        /// The ID of the thruster.
        /// </summary>
        [SerializeField]
        private int id;

        /// <summary>
        /// The ID of the thruster switch converted into an enum.
        /// </summary>
        private WorkstationID thrusterID;
        /// <summary>
        /// The PowerRouting component which controls the flow of power between workstations in-scene.
        /// </summary>
        private PowerRouting.PowerRouting powerRouting;
        /// <summary>
        /// The original rotation of the thruster.
        /// </summary>
        private Quaternion originalRotation;
        #endregion

        #region Unity event functions
        /// <summary>
        /// A Unity event function that subscribes to Flight Engineer actions, sets the thruster this switch is associated with,
        /// and sets up a reference to the Power Routing object.
        /// </summary>
        void Start()
        {
            // Retrieve the Power Routing workstation from the Workstation Manager
            powerRouting = workstationManager.GetWorkstation(WorkstationID.PowerRouting) as PowerRouting.PowerRouting;

            // This is interactable as long as the Flight Engineer is powered, and its original rotation should be stored
            interactable = flightEngineer.IsPowered;
            originalRotation = transform.rotation;

            // Set the actual enum ID of the thruster associated with this switch
            switch (id)
            {
                case 0:
                case 1:
                    thrusterID = WorkstationID.ThrustersAB;
                    break;
                default:
                    thrusterID = WorkstationID.ThrustersCD;
                    break;
            }
            
            // Subscribe to Flight Engineer actions
            flightEngineer.OnPowerOn += OnPowerOn;
            flightEngineer.OnPowerOff += OnPowerOff;
            flightEngineer.OnResetState += ResetWorkstationSwitch;
            flightEngineer.OnEnter += OnEnter;
        }

        /// <summary>
        /// A Unity event function that unsubscribes from Flight Engineer actions.
        /// </summary>
        void OnDestroy()
        {
            // Unsubscribe from Flight Engineer actions
            flightEngineer.OnPowerOn -= OnPowerOn;
            flightEngineer.OnPowerOff -= OnPowerOff;
            flightEngineer.OnResetState -= ResetWorkstationSwitch;
            flightEngineer.OnEnter -= OnEnter;
        }

        /// <summary>
        /// Flips the thruster to activate or vice versa.
        /// </summary>
        protected override void OnMouseDown()
        {
            // If this thruster switch can be flipped, activate it if the Flight Engineer is powered
            if (interactable && Player.LocalCanInput) 
            {
                if (powerRouting.GetPowerStateForWorkstation(thrusterID)) 
                {
                    base.OnMouseDown();

                    // If this switch is activated, set it to the activated position
                    if (activated)
                    {
                        transform.Rotate(0, 0, switchRotation, Space.Self);
                        animator.SetBool("isActivated", true);
                    }
                    // Otherwise, set it to the non-activated position
                    else
                    {
                        transform.Rotate(0, 0, -switchRotation, Space.Self);
                        animator.SetBool("isActivated", false);
                    }

                    // Set the state of this thruster
                    flightEngineer.ChangeSwitchState(id, activated);

                    // Play SFX
                    Audio.AudioPlayer.Instance.FlightEngineerSwitch(transform);
                    flightEngineer.SetThrusterSFX(id, activated, transform);
                } 
                else 
                {
                    Audio.AudioPlayer.Instance.UIError();
                }
            }
        }
        #endregion

        #region Action methods
        /// <summary>
        /// Enables the thruster image and animator activation state when the FlightEngineer is powered on.
        /// </summary>
        protected override void OnPowerOn()
        {
            base.OnPowerOn();
            animator.GetComponent<Image>().enabled = true;
            if (activated)
            {
                animator.SetBool("isActivated", true);
            }
        }

        /// <summary>
        /// Disables the thruster image and animator activation state when the FlightEngineer is powered off.
        /// </summary>
        protected override void OnPowerOff()
        {
            base.OnPowerOff();
            animator.GetComponent<Image>().enabled = false;
            animator.SetBool("isActivated", false);
        }

        /// <summary>
        /// Resets the thruster switch appearance.
        /// </summary>
        protected override void ResetWorkstationSwitch()
        {
            base.ResetWorkstationSwitch();
            transform.rotation = originalRotation;
            animator.SetBool("isActivated", false);
        }

        /// <summary>
        /// Sets the thruster's appearance when the Flight Engineer is entered.
        /// </summary>
        protected void OnEnter()
        {
            // Set the activation state and rotation of this switch
            activated = ShipStateManager.Instance.thrusters[id];
            transform.rotation = originalRotation;

            // If this is activated, rotate the switch to appear activated
            if (activated)
            {
                transform.Rotate(0, 0, switchRotation, Space.Self);
            }
            // If the Flight Engineer is powered, set this thruster's activation state in its animator
            if (flightEngineer.IsPowered)
            {
                animator.SetBool("isActivated", activated);
            }
            // Play SFX
            flightEngineer.SetThrusterSFX(id, activated, transform, true);
        }
    }
    #endregion
}

