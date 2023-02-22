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

namespace UI.HUD
{
    /// <summary>
    /// The slider used in the mission UI.
    /// </summary>
    public class UIHudMissionSlider : MonoBehaviour
    {
        /// <summary>
        /// The current position of the HUD mission slider.
        /// </summary>
        private Transform currentPosition;
        /// <summary>
        /// THe current velocity of this slider.
        /// </summary>
        private Vector3 currentVelocity;

        /// <summary>
        /// The time required to smooth out the position.
        /// </summary>
        [SerializeField]
        private float smoothTime = 1.0f;
        /// <summary>
        /// The maximum speed the slider can scroll at.
        /// </summary>
        [SerializeField]
        private float maxSpeed = 10f;

        /// <summary>
        /// Whether to horizontal movement.
        /// </summary>
        [SerializeField]
        private bool lockXMovement = true;

        /// <summary>
        /// Sets the position of this slider to be the given mission's centered position.
        /// </summary>
        /// <param name="mission">The mission item in the UI whose position should be set.</param>
        public void SetPosition(UIHudMissionItem mission)
        {
            currentPosition = mission.ItemCenterPosition;
        }

        /// <summary>
        /// Unity event function that sets the position of this slider.
        /// </summary>
        void Update()
        {
            if (currentPosition)
            {
                // Set the target to the current position OR the current position but our own x, depending on if lockXMovement is true or not
                var target = lockXMovement ? new Vector3(transform.position.x, currentPosition.position.y, currentPosition.position.z) : currentPosition.position;
                transform.position = Vector3.SmoothDamp(transform.position, target, ref currentVelocity, smoothTime, maxSpeed);
            }
        }
    }
}

