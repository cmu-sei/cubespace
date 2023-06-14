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
using UnityEngine;

namespace Entities.Workstations.NavReaderParts 
{
    /// <summary>
    /// A component for the galaxy display used on the NavReader.
    /// </summary>
    public class NavReaderGalaxy : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The NavReader workstation reference.
        /// </summary>
        [SerializeField]
        private NavReader navReader;
        /// <summary>
        /// The renderer used for the galaxy on the NavReader.
        /// </summary>
        [SerializeField]
        private new MeshRenderer renderer;
        /// <summary>
        /// The speed with which the galaxy rotates.
        /// </summary>
        [SerializeField]
        [Range(0.01f, 0.15f)]
        private float rotationSpeed = 0.015f;
        #endregion

        #region Unity event functions

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            renderer.enabled = navReader.IsPowered;

            navReader.OnPowerOn += OnPowerOn;
            navReader.OnPowerOff += OnPowerOff;
        }

        /// <summary>
        /// Unity event function that enables the renderer if the NavReader is powered on and starts rotating the galaxy.
        /// </summary>
        private void Start()
        {
            if (navReader.IsPowered)
            {
                StartCoroutine(GalaxyAnimation());
            }
        }

        /// <summary>
        /// Unity event function that unsubscribes from power on and power off events.
        /// </summary>
        private void OnDestroy()
        {
            navReader.OnPowerOn -= OnPowerOn;
            navReader.OnPowerOff -= OnPowerOff;
        }
        #endregion

        #region Main functions
        /// <summary>
        /// Enables the renderer and starts spinning the galaxy.
        /// </summary>
        private void OnPowerOn() 
        {
            renderer.enabled = true;
            StartCoroutine(GalaxyAnimation());
        }

        /// <summary>
        /// Turns off the renderer and stops the galaxy spinning.
        /// </summary>
        private void OnPowerOff() 
        {
            renderer.enabled = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// Rotates the galaxy sprite.
        /// </summary>
        /// <returns>A yield statement while waiting for a rotation to finish.</returns>
        IEnumerator GalaxyAnimation() 
        {
            while (navReader.IsPowered)
            {
                // The rotation speed is negated so that it spins in the direction of the arms
                transform.Rotate(0, 0, -rotationSpeed, Space.Self);
                yield return null;
            }
        }
        #endregion
    }
}

