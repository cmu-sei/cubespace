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
using System.Collections.Generic;
using UnityEngine;
using Managers;

namespace Audio
{
    /// <summary>
    /// Sound.cs contains definitions for the Sound class, which contains 
    /// playing information for each AudioClip and the SoundType enum.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        // The type of sound this is
        [SerializeField] private SoundType type;

        // The publicly-accessible type of sound this is
        public SoundType Type
        {
            get
            { 
                return type;
            }
        }

        // The list of possible AudioClips this Sound can use
        [SerializeField] private AudioClip[] audioClips;
        
        [Header("Attributes")]
        // The overall volume of this sound
        [Range(0, 1)]
        public float volume = 1;
        // Pitch will be randomized by +/- pitchRand
        [Range(0, 2)]
        public float pitchRand = 0;
        // Whether to loop the cound
        public bool loop = false;
        // How long the cooldown of this sound lasts
        public float cooldown = 0;

        /// <summary>
        /// Simple getter method returning a random sound in the list of audio clips.
        /// </summary>
        /// <returns>A random AudioClip.</returns>
        public AudioClip GetClip() {
            return audioClips[Random.Range(0, audioClips.Length)];
        }

        /// <summary>
        /// Simple getter method returning the sound at a specific index within the audio clip array.
        /// </summary>
        /// <param name="index">The index of the clip.</param>
        /// <returns>An AudioClip at the specified index.</returns>
        public AudioClip GetClip(int index) {
            return audioClips[index];
        }
    }

    // The type each sound can have, used for standardized sound output
    // Note that adding to the SoundType enum may disrupt existing SoundType assignments in the Inspector
    public enum SoundType {
        Ambience_Ship,
        Ambience_Terminal,
        UI_Select_1,
        UI_Select_2,
        UI_Mouseover_1,
        UI_Mouseover_2,
        UI_Error,
        UI_Exit_1,
        UI_Exit_2,
        UI_Load_Up,
        UI_Misc,
        UI_Move,
        WS_Start,
        WS_Loop,
        WS_End,
        FE_Dial_Tick,
        FE_Engine_Takeoff,
        FE_Engine_Loop,
        FE_Lever_Lock,
        FE_Thruster_Power,
        FE_Thruster_Loop,
        FE_Thruster_Up,
        FE_Tubes,
        FE_Switches,
        PR_Tubes,
        A_Extend,
        CS_Hologram,
        CS_HologramPower,
        Codex_CodexGet,
        SS_Complete,
        SS_TransmissionEnded,
        SS_TransmissionAlert,
        OF_UI_Mouseover,
        OF_UI_Click,
        OF_UI_Connect_Click,
        FE_Lock_Click,
        NR_IncorrectCoord,
        NR_CorrectCoord
    }
}
