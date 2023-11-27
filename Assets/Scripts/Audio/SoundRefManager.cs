/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
    /// <summary>
    /// SoundRefManager contains all Sounds, mapping SoundType enums to their audio clips 
    /// and playback information.
    /// </summary>
    public class SoundRefManager : MonoBehaviour
    {
        // Different mixers used for different sounds
        [Header("Mixer Groups")]
        public AudioMixerGroup UIMixerGroup;
        public AudioMixerGroup SpatializedSFXMixerGroup;
        public AudioMixerGroup UnspatializedSFXMixerGroup;
        public AudioMixerGroup AmbienceMixerGroup;

        // Differnet snapshots provided
        [Header("Snapshot")]
        public AudioMixerSnapshot DefaultSnapshot;
        public AudioMixerSnapshot MuteSnapshot;
        public AudioMixerSnapshot MuteSFXSnapshot;

        // The complete list of possible Sounds we can play
        [SerializeField] private List<Sound> soundList;
        // A Dictionary created from the above list
        private Dictionary<SoundType, Sound> soundDict = new Dictionary<SoundType, Sound>();

        /// <summary>
        /// Instantiates the sound dictionary on GameObject startup by looping through the provided sound list.
        /// </summary>
        void Awake()
        {
            foreach (Sound sound in soundList) {
                if (!soundDict.ContainsKey(sound.Type))
                {
                    soundDict.Add(sound.Type, sound);
                }
                else
                {
                    Debug.LogWarning("Can't add two sounds of same type ("+sound.Type+") to dictionary.", this);
                }
            }
        }

        /// <summary>
        /// Simple getter method for retrieving a sound of a given type.
        /// </summary>
        /// <param name="soundType">The type of the sound to retrieve.</param>
        /// <returns>A Sound, or null if it does not exist in the dictionary.</returns>
        public Sound GetSound(SoundType soundType)
        {
            if (soundDict.TryGetValue(soundType,out var sound))
            {
                return sound;
            }else
            {
                Debug.LogWarning("No sound in SoundRefManager for "+soundType);
                return null;
            }
        }

    }
}

