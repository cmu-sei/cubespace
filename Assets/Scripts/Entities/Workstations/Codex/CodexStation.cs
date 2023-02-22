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
using Mirror;
using Systems.GameBrain;
using Managers;

namespace Entities.Workstations.CodexStationParts
{
    /// <summary>
    /// A class for the codex workstation, used to enable getting pieces of the codex.
    /// </summary>
    public class CodexStation : VMWorkstation
    {
        #region Variables
        /// <summary>
        /// The full hologram model of the codex.
        /// </summary>
        [Header("References")]
        [SerializeField]
        private CodexHologram codexHologram;
        /// <summary>
        /// The canvas containing a button that allows a player to access the Codex VM when enabled.
        /// </summary>
        [SerializeField]
        private Canvas VMCanvas;
        /// <summary>
        /// The animator of the codex hologram.
        /// </summary>
        [SerializeField]
        private Animator codexHologramAnimator;
        /// <summary>
        /// The front pipes on the codex workstation.
        /// </summary>
        [SerializeField]
        private List<WorkstationPipe> frontPipes;
        /// <summary>
        /// The  back pipes on the codex workstation.
        /// </summary>
        [SerializeField]
        private List<WorkstationPipe> backPipes;

        /// <summary>
        /// The time needed to speed up the codex hologram.
        /// </summary>
        [Header("Spin Animation Variables")]
        [SerializeField]
        private float spinUpTime = 4.0f;
        /// <summary>
        /// The time needed to speed down the codex hologram.
        /// </summary>
        [SerializeField]
        private float spinDownTime = 2.5f;
        /// <summary>
        /// The final speed that the hologram should accelerate to when a codex piece is gained.
        /// </summary>
        [SerializeField]
        private float finalSpinSpeed = 70.0f;

        /// <summary>
        /// The emission to set a group of pipes to have when first flashing them.
        /// </summary>
        [Header("Flash Animation Variables")]
        [SerializeField]
        private float startFlashEmissionPower = 2.0f;
        /// <summary>
        /// The emission to set a group of pipes to have when finishing flashing them.
        /// </summary>
        [SerializeField]
        private float endFlashEmissionPower = 0.0f;

        /// <summary>
        /// The time it takes to flash the front pipes.
        /// </summary>
        [Header("Add Codex Piece Animation Variables")]
        [SerializeField]
        private float frontPipeFlashDuration = 4.0f;
        /// <summary>
        /// The time to wait between flashing the front pipes and flashing the back pipes.
        /// </summary>
        [SerializeField]
        private float frontPipeFlashInterval = 1.5f;
        /// <summary>
        /// The time it takes to flash the back pipes.
        /// </summary>
        [SerializeField]
        private float backPipeFlashDuration = 2.5f;
        /// <summary>
        /// The time to wait between flashing the back pipes and setting the number of active pieces.
        /// </summary>
        [SerializeField]
        private float backPipeFlashInterval = 2.5f;

        /// <summary>
        /// The number of pieces obtained for the codex. Derives from a private variable.
        /// </summary>
        public int CodexPieceCount => codexPieceCount;
        /// <summary>
        /// The number of pieces obtained for the codex.
        /// </summary>
        [SyncVar(hook = nameof(OnCodexPieceCountChangeHook))]
        private int codexPieceCount;
        #endregion

        #region Workstation Functions
        /// <summary>
        /// Enables or disables the arm and hologram animations when entering the codex workstation.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();

            // If the codex is powered on already, show the arm and hologram animations
            if (IsPowered) 
            {
                ActivateVisuals();
                VMCanvas.enabled = true;
            }
            // Otherwise, hide both items
            else
            {
                codexHologram.Deactivate();
                VMCanvas.enabled = false;
            }
        }

