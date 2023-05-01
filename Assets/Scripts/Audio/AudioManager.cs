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

namespace Audio {
    /// <summary>
    /// This contains all necessary functions to play audio, and should be called exclusively by AudioPlayer. This structure allows us to switch out the Audio Manager if necessary.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        // The component holding all sounds used in the game
        [SerializeField]
        private SoundRefManager soundRefManager;
        // The child object of the AudioManager that serves as the parent of all created AudioSources
        public GameObject audioSourceParent;
        // The maximum number of AudioSources that can be created for this AudioManager
        [SerializeField]
        private int audioSourceLimit = 60;

        // Public volume settings
        public float masterVolume;
        public float sfxVolume;
        public float transmissionVolume;
        public float ambianceVolume;

        // The prefab to spawn in when creating AudioSources
        public AudioSource sourcePrefab;
        // The available pool of AudioSources, created for sounds dynamically at runtime
        private AudioSourcePool pool;

        // A dictionary storing sounds with the last time they were played, to prevent overlapping sounds
        private Dictionary<SoundType, float> lastPlayedTimes = new Dictionary<SoundType, float>();
        
        /// <summary>
        /// Unity event function that invokes the base Awake function and then spawns in a new AudioSource pool.
        /// </summary>
        public override void Awake() {
            base.Awake();
            GenerateNewPool();
        }

        /// <summary>
        /// Resets the AudioSource pool by destroying the old parent object and creating a new one. We do this to avoid an error when disconnecting.
        /// </summary>
        public void ResetPool()
        {
            // Destroy the AudioSources' parent
            Destroy(audioSourceParent);
            // Create a new parent
            audioSourceParent = Instantiate(new GameObject("AudioSource Parent"), transform);
            // Generate a new AudioSource pool
            GenerateNewPool();
        }

        /// <summary>
        /// Generates a new pool of AudioSources to use for sound playback.
        /// </summary>
        private void GenerateNewPool()
        {
            pool = new AudioSourcePool(sourcePrefab, audioSourceLimit);
        }

        /// <summary>
        /// Sets attributes of the AudioSource after it is created.
        /// </summary>
        /// <param name="source">The AudioSource to modify.</param>
        /// <param name="sound">The sound provided.</param>
        /// <param name="index">The index of the sound within a list of AudioClips. If -1, a random clip is selected.</param>
        private void SetUpSource(AudioSource source, Sound sound, int index = -1)
        {
            // If the index is -1, select a random clip
            if (index == -1)
            {
                source.clip = sound.GetClip();
            }
            // Otherwise, select a clip at the given index
            else
            {
                source.clip = sound.GetClip(index);
            }

            // Set the attributes of the AudioSource
            source.transform.parent = audioSourceParent.transform;
            source.volume = sound.volume;
            source.pitch = 1 + Random.Range(-sound.pitchRand, sound.pitchRand);
            source.bypassReverbZones = false;
            source.spatialBlend = 0.0f;
            source.outputAudioMixerGroup = soundRefManager.UnspatializedSFXMixerGroup;
            source.loop = sound.loop;
            source.maxDistance = sourcePrefab.maxDistance;
            source.minDistance = sourcePrefab.minDistance;
            source.spread = sourcePrefab.spread;
        }

        /// <summary>
        /// Pulls a new AudioSource from the available pool of AudioSources to play a sound.
        /// If calling this method directly, the AudioSource will need to be spatialized.
        /// </summary>
        /// <param name="soundType">The type of sound given to this AudioSource.</param>
        /// <param name="index">The index of the AudioClip of the sound to use.</param>
        /// <returns>An AudioSource with an AudioClip matching the provided sound type.</returns>
        public AudioSource CreateInstance(SoundType soundType, int index = -1)
        {
            // Get an AudioSource from the pool of AudioSources
            AudioSource source = pool.Get();

            // If we've run out of AudioSources, return null, because we can't play the sound then
            if (source == null)
            {
                Debug.LogWarning("no audioSource from pool to use.");
                return null;
            }

            // Get the desired sound from the sound ref manager
            Sound sound = soundRefManager.GetSound(soundType);
            if (sound == null)
            {
                return null;
            }

            // If the sound has a cooldown, see if we need to add or update the dictionary of the last time sounds were played
            if (sound.cooldown > 0)
            {
                // If the sound's cooldown is over, update the dictionary of the last time sounds were played
                if (lastPlayedTimes.ContainsKey(soundType) && lastPlayedTimes[soundType] + sound.cooldown < Time.time)
                {
                    lastPlayedTimes[soundType] = Time.time;
                }
                // Otherwise, if the cooldown is not over, don't do anything
                else if (lastPlayedTimes.ContainsKey(soundType))
                {
                    return null;
                }
                // Otherwise, the sound has never been played, so add it to the dictionary with the current time
                else
                {
                    lastPlayedTimes.Add(soundType, Time.time);
                }
            }

            // Set AudioSource attributes
            SetUpSource(source, sound, index);
            return source;
        }

