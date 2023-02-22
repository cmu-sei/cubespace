/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Systems.GameBrain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NavScreen
{
	/// <summary>
	/// The NavScreen used to confirm a destination.
	/// </summary>
	public class ConfirmDestinationNavScreen : NavScreen
	{
		/// <summary>
		/// The location to confirm on this screen.
		/// </summary>
		private Location locationToConfirm;
		
		/// <summary>
		/// The text of the location.
		/// </summary>
		[Header("Configuration")]
		[SerializeField]
		private TMP_Text locationText;
		/// <summary>
		/// The button used to confirm the location.
		/// </summary>
		[SerializeField]
		private Button confirmButton;
		/// <summary>
		/// The button used to cancel confirming the location.
		/// </summary>
		[SerializeField]
		private Button cancelButton;

		/// <summary>
		/// The screen that the cancel button returns to.
		/// </summary>
		[Header("Flow")]
		[SerializeField]
		private NavScreen cancelButtonReturnsToScreen;
		/// <summary>
		/// The nav screen used to display the destination.
		/// </summary>
		[SerializeField]
		private DestinationDisplayNavScreen destinationDisplayNavScreen;

		/// <summary>
		/// Unity event function that adds button click actions to different buttons.
		/// </summary>
		private void Awake()
		{
			confirmButton.onClick.AddListener(OnConfirmButtonClick);
			cancelButton.onClick.AddListener(OnCancelButtonClick);
		}

		/// <summary>
		/// Sets the current location on this screen.
		/// </summary>
		/// <param name="location">The location whose information should be displayed here.</param>
		public void SetCurrentLocation(Location location)
		{
			locationToConfirm = location;
			locationText.text = location.name;
		}

		/// <summary>
		/// Shows this screen.
		/// </summary>
		public override void ShowScreen()
		{
			base.ShowScreen();
			if (locationToConfirm == null)
			{
				Debug.LogWarning("ConfirmDestination should have SetCurrentLocation called before it is shown.");
			}
		}

		/// <summary>
		/// Hides this screen.
		/// </summary>
		public override void HideScreen()
		{
			locationToConfirm = null;
			base.HideScreen();
		}

		/// <summary>
		/// Sts the nav screen to the destination display nav screen and calls a command to confirm the location on the server.
		/// </summary>
		void OnConfirmButtonClick()
		{
			_navScreenController._navReaderWorkstation.ConfirmLocation(locationToConfirm);
			
			_navScreenController.SetNavScreen(destinationDisplayNavScreen);
		}

		/// <summary>
		/// Sets the nav screen to what it should be when Cancel is clicked.
		/// </summary>
		void OnCancelButtonClick()
		{
			_navScreenController.SetNavScreen(cancelButtonReturnsToScreen);
		}
	}
}
