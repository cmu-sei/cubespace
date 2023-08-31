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
using UnityEngine;

namespace Entities.Workstations
{
    /// <summary>
    /// A component for the LED strip shown on a workstation.
    /// </summary>
    public class WorkstationLEDStrip : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The list of LEDs.
        /// </summary>
        [SerializeField]
        private List<WorkstationLight> LEDs;
        /// <summary>
        /// The number of LEDs in the LED strip.
        /// </summary>
        [HideInInspector]
        public int LEDCount;

        /// <summary>
        /// The wait time between the LEDs lighting up.
        /// </summary>
        public float animationSpeed = 0.3f;

        /// <summary>
        /// The active LED animation.
        /// </summary>
        private Coroutine LEDAnimation;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Sets the number of LEDs in the strip by counting the LEDs in the list.
        /// </summary>
        void Start()
        {
            LEDCount = LEDs.Count;
        }
        #endregion

        #region Activation/deactivation functions
        /// <summary>
        /// Lights up the number of LEDs specified.
        /// </summary>
        /// <param name="numOfLEDsLit">The number of LEDs to light up.</param>
        public void LightNumLEDS(int numOfLEDsLit)
        {
            for (int i = 0; i < LEDs.Count; i++)
            {
                if (i <= numOfLEDsLit) 
                {
                    LEDs[i].Lit = true;  
                } 
                else 
                {
                    LEDs[i].Lit = false;
                }
            }
        }

        /// <summary>
        /// Starts a new animation of lighting up the LED strip.
        /// </summary>
        public void ActivateAnimation()
        {
            if (LEDAnimation != null)
            {
                StopCoroutine(LEDAnimation);
            }
            LEDAnimation = StartCoroutine(LEDStripAnimation());
        }

        /// <summary>
        /// Stops the existing LED Strip animation.
        /// </summary>
        public void DeactivateAnimation()
        {
            if (LEDAnimation != null)
            {
                StopCoroutine(LEDAnimation);
            }
        }

        /// <summary>
        /// Deactivates all LED lights at once.
        /// </summary>
        public void DeactivateAll()
        {
            if (LEDAnimation != null)
            {
                StopCoroutine(LEDAnimation);
            }
            for (int i = 0; i < LEDs.Count; i++)
            {
                LEDs[i].Lit = false;
            }
        }

        /// <summary>
        /// Activates all LED lights at once.
        /// </summary>
        public void ActivateAll()
        {
            if (LEDAnimation != null)
            {
                StopCoroutine(LEDAnimation);
            }
            for (int i = 0; i < LEDs.Count; i++)
            {
                LEDs[i].Lit = true;
            }
        }

        /// <summary>
        /// Plays the general LED strip animation.
        /// </summary>
        /// <returns>A yield statement while waiting for a duration between the lights turning on or off.</returns>
        IEnumerator LEDStripAnimation()
        {
            while (true)
            {
                for (int i = 0; i < LEDs.Count; i++)
                {
                    LEDs[i].Lit = true;
                    yield return new WaitForSeconds(animationSpeed);
                }
                yield return new WaitForSeconds(animationSpeed);
                for (int i = 0; i < LEDs.Count; i++)
                {
                    LEDs[i].Lit = false;
                    yield return new WaitForSeconds(animationSpeed * 0.4f);
                }
                yield return new WaitForSeconds(animationSpeed * 3);
            }
        }
        #endregion
    }
}