        /// <summary>
        /// Pulls a new AudioSource from the available pool of AudioSources to play a sound.
        /// If calling this method directly, the AudioSource will need to be spatialized.
        /// </summary>
        /// <param name="soundType">The type of sound given to this AudioSource.</param>
        /// <param name="transform">The transform the obtained AudioSource should use for its position.</param>
        /// <param name="index">The index of the AudioClip of the sound to use.</param>
        /// <returns>The AudioSource retrieved from the pool.</returns>
        public AudioSource CreateInstance(SoundType soundType, Transform transform, int index = -1)
        {
            // Pull a new AudioSource from the pool
            AudioSource source = CreateInstance(soundType, index);
            // Set the AudioSource's position in 3D space
            source.transform.position = transform.position;
            // Set a mixer group and spatial blend
            source.outputAudioMixerGroup = soundRefManager.SpatializedSFXMixerGroup;
            source.spatialBlend = 1;
            return source;
        }

        /// <summary>
        /// Plays a UI SFX sound.
        /// We use a different function for UI SFX because UI uses a different mixer group.
        /// </summary>
        /// <param name="soundType">The type of the sound that should play.</param>
        public void PlayUISFX(SoundType soundType)
        {
            // Pull a new AudioSource from the pool
            AudioSource source = CreateInstance(soundType);
            // Don't continue if we don't have an AudioSource for whatever reason
            if (source == null)
            {
                return;
            }

            // Set other attributes of the AudioSource
            source.spatialBlend = 0.0f;
            source.bypassReverbZones = true;
            source.outputAudioMixerGroup = soundRefManager.UIMixerGroup;

            // Play the AudioSource
            source.Play();
        }

        /// <summary>
        /// Pull a new instance of an AudioSource to use for ambiance.
        /// We use a different function for UI SFX because Ambiance uses a different mixer group.
        /// </summary>
        /// <param name="soundType">The type of sound given to this AudioSource.</param>
        /// <param name="transform">The transform the obtained AudioSource should use for its position.</param>
        /// <param name="index">The index of the AudioClip of the sound to use.</param>
        /// <returns>The AudioSource retrieved from the pool.</returns>
        public AudioSource CreateAmbienceInstance(SoundType soundType, Transform transform, int index = -1)
        {
            // Pull a new AudioSource from the pool
            AudioSource source = CreateInstance(soundType, index);

            if (source == null)
            {
                return null;
            }

            // Set other attributes of the AudioSource
            source.transform.position = transform.position;
            source.outputAudioMixerGroup = soundRefManager.AmbienceMixerGroup;
            source.spatialBlend = 1;
            return source;
        }

        /// <summary>
        /// Plays a sound of a given type on an AudioSource in a manner that allows multiple sounds to be played on the same AudioSource at once.
        /// </summary>
        /// <param name="soundType">The type of sound given to this AudioSource.</param>
        /// <param name="index">The index of the AudioClip of the sound to use.</param>
        public void PlayOneShot(SoundType soundType, int index = -1)
        {
            // Pull a new AudioSource from the pool
            AudioSource source = CreateInstance(soundType, index);
            // Don't continue if we don't have an AudioSource for whatever reason
            if (source == null)
            {
                return;
            }

            // Set other attributes of the AudioSource
            source.spatialBlend = 0.0f;
            source.bypassReverbZones = true;
            source.outputAudioMixerGroup = soundRefManager.UnspatializedSFXMixerGroup;

            // Play the AudioSource
            source.Play();
        }

        /// <summary>
        /// Plays a sound of a given type on an AudioSource in a manner that allows multiple sounds to be played on the same AudioSource at once.
        /// </summary>
        /// <param name="soundType">The type of sound given to this AudioSource.</param>
        /// <param name="transform">The transform the obtained AudioSource should use for its position.</param>
        /// <param name="index">The index of the AudioClip of the sound to use.</param>
        public void PlayOneShot(SoundType soundType, Transform transform, int index = -1) {

            // If the transform is not specified, just play the sound
            if (transform == null)
            {
                PlayOneShot(soundType);
            }

            // Pull a new AudioSource from the pool
            AudioSource source = CreateInstance(soundType, index);
            if (source == null)
            {
                return;
            }

            // Set other attributes of the AudioSource
            source.spatialBlend = 1.0f;
            source.transform.position = transform.position;
            source.outputAudioMixerGroup = soundRefManager.SpatializedSFXMixerGroup;

            // Play the AudioSource
            source.Play();
        }

        /// <summary>
        /// Places the given AudioSource at the same location as the given transform.
        /// </summary>
        /// <param name="source">The AudioSource to place at a specific location.</param>
        /// <param name="transform">The transform of an object with the position where the AudioSource should be placed.</param>
        public void AttachInstanceToGameObject(AudioSource source, Transform transform) {

            if (source != null && transform != null)
            {
                // Set the position of the AudioSource and its parent
                source.transform.position = transform.position;
                source.transform.parent = audioSourceParent.transform;
                // Set other attributes of the AudioSource
                source.spatialBlend = 1.0f;
                source.outputAudioMixerGroup = soundRefManager.SpatializedSFXMixerGroup;

                // Play the AudioSource
                source.Play();
            }
        }

