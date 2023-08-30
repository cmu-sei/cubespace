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
using TMPro;
using UI.CustomCursor;
using UnityEngine;

namespace Entities.Workstations.FlightEngineerParts
{
    /// <summary>
    /// A component for a dial at the Flight Engineer workstation.
    /// </summary>
    public class TrajectoryDial : WorkstationDial
    {
        #region Variables
        [Header("References")]
        /// <summary>
        /// A reference to the FlightEngineer workstation.
        /// </summary>
        [SerializeField]
        private FlightEngineer flightEngineer;
        /// <summary>
        /// The pipe to the left of the dial.
        /// </summary>
        [SerializeField]
        private WorkstationPipe inPipe;
        /// <summary>
        /// The pipe to the right of the dial (to the left of the angle text).
        /// </summary>
        [SerializeField]
        private WorkstationPipe outPipe;
        /// <summary>
        /// The formatted current angle of the dial.
        /// </summary>
        [SerializeField]
        private TMP_InputField angleText;

        /// <summary>
        /// The custom cursor object displayed when mousing over a dial.
        /// </summary>
        [SerializeField]
        private OnMouseoverAndDrag mouseoverCursor;

        /// <summary>
        /// The ID of this dial (red, green, or yellow).
        /// </summary>
        [Header("Dial ID")]
        public DialID dialID;

        /// <summary>
        /// The previous angle of the dial.
        /// </summary>
        private int prevAngle = 0;
        #endregion

        #region Unity event functions
        /// <summary>
        /// A Unity event function that disables the lock button on start and sets the dial's previous positions and rotations.
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// A Unity event function that subscribes to lock and workstation actions.
        /// </summary>
        void OnEnable()
        {
            // Static events
            FlightEngineer.OnLock += OnLock;

            // Non-static events
            flightEngineer.OnPowerOn += OnPowerOn;
            flightEngineer.OnPowerOff += OnPowerOff;
            flightEngineer.OnEnter += OnEnter;
            flightEngineer.OnExit += OnExit;
            flightEngineer.OnResetState += OnResetState;
        }

        /// <summary>
        /// A Unity event function that unsubscribes from lock and workstation actions.
        /// </summary>
        void OnDisable()
        {
            // Static events
            FlightEngineer.OnLock -= OnLock;

            // Non-static
            flightEngineer.OnPowerOn -= OnPowerOn;
            flightEngineer.OnPowerOff -= OnPowerOff;
            flightEngineer.OnEnter -= OnEnter;
            flightEngineer.OnExit -= OnExit;
            flightEngineer.OnResetState -= OnResetState;
        }

        /// <summary>
        /// A Unity event function that updates the dial's text and angle and plays a sound.
        /// </summary>
        protected override void OnMouseDrag()
        {
            base.OnMouseDrag();

            // If the dial can be rotated by the player, format the angle for display
            if (IsInteractable())
            {
                angleText.text = FormatAngle(totalAngle).ToString("000");
            }

            // Play an SFX if the angle has changed from the previous frame
            if ((int) totalAngle != prevAngle)
            {
                Audio.AudioPlayer.Instance.FlightEngineerDialTick(transform);
                // Handle negative numbers by converting the current angle to an integer
                prevAngle = (int)totalAngle;
            }
        }

        /// <summary>
        /// A Unity event function that sets the new current angle of the dial when the player stops rotating it.
        /// </summary>
        protected void OnMouseUp()
        {
            flightEngineer.ChangeDialAngle(dialID, (int)totalAngle);
        }
        #endregion

        #region Dial methods
        /// <summary>
        /// Checks whether the dial can be interacted with by the local player.
        /// </summary>
        /// <returns>Whether the Flight Engineer is powered and the dials are locked.</returns>
        protected override bool IsInteractable()
        {
            return flightEngineer.IsPowered && !flightEngineer.DialsLocked;
        }

        /// <summary>
        /// Activates emission on the outgoing pipe and plays a sound effect upon locking trajectories.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();
            
            // Turn the pipe on
            outPipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
            // Play a sound effect if the local player is at the Flight Engineer workstation
            if (flightEngineer.playerAtWorkstation && flightEngineer.playerAtWorkstation.isLocalPlayer) 
            {
                Audio.AudioPlayer.Instance.FlightEngineerTubeOn((int) dialID);
            }
        }

        /// <summary>
        /// Deactivates emission on the outgoing pipe, plays a sound effect, and sets the dial angle to its current value.
        /// </summary>
        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            // Set the dial angle
            flightEngineer.ChangeDialAngle(dialID, (int) totalAngle);

