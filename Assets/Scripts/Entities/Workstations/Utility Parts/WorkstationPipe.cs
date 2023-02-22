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
using Managers;
using UnityEngine;

namespace Entities.Workstations 
{
    /// <summary>
    /// A class defining behavior of a pipe at a workstation.
    /// </summary>
    public class WorkstationPipe : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The WorkstationManager object.
        /// </summary>
        [SerializeField]
        private WorkstationManager workstationManager;
        /// <summary>
        /// The ID of the workstation this pipe is connected to.
        /// </summary>
        [SerializeField]
        private WorkstationID workstationID;

        /// <summary>
        /// The emission power at which the pipe is considered "on".
        /// </summary>
        public const float ON_EMISSION_POWER = 1.1f;
        /// <summary>
        /// The emission power at which the pipe is considered "off".
        /// </summary>
        public const float OFF_EMISSION_POWER = 0f;

        /// <summary>
        /// The time needed to fully fade the emission on the pipe.
        /// </summary>
        public float fadeTime = 1f;
        /// <summary>
        /// Whether the pipe is set up and ready to perform emission logic. Derives from a public variable.
        /// </summary>        
        public bool IsSetUp
        {
            get
            {
                return isSetUp;
            }
        }
        /// <summary>
        /// Whether the pipe is set up and ready to perform emission logic.
        /// </summary>
        private bool isSetUp = false;

        /// <summary>
        /// The workstation this pipe is connected to.
        /// </summary>
        private Workstation workstation;
        /// <summary>
        /// The renderer to use for the pipe.
        /// </summary>
        private new Renderer renderer;
        /// <summary>
        /// The property block of the pipe.
        /// </summary>
        private MaterialPropertyBlock propBlock;
        /// <summary>
        /// The emission animation currently playing for the pipe.
        /// </summary>
        private Coroutine currentAnimation;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets a reference to the pipe's renderer and property block, gets the workstation
        /// this pipe is assigned to, and subscribes to the workstation's OnEnter action via Setup.
        /// </summary>
        void Start()
        {   
            renderer = GetComponent<Renderer>(); 
            propBlock = new MaterialPropertyBlock();
            
            workstation = workstationManager.GetWorkstation(workstationID);
            workstation.OnEnter += Setup;
        }

        /// <summary>
        /// Unity event function that unsubscribes from its workstation's OnEnter setup.
        /// </summary>
        void OnDestroy()
        {
            if (workstation != null)
            {
                workstation.OnEnter -= Setup;
            }
            else
            {
                Debug.Log("workstation pipe destroyed, and has no workstation?");
            }
        }
        #endregion

        #region Main methods
        /// <summary>
        /// Prevents this object from running on the server, and otherwise marks it as setup.
        /// </summary>
        private void Setup()
        {
            if (renderer == null || propBlock == null || ShipStateManager.Instance.isServerOnly)
            {
                Destroy(this); // Can't set shader properties on server
            }
            isSetUp = true;
        }

        /// <summary>
        /// Sets the emission power of the pipe by starting a new coroutine.
        /// </summary>
        /// <param name="power">The power to set the emission to.</param>
        /// <param name="fade">Whether to fade the emission.</param>
        public void SetEmissionPower(float power, bool fade = true) 
        {
            if (currentAnimation != null) 
            {
                StopCoroutine(currentAnimation);
            }

            if (fade)
            {
                currentAnimation = StartCoroutine(FadeToIntensity(power));
            }
            else
            {
                SetEmissionPowerBlock(power);
            }
        }

        /// <summary>
        /// Fades the emission to a given power level.
        /// </summary>
        /// <param name="targetPower">The power to set the pipe to.</param>
        /// <returns>A yield statement that waits while fading the emission.</returns>
        IEnumerator FadeToIntensity(float targetPower)
        {
            if (workstation != null && workstation.playerAtWorkstation != null && workstation.playerAtWorkstation.isLocalPlayer)
            {
                if (renderer == null)
                {
                    Start();
                }
                float timeElapsed = 0f;
                renderer.GetPropertyBlock(propBlock);
                float startPower = propBlock.GetFloat("_EmissionPower");
                yield return new WaitForSeconds(0.8f);

                while (timeElapsed < fadeTime)
                {
                    SetEmissionPowerBlock(Mathf.Lerp(startPower, targetPower, timeElapsed / fadeTime));
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Sets the emission power block.
        /// </summary>
        /// <param name="power">The power to use on the emission block.</param>
        private void SetEmissionPowerBlock(float power) 
        {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_EmissionPower", power);
            renderer.SetPropertyBlock(propBlock);
        }

        /// <summary>
        /// Sets the color that the pipe should use for its emission.
        /// </summary>
        /// <param name="baseColor">The base color of the pipe.</param>
        /// <param name="emissionColor">The color the pipe should emit.</param>
        public void SetPipeColor(Color baseColor, Color emissionColor) 
        {
            renderer.GetPropertyBlock(propBlock);

            propBlock.SetColor("_EmissionColor", emissionColor);
            propBlock.SetColor("_BaseColor", baseColor);

            renderer.SetPropertyBlock(propBlock);
        }

        /// <summary>
        /// Sets the speed that the workstation pipe should use.
        /// </summary>
        /// <param name="speed">The time the emission should scroll.</param>
        public void SetScrollSpeed(float speed) 
        {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_ScrollSpeed", speed);
            renderer.SetPropertyBlock(propBlock);
        }
        #endregion
    }
}


