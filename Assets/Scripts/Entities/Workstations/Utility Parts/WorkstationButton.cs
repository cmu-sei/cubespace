/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using Mirror;
using System.Collections;
using UnityEngine;

namespace Entities.Workstations
{
    /// <summary>
    /// General class defining a button at a workstation.
    /// </summary>
    public class WorkstationButton : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The time it takes to depress a button.
        /// </summary>
        [SerializeField]
        protected float depressTime = 0.4f;
        /// <summary>
        /// Whether the button is interactable.
        /// </summary>
        public bool interactable;
        /// <summary>
        /// Whether the button has been pressed.
        /// </summary>
        private bool pressed = false;

        /// <summary>
        /// The start position of the button.
        /// </summary>
        private Vector3 startPosition;
        /// <summary>
        /// The final depressed position of the button.
        /// </summary>
        public Transform depressPosition;
        /// <summary>
        /// A UnityEvent to call when the button is pressed.
        /// </summary>
        public UnityEngine.Events.UnityEvent OnPress;

        /// <summary>
        /// Whether the button is interactable or not.
        /// </summary>
        public bool Interactable 
        {
            set 
            { 
                if (!interactable && value) 
                {
                    OnBecomeInteractable();
                    interactable = value;
                } 
                else if (interactable && !value) 
                {
                    OnBecomeUninteractable();
                    interactable = value;
                }
            }
            get 
            { 
                return interactable; 
            }
        }
        #endregion

        #region Unity event functions
        /// <summary>
        /// Puts the button in a depressed position when clicked.
        /// </summary>
        public virtual void OnMouseDown() 
        {
            if (interactable && !pressed && Player.LocalCanInput) 
            {
                if (depressPosition)
                {
                    StartCoroutine(DepressButton());
                    OnPressSFX();
                }
                else
                {
                    if (((CustomNetworkManager)NetworkManager.singleton).isInDebugMode)
                        Debug.Log("No Depress position set for workstation button! Just calling OnPress functions without the animation");
                    OnFullPress();
                    OnFinish();
                }
            }
        }
        #endregion

        #region Press methods
        /// <summary>
        /// Gradually depresses the button.
        /// </summary>
        /// <returns>A yield statement while waiting for the button to depress.</returns>
        private IEnumerator DepressButton()
        {
            pressed = true;
            startPosition = transform.position;

            for (float i = 0.1f; i < 1f; i += 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, depressPosition.position, i);
                yield return new WaitForSeconds(depressTime * 0.1f);
            }
            transform.position = depressPosition.position;
            OnFullPress();

            for (float i = 0.1f; i < 1f; i += 0.1f)
            {
                transform.position = Vector3.Lerp(depressPosition.position, startPosition, i);
                yield return new WaitForSeconds(depressTime * 0.07f);
            }
            transform.position = startPosition;
            OnFinish();
            pressed = false;
        }

        //Override the following methods to make a specialized button

        /// <summary>
        /// Invokes an OnPress event when the button is fully depressed.
        /// </summary>
        protected virtual void OnFullPress() 
        {
            OnPress.Invoke();
        }

        /// <summary>
        /// Called when the button reverts back to its original position.
        /// </summary>
        protected virtual void OnFinish() 
        {

        }

        /// <summary>
        /// Called when the button becomes interactable.
        /// </summary>
        protected virtual void OnBecomeInteractable() 
        {

        }

        /// <summary>
        /// Called when the button becomes uninteractable.
        /// </summary>
        protected virtual void OnBecomeUninteractable() 
        {

        }

        /// <summary>
        /// Enables interactability when called. This should be called when the button is powered on.
        /// </summary>
        protected virtual void OnPowerOn() {
            Interactable = true;
        }

        /// <summary>
        /// Disables interactability when called. This should be called when the button is powered off.
        /// </summary>
        protected virtual void OnPowerOff() {
            Interactable = false;
        }

        /// <summary>
        /// Plays a miscellaneous UI sound effect.
        /// </summary>
        protected virtual void OnPressSFX() {
            Audio.AudioPlayer.Instance.UIMisc(transform);
        }
        #endregion
    }
}

