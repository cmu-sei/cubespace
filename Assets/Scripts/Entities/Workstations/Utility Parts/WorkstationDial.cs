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
    /// Class for a basic dial used at a workstation. Most behavior in this class should be overridden.
    /// </summary>
    public class WorkstationDial : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Values representing what to set as the original rotation vector of the dial.
        /// </summary>
        [SerializeField]
        Vector4 originalRotVector = new Vector4(0.35897f, -0.60967f, -0.36094f, -0.60758f);

        /// <summary>
        /// Whether this dial has reached its target angle. This should initially be false, as it is flipped
        /// to true in OnActivate.
        /// </summary>
        [HideInInspector]
        public bool activated = false;
        /// <summary>
        /// The angle the player should spin the dial towards.
        /// </summary>
        [HideInInspector]
        public int targetAngle = -1;

        /// <summary>
        /// The original rotation of the dial.
        /// </summary>
        protected Quaternion originalRot;
        /// <summary>
        /// The previous position of the original rotation.
        /// </summary>
        protected Vector3 prevPos;
        /// <summary>
        /// The total angle of the rotation, normalized to 360 degrees in OnMouseDrag.
        /// </summary>
        protected float totalAngle;
        /// <summary>
        /// The factor affecting how quickly this dial changes its angle when moved by player input.
        /// </summary>
        protected float speedMultiplier = 0.15f;
        /// <summary>
        /// The upper limit of how quickly the dial can move (used for the lower limit as well).
        /// </summary>
        protected float maxSpeed = 15.0f;
        #endregion

        #region Unity event functions
        /// <summary>
        /// A Unity event function that sets the previous position and original rotations of this dial when it is first created.
        /// </summary>
        protected virtual void Start()
        {
            // Set base values for the previous position and original rotation of the dial
            prevPos = transform.position;
            originalRot = new Quaternion(originalRotVector.x, originalRotVector.y, originalRotVector.z, originalRotVector.w);
            
            // Set the total angle reached by the dial (no angle reached)
            totalAngle = 0;
        }

        /// <summary>
        /// A Unity event function that sets the dial's previous position to the player's current mouse position when they
        /// start moving the dial, so long as they can move it.
        /// </summary>
        protected virtual void OnMouseDown() 
        {
            if (Player.LocalCanInput)
            {
                prevPos = Input.mousePosition;
            }
        }

        /// <summary>
        /// A Unity event function that sets the dial's angle based on a series of equations that modify the rotation it was moved.
        /// </summary>
        protected virtual void OnMouseDrag()
        {
            // If the dial can be moved by this player, set its rotation
            if (IsInteractable() && Player.LocalCanInput) 
            {
                // First, get the change in the dial's rotation between frames
                Vector3 vector = Input.mousePosition - prevPos;
                // Then clamp that rotation between the minimum and maximum speed it can move at
                float rotation = Mathf.Clamp(vector.x * speedMultiplier, -maxSpeed, maxSpeed);

                // If the total angle moved so far and this new rotation sum to be greater than 359 degrees, the real increase in rotation is negative
                if (totalAngle + rotation > 359.0f) 
                {
                    // This line was causing a bug that prevented the dial from spinning clockwise beyond 359, commenting it out fixed it
                    // Not sure what should or shouldn't be here
                    //rotation = 359f - totalAngle;
                }
                // Otherwise, if both sum to be less than -360 degrees, the real increase in rotation is positive
                else if (totalAngle + rotation < -360.0f) 
                {
                    rotation = -360f - totalAngle;
                }

                // Increase the total angle of the dial and rotate it
                totalAngle += rotation;
                transform.Rotate(new Vector3(0, 0, rotation), Space.Self);

                // If the dial is not activated and the dial's angle equals its target angle, flip the dial to be activated
                if (!activated && FormatAngle(totalAngle) == targetAngle) 
                {
                    OnActivate();
                }
                // Otherwise, if the dial had reached its target angle before but is not there now, deactivate the dial
                else if (activated && FormatAngle(totalAngle) != targetAngle) 
                {
                    OnDeactivate();
                }
                
                // The previous position of the dial is the current mouse position
                prevPos = Input.mousePosition;
            }
        }
        #endregion

        #region Dial methods
        /// <summary>
        /// A function that can be used to check if the player is currently allowed to manipulate this dial.
        /// This function should be overriden.
        /// </summary>
        /// <returns>Whether the dial is interactable. This is true by default.</returns>
        protected virtual bool IsInteractable()
        {
            return true;
        }

        /// <summary>
        /// A function that can be used to perform logic when this dial is activated.
        /// This function should be overriden, and its override should call base.OnActivate.
        /// </summary>
        protected virtual void OnActivate() 
        {
            activated = true;
        }

        /// <summary>
        /// A function that can be used to perform logic when this dial is deactivated.
        /// This function should be overriden, and its override should call base.OnDeactivate.
        /// </summary>
        protected virtual void OnDeactivate() 
        {
            activated = false;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Normalizes a given angle to be within the range of 0 to 360 as an integer.
        /// </summary>
        /// <param name="angle">An angle to format.</param>
        /// <returns>The integer representation of the angle, normalized between 0 and 360.</returns>
        public static int FormatAngle(float angle) 
        {
            return (((int) angle) % 360 + 360) % 360;
        }

        /// <summary>
        /// Normalizes the total angle of the dial to be within the range of 0 to 360 as an integer.
        /// </summary>
        /// <returns>The integer representation of the total angle, normalized between 0 and 360.</returns>
        public int GetAngle() 
        {
            return FormatAngle(totalAngle);
        }
        #endregion
    }
}


