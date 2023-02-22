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

namespace Audio {
    /// <summary>
    /// General class for storing and retrieving AudioSources.
    /// </summary>
    public class AudioSourcePool : Systems.PollingPool<AudioSource>
    {
        // Start with a pool of 60 audio sources to frontload instantiation cost
        [SerializeField]
        private int numInitialAudioSources = 60;

        /// <summary>
        /// Creates a new pool of AudioSources that can be used to play sounds.
        /// </summary>
        /// <param name="prefab">The AudioSource base prefab.</param>
        /// <param name="poolLimit">The maximum number of AudioSources that can be used at one time.</param>
        public AudioSourcePool(AudioSource prefab, int poolLimit = int.MaxValue) : base(prefab, poolLimit)
        {
            // Create many new AudioSources and add them to the pool
            for (int i = 0; i < numInitialAudioSources; i++)
            {
                pool.Enqueue(InstantiateObject());
            }
        }

        /// <summary>
        /// Disables an AudioSource in the pool using the linked list node which contains that AudioSource.
        /// </summary>
        /// <param name="current">The linked list node containing the AudioSource to disable.</param>
        protected override void Disable(LinkedListNode<AudioSource> current)
        {
            // Avoid a null current.Value by replacing a null with a new AudioSource
            if (current.Value == null) current.Value = InstantiateObject();
            AudioSource source = current.Value;
            source.transform.position = Vector3.zero;
            base.Disable(current);
        }

        /// <summary>
        /// Creates a new AudioSource.
        /// </summary>
        /// <returns>A new AudioSource.</returns>
        protected override AudioSource InstantiateObject()
        {
            // Create a new AudioSource and make its parent the one designated in the AudioManager
            AudioSource temp = base.InstantiateObject();
            temp.transform.SetParent(AudioManager.Instance.audioSourceParent.transform);
            return temp;
        }

        /// <summary>
        /// Check to see if the given AudioSource exists and is playing.
        /// </summary>
        /// <param name="component">The AudioSource component to check.</param>
        /// <returns>If the given AudioSource both exists and is playing.</returns>
        protected override bool IsActive(AudioSource component)
        {
            return component && component.isPlaying;
        }
    }
}

