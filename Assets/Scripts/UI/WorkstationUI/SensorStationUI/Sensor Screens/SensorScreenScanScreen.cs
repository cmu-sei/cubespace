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
using TMPro;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// The screen displayed when initially scanning.
    /// </summary>
    public class SensorScreenScanScreen : SensorScreen
    {
        /// <summary>
        /// The text displayed when at an unscanned location.
        /// </summary>
        private const string UNSCANNED_LOCATION_SCAN_PROMPT = "<color=\"red\">SCAN</color> TO PERFORM INITIAL RECONNAISSANCE";
        /// <summary>
        /// The text displayed when at a previously-scanned location.
        /// </summary>
        private const string SCANNED_LOCATION_SCAN_PROMPT = "<color=\"red\">SCAN</color> TO REFRESH RECONNAISSANCE";
        /// <summary>
        /// The text displayed when there is a video incoming.
        /// </summary>
        private const string VIDEO_FEED_TEXT = "VIDEO FEED: ?????";

        /// <summary>
        /// The text indicating surroundings.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _surroundingsText;
        /// <summary>
        /// The text displayed when the player should scan the location.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _scanPromptText;
        /// <summary>
        /// The text displayed when video is received.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _videoFeedText;
        
        /// <summary>
        /// Sets the scan text, button text, surroundings text, and video feed text when the screen is activated.
        /// </summary>
        /// <param name="currentLocationScanned">Whether the current location has been scanned.</param>
        /// <param name="currentLocationSurroundings">Detail on the current location's surroundings.</param>
        public void ActivateScanScreen(bool currentLocationScanned, string currentLocationSurroundings)
        {
            if (currentLocationScanned)
            {
                _scanPromptText.text = SCANNED_LOCATION_SCAN_PROMPT;
                buttonText = "Refresh Scan";
            }
            else
            {
                _scanPromptText.text = UNSCANNED_LOCATION_SCAN_PROMPT;
                buttonText = "Scan";
            }
            _surroundingsText.text = currentLocationSurroundings;
            _videoFeedText.text = VIDEO_FEED_TEXT;

            base.Activate();
        }

        /// <summary>
        /// Activates this screen when there is no location data. Ideally, this should not happen.
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            Debug.LogError("Scan Screen activated without location data!");
        }

        /// <summary>
        /// Performs a scan when the button is clicked.
        /// </summary>
        public override void OnButtonClick()
        {
            base.OnButtonClick();
            _sensorScreenController.sensorStation.Scan();
        }
    }
}
