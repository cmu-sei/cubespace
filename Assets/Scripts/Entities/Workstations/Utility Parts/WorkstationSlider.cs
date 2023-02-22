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
    /// A component for a slider used at a workstation, moved with the mouse.
    /// </summary>
    public class WorkstationSlider : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Whether the slider is interactable.
        /// </summary>
        public bool interactable = true;
        /// <summary>
        /// Whether the slider is activated.
        /// </summary>
        [SerializeField]
        protected bool activated = false;

        /// <summary>
        /// The plane representing the surface that the slider is on.
        /// </summary>
        public MeshCollider contactPlane;

        /// <summary>
        /// The rail along which the slider moves.
        /// </summary>
        public Collider rail;
        /// <summary>
        /// The height at which the slider is activated.
        /// </summary>
        public float activeDistance = 11.5f;
        /// <summary>
        /// The resistance of the slider. The smaller the number, the less resistance, and vice versa.
        /// The rail for the slider must be proportionally extended past active distance (twice as long for half the resistance).
        /// </summary>
        public float resistanceFactor = 1f; 

        /// <summary>
        /// The original position of the slider.
        /// </summary>
        private Vector3 originalPos;
        /// <summary>
        /// The point on the rail where the slider is located.
        /// </summary>
        private Vector3 pointToRail;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that sets the original point of the slider and its current position on the rail.
        /// </summary>
        protected virtual void Start()
        {
            originalPos = transform.position;
            pointToRail = rail.ClosestPoint(originalPos) - originalPos;
        }

        /// <summary>
        /// Updates the position of the lever with regards to mouse movement.
        /// </summary>
        protected virtual void OnMouseDrag()
        {
            if (interactable && Player.LocalCanInput)
            {
                RaycastHit hit;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                contactPlane.Raycast(mouseRay, out hit, 20);
                Vector3 currentPoint = rail.ClosestPoint(hit.point);
                Vector3 newPos = currentPoint - pointToRail;

                transform.position = ((newPos - originalPos) * (1f / resistanceFactor)) + originalPos;

                // Check if slider is in the active position
                if (IsActivated() && !activated)
                {
                    activated = true;
                    OnActivate();
                }
                else if (!IsActivated() && activated)
                {
                    activated = false;
                    OnDeactivate();
                }
            }
        }
        #endregion

        #region Activation/deactivation methods
        /// <summary>
        /// Checks whether the slider is in an active position. This defaults to checking the y position, 
        /// but it can be overridden.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsActivated() 
        {
            return transform.localPosition.y > activeDistance;
        }

        /// <summary>
        /// General function that can be used to perform logic when the slider is activated.
        /// </summary>
        protected virtual void OnActivate() 
        {

        }

        /// <summary>
        /// General function that can be used to perform logic when the slider is deactivated.
        /// </summary>
        protected virtual void OnDeactivate() 
        {
            
        }
        #endregion

        #region Power/reset methods
        /// <summary>
        /// Makes this slider interactable when powered on.
        /// </summary>
        protected virtual void OnPowerOn()
        {
            interactable = true;
        }

        /// <summary>
        /// Makes this slider interactable when powered off.
        /// </summary>
        protected virtual void OnPowerOff()
        {
            interactable = false;
        }

        /// <summary>
        /// Resets the posiiton of the slider back to its original position.
        /// </summary>
        protected virtual void ResetState()
        {
            transform.position = originalPos;
        }
        #endregion
    }
}

