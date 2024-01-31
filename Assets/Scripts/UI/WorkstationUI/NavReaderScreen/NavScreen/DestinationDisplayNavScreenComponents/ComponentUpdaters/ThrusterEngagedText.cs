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
using TMPro;
using UI.ColorPalettes;
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// The text displaying that whether a thruster is engaged.
	/// </summary>
	public class ThrusterEngagedText : MonoBehaviour, IRefreshableUI
	{
		/// <summary>
		/// The index of the thruster whose text should be set.
		/// </summary>
		[SerializeField]
		public int thrusterIndex;
		/// <summary>
		/// Whether the thruster is engaged; 
		/// </summary>
		private bool thrusterEngaged;
		/// <summary>
		/// The total character width allowed for the dots.
		/// </summary>
		[SerializeField]
		private int totalCharWidth = 20;
		
		/// <summary>
		/// The text used for the labels.
		/// </summary>
		[SerializeField]
		private TMP_Text labelText;
		/// <summary>
		/// The text used to space between objects.
		/// </summary>
		[SerializeField]
		private TMP_Text spacingText;
		/// <summary>
		/// The text object representing the status of the thruster.
		/// </summary>
		[SerializeField]
		private TMP_Text statusText;

		/// <summary>
		/// Sets the thruster text.
		/// </summary>
		/// <param name="index">The indexed label of the thruster.</param>
		public void SetThruster(int index)
		{
			thrusterIndex = index;
			labelText.text = "THRUSTER " + "ABCD"[index];
			RefreshDisplay();
		}

		/// <summary>
		/// Refreshes the display of the text.
		/// </summary>
		public void RefreshDisplay()
		{
			bool on = ShipStateManager.Instance.GetThrusterOn(thrusterIndex);
			thrusterEngaged = on; // This exists in case we need to update in onEnable or for debugging
			statusText.text = on ? "ENGAGED" : "DISENGAGED";
			
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
