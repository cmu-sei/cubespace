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
using Entities.Workstations;

namespace UI.HUD
{
    /// <summary>
    /// The button used to exit the workstation.
    /// </summary>
    public class UIExitWorkstationButton : Managers.ConnectedSingleton<UIExitWorkstationButton>
    {
        /// <summary>
        /// The button object displayed to the player to leave the workstation.
        /// </summary>
        [SerializeField]
        private GameObject _buttonObject;
        /// <summary>
        /// The UI button the player clicks to leave the workstation.
        /// </summary>
        [SerializeField]
        private Button _button;

        /// <summary>
        /// The current workstation the player is at. Null by default.
        /// </summary>
        private Workstation _currentStation = null;

        /// <summary>
        /// Whether this button is hidden by the mission log.
        /// </summary>
        private bool hiddenByMissionLog = false;
        /// <summary>
        /// Whether this button is hidden by the video.
        /// </summary>
        private bool hiddenByVideo = false;

        /// <summary>
        /// Unity event function that adds the OnButtonClick function as a listener for the UI button.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            _button.onClick.AddListener(OnButtonClick);
            DisableButton();
        }

        /// <summary>
        /// Shows the button as long as it is hidden.
        /// </summary>
        public void ShowButton()
        {
            if (_currentStation != null && !hiddenByMissionLog && !hiddenByVideo)
            {
                _buttonObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the button if hidden by the HUD panel, or vice versa.
        /// </summary>
        /// <param name="hidden">Whether this button is hidden by the HUD panel.</param>
        public void SetHiddenByHudPanel(bool hidden)
        {
            hiddenByMissionLog = hidden;

            if (hidden)
            {
                HideButton();
            }
            else
            {
                ShowButton();
            }
        }

        /// <summary>
        /// Hides the button if hidden by a video, or vice versa.
        /// </summary>
        /// <param name="hidden">Whether this button is hidden by a video.</param>
        public void SetHiddenByVideo(bool hidden)
        {
            hiddenByVideo = hidden;

            if (hidden)
            {
                HideButton();
            }
            else
            {
                ShowButton();
            }
        }

        /// <summary>
        /// Hides the button.
        /// </summary>
        public void HideButton()
        {
            _buttonObject.SetActive(false);
        }

        /// <summary>
        /// Enables the button.
        /// </summary>
        /// <param name="station">The workstation the player is at.</param>
        public void EnableButton(Workstation station)
        {
            _currentStation = station;
            ShowButton();
        }

        /// <summary>
        /// Disables the button.
        /// </summary>
        public void DisableButton()
        {
            _buttonObject.SetActive(false);
            _currentStation = null;
        }

        /// <summary>
        /// Exits the current workstation.
        /// </summary>
        private void OnButtonClick()
        {
            if (_currentStation == null)
            {
                Debug.LogWarning("Exit Workstation button clicked without a current station set");
                return;
            }
            if (hiddenByVideo || hiddenByMissionLog)
            {
                Debug.LogWarning("Exit Workstation button clicked while it should be hidden");
                return;
            }

            _currentStation.Deactivate();
        }
    }
}

