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
using UnityEngine;
using Managers;
using UnityEngine.Audio;
using Entities.Workstations;
using Systems;
using Mirror;

namespace Audio
{
    /// <summary>
    /// This class contains a function to play every sound effect, and should be treated like an API.
    /// This lets us easily switch out systems like FMOD or the current audio manager without touching other code.
    /// </summary>
    public class AudioPlayer : Singleton<AudioPlayer>
    {
        [SerializeField] private SoundRefManager soundRefManager;
        [SerializeField] private AudioMixerGroup masterMixer;
        [SerializeField] private AudioListener listener;
        public GameObject following;

        [Header("Flight Engineer Thruster Volume Variables")]
        [SerializeField]
        float pitchRaisePerThruster = 0.1f;
        [SerializeField]
        float volumeRaisePerThruster = 0.2f;

        private AudioSource ambience;
        private GameObject player;
        // The original rotation of the AudioListener
        private Quaternion originalRotation;
        // Whether the AudioListener is rotating
        private bool rotating;

        // General Workstation variables
        // The AudioSource used to play workstation ambiance
        private AudioSource workstationHum;

        // Flight Engineer variables
        // The AudioSource used to play the engine rev sound effect
        private AudioSource engineRev;
        // The maximum pitch the engine rev sound effect should reach
        private float maxPitchEngineRev = 3f;
        // The AudioSource used to play the thruster loop
        private AudioSource thrusterLoopLow;
        // Each of the 4 thruster sounds
        private AudioSource[] thrusterLoopHigh = new AudioSource[4];
        // The coroutines used to fade in/out each of the thruster sounds
        private Coroutine[] thrusterLoopHighFades = new Coroutine[4];
        // Each of the 3 tube sounds
        private AudioSource[] flightEngineerTubes = new AudioSource[3];
        // The coroutines used to face in/out  the flight engineer rube sounds
        private Coroutine[] flightEngineerTubesFades = new Coroutine[3];

        // Power Routing variables
        // Each of the 8 power routing tube sounds
        private AudioSource[] powerRoutingTubes = new AudioSource[8];
        // The coroutines used to fade in/out the power routing tube sounds
        private Coroutine[] powerRoutingTubesFades = new Coroutine[8];

        // The AudioSource used to play the antenna extension sound
        private AudioSource antennaExtend;
        // The AudioSource used to play the hologram sound on cube insertion
        private AudioSource cubeHologram;
        // The AudioSource used to play the transmission alert sound at the sensor station
        private AudioSource transmissionAlert;


        /// <summary>
        /// Unity event function used for setup on instantiation of this object.
        /// </summary>
        public override void Start()
        {
            base.Start();
            // Set the loading message
            LoadingSystem.Instance.UpdateLoadingMessage("Auditorizing Information...");

            // Set the object this AudioPlayer is following and use its rotation
            following = gameObject;
            originalRotation = listener.transform.rotation;
        }

        /// <summary>
        /// Unity event function that sets the AudioListener position and rotation every frame.
        /// </summary>
        void Update()
        {
            // If we have a listener and we're following an object, set the AudioListener's position
            if (listener && following)
            {
                // Set the AudioListener's position to be the same as the position of the one we are following
                listener.transform.position = following.transform.position;
                // If rotating, update the rotation
                if (rotating)
                {
                    listener.transform.rotation = following.transform.rotation;
                }
            }
            else
            {
                if (((CustomNetworkManager)NetworkManager.singleton).isInDebugMode)
                    Debug.LogWarning("audio listener tracking null!");
            }
        }

        /// <summary>
        /// .Sets the player reference to be the given player object and sets the listener to that player's location.
        /// </summary>
        /// <param name="playerObject">The local player GameObject.</param>
        public void OnConnect(GameObject playerObject)
        {
            // Attach the given GameObject to the local player reference
            player = playerObject;
            // Reset the listener object's location
            ResetListener();
            // If we have no ambiance set, make a new ambiance
            if (ambience == null)
            {
                ambience = AudioManager.Instance.CreateAmbienceInstance(SoundType.Ambience_Ship, AudioManager.Instance.transform);
            }
            // Fade in the volume
            AudioManager.Instance.FadeIn(ambience, 0.8f);
        }

