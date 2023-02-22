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
using UnityEngine.EventSystems;

namespace Audio
{
    /// <summary>
    /// Class defining different types of buttons that can be clicked, triggering different sounds when clicked.
    /// </summary>
    public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        // The specific type of this button, but publicly-accessible
        public enum ButtonType { genericButton, genericButton2, exitButton, onOffButton, slideButton, modalFinalButton, customizeButton, connectButton, lockButton }
        // The specific type of this button, customizable in the Inspector
        public ButtonType buttonType;
        // Whether to spatialize this sound at this component's transform
        public bool spatialized = true;
        // Whether this button activates another element
        public bool activated = false;
        
        /// <summary>
        /// Plays a sound on hover over this button object.
        /// </summary>
        /// <param name="pointerEventData">The data of the hover event. A necessary parameter, but unused.</param>
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            // Set a location to feed into the mouseover events
            Transform location;
            // If this button exists in 3D space, set the transform of this object to be where the sound plays
            if (spatialized)
            {
                location = transform;
            // Otherwise, there is no location where this sound should play
            }
            else
            {
                location = null;
            }

            // Play a sound on hover based on the set button type
            switch (buttonType)
            {
                // Basic UI mouse hover
                case ButtonType.onOffButton:
                case ButtonType.exitButton:
                case ButtonType.modalFinalButton:
                case ButtonType.genericButton:
                    AudioPlayer.Instance.UIMouseover(0, location);
                    break;
                // Variation of the mouse hover
                case ButtonType.slideButton:
                case ButtonType.genericButton2:
                    AudioPlayer.Instance.UIMouseover(1, location);
                    break;
                // Hover sound for offline objects and the lock button
                case ButtonType.customizeButton:
                case ButtonType.connectButton:
                case ButtonType.lockButton:
                    AudioPlayer.Instance.OfflineUI_Hover();
                    break;
            }
        }

        /// <summary>
        /// Plays a sound on click on this button object.
        /// </summary>
        /// <param name="pointerEventData">The data of the hover event. A necessary parameter, but unused.</param>
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            // Set a location to feed into the mouseover events
            Transform location;
            // If this button exists in 3D space, set the transform of this object to be where the sound plays
            if (spatialized)
            {
                location = transform;
            }
            // Otherwise, there is no location where this sound should play
            else
            {
                location = null;
            }

            // Play a sound on hover based on the set button type
            switch (buttonType)
            {
                // General selection sound
                case ButtonType.genericButton:
                    AudioPlayer.Instance.UISelect(0, location);
                    break;
                // Variation of selection sound
                case ButtonType.genericButton2:
                    AudioPlayer.Instance.UISelect(1, location);
                    break;
                // Exiting sound
                case ButtonType.exitButton:
                    AudioPlayer.Instance.UIExit(1, location);
                    break;
                // Turning something on or off
                case ButtonType.onOffButton:
                    if (activated)
                    {
                        AudioPlayer.Instance.UISelect(0, location);
                    }
                    else
                    {
                        AudioPlayer.Instance.UIExit(1, location);
                    }
                    break;
                // Yes/No selection on the HUD modal
                case ButtonType.modalFinalButton:
                    AudioPlayer.Instance.UIMisc();
                    break;
                // Button on the slide area within the Mission Log
                case ButtonType.slideButton:
                    AudioPlayer.Instance.UIMove(location);
                    break;
                // Offline customization option sound
                case ButtonType.customizeButton:
                    AudioPlayer.Instance.OfflineUI_Click();
                    break;
                // Offline connect button sound
                case ButtonType.connectButton:
                    AudioPlayer.Instance.OfflineUI_Connect_Click();
                    break;
                // Flight Engineer lock button sound
                case ButtonType.lockButton:
                    AudioPlayer.Instance.LockButtonClick(location);
                    break;
            }
        }
    }
}

