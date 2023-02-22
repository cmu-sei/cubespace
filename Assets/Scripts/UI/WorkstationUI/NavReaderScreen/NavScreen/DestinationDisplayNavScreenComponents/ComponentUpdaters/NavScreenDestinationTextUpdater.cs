/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using TMPro;
using UI.ColorPalettes;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// Class that changes the text and color of multiple TMP texts on the cube drive encoder.
	/// </summary>
	public class NavScreenDestinationTextUpdater : MonoBehaviour
	{
		/// <summary>
		/// The status text.
		/// </summary>
		[SerializeField]
		private TMP_Text statusText;
		/// <summary>
		/// The label of the destination text on a NavReader screen.
		/// </summary>
		[SerializeField]
		private TMP_Text labelText;
		/// <summary>
		/// The text objects whose color should be updated.
		/// </summary>
		[SerializeField]
		private TMP_Text[] textsToUpdateColor;
		/// <summary>
		/// The color palette used.
		/// </summary>
		[SerializeField]
		private ColorPalette palette;

		/// <summary>
		/// Updates the text on the NavReader encoder.
		/// </summary>
		/// <param name="ready">Whether this text object is ready.</param>
		/// <param name="label">The label used on the text.</param>
		/// <param name="status">The status of the destination.</param>
		public void UpdateText(bool ready, string label, string status)
		{
			statusText.text = status;
			labelText.text = label;
			Color c = palette.GetNavReaderDestinationScreenTextColor(ready);
			
			statusText.color = c;
			labelText.color = c;
			foreach (var tmp in textsToUpdateColor)
			{
				tmp.color = c;
			}
		}
	}
}
