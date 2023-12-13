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
using Audio;

namespace UI
{
    /// <summary>
    /// A class used for a slider which sets the volume.
    /// </summary>
    public class UIVolumeSlider : MonoBehaviour
    {
        /// <summary>
        /// THe master volume mixer object.
        /// </summary>
        [SerializeField]
        private UnityEngine.Audio.AudioMixerGroup masterMixer;
        /// <summary>
        /// The group used within the mixer.
        /// </summary>
        [SerializeField]
        private string groupName = "SFXVol";
        /// <summary>
        /// The slider UI object used by the player to change the volume.
        /// </summary>
        [SerializeField]
        private Slider slider;

        /// <summary>
        /// Sets the volume of the group when the slider's value changes.
        /// </summary>
        public void SetVolume() 
        {
            AudioPlayer.Instance.SetGroupVolume(Mathf.Log10(slider.value) * 20, groupName);
        }
    }
}