        /// <summary>
        /// Turns off the arm and hologram animations.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            DeactivateVisuals();
        }

        /// <summary>
        /// Enables or disables the arm and hologram animations and calls Gamebrain to set the codex power state.
        /// </summary>
        /// <param name="isPowered">Whether or not the codex is powered.</param>
        public override void ChangePower(bool isPowered)
        {
            base.ChangePower(isPowered);

            // Call the server to set the power state of the codex workstation on Gamebrain
            CmdSetCodexPower(isPowered);

            // If this client is at the codex workstation, turn on or off the animations and VM canvas
            if (playerAtWorkstation != null && playerAtWorkstation.isLocalPlayer)
            {   
                if (isPowered)
                {
                    ActivateVisuals();
                    VMCanvas.enabled = true;
                }
                else
                {
                    DeactivateVisuals();
                    VMCanvas.enabled = false;
                }
            }
        }
        #endregion

        #region VMWorkstation methods
        /// <summary>
        /// Updates the VM
        /// </summary>
        /// <param name="hasChanged"></param>
        /// <param name="data">The game data received from Gamebrain.</param>
        protected override void OnShipDataReceived(bool hasChanged, GameData data)
        {
            // If the game data is different or the VM URL given is blank, set the VM URL and the number of codex pieces
            if (hasChanged || string.IsNullOrEmpty(_vmURL))
            {
                _vmURL = data.ship.GetURLForStation(StationID);
                codexPieceCount = data.session.teamCodexCount;
            }
        }

        /// <summary>
        /// Enables or disables the canvas used to access the VM.
        /// </summary>
        /// <param name="isEnabled">Whether the VM canvas should be shown.</param>
        protected override void SetAccessUIState(bool isEnabled)
        {
            VMCanvas.enabled = isEnabled;
        }
        #endregion

        #region SyncVar hooks
        /// <summary>
        /// Updates the visual number of codex pieces shown on the codex workstation when the number of codex pieces changes.
        /// </summary>
        /// <param name="oldVal">The old number of codex pieces.</param>
        /// <param name="newVal">The new number of codex pieces.</param>
        private void OnCodexPieceCountChangeHook(int oldVal, int newVal)
        {
            // Starts an animation adding more codex pieces if more pieces have been added
            if (newVal > oldVal)
            {
                StartCoroutine(AddCodexPieceAnim(newVal));
            }
            // Otherwise, if the hologram is active, remove pieces from it
            else if (codexHologram.IsActive)
            {
                Debug.LogWarning("Number of codex pieces decreased");
                codexHologram.SetNumberActivePieces(newVal);
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Sends a request on the server to Gamebrain to change the power status of the codex workstation.
        /// </summary>
        /// <param name="isPowered">Whether the codex is powered.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetCodexPower(bool isPowered)
        {
            ShipStateManager.Instance.ShipGameBrainUpdater.TrySetCodexPower(isPowered);
        }
        #endregion

        #region Visual methods
        /// <summary>
        /// Activates the arm and hologram animations, on activate and power up.
        /// </summary>
        private void ActivateVisuals()
        {
            codexHologram.Activate();
            codexHologramAnimator.SetBool("isActivated", true);
        }

        /// <summary>
        /// Deactivates the arm and hologram animations, on deactivate and power down.
        /// </summary>
        private void DeactivateVisuals() 
        {
            codexHologram.Deactivate();
            codexHologramAnimator.SetBool("isActivated", false);
        }
        #endregion

        #region Animation coroutines
        /// <summary>
        /// Adds a codex piece to the hologram over time.
        /// </summary>
        /// <param name="totalPieces">The number of pieces obtained for the codex hologram.</param>
        /// <returns>A yield statement while waiting for the codex piece to be added.</returns>
        IEnumerator AddCodexPieceAnim(int totalPieces) 
        {
            // Play an audio cue
            Audio.AudioPlayer.Instance.CodexGet();
            // Speed up the animation
            StartCoroutine(SpeedUpSpin());
            // Flash the front pipes
            StartCoroutine(FlashPipes(frontPipes, frontPipeFlashDuration));
            yield return new WaitForSeconds(frontPipeFlashInterval);
            // Flash the back pipes
            StartCoroutine(FlashPipes(backPipes, backPipeFlashDuration));
            yield return new WaitForSeconds(backPipeFlashInterval);
            // Set the number of active codex pieces
            codexHologram.SetNumberActivePieces(totalPieces);
        }

        /// <summary>
        /// Speeds up and slows down the spin animation of the codex hologram.
        /// </summary>
        /// <returns>A yield statement while waiting for the animation to speed up and slow down.</returns>
        IEnumerator SpeedUpSpin()
        {
            // The final spin speed we want is the given speed minus the default animation value
            float newFinalSpinSpeed = finalSpinSpeed - 1.0f;

            // Gradually speed up the spin of the hologram
            float timeElapsed = 0f;
            while (timeElapsed < spinUpTime)
            {
                codexHologram.SetSpinSpeed(1.0f + newFinalSpinSpeed * (timeElapsed / spinUpTime));
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Gradually slow down the spin of the hologram
            timeElapsed = 0f;
            while (timeElapsed < spinDownTime)
            {
                codexHologram.SetSpinSpeed(1.0f + (newFinalSpinSpeed - (newFinalSpinSpeed * (timeElapsed / spinDownTime))));
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Set the animation spin speed back to its default
            codexHologram.SetSpinSpeed(1.0f);
        }

        /// <summary>
        /// Flashes the emission of a given set of pipes.
        /// </summary>
        /// <param name="pipes">The pipes to flash the emission on.</param>
        /// <param name="duration">The time to wait in between flashing the pipes on and off.</param>
        /// <returns>A yield statement while waiting to flash the pipes off.</returns>
        IEnumerator FlashPipes(List<WorkstationPipe> pipes, float duration)
        {
            pipes.ForEach(p => p.SetEmissionPower(startFlashEmissionPower));
            yield return new WaitForSeconds(duration);
            pipes.ForEach(p => p.SetEmissionPower(endFlashEmissionPower));
        }
        #endregion
    }
}

