/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using UnityEngine;

namespace Entities.Workstations.CubeStationParts
{
    /// <summary>
    /// A component for the slot in the Cube Drive where the cube is inserted.
    /// </summary>
    public class CubeSlot : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The cube drive workstation.
        /// </summary>
        [SerializeField]
        private CubeStation station;
        /// <summary>
        /// The MeshRenderer of the cube inserted into the slot.
        /// </summary>
        [SerializeField]
        private MeshRenderer cubeMesh;

        /// <summary>
        /// Whether the cube is inserted into the cube drive.
        /// </summary>
        private bool IsCubeInserted => ShipStateManager.Instance.CubeState == CubeState.InCubeDrive;
        /// <summary>
        /// The animator component of this CubeSlot.
        /// </summary>
        private Animator animator;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that sets the cube state immediately on startup of this object.
        /// </summary>
        private void Start()
        {
            animator = GetComponent<Animator>();
            SetCubeMesh(IsCubeInserted);
        }

        /// <summary>
        /// Unity event function that subscribes to the cube state change action on the ShipStateManager.
        /// </summary>
        private void OnEnable()
        { 
            ShipStateManager.OnCubeStateChange += OnCubeStateChanged;
        }

        /// <summary>
        /// Unity event function that unsubscribes from the cube state change action on the ShipStateManager.
        /// </summary>
        private void OnDisable()
        {
            ShipStateManager.OnCubeStateChange -= OnCubeStateChanged;
        }

        /// <summary>
        /// Unity event function that sets the animator state and cube mesh and aprite appearance.
        /// </summary>
        private void OnMouseDown()
        {
            if (IsCubeInserted || !station.playerAtWorkstation || !Player.LocalCanInput)
            {
                return;
            }
            if (station.IsPowered && ShipStateManager.Instance.PlayerIsHoldingCube(station.playerAtWorkstation))
            {
                // This animation calls OnAnimationFinish, which actually updates the cube state through a series of calls in ShipStateManager
                animator.SetBool("CubeInserted", true);

                // Locally change the mesh and sprite, for instant feedback without waiting for server callback
                SetCubeMesh(true);
                UI.HUD.HUDController.Instance.SetCubeSprite(false);
            }
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Sets the cube mesh and animator state based on the new cube state.
        /// </summary>
        /// <param name="cubeState">The new state of the cube.</param>
        private void OnCubeStateChanged(CubeState cubeState)
        {
           SetCubeMesh(cubeState == CubeState.InCubeDrive);
           if (cubeState != CubeState.InCubeDrive && animator) 
           {
               animator.SetBool("CubeInserted", false);
           }
        }
        #endregion

        #region Animation methods
        /// <summary>
        /// Animation function that activates the cube hologram SFX and formally inserts the cube.
        /// This is fired at the end of the cube's animation.
        /// </summary>
        public void OnAnimationFinish()
        {
            ShipStateManager.Instance.InsertCube();
            
            Audio.AudioPlayer.Instance.CubeHologramPowerOn(transform);
            if (station.playerAtWorkstation != null)
            {
                Audio.AudioPlayer.Instance.CubeHologramLoopOn();
            }
        }
        #endregion

        #region Cube mesh
        /// <summary>
        /// Enables or disables the holographic cube mesh.
        /// </summary>
        /// <param name="state">Whether to show the hologram of the cube.</param>
        public void SetCubeMesh(bool state)
        {
            cubeMesh.enabled = state;
        }
        #endregion
    }
}

