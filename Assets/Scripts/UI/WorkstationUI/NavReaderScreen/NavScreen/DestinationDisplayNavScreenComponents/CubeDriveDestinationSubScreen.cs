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
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// The sub screen used to show the cube drive.
	/// </summary>
	public class CubeDriveDestinationSubScreen : NavScreen, IRefreshableUI
	{
		/// <summary>
		/// The text displaying the cube's current status.
		/// </summary>
		[SerializeField]
		private CubeStatusText _cubeStatusText;

		/// <summary>
		/// Refreshes the cube status text to reflect the cube's current state.
		/// </summary>
		public void RefreshDisplay()
		{
			_cubeStatusText.SetText(ShipStateManager.Instance.CubeState);
		}

		/// <summary>
		/// Sets the cube status text object to reflect the state of the cube.
		/// </summary>
		/// <param name="state">The state of the cube.</param>
		public void SetCubeStatusText(CubeState state)
        {
			_cubeStatusText.SetText(state);
        }
	}
}

        
