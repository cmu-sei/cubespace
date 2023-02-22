/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;
using UnityEngine.UI;
using UI.ColorPalettes;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// A class defining a square that spins on the NavReader to show status.
	/// </summary>
	public class SpinningUISquare : MonoBehaviour
	{
		/// <summary>
		/// The full rotation speed of the square.
		/// </summary>
		public float RotationSpeed => clockwise ? rotationSpeed : -rotationSpeed;
		/// <summary>
		/// The speed of the square's rotation.
		/// </summary>
		[SerializeField, Min(0)]
		private float rotationSpeed = 8;
		/// <summary>
		/// The speed at which the square should snap to a point.
		/// </summary>
		[SerializeField, Min(0)]
		private float snapSpeed = 80f;
		/// <summary>
		/// Whether the square should spin clockwise or counter-clockwise.
		/// </summary>
		[SerializeField]
		private bool clockwise = true;
		/// <summary>
		/// Whether the square is spinning.
		/// </summary>
		private bool spinning = false;

		/// <summary>
		/// The image components of the square to color.
		/// </summary>
		[SerializeField]
		private Image[] imagesToColor;
		
		/// <summary>
		/// Sets whether the square is spinning and the color of the square.
		/// </summary>
		/// <param name="spin">Whether to spin the square.</param>
		public void SetSpinning(bool spin)
		{
			spinning = spin;
			
			Color newSquareColor = spin ? ColorPalette.activeColorPalette.NavSquareBaseColor : ColorPalette.activeColorPalette.NavItemCompleteColor;
			foreach (Image i in imagesToColor)
            {
				i.color = newSquareColor;
            }
		}

		/// <summary>
		/// Unity event function that rotates the square or snaps it.
		/// </summary>
		private void Update()
		{
			if (spinning)
			{
				RotateTick();
			}
			else
			{
				SnapTick();
			}
		}

		/// <summary>
		/// Rotates the square.
		/// </summary>
		private void RotateTick()
		{
			transform.Rotate(0.0f, 0.0f, RotationSpeed * Time.deltaTime);
		}
		
		/// <summary>
		/// Snaps the square to a position instead of rotating.
		/// </summary>
		private void SnapTick()
		{
			Vector3 rotationVector = transform.localEulerAngles;
			if (transform.localEulerAngles != Vector3.zero)
			{
				// This lerp is clamped, so the above if shouldn't runwaway
				rotationVector = new Vector3(0.0f, 0.0f, Mathf.LerpAngle(rotationVector.z, 0.0f, Time.deltaTime * snapSpeed));
				transform.localEulerAngles = rotationVector;
			}
		}
	}
}
