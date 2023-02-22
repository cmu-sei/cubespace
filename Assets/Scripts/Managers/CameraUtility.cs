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

namespace Managers
{
	/// <summary>
	/// Performs bit manipulations on the main camera culling mask to make differnt layer masks visible.
	/// </summary>
	public class CameraUtility
	{
		/// <summary>
		/// Changes whether a LayerMask is rendered by the culling mask of the main camera.
		/// </summary>
		/// <param name="mask">The LayerMask to manipulate the main camera's culling mask with.</param>
		/// <param name="visible">Whether the given LayerMask should be visible.</param>
		public static void SetLayerMaskVisible(LayerMask mask, bool visible)
		{
			// Get the main camera and its culling mask
			Camera cam = Camera.main;
			int visibleMask = cam.cullingMask;

			// If this object should be visible, the culling mask should be the current mask or the given mask
			if (visible)
			{
				cam.cullingMask = visibleMask | mask;
			}
			// Otherwise, the culling mask should be the current mask and the opposite of the given mask
			else
			{
				cam.cullingMask = visibleMask & ~mask;
			}
		}
	}
}
