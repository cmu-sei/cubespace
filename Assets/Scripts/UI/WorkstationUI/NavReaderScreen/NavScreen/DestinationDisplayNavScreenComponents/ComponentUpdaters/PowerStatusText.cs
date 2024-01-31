/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Entities.Workstations;
using TMPro;
using UI.ColorPalettes;
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// Sets the text representing the power status of the assigned workstation.
	/// </summary>
	public class PowerStatusText : MonoBehaviour, IRefreshableUI
	{
		/// <summary>
		/// Whether the workstation is ready for launch based on its power state.
		/// </summary>
		private bool readyForLaunch;
		/// <summary>
		/// The workstation whose power should be displayed.
		/// </summary>
		private Workstation workstation;
		/// <summary>
		/// The total width of the spacing text.
		/// </summary>
		[SerializeField]
		private int totalCharWidth = 20;
		/// <summary>
		/// The text used to label the workstation.
		/// </summary>
		[SerializeField]
		private TMP_Text labelText;
		/// <summary>
		/// The text spaced between the label and the status.
		/// </summary>
		[SerializeField]
		private TMP_Text spacingText;
		/// <summary>
		/// The text showing the status of the workstation's power.
		/// </summary>
		[SerializeField]
		private TMP_Text statusText;

		/// <summary>
		/// Sets the workstation.
		/// </summary>
		/// <param name="statusWorkstation">The workstation to get the status of.</param>
		public void SetWorkstation(Workstation statusWorkstation)
		{
			workstation = statusWorkstation;
			RefreshDisplay();
		}

		/// <summary>
		/// Refreshes the display on the power status text of the workstation.
		/// </summary>
		public void RefreshDisplay()
		{
			bool on = workstation.IsPowered;
			readyForLaunch = on;
			statusText.text = on ? "ON" : "OFF";
			labelText.text = Workstation.GetPrettyName(workstation.StationID);
			int dotsNeeded = totalCharWidth - labelText.text.Length - statusText.text.Length;
			
			spacingText.text = "";
			for (int i = 0; i < dotsNeeded; i++)
			{
				spacingText.text += ".";
			}
			
			labelText.color = ColorPalette.GetNavReaderDestinationScreenTextColor(on);
			spacingText.color = ColorPalette.GetNavReaderDestinationScreenTextColor(on);
			statusText.color = ColorPalette.GetNavReaderDestinationScreenTextColor(on);
		}
	}
}
