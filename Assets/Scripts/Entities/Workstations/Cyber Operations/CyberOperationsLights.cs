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

namespace Entities.Workstations.CyberOperationsParts
{
    /// <summary>
    /// A component which controls lights on the CyberOperations workstation.
    /// </summary>
    public class CyberOperationsLights : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// A reference to the CyberOperations workstation.
        /// </summary>
        private CyberOperations _cyberOperations;
        
        /// <summary>
        /// The light on the mouse.
        /// </summary>
        [SerializeField]
        private WorkstationLight _mouseLight;
        /// <summary>
        /// The status light to indicate whether the workstation can be used.
        /// </summary>
        [SerializeField]
        private WorkstationLight _statusLight;
        /// <summary>
        /// The light indicating whether this workstation has power.
        /// </summary>
        [SerializeField]
        private WorkstationLight _powerLight;
        /// <summary>
        /// The square lights that randomly blink on CyberOperations.
        /// </summary>
        [SerializeField]
        private WorkstationLight[] _squareLights;
        /// <summary>
        /// The possible colors to randomize the lights as.
        /// </summary>
        [SerializeField]
        private Color[] _possibleColors;
        /// <summary>
        /// The interval between blinking lights.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 10f)]
        private float _blinkingInterval = 1f;

        /// <summary>
        /// The animation currently playing.
        /// </summary>
        private Coroutine _currentAnim;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that subscribes to enter and exit events on the CyberOperations workstation.
        /// </summary>
        private void OnEnable()
        {
            if (_cyberOperations == null)
            {
                _cyberOperations = GetComponent<CyberOperations>();
            }
            _cyberOperations.OnEnter += OnEnter;
            _cyberOperations.OnExit += OnExit;
        }

        /// <summary>
        /// Unity event function that unsubscribes from enter and exit events on the CyberOperations workstation.
        /// </summary>
        private void OnDisable()
        {
            _cyberOperations.OnEnter -= OnEnter;
            _cyberOperations.OnExit -= OnExit;
        }
        #endregion

        #region Event callback methods
        /// <summary>
        /// Emits lights and starts blinking lights when entering the CyberOperations workstation.
        /// </summary>
        private void OnEnter()
        {
            _statusLight.Lit = true;
            _powerLight.Lit = true;
            _mouseLight.Pulsing = true;

            if (_currentAnim != null)
            {
                StopCoroutine(_currentAnim);
            }
            _currentAnim = StartCoroutine(RandomBlinkingLights());
        }

        /// <summary>
        /// Stops emitting lights and stops blinking lights when exiting the CyberOperations workstation.
        /// </summary>
        private void OnExit()
        {
            _statusLight.Lit = false;
            _powerLight.Lit = false;
            _mouseLight.Pulsing = false;

            if (_currentAnim != null)
            {
                StopCoroutine(_currentAnim);
            }
        }

        /// <summary>
        /// Randomly blinks lights on the CyberOperations workstation.
        /// </summary>
        /// <returns>A yield statement that blinks waits between blinking lights randomly.</returns>
        private IEnumerator RandomBlinkingLights()
        {
            int blinkingLightsArrayRange = _squareLights.Length - 1;
            int maxLightsChangedPerTick = blinkingLightsArrayRange / 2;
            int colorArrayRange = _possibleColors.Length - 1;

            while (true)
            {
                for (int i = 0; i < Random.Range(0, maxLightsChangedPerTick); i++)
                {
                    int r = Random.Range(0, blinkingLightsArrayRange);
                    Color c = _possibleColors[Random.Range(0, colorArrayRange)];
                    _squareLights[r].LightColor = c;

                    // Needed to see color change
                    _squareLights[r].LightInstant();
                }

                float delay = Mathf.Clamp(Random.Range(_blinkingInterval - 0.5f, _blinkingInterval + 2f), 0.1f, _blinkingInterval + 2f);
                yield return new WaitForSeconds(delay);
            }
        }
        #endregion
    }
}


