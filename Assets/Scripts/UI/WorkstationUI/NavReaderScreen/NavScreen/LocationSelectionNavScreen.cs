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
using Systems.GameBrain;
using TMPro;
using UI.NavScreen.LocationSelectionNavScreenComponents;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace UI.NavScreen
{
	/// <summary>
	/// The location selection navigation screen.
	/// </summary>
	public class LocationSelectionNavScreen : NavScreen
	{
		/// <summary>
		/// The button used to go back a location page.
		/// </summary>
		[Header("Buttons")]
		[SerializeField]
		private Button leftButton;
		/// <summary>
		/// The button used to go forward a location page.
		/// </summary>
		[SerializeField]
		private Button rightButton;
		/// <summary>
		/// The button used to select a location.
		/// </summary>
		[SerializeField]
		private SelectLocationButton selectLocationButton;
		/// <summary>
		/// The button used to go to the "Enter Coordinates" screen.
		/// </summary>
		[SerializeField]
		private Button enterCoordinatesButton;
		/// <summary>
		/// The image of the location shown in the NavReader.
		/// </summary>
		[Header("Visuals")]
		[SerializeField]
		private Image locationImage;
		/// <summary>
		/// The name of the location.
		/// </summary>
		[SerializeField]
		private TMP_Text locationName;
		/// <summary>
		/// The screen used to confirm the location selected.
		/// </summary>
		[Header("Flow")]
		[SerializeField]
		private ConfirmDestinationNavScreen confirmLocationScreen;
		/// <summary>
		/// The nav screen where a player can enter coordinates.
		/// </summary>
		[SerializeField]
		private NavScreen enterCoordinatesNavScreen;
		/// <summary>
		/// The dictionary mapping IDs to images.
		/// </summary>
		[FormerlySerializedAs("_locationImageMap")]
		[SerializeField]
		private IDToImageMap imageMapMap;

		/// <summary>
		/// An enum representing the change in direction the player has made on the selection screen.
		/// </summary>
		public enum DirectionDelta
		{
			Left = -1,
			Right = 1
		}

		/// <summary>
		/// The index of the location currently displayed.
		/// </summary>
		private int visibleLocationIndex;

		/// <summary>
		/// Unity event function that adds listeners to buttons.
		/// </summary>
		private void Awake()
		{
			leftButton.onClick.AddListener(OnLeftClick);
			rightButton.onClick.AddListener(OnRightClick);
			enterCoordinatesButton.onClick.AddListener(OnEnterCoordinatesClick);
			selectLocationButton.onClick.AddListener(OnSelectLocationClick);
		}

		/// <summary>
		/// Shows the location selected.
		/// </summary>
		/// <param name="location">The location to use.</param>
		private void ShowLocation(Location location)
		{
			if (location == null || location.locationID == null)
			{
				locationName.text = "Loading...";
				selectLocationButton.SetAtCurrentLocation(true);
				return;
			}
			bool showingCurrent;

			if (ShipStateManager.Instance.GetCurrentLocation() != null) 
			{
			 	showingCurrent = location.locationID == ShipStateManager.Instance.GetCurrentLocation().locationID;
			} 
			else 
			{
				showingCurrent = false;
			}
			
			locationName.text = $"{location.name}";
			locationImage.sprite = imageMapMap.GetImage(location.locationID, true);
			selectLocationButton.SetAtCurrentLocation(showingCurrent);
		}

		/// <summary>
		/// An overload function for being told which location to use directly.
		/// </summary>
		/// <param name="location">The location to switch to.</param>
		public void ChangeLocation(Location location)
		{
			visibleLocationIndex = ShipStateManager.Instance.GetLocationIndex(location);
			if (visibleLocationIndex == -1)
			{
				visibleLocationIndex = 0;
			}

			confirmLocationScreen.SetCurrentLocation(location);
			ShowLocation(location);
		}

		/// <summary>
		/// Changes the location shown on the NavReader.
		/// </summary>
		/// <param name="locationID">The ID of the location to change to.</param>
		public void ChangeLocation(string locationID)
		{
			Location loc = ShipStateManager.Instance.GetLocation(locationID);
			ChangeLocation(loc);
		}

		/// <summary>
		/// Changes to a different location screen.
		/// </summary>
		/// <param name="direction">The direction the player clicked in.</param>
		public void ChangeLocation(DirectionDelta direction)
		{
			int indexChange = (int) direction;
			
			visibleLocationIndex += indexChange;

			// Wraparound
			if (visibleLocationIndex < 0)
			{
				visibleLocationIndex = ShipStateManager.Instance.unlockedLocations.Count - 1;
			}
			else if (visibleLocationIndex >= ShipStateManager.Instance.unlockedLocations.Count)
			{
				visibleLocationIndex = 0;
			}

			Location location = GetCurrentVisibleLocation();

			if (location != null)
			{
				confirmLocationScreen.SetCurrentLocation(location);
				ShowLocation(location);
			}
		}

		/// <summary>
		/// Gets the currently visible location.
		/// </summary>
		/// <returns>The location at the visible location index in the list of unlocked locations.</returns>
		private Location GetCurrentVisibleLocation()
		{
			if (ShipStateManager.Instance.unlockedLocations.Count == 0)
			{
				Debug.Log("0 locations. Nav screen standing by for location data.");
				return null;
			}

			if (visibleLocationIndex < 0)
			{
				visibleLocationIndex = 0;
			}
			else if (visibleLocationIndex >= ShipStateManager.Instance.unlockedLocations.Count)
			{
				visibleLocationIndex = ShipStateManager.Instance.unlockedLocations.Count-1;
			}
			
			return ShipStateManager.Instance.unlockedLocations[visibleLocationIndex];
		}

		/// <summary>
		/// Shows the currently visible location.
		/// </summary>
		public override void ShowScreen()
		{
			Location location = GetCurrentVisibleLocation();
			if (location != null)
			{
				ShowLocation(location);
				confirmLocationScreen.SetCurrentLocation(location);
			}

			base.ShowScreen();
		}

		/// <summary>
		/// Sets the navigation screen to the screen shown when confirming a selection.
		/// </summary>
		private void OnSelectLocationClick()
		{
			_navScreenController.SetNavScreen(confirmLocationScreen);
		}

		/// <summary>
		/// Moves to the previous location in the list.
		/// </summary>
		private void OnLeftClick()
		{
			ChangeLocation(DirectionDelta.Left);
		}

		/// <summary>
		/// Moves to the next location in the list.
		/// </summary>
		private void OnRightClick()
		{
			ChangeLocation(DirectionDelta.Right);
		}

		/// <summary>
		/// Switches to the enter coordinates nav screen on click.
		/// </summary>
		private void OnEnterCoordinatesClick()
		{
			_navScreenController.SetNavScreen(enterCoordinatesNavScreen);
		}

		/// <summary>
		/// Forcefully hides and shows this screen to refresh the display.
		/// </summary>
		public virtual void ForceRefresh()
		{
			HideScreen();
			ShowScreen();
		}
	}
}