        /// <summary>
        /// Fades in a sound at an AudioSource with the provided index in the given array.
        /// </summary>
        /// <param name="sources">A list of possible AudioSources.</param>
        /// <param name="coroutines">All fade in operations.</param>
        /// <param name="index">The index of the AudioSource playing and the fade in operation running.</param>
        /// <param name="soundType">The type of sound given to the AudioSource.</param>
        /// <param name="fadeTime">The time to fade in the sound.</param>
        /// <param name="transform">The transform the AudioSource at the given index should use for its position.</param>
        public void FadeInIndexedLoop(AudioSource[] sources, Coroutine[] coroutines, int index, 
                    SoundType soundType, float fadeTime, Transform transform = null)
        {
            // If the AudioSource at the given index does not exist or is not playing, pull a new AudioSource from the pool and 
            if (sources[index] == null || !sources[index].isPlaying)
            {
                sources[index] = CreateInstance(soundType, index);
            }
            // If a transform was provided, this sound should play at that transform's location
            if (transform != null)
            {
                AttachInstanceToGameObject(sources[index], transform);
            }

            // If we're already trying to fade this sound, stop the fade so we can start a new one
            if (coroutines[index] != null)
            {
                StopCoroutine(coroutines[index]);
            }
            // Begin a coroutine to fade in the sound over time
            coroutines[index] = StartCoroutine(FadeInCoroutine(sources[index], fadeTime, soundRefManager.GetSound(soundType).volume));
        }

        /// <summary>
        /// Fades out a sound at an AudioSource with the provided index in the given array.
        /// </summary>
        /// <param name="sources">A list of possible AudioSources.</param>
        /// <param name="coroutines">All fade out operations.</param>
        /// <param name="index">The index of the AudioSource playing and the fade out operation running.</param>
        /// <param name="fadeTime">The time to fade out the sound.</param>
        public void FadeOutIndexedLoop(AudioSource[] sources, Coroutine[] coroutines, int index, float fadeTime)
        {
            // If there is an AudioSource at this index, run a new fade out coroutine
            if (sources[index] != null)
            {
                // If there is a coroutine at this index, stop it
                if (coroutines[index] != null)
                {
                    StopCoroutine(coroutines[index]);
                }
                StartCoroutine(FadeOutCoroutine(sources[index], fadeTime));
                sources[index] = null;
            }
        }

        /// <summary>
        /// Calls the fade out operation.
        /// </summary>
        /// <param name="source">The AudioSource to fade out.</param>
        /// <param name="fadeTime">The time to fade out the sound.</param>
        public void FadeOut(AudioSource source, float fadeTime)
        {
            if (source == null)
            {
                Debug.LogWarning("Null audio source asked for. Check this stack trace and find the component with a null sound source.");
                return;
            }

            // Start the fade out process
            StartCoroutine(FadeOutCoroutine(source, fadeTime));
        }

        /// <summary>
        /// Process to gradually fade out an AudioSource over the given time duration.
        /// </summary>
        /// <param name="source">The AudioSource to fade out.</param>
        /// <param name="fadeTime">The time to fade out the AudioSource.</param>
        /// <returns>A yield to wait while not done fading out.</returns>
        private IEnumerator FadeOutCoroutine(AudioSource source, float fadeTime)
        {
            // The start volume is the initial volume of the AudioSource
            float startVolume = source.volume;
    
            // Fade out a bit every frame
            while (source.volume > 0)
            {
                source.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
    
            // Stop fading and reset the volume
            source.Stop();
            source.volume = startVolume;
        }

        /// <summary>
        /// Calls the fade in operation.
        /// </summary>
        /// <param name="source">The AudioSource to fade in.</param>
        /// <param name="fadeTime">The time to fade in the sound.</param>
        public void FadeIn(AudioSource source, float fadeTime)
        {
            // Start the fade in process
            StartCoroutine(FadeInCoroutine(source, fadeTime, source.volume));
        }

        /// <summary>
        /// Process to gradually fade in an AudioSource over the given time duration.
        /// </summary>
        /// <param name="source">The AudioSource to fade in.</param>
        /// <param name="fadeTime">The time to fade in the AudioSource.</param>
        /// <param name="targetVolume">The volume to set the AudioSource at following the fade in.</param>
        /// <returns></returns>
        private IEnumerator FadeInCoroutine(AudioSource source, float fadeTime, float targetVolume = 1f)
        {
            if (source != null)
            {
                // Initially, the sound is muted
                source.volume = 0;

                // Start playing the AudioSource
                source.Play();

                // Fade in a bit every frame
                while (source.volume < targetVolume)
                {
                    source.volume += targetVolume * Time.deltaTime / fadeTime;
                    yield return null;
                }

                // Set the volume following the fade in process
                source.volume = targetVolume;
            }
        }
    }
}

