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
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Entities.Workstations.FlightEngineerParts
{
    /// <summary>
    /// A component defining behavior of the lock button on the FlightEngineer, used after all dials
    /// have been turned to their target positions as part of the launch sequence.
    /// </summary>
    public class LockButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        #region Variables
        /// <summary>
        /// The Flight Engineer workstation.
        /// </summary>
        [Header("GameObject References")]
        [FormerlySerializedAs("workstation")]
        [SerializeField]
        private FlightEngineer flightEngineer;
        /// <summary>
        /// The button overlay object used for the lock button.
        /// </summary>
        [SerializeField]
        private Button button;
        /// <summary>
        /// The model of the lock button.
        /// </summary>
        [SerializeField]
        private GameObject buttonModel;
        /// <summary>
        /// The renderer of the lock button object.
        /// </summary>
        [SerializeField]
        private Renderer buttonRenderer;

        /// <summary>
        /// The material displayed on the lock button when the dials are locked.
        /// </summary>
        [Header("Materials")]
        [SerializeField]
        private Material onLockMaterial;
        /// <summary>
        /// The material displayed on the lock button when the dials are turned to their target positions.
        /// </summary>
        [SerializeField]
        private Material onActivateMaterial;
        /// <summary>
        /// The material displayed on the lock button when hovering over the button after the dials have been turned to their targets.
        /// </summary>
        [SerializeField]
        private Material onHoverMaterial;

        [Header("Button text colors")]
        /// <summary>
        /// The color to set the button text to have when the dials are locked.
        /// </summary>
        [SerializeField]
        private Color onLockTextColor = Color.white;
        /// <summary>
        /// The color to set the button text to have when the dials are activated.
        /// </summary>
        [SerializeField]
        private Color onActivateTextColor = Color.black;
        /// <summary>
        /// The color to set the button text to have when the mouse cursor hovers over it.
        /// </summary>
        [SerializeField]
        private Color onHoverTextColor = new Color(0.7169812f, 0.7169812f, 0.7169812f, 1);
        /// <summary>
        /// The color to set the button text to have on resetting it; this should be its original color.
        /// </summary>
        [SerializeField]
        private Color unselectedTextColor = new Color(.255f, .255f, .255f, 1);

        /// <summary>
        /// The original material for the button renderer.
        /// </summary>
        private Material unselectedMaterial;
        /// <summary>
        /// The text object for the button. Its color changes depending on the button state.
        /// </summary>
        private TextMeshProUGUI buttonText;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that sets the unselected material and text, and resets the button's appearance to its default.
        /// </summary>
        private void Awake()
        {
            unselectedMaterial = buttonRenderer.material;
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            ResetState();
        }

        /// <summary>
        /// Unity event function that subscribes to events on the Flight Engineer when the LockButton component is enabled.
        /// </summary>
        private void OnEnable() 
        {
            // Static events
            FlightEngineer.OnLock += OnLock;

            // Non-static events
            flightEngineer.OnResetState += ResetState;
            flightEngineer.OnEnter += OnEnter;
            flightEngineer.OnExit += OnExit;
        }

        /// <summary>
        /// Unity event function that unsubscribes from events on the Flight Engineer when the LockButton component is disabled.
        /// </summary>
        private void OnDisable() 
        {
            // Static events
            FlightEngineer.OnLock -= OnLock;

            // Non-static events
            flightEngineer.OnResetState -= ResetState;
            flightEngineer.OnEnter -= OnEnter;
            flightEngineer.OnExit -= OnExit;
        }
        #endregion

        #region State callbacks / appearance methods
        /// <summary>
        /// Sets the button's appearance to what it looks like when all dials have been locked.
        /// </summary>
        public void OnLock()
        {
            Debug.Log("Disabling LockButton. Invoked from OnLock. [LockButton.cs:144]");
            if (button == null || buttonRenderer == null || buttonText == null)
            {
                Debug.LogError("NULL button something or another [LockButton.cs:147]");
            }

            button.enabled = false;
            buttonRenderer.material = onLockMaterial;
            buttonText.color = onLockTextColor;
        }

        /// <summary>
        /// Sets the button's appearance to what it looks like when all dials have been set to their target angles, but not yet locked.
        /// </summary>
        public void OnActivate()
        {
            button.enabled = true;
            buttonRenderer.material = onActivateMaterial;
            buttonText.color = onActivateTextColor;
        }

        /// <summary>
        /// Sets the button's appearance to what it looks like when all dials have been activated and the mouse cursor is hovering over the button.
        /// </summary>
        private void OnHover()
        {
            button.enabled = true;
            buttonRenderer.material = onHoverMaterial;
            buttonText.color = onHoverTextColor;
        }

        /// <summary>
        /// Resets the button's appearance back to its default.
        /// </summary>
        public void ResetState()
        {
            button.enabled = false;
            buttonRenderer.material = unselectedMaterial;
            buttonText.color = unselectedTextColor;
        }
        #endregion

        #region Pointer methods
        /// <summary>
        /// Changes the button's appearance to a hover state (if the button is enabled) when the lock button is entered by a player's mouse cursor.
        /// </summary>
        /// <param name="pointerEventData">The event data about the mouse entry.</param>
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (button.enabled && Player.LocalCanInput) 
            {
                OnHover();
                Audio.AudioPlayer.Instance.OfflineUI_Hover();
            }
        }

        /// <summary>
        /// Changes the button's appearance to an activated state (if the button is enabled) when the lock button is exited by a player's mouse cursor.
        /// </summary>
        /// <param name="pointerEventData">The event data about the mouse exit.</param>
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if (button.enabled && Player.LocalCanInput)
            {
                OnActivate();
            }
        }

        /// <summary>
        /// Plays a sound effect (if the button is enabled) when the lock button is clicked by a player's mouse cursor.
        /// </summary>
        /// <param name="pointerEventData">The event data about the mouse click.</param>
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if (button.enabled && Player.LocalCanInput) 
            {
                Audio.AudioPlayer.Instance.OfflineUI_Click();
            }
        }
        #endregion

        #region Workstation method callbacks
        /// <summary>
        /// Sets the visual appearance of the button based on the state of the dials at the Flight Engineer.
        /// </summary>
        public void OnEnter()
        {
            SetVisualState();
        }

        /// <summary>
        /// Performs logic when the player exits the Flight Engineer. Currently blank.
        /// </summary>
        public void OnExit()
        {
            // No logic required here
        }
        #endregion

        #region Visual state methods
        /// <summary>
        /// Sets the visual state of the button based on the state of the dials.
        /// </summary>
        public void SetVisualState()
        {
            // If the FlightEngineer is powered, see what state the button should display
            if (flightEngineer.IsPowered)
            {
                // If the dials are locked, show the button as locked
                if (flightEngineer.DialsLocked)
                {
                    OnLock();
                }
                // Otherwise, if the dials are just activated, show the button as activated
                else if (flightEngineer.AreDialsActivated())
                {
                    OnActivate();
                }
                // Otherwise, reset the button's appearance
                else
                {
                    ResetState();
                }
            }
            // If the FlightEngineer isn't powered, reset the button's appearance
            else
            {
                ResetState();
            }
        }
        #endregion
    }
}

