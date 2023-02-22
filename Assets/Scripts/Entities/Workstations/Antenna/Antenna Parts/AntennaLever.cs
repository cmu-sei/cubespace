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
using UnityEngine.Serialization;using Managers;
namespace Entities.Workstations.AntennaParts
{
	/// <summary>
	/// A component for a lever used on the antenna to connect and disconnect from a remote network.
	/// </summary>
	public class AntennaLever : WorkstationLever
	{
        #region Variables
        /// <summary>
        /// The antenna this lever is connected to.
        /// </summary>
        [FormerlySerializedAs("workstation")]
		[SerializeField]
		private Antenna antenna;
		/// <summary>
		/// The time it takes to fully reset the lever's position.
		/// </summary>
		[SerializeField]
		private float timeToResetRotation = 1.0f;
		/// <summary>
		/// A number offsetting the active rotation when trying to instantly reset the lever position.
		/// </summary>
		[SerializeField]
		private float rotationOffset = 57.687f;
		#endregion

		#region Unity event functions
		/// <summary>
		/// Subscribes to event actions on the antenna.
		/// </summary>
		private void OnEnable()
		{
			antenna.OnPowerOff += OnPowerOff;
			antenna.OnEnter += OnEnter;
			antenna.OnResetState += ResetState;
		}

		/// <summary>
		/// Unsubscribes from event actions on the antenna.
		/// </summary>
		private void OnDisable()
		{
			antenna.OnPowerOff -= OnPowerOff;
			antenna.OnEnter -= OnEnter;
			antenna.OnResetState -= ResetState;
		}

		/// <summary>
		/// Plays an error sound on mouse down if first contact is complete and the antenna is powered.
		/// </summary>
		protected void OnMouseDown()
		{
			if (!ShipStateManager.Instance.FirstContactEstablished && antenna.IsPowered)
			{
				Audio.AudioPlayer.Instance.UIError(transform);
			}
		}
		/// <summary>
		/// Updates the position of the lever on mouse drag if first contact is complete and the antenna is powered.
		/// </summary>
		protected override void OnMouseDrag()
		{
			if (ShipStateManager.Instance.FirstContactEstablished && antenna.IsPowered)
			{
				base.OnMouseDrag();
			}
		}
		#endregion

		#region Action methods
		/// <summary>
		/// Resets the position of the lever when the antenna is powered off.
		/// </summary>
		protected void OnPowerOff()
		{
			StartCoroutine(ResetLeverRotation());
		}

		/// <summary>
		/// Resets the position of the lever when theantenna is entered.
		/// </summary>
		protected void OnEnter()
		{
			// If the antenna is connected or trying to connect and powered, set the lever's rotation to be a specific offset off from the current rotation
			if ((antenna.ConnectionState == AntennaState.Connected || antenna.ConnectionState == AntennaState.Connecting) && antenna.IsPowered)
			{
				transform.localRotation = Quaternion.Euler(activeRot - rotationOffset, 0, 0);
			}
			// Otherwise, just set the rotation to be the lever's original rotation
			else
			{
				transform.localRotation = originalRot;
			}
		}
		#endregion

		#region Lever methods

		/// <summary>
		/// Plays a lock sound and sets the antenna to be connected when the lever is moved into an active position.
		/// </summary>
		protected override void OnActivate()
		{
			base.OnActivate();
			Audio.AudioPlayer.Instance.AntennaLeverLock(transform);
			antenna.TrySetAntenna(AntennaState.Connected);
		}
		/// <summary>
		/// Plays a lock sound and sets the antenna to be disconnected when the lever is moved into an inactive position.
		/// </summary>
		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			Audio.AudioPlayer.Instance.AntennaLeverLock(transform);
			antenna.TrySetAntenna(AntennaState.Disconnected);
		}
        #endregion
        #region Coroutines
        /// <summary>
        /// Resets the rotation of the antenna lever back to its starting position.
        /// </summary>
        /// <returns>A yield while waiting for the lever to rotate back to its starting rotation.</returns>
        IEnumerator ResetLeverRotation()
		{
			// Get the initial rotation of the lever
			Quaternion startRot = transform.localRotation;
			float timeElapsed = 0;

			// Smoothly rotate the lever
			while (timeElapsed < timeToResetRotation)
			{
				transform.localRotation = Quaternion.Slerp(startRot, originalRot, timeElapsed / timeToResetRotation);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			// Reset the rotation back to the starting rotation of the lever
			transform.localRotation = originalRot;
		}
        #endregion
    }
}