            // Turn the pipe off and play a sound effect
            outPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            Audio.AudioPlayer.Instance.FlightEngineerTubeOff((int) dialID);
        }
        #endregion

        #region Action callbacks
        /// <summary>
        /// Disables the mouseover cursor when the lock button is pressed.
        /// </summary>
        public void OnLock()
        {
            Debug.Log("Calling SetMouseoverCursorState(false), invoked from OnLock [TrajectoryDial.cs:184]");
            SetMouseoverCursorState(false);
        }

        /// <summary>
        /// Enables the emissions of the pipes and the mouse cursor when this the Flight Engineer is powered on.
        /// </summary>
        protected void OnPowerOn()
        {
            // Turn the incoming pipe's emission on
            inPipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
            // If the dial is activated and all dials are locked, turn on the emission of the outgoing pipe
            if (activated && flightEngineer.DialsLocked) 
            {
                outPipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
            }
            // Enable the mouse cursor
            SetMouseoverCursorState(true);
        }

        /// <summary>
        /// Disables the emissions of the pipes and the mouse cursor when this the Flight Engineer is powered off.
        /// </summary>
        protected void OnPowerOff()
        {
            // Turn the incoming pipe's emission on
            inPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            // If the dial is activated and all dials are locked, turn off the emission of the outgoing pipe
            if (activated && flightEngineer.DialsLocked) 
            {
                outPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            }
            // Disable the mouse cursor
            SetMouseoverCursorState(false);
        }

        /// <summary>
        /// Turns emission pipes on and sets the state of the dial when entering the Flight Engineer.
        /// </summary>
        public void OnEnter() 
        {
            // If the Flight Engineer is powered, turn on the incoming pipe emission and the cursor (depending on if all dials are locked)
            if (flightEngineer.IsPowered) 
            {
                inPipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
                SetMouseoverCursorState(!flightEngineer.DialsLocked);
            } 
            // Otherwise, turn off the incoing pipe emission
            else 
            {
                inPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            }
            // Initialize the state of the dials
            UpdateDialAppearance(flightEngineer.GetDialInfo(dialID));
        }

        /// <summary>
        /// Plays a sound effect for turning off a tube when exiting the Flight Engineer.
        /// </summary>
        public void OnExit() 
        {
            Audio.AudioPlayer.Instance.FlightEngineerTubeOff((int) dialID);
        }

        /// <summary>
        /// Resets the state of the dial when the Flight Engineer is reset.
        /// </summary>
        public void OnResetState()
        {
            // Reset the dial's activation state
            activated = false;

            // Reset the visual rotation of the dial, the total angle, and the stored angle of the dial on the server
            transform.rotation = originalRot;
            totalAngle = 0;
            flightEngineer.ChangeDialAngle(dialID, 0, true);

            // Set the emission power of the pipes and disable the mouseover cursor
            inPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            outPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            SetMouseoverCursorState(false);
        }
        #endregion

        #region Dial appearance methods
        /// <summary>
        /// Sets the appearance of a dial based on the given info provided and enables/disables the mouse cursor.
        /// </summary>
        /// <param name="dialInfo">The attributes of the dial.</param>
        public void UpdateDialAppearance(DialInfo dialInfo)
        {
            // Get the current and target angles of the dial
            totalAngle = dialInfo.value;
            targetAngle = dialInfo.target;

            // If the current angle equals the target angle, check if the lock button should light up and be enabled
            if (FormatAngle(totalAngle) == targetAngle) 
            {
                // If the local player is at the Flight Engineer, play the tube sound effect
                if (flightEngineer.playerAtWorkstation && flightEngineer.playerAtWorkstation.isLocalPlayer) 
                {
                    StartCoroutine(DelayTubeSFX());
                }

                // Turn on the emission of the pipe and activate the dial
                outPipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
                activated = true;

                // If the dials are locked, call the locking logic
                if (flightEngineer.DialsLocked)
                {
                    OnLock();
                }
                // Otherwise, call the unlocking logic
                else if (flightEngineer.IsPowered)
                {
                    OnUnlock();
                }
            }
            // Otherwise, mark the dial as unactivated, disable the lock button, and turn off the emission
            else 
            {
                activated = false;
                outPipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
            }

            // Set the rotation of the dial, manually rotate it, and set the text
            transform.rotation = originalRot;
            transform.Rotate(0, 0, totalAngle);
            angleText.text = FormatAngle(totalAngle).ToString("000");
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Enables the mouseover cursor when the trajectories are unlocked (if they are unlocked).
        /// </summary>
        public void OnUnlock()
        {
            SetMouseoverCursorState(true);
        }

        /// <summary>
        /// Delays playing the SFX for turning a tube on.
        /// </summary>
        /// <returns>A yield statement while waiting for a random amount of time before playing the tube SFX.</returns>
        IEnumerator DelayTubeSFX() 
        {
            yield return new WaitForSeconds(Random.Range(0f, 0.8f));
            Audio.AudioPlayer.Instance.FlightEngineerTubeOn((int) dialID);
        }

        /// <summary>
        /// Manually rotates the dial to a specified angle.
        /// </summary>
        /// <param name="enteredAngle">The angle to manually rotate the dial to.</param>
        public void ManuallyRotateDial(int enteredAngle)
        {
            flightEngineer.ChangeDialAngle(dialID, enteredAngle);
        }
        #endregion

        #region Mouse cursor methods
        /// <summary>
        /// Sets the active state of the mouse cursor.
        /// </summary>
        private void SetMouseoverCursorState(bool isActive)
        {
            Debug.Log("disabling special cursor for dial [TrajectoryDial.cs:352]");
            if (mouseoverCursor == null)
            {
                Debug.LogError("NULL mouseoverCursor [TrajectoryDial.cs:355]");
            }
            mouseoverCursor.active = isActive;
        }
        #endregion
    }
}

