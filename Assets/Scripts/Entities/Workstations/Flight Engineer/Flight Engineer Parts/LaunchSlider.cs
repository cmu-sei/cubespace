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
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Workstations.FlightEngineerParts 
{
    /// <summary>
    /// The slider used by the Flight Engineer to attempt to jump to a new location.
    /// </summary>
    public class LaunchSlider : WorkstationSlider
    {
        #region Variables
        /// <summary>
        /// The FlightEngineer object.
        /// </summary>
        [Header("GameObject References")]
        [FormerlySerializedAs("workstation")]
        [SerializeField]
        private FlightEngineer flightEngineer;
        /// <summary>
        /// A list of the pipes on the Flight Engineer whose emission power is set when the slider is dragged.
        /// </summary>
        [SerializeField]
        private List<WorkstationPipe> pipes;
        /// <summary>
        /// Parent object containing the strip of LEDs.
        /// </summary>
        [SerializeField]
        private WorkstationLEDStrip LEDParent;

        /// <summary>
        /// The maximum emission power of the pipes.
        /// </summary>
        [Header("Variables")]
        [SerializeField]
        private float maximumEmissionPower = 4.0f;
        /// <summary>
        /// The time to wait before performing an additional reset on the launch slider.
        /// </summary>
        [SerializeField]
        private float timeToWaitBeforeReset = 2.5f;

        /// <summary>
        /// The original vertical starting position of the slider. The lever uses X as its marker for position just due to its orientation.
        /// </summary>
        private float startX;
        #endregion

        #region Unity event functions
        /// <summary>
        /// A Unity event function that sets the interactability and initial position of the slider.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            interactable = flightEngineer.IsPowered;
            startX = transform.localPosition.x;
        }

        /// <summary>
        /// A Unity event function that subscribes to Flight Engineer actions.
        /// </summary>
        void OnEnable()
        {
            // Static events
            FlightEngineer.OnLaunchableChange += OnLaunchableChange;

            // Non-static workstation events
            flightEngineer.OnPowerOn += OnPowerOn;
            flightEngineer.OnPowerOff += OnPowerOff;
            flightEngineer.OnResetState += ResetState;
        }

        /// <summary>
        /// A Unity event function that unsubscribes from Flight Engineer actions.
        /// </summary>
        void OnDisable()
        {
            // Static events
            FlightEngineer.OnLaunchableChange -= OnLaunchableChange;

            // Non-static workstation events
            flightEngineer.OnPowerOn -= OnPowerOn;
            flightEngineer.OnPowerOff -= OnPowerOff;
            flightEngineer.OnResetState -= ResetState;
        }

        /// <summary>
        /// A Unity event function that re-displays the LED strip when the slider is dropped.
        /// </summary>
        protected void OnMouseDown() 
        {
            // Deactivate the LED lighting animation and re-light it based on the distance moved
            if (interactable) 
            {
                LEDParent.DeactivateAnimation();
                int numOfLEDsLit = Mathf.FloorToInt(((startX - transform.localPosition.x) / activeDistance) * LEDParent.LEDCount);
                LEDParent.LightNumLEDS(numOfLEDsLit);
            }
        }

        /// <summary>
        /// Unity event function that moves the slider when it is dragged by the mouse.
        /// </summary>
        protected override void OnMouseDrag() 
        {
            // Only move the slider if it is interactable (i.e., if we are ready to launch)
            if (interactable) 
            {
                base.OnMouseDrag();

                // Set the number of LEDs to light based on the distance moved by the slider out of the total distance
                float percent = (startX - transform.localPosition.x) / activeDistance;
                int numOfLEDsLit = Mathf.FloorToInt(percent * LEDParent.LEDCount);
                LEDParent.LightNumLEDS(numOfLEDsLit);

                // Play an accompanying SFX
                Audio.AudioPlayer.Instance.FlightEngineerSetEnginePercent(percent);

                // Set the emission power of the pipes based on the distance moved
                pipes.ForEach(p => p.SetEmissionPower(Mathf.Clamp(Mathf.Lerp(1f, maximumEmissionPower, percent), 1f, maximumEmissionPower), false));
            }
        }

        /// <summary>
        /// Unity event function that activates the LED strip animation based on where the lever was let go.
        /// </summary>
        protected void OnMouseUp() 
        {
            if (interactable) 
            {
                LEDParent.ActivateAnimation();
            }
        }
        #endregion

        #region Action methods
        /// <summary>
        /// Activates or deactivates the lights on the LED strip based on whether this slider is launchable.
        /// </summary>
        /// <param name="isLaunchable">Whether the slider is launchable.</param>
        protected void OnLaunchableChange(bool isLaunchable)
        {
            Debug.Log("OnLaunchableChange invoked, updating LEDs for slider [LaunchSlider.cs:156]");
            // If the slider can launch, activate the LEDs
            if (isLaunchable)
            {
                if (LEDParent == null)
                {
                    Debug.LogError("LEDParent null [LaunchSlider.cs:162]");
                }
                LEDParent.ActivateAnimation();
            }
            // Otherwise, turn off the LEDs
            else
            {
                if (LEDParent == null)
                {
                    Debug.LogError("LEDParent null [LaunchSlider.cs:171]");
                }
                LEDParent.DeactivateAll();
            }

            if (flightEngineer == null)
            {
                Debug.LogError("Null flight engineer [LaunchSlider.cs:178]");
            }
            // This slider can be moved if the ship is launchable and the Flight Engineer is powered
            interactable = flightEngineer.IsLaunchable() && flightEngineer.IsPowered;
        }

        /// <summary>
        /// Deactivates the lights, disables interaction with the slider, and resets the slider's position.
        /// </summary>
        protected override void ResetState()
        {
            LEDParent.DeactivateAll();
            interactable = false;
            StartCoroutine(DelayReset());
        }

        /// <summary>
        /// A helper coroutine to delay resetting the position of the slider.
        /// This is needed so that the slider doesn't jump back down as soon as a launch occurs.
        /// </summary>
        /// <returns>A yield while waiting until the slider can move back to its initial position.</returns>
        private IEnumerator DelayReset()
        {
            yield return new WaitForSeconds(timeToWaitBeforeReset);
            base.ResetState();
        }

        /// <summary>
        /// Changes the interactability of the slider and checks if the LEDs should be activated when the Flight Engineer is powered on.
        /// </summary>
        protected override void OnPowerOn()
        {
            // Check if the slider is interactable
            interactable = flightEngineer.IsLaunchable() && flightEngineer.IsPowered;

            // If the Flight Engineer is launchable, check if this slider is also launchable
            if (flightEngineer.IsLaunchable()) 
            {
                OnLaunchableChange(true);
            }
        }

        /// <summary>
        /// Changes the interactability of the slider and checks if the LEDs should be deactivated when the Flight Engineer is powered off.
        /// </summary>
        protected override void OnPowerOff()
        {
            // If the Flight Engineer is launchable, this slider should not be, because the Flight Engineer would not be powered
            if (flightEngineer.IsLaunchable()) 
            {
                OnLaunchableChange(false);
            }

            // Check if the slider is interactable
            interactable = flightEngineer.IsLaunchable() && flightEngineer.IsPowered;
        }
        #endregion

        #region WorkstationSlider methods
        /// <summary>
        /// Checks whether the slider is in a position where it can be activated.
        /// </summary>
        /// <returns>Whether the slider is in the position where it can be activated.</returns>
        protected override bool IsActivated()
        {
            return startX - transform.localPosition.x > activeDistance;
        }

        /// <summary>
        /// Attempts a jump and performs audiovisual effects if this slider is used in the active position.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();

            // If the Flight Engineer is launchable and powered, attempt a jump, display visual effects, play sounds
            if (flightEngineer.IsLaunchable() && flightEngineer.IsPowered)
            {
                // Try jumping to a different location
                ShipStateManager.Instance.CmdJump();

                // Activate all lights and play the lever lock and launch sounds
                LEDParent.ActivateAll();
                Audio.AudioPlayer.Instance.FlightEngineerLeverLock(transform);
                Audio.AudioPlayer.Instance.EngineTakeoff();

                // Mark the slider as non-interactable and pull the player out of the Flight Engineer
                interactable = false;
                flightEngineer.Deactivate(true);
            }
        }
        #endregion
    }
}


