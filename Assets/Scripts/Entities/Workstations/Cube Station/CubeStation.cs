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
using Managers;
using UnityEngine;

namespace Entities.Workstations.CubeStationParts
{
    /// <summary>
    /// A class for the cube workstation, used as the final step required to launch the ship.
    /// </summary>
    public class CubeStation : Workstation
    {
        #region Variables
        /// <summary>
        /// The mesh for rendering the holographic cube.
        /// </summary>
        [SerializeField]
        private GameObject holoCubeMesh;
        /// <summary>
        /// The inner walls where the cube is inserted.
        /// </summary>
        [SerializeField]
        private WorkstationPipe insertWalls;
        /// <summary>
        /// The pipes whose emission power is turned on or off depending on if the cube is in the Cube Drive.
        /// </summary>
        [SerializeField]
        private List<WorkstationPipe> pipes;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that subscribes to a cube state change event on the ShipStateManager.
        /// </summary>
        private void OnEnable()
        {
            ShipStateManager.OnCubeStateChange += OnCubeStateChange;
        }

        /// <summary>
        /// Unity event function that unsubscribes from a cube state change event on the ShipStateManager.
        /// </summary>
        private void OnDisable()
        {
            ShipStateManager.OnCubeStateChange -= OnCubeStateChange;
        }
        #endregion

        #region Callback methods
        /// <summary>
        /// Activates the holographic cube mesh and pipes if the cube is placed in the cube drive.
        /// </summary>
        /// <param name="cubeState">The state of the cube.</param>
        private void OnCubeStateChange(CubeState cubeState)
        {
            holoCubeMesh.SetActive(cubeState == CubeState.InCubeDrive);
            SetPipes(cubeState == CubeState.InCubeDrive);
        }
        #endregion

        #region Workstation methods
        /// <summary>
        /// Sets the emission of the cube walls when the cube station is powered or not.
        /// </summary>
        /// <param name="isPowered"></param>
        public override void ChangePower(bool isPowered)
        {
            base.ChangePower(isPowered);
            SetGlowyWalls(isPowered);
        }

        /// <summary>
        /// Enables the cube hologram SFX and the pipes and walls when the workstation is entered,
        /// if the cube has been inserted.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();

            if (ShipStateManager.Instance.CubeState == CubeState.InCubeDrive)
            {
                Audio.AudioPlayer.Instance.CubeHologramLoopOn();
            }
            SetPipes(ShipStateManager.Instance.CubeState == CubeState.InCubeDrive);
            SetGlowyWalls(IsPowered);
        }

        /// <summary>
        /// Disables the cube hologram SFX when the workstation is exited.
        /// </summary>
        protected override void Exit()
        {
            base.Exit();
            Audio.AudioPlayer.Instance.CubeHologramLoopOff();
        }
        #endregion

        #region Wall/pipe methods
        /// <summary>
        /// Sets the emission power of the inner cube walls.
        /// </summary>
        /// <param name="on">Whether to enable the cube walls' emission.</param>
        private void SetGlowyWalls(bool on)
        {
            if (on)
            {
                insertWalls.SetEmissionPower(1.1f);
            }
            else
            {
                insertWalls.SetEmissionPower(0f);
            }
        }

        /// <summary>
        /// Sets the emission power of the pipes.
        /// </summary>
        /// <param name="on">Whether to enable the pipes.</param>
        private void SetPipes(bool on) 
        {
            if (on)
            {
                pipes.ForEach(p => p.SetEmissionPower(1.1f));
            }
            else
            {
                pipes.ForEach(p => p.SetEmissionPower(0f));
            }
        }
        #endregion
    }
}