        /// <summary>
        /// Removes the reference to the GameObject being followed when a player disconnects.
        /// </summary>
        public void OnDisconnect()
        {
            following = gameObject;
        }

        /// <summary>
        /// Sets the location of the AudioListener object to be that of the location of the given GameObject.
        /// </summary>
        /// <param name="attenuationObject">The object to make this listener follow.</param>
        /// <param name="isRotating">Whether the object to match is rotating.</param>
        public void SetListenerLocation(GameObject attenuationObject, bool isRotating)
        {
            // Set the object being followed to the object passed in and whether we're rotating
            following = attenuationObject;
            rotating = isRotating;
            // If the object is not rotating, the listener shouldn't be rotating
            if (!isRotating)
            {
                listener.transform.rotation = originalRotation;
                rotating = false;
            }
        }

        /// <summary>
        /// Resets the location of the AudioListener to be the player.
        /// </summary>
        public void ResetListener()
        {
            SetListenerLocation(player, false);
        }

        /*
         * These functions are to allow easy switching out of the audio system.
         * No script outside of AudioPlayer should interact with AudioManager.
         */

        /// <summary>
        /// Gets the volume reference of a shared group for sounds based on the group name provided.
        /// The possible groups are SFX, Transmission, Ambiance, and Master.
        /// </summary>
        /// <param name="groupName">The name of the sound group.</param>
        /// <returns>A reference to the volume of the sound group.</returns>
        private ref float GetGroupVolumeRef(string groupName)
        {
            switch (groupName)
            {
                case("SFXVol"):
                    return ref AudioManager.Instance.sfxVolume;
                case("TransmissionVol"):
                    return ref AudioManager.Instance.transmissionVolume;
                case("AmbienceVol"):
                    return ref AudioManager.Instance.ambianceVolume;
                default:
                    return ref AudioManager.Instance.masterVolume;
            }
        }

        /// <summary>
        /// Sets the volume for a group of sounds. Used in the UI settings menu.
        /// </summary>
        /// <param name="volume">The volume to set the group volume to.</param>
        /// <param name="groupName">The name of the group whose volume should be changed.</param>
        public void SetGroupVolume(float volume, string groupName)
        {
            ref float vol = ref GetGroupVolumeRef(groupName);
            vol = volume;
            masterMixer.audioMixer.SetFloat(groupName, volume);
        }

        /// <summary>
        /// Gets the volume of a shared group for sounds based on the group name provided.
        /// </summary>
        /// <param name="groupName">The name of the sound group.</param>
        /// <returns>A reference to the volume of the sound group.</returns>
        public float GetGroupVolume(string groupName)
        {
            return GetGroupVolumeRef(groupName);
        }

        /* SNAPSHOTS */

        /// <summary>
        /// Sets whether to mute the game via snapshots. Mutes all SFX + transmissions
        /// </summary>
        /// <param name="muted">Whether to mute the snapshot (muting the game).</param>
        public void SetMuteSnapshot(bool muted)
        {
            // If muting the game, transition to muting after one second
            if (muted)
            {
                soundRefManager.MuteSnapshot.TransitionTo(1f);
            }
            // Otherwise, transition to the default snapshot after one second
            else
            {
                soundRefManager.DefaultSnapshot.TransitionTo(1f);
            }
        }

        /// <summary>
        /// Sets whether to mute the sound effects via snapshots. Mutes only SFX
        /// </summary>
        /// <param name="muted">Whether to mute the snapshot (muting the sound effects).</param>
        public void SetMuteSFXSnapshot(bool muted)
        {
            // If muting the sound effects, transition to muting after one second
            if (muted)
            {
                soundRefManager.MuteSFXSnapshot.TransitionTo(1f);
            }
            // Otherwise, transition to the default snapshot after one second
            else
            {
                soundRefManager.DefaultSnapshot.TransitionTo(1f);
            }
        }

        /* UI */

