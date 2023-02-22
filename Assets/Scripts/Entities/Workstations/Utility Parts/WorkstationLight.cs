/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using System.Collections;
using UnityEngine;

namespace Entities.Workstations
{
    /// <summary>
    /// The component for a light used at a workstation.
    /// </summary>
    public class WorkstationLight : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Whether this workstation light is lit up.
        /// </summary>
        [SerializeField]
        private bool lit;
        /// <summary>
        /// The time it takes for the light to fade on or off.
        /// </summary>
        [SerializeField]
        private float lightTime = 0.25f;
        /// <summary>
        /// The index of the material which can be modified.
        /// </summary>
        [SerializeField]
        private int materialIndex = 0;
        /// <summary>
        /// Whether this light is pulsing.
        /// </summary>
        [SerializeField]
        private bool pulsing = false;
        /// <summary>
        /// The total time for the light to pulse.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 8f)]
        private float pulseDuration = 3f;
        /// <summary>
        /// The multiplier that should be applied to the light.
        /// </summary>
        public float totalLightMultiplier = 2f;

        /// <summary>
        /// Whether the light is lit or not. Setting this variable affects the visual appearance of the light.
        /// </summary>
        public bool Lit 
        {
            get 
            { 
                return lit; 
            }
            set 
            {   
                if (lit != value) 
                {
                    lit = value;
                    if (lit) 
                    {
                        Light();
                    } 
                    else 
                    {
                        Unlight();
                    }
                }
            }
        }

        /// <summary>
        /// Whether this light is pulsing.
        /// </summary>
        public bool Pulsing
        {
            get
            {
                return pulsing;
            }
            set
            {
                if (value != pulsing)
                {
                    lit = value;
                    if (currentAnim != null)
                    {
                        StopCoroutine(currentAnim);
                        currentAnim = null;
                    }

                    if (value)
                    {
                        UnlightInstant();
                        currentAnim = StartCoroutine(LightPulsing());
                    }
                    else
                    {
                        UnlightInstant();
                    }

                    pulsing = value;
                }
            }
        }

        /// <summary>
        /// The renderer used on this light.
        /// </summary>
        private Renderer _renderer;
        /// <summary>
        /// The material used on this light.
        /// </summary>
        private Material _material;

        /// <summary>
        /// The color the light should be set to. If this is black, it will become the same as the emissiveColor.
        /// </summary>
        [SerializeField]
        private Color lightColor;

        /// <summary>
        /// The color of the light.
        /// </summary>
        public Color LightColor
        {
            get
            {
                return lightColor;
            }
            set
            {
                lightColor = value;
            }
        }

        /// <summary>
        /// The currently playing animation.
        /// </summary>
        private Coroutine currentAnim;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the renderer and material of the light.
        /// </summary>
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _material = _renderer.materials[materialIndex];
        }

        /// <summary>
        /// Unity event function that starts pulsing the light, or lights/unlights it.
        /// </summary>
        protected virtual void Start() 
        {
            if (pulsing)
            {
                currentAnim = StartCoroutine(LightPulsing());
            }
            else if (lit) 
            {
                LightInstant();
            } 
            else 
            {
                UnlightInstant();
            }
        }
        #endregion

        #region Lighting functions
        /// <summary>
        /// Lights up the light by changing the emission and starting a coroutine.
        /// </summary>
        protected virtual void Light()
        {
            _renderer.materials[materialIndex].EnableKeyword("_EMISSIVE_COLOR_MAP");
            _renderer.materials[materialIndex].EnableKeyword("_EMISSION");
            if (currentAnim != null) 
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                currentAnim = StartCoroutine(LightAnim());
            }
            else
            {
                LightInstant();
            }
        }

        /// <summary>
        /// Unlights the light.
        /// </summary>
        protected virtual void Unlight() 
        {
            if (currentAnim != null) 
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                currentAnim = StartCoroutine(UnlightAnim());
            }
            else
            {
                UnlightInstant();
            }
        }

        /// <summary>
        /// Gradually lights the light.
        /// </summary>
        /// <returns>A yield statement while waiting for the light to light up.</returns>
        IEnumerator LightAnim() 
        {
            if (_renderer != null && _renderer.materials.Length > 0)
            {
                float lightMultiplier = 0;
                while (lightMultiplier < totalLightMultiplier)
                {
                    lightMultiplier += (totalLightMultiplier / lightTime) * Time.deltaTime;
                    _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * lightMultiplier);
                    yield return null;
                }
                _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * totalLightMultiplier);
            }
        }

        /// <summary>
        /// Gradually unlights the light.
        /// </summary>
        /// <returns>A yield statement while waiting for the light to unlight.</returns>
        IEnumerator UnlightAnim() 
        {
            if (_renderer != null && _renderer.materials.Length > 0)
            {
                float lightMultiplier = totalLightMultiplier;
                while (lightMultiplier > 0)
                {
                    lightMultiplier -= (totalLightMultiplier / lightTime) * Time.deltaTime;
                    _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * lightMultiplier);
                    yield return null;
                }
                _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * 0f);
                _renderer.materials[materialIndex].DisableKeyword("_EMISSION");
            }            
        }

        /// <summary>
        /// Starts pulsing the light.
        /// </summary>
        /// <returns>A yield statement while waiting for the light to light up or unlight.</returns>
        IEnumerator LightPulsing()
        {
            if (_renderer != null && _renderer.materials.Length > 0)
            {
                float lightMultiplier = 0;
                _renderer.materials[materialIndex].EnableKeyword("_EMISSION");
                _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * 0);

                while (true)
                {
                    while (lightMultiplier < totalLightMultiplier)
                    {
                        lightMultiplier += (totalLightMultiplier / pulseDuration) * Time.deltaTime;
                        _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * lightMultiplier);
                        yield return null;
                    }
                    while (lightMultiplier > totalLightMultiplier * 0.35f)
                    {
                        lightMultiplier -= (totalLightMultiplier / pulseDuration) * Time.deltaTime;
                        _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * lightMultiplier);
                        yield return null;
                    }
                }
            }
        }

        // Calling these from outside this script will mess up Lit variable, so either use Lit or only use the instant functions; do not use both
        /// <summary>
        /// Instantly lights a light by enabling its emission.
        /// </summary>
        public void LightInstant()
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }
            _renderer.materials[materialIndex].EnableKeyword("_EMISSION");
            _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * totalLightMultiplier);
        }

        /// <summary>
        /// Instantly unlights a light by disabling its emission.
        /// </summary>
        public void UnlightInstant()
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }
            _renderer.materials[materialIndex].SetColor("_EmissionColor", lightColor * 0f);
            _renderer.materials[materialIndex].DisableKeyword("_EMISSION");
        }

        /// <summary>
        /// Sets whether the light is lit or not.
        /// </summary>
        /// <param name="newLit">Whether the light is lit or not.</param>
        public void SetLit(bool newLit)
        {
            lit = newLit;
        }
        #endregion
    }
}


