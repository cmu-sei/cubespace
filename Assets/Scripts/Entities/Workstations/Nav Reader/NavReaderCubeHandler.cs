/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Entities.Workstations.CubeStationParts;
using Managers;
using Mirror;
using UI.UIInterfaces;
using UnityEngine;
using UI.NavScreen.NavScreenComponents;

namespace Entities.Workstations.NavReaderParts
{
	/// <summary>
	/// The component of the NavReader used to create and eject the cube.
	/// </summary>
    [RequireComponent(typeof(Animator))]
	public class NavReaderCubeHandler : NetworkBehaviour, IRefreshableUI
	{
        #region Variables
        /// <summary>
        /// The cube object itself.
        /// </summary>
        [Header("Physical GameObjects")]
		[SerializeField]
		private GameObject cube;
		/// <summary>
		/// The button used to eject the cube.
		/// </summary>
		[SerializeField]
		private WorkstationButton cubeButton;
		/// <summary>
		/// The light on the cube button.
		/// </summary>
		[SerializeField]
		private WorkstationLight cubeButtonLight;
		/// <summary>
		/// The light strip that lights up in a runway fashion.
		/// </summary>
		[SerializeField]
		private WorkstationLEDStrip cubeButtonLightStrip;
		/// <summary>
		/// The navigation subscreen.
		/// </summary>
		[SerializeField]
		private CubeDriveDestinationSubScreen cubeSubscreen;
		/// <summary>
		/// The animation clip used to eject the cube.
		/// </summary>
		[Header("Cube Animations")]
		[SerializeField]
		private AnimationClip ejectClip;
		/// <summary>
		/// The particle system that plays when the cube is ready to eject.
		/// </summary>
		[Header("Particle System")]
		[SerializeField]
		private ParticleSystem cubeAvailableParticleSystem;

		/// <summary>
		/// The animator which pushes the cube out of the slot.
		/// </summary>
		private Animator cubeAnimator;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the aimator used to move the cube.
        /// </summary>
        private void Awake() 
		{
			cubeAnimator = GetComponent<Animator>();
		}

		/// <summary>
		/// Unity event function that subscribes to an event on the ShipStateManager firing when the cube state changes.
		/// </summary>
		private void OnEnable()
		{
			ShipStateManager.OnCubeStateChange += OnCubeStateChange;
		}

		/// <summary>
		/// Unity event function that unsubscribes from an event on the ShipStateManager firing when the cube state changes.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnCubeStateChange -= OnCubeStateChange;
		}
        #endregion

        #region Mirror methods
        /// <summary>
        /// Force updates the UI on the NavReader.
        /// </summary>
        public override void OnStartClient()
		{
			base.OnStartClient();
			RefreshDisplay();
		}
        #endregion

        #region Event callback methods
        /// <summary>
        /// Activates cube animation and sets its status text.
        /// </summary>
        /// <param name="cubeState">The new cube state.</param>
        private void OnCubeStateChange(CubeState cubeState)
		{
			SetCubeAvailable(cubeState == CubeState.InNavReader);
			cubeSubscreen.SetCubeStatusText(cubeState);
		}
        #endregion

        #region Main methods
        /// <summary>
        /// Sets the cube to be available or unavailable and activates a corresponding animation.
        /// </summary>
        /// <param name="available">Whether the cube is available.</param>
        public void SetCubeAvailable(bool available)
		{
			cubeAnimator.SetBool("IdleActive", available);
			cubeButton.interactable = available;
			if (available)
			{
				cubeButtonLightStrip.ActivateAnimation();
				cubeAnimator.SetTrigger("Encode");
				cubeButtonLight.Pulsing = true;
				if (cubeAvailableParticleSystem) cubeAvailableParticleSystem.Play();
			}
			else
			{
				cubeButtonLightStrip.DeactivateAnimation();
				cubeButtonLight.Pulsing = false;
				if (cubeAvailableParticleSystem) cubeAvailableParticleSystem.Stop();
			}
		}

		/// <summary>
		/// Manually calls the cube state change method to force the UI to update.
		/// </summary>
		public void RefreshDisplay()
		{
			OnCubeStateChange(ShipStateManager.Instance.CubeState);
		}

		/// <summary>
		/// Sets the animation trigger to eject the cube.
		/// </summary>
		public void EjectCube()
        {
			cubeAnimator.SetTrigger("Eject");
		}
		#endregion
	}
}