        /// <summary>
        /// Plays an error sound for interacting with non-interactive UI.
        /// </summary>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UIError(Transform transform = null)
        {
            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(SoundType.UI_Error);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(SoundType.UI_Error, transform);
            }
        }


        /// <summary>
        /// Plays a sound for interacting with moving UI.
        /// </summary>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UIMove(Transform transform = null)
        {
            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(SoundType.UI_Move);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(SoundType.UI_Move, transform);
            }
        }

        /// <summary>
        /// Plays a sound for exiting a UI interface.
        /// </summary>
        /// <param name="version">The version of the sound to play.</param>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UIExit(int version = 0, Transform transform = null)
        {
            // Play a different exit sound depending on the version of the sound specified
            SoundType soundType;
            switch (version)
            {
                default:
                case 0:
                    soundType = SoundType.UI_Exit_1;
                    break;
                case 1:
                    soundType = SoundType.UI_Exit_2;
                    break;
            }

            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(soundType);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(soundType, transform);
            }
        }

        /// <summary>
        /// Plays a sound for clicking on a UI button.
        /// </summary>
        /// <param name="version">The version of the sound to play.</param>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UISelect(int version = 0, Transform transform = null)
        {
            // Play a different selection sound depending on the version of the sound specified
            SoundType soundType;
            switch (version)
            {
                default:
                case 0:
                    soundType = SoundType.UI_Select_1;
                    break;
                case 1:
                    soundType = SoundType.UI_Select_1;  // This can be changed to SoundType.UI_Select_2 if desired
                    break;
            }

            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(soundType);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(soundType, transform);
            }
        }

        /// <summary>
        /// Plays a sound for mousing over a UI item.
        /// </summary>
        /// <param name="version">The version of the sound to play.</param>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UIMouseover(int version = 0, Transform transform = null)
        {
            // Play a different mouse over sound depending on the version of the sound specified
            SoundType soundType;
            switch(version)
            {
                default:
                case 0:
                    soundType = SoundType.UI_Mouseover_1;
                    break;
                case 1:
                    soundType = SoundType.UI_Mouseover_2;
                    break;
            }

            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(soundType);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(soundType, transform);
            }
        }

        /// <summary>
        /// Plays a sound for a miscellaneous UI interaction.
        /// </summary>
        /// <param name="transform">The transform whose position should be used for where to play this sound.</param>
        public void UIMisc(Transform transform = null)
        {
            // If the transform is not provided, play the sound as basic UI SFX
            if (transform == null)
            {
                AudioManager.Instance.PlayUISFX(SoundType.UI_Misc);
            }
            // Otherwise, play the sound at the location of the given transform
            else
            {
                AudioManager.Instance.PlayOneShot(SoundType.UI_Misc, transform);
            }           
        }

        /* General workstation sound effects */

        /// <summary>
        /// Plays a sound for powering on a workstation.
        /// </summary>
        /// <param name="transform">The workstation transform whose position should be used for where to play this sound.</param>
        public void WorkstationPowerOn(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.WS_Start, transform);
        }

        /// <summary>
        /// Plays a sound for powering off a workstation.
        /// </summary>
        /// <param name="transform">The workstation transform whose position should be used for where to play this sound.</param>
        public void WorkstationPowerOff(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.WS_End, transform);
        }

        /// <summary>
        /// Fades in the humming sound for a workstation and creates an instance of an AudioSource to play that sound.
        /// </summary>
        /// <param name="transform">The workstation transform whose position should be used for where to play this sound.</param>
        public void WorkstationHumOn(Transform transform)
        {
            workstationHum = AudioManager.Instance.CreateInstance(SoundType.WS_Loop);
            AudioManager.Instance.AttachInstanceToGameObject(workstationHum, transform);
            AudioManager.Instance.FadeIn(workstationHum, 1.0f);
        }

        /// <summary>
        /// Fades out the humming sound for a workstation.
        /// </summary>
        public void WorkstationHumOff()
        {
            AudioManager.Instance.FadeOut(workstationHum, 1.0f);
        }

        /// <summary>
        /// Continually creates new ambiance AudioSources for a terminal every few seconds.
        /// </summary>
        /// <param name="transform">The workstation transform whose position should be used for where to play this sound.</param>
        /// <returns>A yield while waiting for a random amount of seconds to pass.</returns>
        public IEnumerator TerminalAmbience(Transform transform)
        {
            // Loops creating and playing an ambiance, unless it does not exist
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(8f, 15f));
                AudioSource source = AudioManager.Instance.CreateAmbienceInstance(SoundType.Ambience_Terminal, transform);
                if (source == null)
                {
                    break;
                }

                // Set spatialization attributes
                source.maxDistance = 2.5f;
                source.minDistance = 0.5f;
                source.spread = 0;

                // Play the ambiance
                source.Play();
            }
        }

        /* Flight Engineer workstation */

        /// <summary>
        /// Plays a sound for when a dial on the Flight Engineer workstation is turned to a new number.
        /// </summary>
        /// <param name="transform">The transform of the dial whose position should be used for where to play this sound.</param>
        public void FlightEngineerDialTick(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Dial_Tick, transform);
        }

        /// <summary>
        /// Plays a sound for when the thrusters on the Flight Engineer workstation is flipped.
        /// </summary>
        /// <param name="transform">The transform of the switch whose position should be used for where to play this sound.</param>
        public void FlightEngineerSwitch(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Switches, transform);
        }

        /// <summary>
        /// Plays a sound for when the launch slider on the Flight Engineer workstation is pushed up.
        /// </summary>
        /// <param name="transform">The transform of the lever whose position should be used for where to play this sound.</param>
        public void FlightEngineerLeverLock(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Lever_Lock, transform);
        }

        /// <summary>
        /// Plays a sound for when the ship launches.
        /// </summary>
        public void EngineTakeoff()
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Engine_Takeoff);
        }

        /// <summary>
        /// Plays a sound for when the ship is ready to launch (but has not launched yet).
        /// </summary>
        public void FlightEngineerStartEngine()
        {
            // If we have no engine rev AudioSource active or playing, make a new instance of one
            if (engineRev == null || !engineRev.isPlaying)
            {
                engineRev = AudioManager.Instance.CreateInstance(SoundType.FE_Engine_Loop);
                AudioManager.Instance.FadeIn(engineRev, 0.5f);
            }
        }

        /// <summary>
        /// Changes the pitch of the engine rev AudioSource when the launch slider is moved.
        /// </summary>
        /// <param name="percent">The percent that the launch slider has traveled to its launch position.</param>
        public void FlightEngineerSetEnginePercent(float percent)
        {
            if (engineRev)
            {
                engineRev.pitch = 1 + percent * maxPitchEngineRev;
            }
        }

        /// <summary>
        /// Fades out the start engine sound effect.
        /// </summary>
        public void FlightEngineerStopEngine()
        {
            if (engineRev != null)
            {
                AudioManager.Instance.FadeOut(engineRev, 1f);
                engineRev = null;
            }
        }

        /// <summary>
        /// Sets the intensity of the thruster hum based on the number of thrusters activated.
        /// </summary>
        /// <param name="numThrusters">The number of thrusters currently activated.</param>
        public void FlightEngineerSetThrusterHumNumber(int numThrusters)
        {
            // If no thrusters are on, fade the loop out and turn the sound off
            if (numThrusters == 0 && thrusterLoopLow)
            {
                AudioManager.Instance.FadeOut(thrusterLoopLow, 0.2f);
                thrusterLoopLow = null;
                return;
            }

            // If the hum is currently off, instantiate a new thruster loop
            if (numThrusters > 0 && !thrusterLoopLow)
            {
                thrusterLoopLow = AudioManager.Instance.CreateInstance(SoundType.FE_Thruster_Loop);
                AudioManager.Instance.FadeIn(thrusterLoopLow, 0.5f);
            }

            // If the hum is on, set the pitch/volume of the hum
            if (numThrusters > 0)
            {
                thrusterLoopLow.pitch = 1 + (numThrusters - 1) * pitchRaisePerThruster;
                thrusterLoopLow.volume = 0.2f + numThrusters * volumeRaisePerThruster;
            }
        }

        /// <summary>
        /// Plays a sound for when a thruster is flipped on, and not when not entering the Flight Engineer.
        /// </summary>
        public void FlightEngineerThrusterOn()
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Thruster_Up);
        }

        /// <summary>
        /// Fades in a sound for when a thruster has been flipped on.
        /// </summary>
        /// <param name="index">The index of the AudioSource playing and the fade in operation running.</param>
        /// <param name="transform">The transform the AudioSource at the given index should use for its position.</param>
        public void FlightEngineerThrusterPulseOn(int index, Transform transform)
        {
            AudioManager.Instance.FadeInIndexedLoop(thrusterLoopHigh, thrusterLoopHighFades, index, SoundType.FE_Thruster_Power, 2f, transform);
        }

        /// <summary>
        /// Fades out a sound for when a thruster has been flipped on.
        /// </summary>
        /// <param name="index">The index of the AudioSource playing and the fade out operation running.</param>
        public void FlightEngineerThrusterPulseOff(int index)
        {
            AudioManager.Instance.FadeOutIndexedLoop(thrusterLoopHigh, thrusterLoopHighFades, index, 1f);
        }

        /// <summary>
        /// Fades in a sound for when a dial at the Flight Engineer workstation reaches its target number.
        /// </summary>
        /// <param name="index">The ID of the Workstation dial, converted to an integer.</param>
        public void FlightEngineerTubeOn(int index)
        {
            AudioManager.Instance.FadeInIndexedLoop(flightEngineerTubes, flightEngineerTubesFades, index, SoundType.FE_Tubes, 4f);
        }

        /// <summary>
        /// Fades out the sound for when a dial at the Flight Engineer workstation has not reached/is not longer at its target number.
        /// </summary>
        /// <param name="index">The ID of the Workstation dial, converted to an integer.</param>
        public void FlightEngineerTubeOff(int index)
        {
            AudioManager.Instance.FadeOutIndexedLoop(flightEngineerTubes, flightEngineerTubesFades, index, 1f);
        }

        /// <summary>
        /// Plays a sound for when the lock button is selected.
        /// </summary>
        /// <param name="transform">The lock button transform whose position should be used for where to play this sound.</param>
        public void LockButtonClick(Transform transform)
        {
             AudioManager.Instance.PlayOneShot(SoundType.FE_Lock_Click, transform);
        }

        /* Power Routing Workstation */

        /// <summary>
        /// Fades in the sound for a workstation being powered on at the Power Routing workstation.
        /// </summary>
        /// <param name="id">The Workstation ID of the workstation whose associated tube sound should fade in.</param>
        /// <param name="transform">The pipe transform whose position should be used for where to play this sound.</param>
        public void PowerRoutingTubeOn(WorkstationID id, Transform transform)
        {
            int index = PowerRoutingConvertStationID(id);
            AudioManager.Instance.FadeInIndexedLoop(powerRoutingTubes, powerRoutingTubesFades, index, SoundType.PR_Tubes, 2f, transform);
        }

        /// <summary>
        /// Fades out the sound for a workstation being powered on at the Power Routing workstation. This occurs when that workstation is switched off.
        /// </summary>
        /// <param name="id">The Workstation ID of the workstation whose associated tube sound should fade out.</param>
        public void PowerRoutingTubeOff(WorkstationID id)
        {
            int index = PowerRoutingConvertStationID(id);
            AudioManager.Instance.FadeOutIndexedLoop(powerRoutingTubes, powerRoutingTubesFades, index, 2f);
        }

        /// <summary>
        /// Converts a Workstation ID to a specific index. Hardcoded to switch only two indices to different numbers. This is done to keep sound manipulation within the array of power routing tubes fade coroutines.
        /// </summary>
        /// <param name="id">The Workstation ID of the workstation whose index may need changing.</param>
        /// <returns>The index of the workstation ID, possibly switched to a different number.</returns>
        private int PowerRoutingConvertStationID(WorkstationID id)
        {
            int index = (int) id;
            switch (index)
            {
                // If given a Workstation IDgreater than 7, change it to map to a different AudioSource
                case 8:
                    return 2;
                case 9:
                    return 6;
                default:
                    return index;
            }
        }

        /* Antenna Workstation */

        /// <summary>
        /// Plays a sound for when the antenna lever is pulled to its maximum.
        /// </summary>
        /// <param name="transform">The transform of the antenna lever, used for 3D sound spatialization.</param>
        public void AntennaLeverLock(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.FE_Lever_Lock, transform);
        }

        /* Cube Drive Workstation */
        public void CubeHologramPowerOn(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.CS_HologramPower, transform);
        }

        /// <summary>
        /// Plays a sound for when the cube hologram should appear (when the cube is inserted into the Cube Drive workstation).
        /// </summary>
        public void CubeHologramLoopOn()
        {
            if (!cubeHologram)
            {
                cubeHologram = AudioManager.Instance.CreateInstance(SoundType.CS_Hologram);
                cubeHologram.Play();
            }
        }

        /// <summary>
        /// Stops the sound for the cube hologram (when the cube is not in the Cube Drive workstation).
        /// </summary>
        public void CubeHologramLoopOff()
        {
            if (cubeHologram)
            {
                cubeHologram.Stop();
                cubeHologram = null;
            }
        }

        /* Codex Workstation */

        /// <summary>
        /// Plays a sound for when the team unlocks a codex.
        /// </summary>
        public void CodexGet()
        {
            AudioManager.Instance.PlayOneShot(SoundType.Codex_CodexGet);
        }

        /* Sensor Station */

        /// <summary>
        /// Plays a sound for when a scan has been completed.
        /// </summary>
        /// <param name="transform">The sensor station screen transform, used for 3D sound spatialization.</param>
        public void ScanComplete(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.SS_Complete, transform);
        }

        /// <summary>
        /// Plays a sound for when a transmission has ended.
        /// </summary>
        /// <param name="transform">The sensor station screen transform, used for 3D sound spatialization.</param>
        public void TransmissionEnded(Transform transform)
        {
            AudioManager.Instance.PlayOneShot(SoundType.SS_TransmissionEnded, transform);
        }

        /// <summary>
        /// Plays a sound for when an alert is present at the sensor station.
        /// </summary>
        public void TransmissionAlert()
        {
            if (transmissionAlert == null || !transmissionAlert.isPlaying)
            {
                transmissionAlert = AudioManager.Instance.CreateInstance(SoundType.SS_TransmissionAlert);
                AudioManager.Instance.FadeIn(transmissionAlert, 0.1f);
            }
        }

        /// <summary>
        /// Stops the sound for when an alert is present at the sensor station.
        /// </summary>
        public void StopTransmissionAlert()
        {
            if (transmissionAlert != null)
            {
                AudioManager.Instance.FadeOut(transmissionAlert, 0.2f);
                transmissionAlert = null;
            }
        }

        /* Nav Reader Workstation */

        /// <summary>
        /// Plays a sound for when incorrect coordinates are entered.
        /// </summary>
        public void IncorrectCoordinates()
        {
            AudioManager.Instance.PlayOneShot(SoundType.NR_IncorrectCoord);
        }

        /// <summary>
        /// Plays a sound for when correct coordinates are entered.
        /// </summary>
        public void CorrectCoordinates()
        {
            AudioManager.Instance.PlayOneShot(SoundType.NR_CorrectCoord);
        }

        /* Offline Scene */

        /// <summary>
        /// Plays a sound for when an offline scene UI element is moused over.
        /// </summary>
        public void OfflineUI_Hover()
        {
            AudioManager.Instance.PlayOneShot(SoundType.OF_UI_Mouseover);
        }

        /// <summary>
        /// Plays a sound for when an offline scene UI element is clicked.
        /// </summary>
        public void OfflineUI_Click()
        {
            AudioManager.Instance.PlayOneShot(SoundType.OF_UI_Click);
        }

        /// <summary>
        /// Plays a sound for when the offline scene "Connect" button is clicked.
        /// </summary>
        public void OfflineUI_Connect_Click()
        {
            AudioManager.Instance.PlayOneShot(SoundType.OF_UI_Connect_Click);
        }
    }
}

