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
using UnityEngine;
using Entities.Workstations;

namespace UI.NavScreen
{
	/// <summary>
	/// A component that dictates the behavior of the screen on the NavReader.
	/// </summary>
	public class NavScreenController : MonoBehaviour
	{
        #region Variables
		/// <summary>
		/// All screens present within the NavReader.
		/// </summary>
        [SerializeField]
		private NavScreen[] _allScreens;
		/// <summary>
		/// The NavReader screen used for location selection.
		/// </summary>
		[SerializeField]
		private LocationSelectionNavScreen locationSelectionNavScreen;
		/// <summary>
		/// The NavReader screen used for displaying the ship's destination.
		/// </summary>
		[SerializeField]
		private DestinationDisplayNavScreen destinationDisplayNavScreen;
		/// <summary>
		/// A public reference to the NavReader workstation.
		/// </summary>
		public NavReader _navReaderWorkstation;
		/// <summary>
		/// The current screen displayed on the NavReader.
		/// </summary>
		private NavScreen _currentNavScreen;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets all NavScreen components and initializes the controllers
		/// of the NavReader screens.
        /// </summary>
        private void Awake()
		{
			_allScreens = GetComponentsInChildren<NavScreen>();
			InitializeNavScreens();
		}

		/// <summary>
		/// Unity event function that subscribes to ShipStateManager location actions.
		/// </summary>
		private void OnEnable()
		{
			ShipStateManager.OnSetLocationChange += OnSetLocationChange;
			ShipStateManager.UnlockedLocationsChangedHook += UnlockedLocationsChangedHook;
		}

		/// <summary>
		/// Unity event function that unsubscribes from ShipStateManager location actions.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnSetLocationChange -= OnSetLocationChange;
			ShipStateManager.UnlockedLocationsChangedHook -= UnlockedLocationsChangedHook;
		}
		#endregion

		#region Event callbacks
		/// <summary>
		/// Sets the NavReader screen when the state of setting the location changes.
		/// </summary>
		/// <param name="locationSet">Whether the location was set or not.</param>
		private void OnSetLocationChange(bool locationSet)
		{
			if (locationSet)
			{
				SetNavScreen(destinationDisplayNavScreen);
			}
			else
			{
				SetNavScreen(locationSelectionNavScreen);
			}
		}

		/// <summary>
		/// Force-refreshes the location selection NavReader screen when the list of unlocked locations changes.
		/// </summary>
		/// <param name="obj">The list of locations. Unused, but necessary.</param>
		private void UnlockedLocationsChangedHook(Location[] obj)
		{
			//Force a refresh of the location selection view when we get new locations from the server, if its open.
			if (_currentNavScreen == locationSelectionNavScreen)
			{
				locationSelectionNavScreen.ForceRefresh();
			}
		}
        #endregion

        #region NavScreen methods
        /// <summary>
        /// Resets all NavReader screens.
        /// </summary>
        public void ResetNavScreens()
		{
			foreach (var screen in _allScreens)
			{
				screen.ResetNavScreen();
			}
		}

		/// <summary>
		/// Displays the active NavReader screen.
		/// </summary>
		public void RefreshNavScreen()
		{
			_currentNavScreen.ShowScreen();
		}

		/// <summary>
		/// Sets the active NavReader screen. Null can be provided to clear the currently stored screen.
		/// </summary>
		/// <param name="screen">The new NavScreen to switch to.</param>
		public void SetNavScreen(NavScreen screen)
		{
			if (_currentNavScreen != null)
			{
				_currentNavScreen.HideScreen();
			}

			_currentNavScreen = screen;
			if (_currentNavScreen != null)
			{
				_currentNavScreen.ShowScreen();
			}
		}

		/// <summary>
		/// Initializes the controller for all NavScreens to be this object.
		/// </summary>
		public void InitializeNavScreens()
		{
			foreach (var ns in _allScreens)
			{
				ns.SetController(this);
			}
		}

		/// <summary>
		/// Resets the NavScreen displayed to the one showing the current location.
		/// </summary>
		public void SetToDefaultScreen()
		{
			locationSelectionNavScreen.ChangeLocation(ShipStateManager.Instance.GetCurrentLocation());
			SetNavScreen(locationSelectionNavScreen);
		}

		/// <summary>
		/// Checks whether the given NavScreen is the current NavScreen.
		/// </summary>
		/// <param name="screen">The NavScreen to check against.</param>
		/// <returns>Whether the current NavScreen is the NavScreen provided.</returns>
		public bool IsCurrentNavScreen(NavScreen screen)
		{
			return _currentNavScreen == screen;
		}
		#endregion
	}
}
