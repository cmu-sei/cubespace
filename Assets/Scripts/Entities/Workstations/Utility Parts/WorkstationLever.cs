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
namespace Entities.Workstations{
    /// <summary>
    /// A class representing a lever pulled by a client at a workstation.
    /// </summary>
    public class WorkstationLever : MonoBehaviour
    {
        /// <summary>
        /// Whether the lever has been flipped on or not.
        /// </summary>
        [SerializeField]
        protected bool activated = false;
        /// <summary>
        /// The pivot point of the lever.
        /// </summary>
        [SerializeField]        private Transform pivot;
        /// <summary>
        /// The plane representing the surface that the lever is on.
        /// </summary>
        public MeshCollider contactPlane;
        /// <summary>
        /// The rail along which the lever rotates.
        /// </summary>
        public Collider rail;

        /// <summary>
        /// The height at which the lever is activated.
        /// </summary>
        [SerializeField]
        protected float activeRot = 145.0f;
        /// <summary>
        /// The height at which the lever is deactivated.
        /// </summary>
        [SerializeField]
        protected float inactiveRot = 32.0f;
        /// <summary>
        /// The original rotation of the lever (its starting position).
        /// </summary>
        protected Quaternion originalRot;

        #region Unity event functions
        /// <summary>
        /// Sets the original rotation of the antenna.
        /// </summary>
        protected virtual void Start()
        {
            originalRot = transform.localRotation;
        }

        /// <summary>
        /// Updates the position of the lever with regards to mouse movement.
        /// </summary>
        protected virtual void OnMouseDrag()
        {
            // Raycast from the mouse to the lever
            RaycastHit hit;
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            contactPlane.Raycast(mouseRay, out hit, 20);
            // Let the user move the lever by getting to the closest point where they've clicked
            Vector3 currentPoint = rail.ClosestPoint(hit.point);
            // Get the new angle of the lever
            float angle = Vector3.Angle(Vector3.down, currentPoint - pivot.position);
            // Set the local rotation using the new angle
            transform.localRotation = Quaternion.Euler(new Vector3(angle - 57.687f, 0, 0));
            // Check if slider is in the active position; if so, call activation logic
            if (isActivatedPosition(angle) && !activated)
            {
                activated = true;
                OnActivate();
            }
            // Otherwise, if the slider is in the inactive state, call deactivation logic
            else if (isDeactivatedPosition(angle) && activated)
            {
                activated = false;
                OnDeactivate();
            }
        }
        #endregion

        #region Action functions
        /// <summary>
        /// Called when the lever is moved into an active position.
        /// </summary>
        protected virtual void OnActivate()
        {            // Blank, as the logic should be implemented in an override function
        }
        /// <summary>
        /// Called when the lever is moved into an inactive position.
        /// </summary>
        protected virtual void OnDeactivate()
        {            // Blank, as the logic should be implemented in an override function
        }

        /// <summary>
        /// Resets the local rotation of the lever back to its original rotation when Reset is clicked 
        /// in the Inspector's context menu, or when this is added to a GameObject for the first time.
        /// <para>
        /// This should be invoked as an action when the lever needs to be reset.
        /// </para>
        /// </summary>
        protected virtual void ResetState()        {
            transform.localRotation = originalRot;
        }
        #endregion
        #region Helper functions
        /// <summary>
        /// Checks whether the lever is in a position where it can be activated.
        /// </summary>
        /// <param name="angle">The current angle of the lever.</param>
        /// <returns>Whether the angle is past the point where the lever activates.</returns>
        protected virtual bool isActivatedPosition(float angle) 
        {
            return angle > activeRot;
        }

        /// <summary>
        /// Checks whether the lever is in a position where it can be deactivated.
        /// </summary>
        /// <param name="angle">The current angle of the lever.</param>
        /// <returns>Whether the angle is past the point where the lever deactivates.</returns>
        protected virtual bool isDeactivatedPosition(float angle)
        {
            return angle < inactiveRot;
        }
        #endregion
    }
}

