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
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Toggles the state of objects when a button this component is attached to is clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIEnableObjectsToggleButton : MonoBehaviour
    {
        /// <summary>
        /// The objects to enable/disable.
        /// </summary>
        [SerializeField]
        protected GameObject[] toggleObjects;
        /// <summary>
        /// Whether the objects are at first visible or invisible.
        /// </summary>
        [SerializeField]
        protected bool startingViewState = false;

        /// <summary>
        /// The UI button the player clicks.
        /// </summary>
        protected Button button;
        /// <summary>
        /// The state of the objects.
        /// </summary>
        protected bool viewState;

        /// <summary>
        /// Unity event function that adds the toggle function as a listener to the UI button.
        /// </summary>
        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Toggle);
        }

        /// <summary>
        /// Unity event function that sets the view of each button according to the state it should have on the start.
        /// </summary>
        void Start()
        {
            SetView(startingViewState);
        }

        /// <summary>
        /// Sets whether each object in the view is visible.
        /// </summary>
        /// <param name="view">Whether the objects are visible or not.</param>
        protected virtual void SetView(bool view)
        {
            viewState = view;
            foreach (var obj in toggleObjects)
            {
                obj.SetActive(view);
            }
        }
        
        /// <summary>
        /// Toggles the state of the button to be what it currently is not.
        /// </summary>
        public void Toggle()
        {
            SetView(!viewState);
        }
    }
}
