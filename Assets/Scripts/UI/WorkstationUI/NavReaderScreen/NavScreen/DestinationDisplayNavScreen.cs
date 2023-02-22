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
using Entities.Workstations.CubeStationParts;
using Entities.Workstations.PowerRouting;
using Managers;
using UI.NavScreen.NavScreenComponents;
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen
{
	/// <summary>
	/// The screen on the NavReader displaying the destination. This script specifically handles buttons and spinning squares.
	/// </summary>
	public class DestinationDisplayNavScreen : NavScreen
	{
		/// <summary>
		/// The button switching screens to the flight engineer status screen.
		/// </summary>
		[Header("Flight Engineer SubScreen")]
		[SerializeField]
		private DestinationDisplayButton flightEngineerButton;
		/// <summary>
		/// The subscreen showing the stauts of trajectories and thrusters at the Flight Engineer.
		/// </summary>
		[SerializeField]
		private NavScreen flightEngineerStatusSubScreen;
		/// <summary>
		/// The square associated with the Flight Engineer subscreen.
		/// </summary>
		[SerializeField]
		private SpinningUISquare flightSquare;

		/// <summary>
		/// The button switching screens to the workstation power status screen.
		/// </summary>
		[Header("Power SubScreen")]
		[SerializeField]
		private DestinationDisplayButton powerButton;
		/// <summary>
		/// The subscreen showing the power status.
		/// </summary>
		[SerializeField]
		private NavScreen powerStatusSubScreen;
		/// <summary>
		/// The square associated with the power subscreen.
		/// </summary>
		[SerializeField]
		private SpinningUISquare powerSquare;

		/// <summary>
		/// The button switching screens to the cube drive status screen.
		/// </summary>
		[Header("Cube SubScreen")]
		[SerializeField]
		private DestinationDisplayButton cubeDriveButton;
		/// <summary>
		/// The subscreen used for the cube drive workstation.
		/// </summary>
		[SerializeField]
		private NavScreen cubeDriveSubScreen;
		/// <summary>
		/// The square associated with the cube subscreen.
		/// </summary>
		[SerializeField]
		private SpinningUISquare cubeSquare;

		/// <summary>
		/// The button used to display the ship's destination.
		/// </summary>
		[Header("Destination")]
		[SerializeField]
		private DestinationDisplayButton destinationButton;

		/// <summary>
		/// The workstation manager object, used to retrieve the PowerRouting workstation.
		/// </summary>
		[Header("References")]
		[SerializeField]
		private WorkstationManager _workstationManager;

		/// <summary>
		/// The currently selected subscreen.
		/// </summary>
		private NavScreen _currentlySelectedSubScreen;

		/// <summary>
		/// The delay between updating statuses.
		/// </summary>
		private float statusUpdateDelay = 0.2f;
		/// <summary>
		/// The time it takes to update a status display.
		/// </summary>
		private float updateTimer = 1;

		/// <summary>
		/// Unity event function that sets this as the destination display screen for multiple buttons and stops spinning the squares.
		/// </summary>
		private void Awake()
		{
			flightEngineerButton.SetDestinationDisplayNavScreen(this);
			powerButton.SetDestinationDisplayNavScreen(this);
			cubeDriveButton.SetDestinationDisplayNavScreen(this);
			destinationButton.SetDestinationDisplayNavScreen(this);
			flightSquare.SetSpinning(false);
			powerSquare.SetSpinning(false);
			cubeSquare.SetSpinning(false);
		}

		/// <summary>
		/// Unity event fucntion that increases the delay time needed for the status to update.
		/// </summary>
		private void OnEnable()
		{
			updateTimer = statusUpdateDelay + 1;
		}
		
		/// <summary>
		/// Unity event function that updates statuses.
		/// </summary>
		void Update()
		{
			updateTimer += Time.deltaTime;
			if (updateTimer > statusUpdateDelay)
			{
				UpdateAllStatuses();
				(_currentlySelectedSubScreen as IRefreshableUI)?.RefreshDisplay();
				updateTimer = 0;
			}
		}

		/// <summary>
		/// Selects a subscreen using the one provided. Mimics the NavScreen controller.
		/// </summary>
		/// <param name="selectScreen">The selected screen to use as the currnet subscreen.</param>
		public void SelectSubScreen(NavScreen selectScreen)
		{
			if (_currentlySelectedSubScreen == selectScreen)
			{
				((IRefreshableUI)_currentlySelectedSubScreen)?.RefreshDisplay();
			}

			foreach (var screen in new[] { powerStatusSubScreen, cubeDriveSubScreen, flightEngineerStatusSubScreen })
			{
				if (screen == selectScreen)
				{
					screen.gameObject.SetActive(true);
					screen.ShowScreen();
				}
				else
				{
					screen.HideScreen();
					screen.gameObject.SetActive(false);
				}
			}

			_currentlySelectedSubScreen = selectScreen;
			UpdateAllStatuses();
		}

		/// <summary>
		/// Shows this screen and sets a default subscreen.
		/// </summary>
		public override void ShowScreen()
		{
			base.ShowScreen();
			
			NavScreen defaultScreen = _currentlySelectedSubScreen;
			if (defaultScreen == null)
			{
				defaultScreen = flightEngineerStatusSubScreen;
			}
			SelectSubScreen(defaultScreen);
		}

		/// <summary>
		/// Hides the currently selected subscreen.
		/// </summary>
		public override void HideScreen()
		{
			if (_currentlySelectedSubScreen != null)
			{
				_currentlySelectedSubScreen.HideScreen();
			}

			base.HideScreen();
		}
		
		/// <summary>
		/// Updates all status screens.
		/// </summary>
		private void UpdateAllStatuses()
		{
			UpdateFlightEngineerStatus();
			UpdatePowerStatus();
			UpdateCubeDriveStatus();
			UpdateDestinationStatus();
		}

		/// <summary>
		/// Updates the power status of workstations needed to launch.
		/// </summary>
		private void UpdatePowerStatus()
		{
			bool powerComplete = ((PowerRouting) _workstationManager.GetWorkstation(WorkstationID.PowerRouting)).GetAllPoweredForLaunch();
			powerButton.SetIsComplete(powerComplete);
			powerButton.SetIsSelected(_currentlySelectedSubScreen == powerStatusSubScreen);
			powerSquare.SetSpinning(!powerComplete);
		}

		/// <summary>
		/// Updates the Flight Engineer status.
		/// </summary>
		private void UpdateFlightEngineerStatus()
		{
			bool flightComplete = ShipStateManager.Instance.GetAllThrustersOn() && ShipStateManager.Instance.TrajectoriesLocked;
			flightEngineerButton.SetIsComplete(flightComplete);
			flightEngineerButton.SetIsSelected(_currentlySelectedSubScreen == flightEngineerStatusSubScreen);
			flightSquare.SetSpinning(!flightComplete);
		}

		/// <summary>
		/// Updates the status of the cube drive.
		/// </summary>
		private void UpdateCubeDriveStatus()
		{
			bool cubeComplete = ShipStateManager.Instance.CubeState == CubeState.InCubeDrive;
			cubeDriveButton.SetIsComplete(cubeComplete);
			cubeDriveButton.SetIsSelected(_currentlySelectedSubScreen == cubeDriveSubScreen);
			cubeSquare.SetSpinning(!cubeComplete);
		}

		/// <summary>
		/// Updates attributes on the destination button relative to the current location index.
		/// </summary>
		private void UpdateDestinationStatus()
		{
			destinationButton.SetIsComplete(ShipStateManager.Instance.LocationSet);
			destinationButton.SetInteractable(false);

            destinationButton.SetButtonText($"Destination\n{ShipStateManager.Instance.unlockedLocations[ShipStateManager.Instance.CurrentSetLocationIndex].name}");
		}

		/// <summary>
		/// Selects the specific subscreen that matches the destination display button provided.
		/// </summary>
		/// <param name="destinationDisplayButton"></param>
		public void ButtonSelected(DestinationDisplayButton destinationDisplayButton)
		{
			if (destinationDisplayButton == null)
			{
				Debug.LogWarning("Going to blank subscreen of destinationdisplay. Possibly unintended behaviour.");
				SelectSubScreen(null);
			} 
			else if (destinationDisplayButton == powerButton)
			{
				SelectSubScreen(powerStatusSubScreen);
			} 
			else if (destinationDisplayButton == flightEngineerButton)
			{
				SelectSubScreen(flightEngineerStatusSubScreen);
			} 
			else if (destinationDisplayButton == cubeDriveButton)
			{
				SelectSubScreen(cubeDriveSubScreen);
			} 
			else if (destinationDisplayButton == destinationButton)
			{
				// Do nothing
			}
			else
			{
				Debug.LogWarning("Can't handle button press in DestinationDisplayNavScreen.", destinationDisplayButton);
			}
		}
	}
}
