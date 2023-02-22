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
using UnityEngine.UI;

namespace UI.NavScreen.LocationSelectionNavScreenComponents
{
	/// <summary>
	/// The button used to select a location.
	/// </summary>
	public class SelectLocationButton : Button
	{
		/// <summary>
		/// The label used on the button for the current location.
		/// </summary>
		private string atCurrentLabel = "Current Location";
		/// <summary>
		/// The label used on the button to select the location.
		/// </summary>
		private string selectLocation = "Select This Location";
		/// <summary>
		/// The label of the button.
		/// </summary>
		private TMP_Text buttonLabel;

		/// <summary>
		/// Unity event function that gets the button label component of this object.
		/// </summary>
		protected override void Awake()
		{
			buttonLabel = GetComponentInChildren<TMP_Text>();
		}

		/// <summary>
		/// Sets the text of the button regarding whether the ship is at the location targeted on the NavReader.
		/// </summary>
		/// <param name="atCurrent">Whether the ship is at the current location.</param>
		public void SetAtCurrentLocation(bool atCurrent)
		{
			buttonLabel.text = atCurrent ? atCurrentLabel : selectLocation;
			interactable = !atCurrent;
		}
	}
}
