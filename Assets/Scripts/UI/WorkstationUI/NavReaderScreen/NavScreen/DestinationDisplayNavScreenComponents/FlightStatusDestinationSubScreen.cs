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
using Managers;
using TMPro;
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// A screen shown on the NavReader to set the flight status destination.
	/// </summary>
	public class FlightStatusDestinationSubScreen : NavScreen, IRefreshableUI
	{
		/// <summary>
		/// An enum representation of the trajectory and thruster screens.
		/// </summary>
		public enum StatusScreen
		{
			Trajectory,
			Thrusters
		}

		/// <summary>
		/// The page showing trajectories.
		/// </summary>
		[Header("Pages")]
		[SerializeField]
		private NavScreen trajectoryPage;
		/// <summary>
		/// The page showing thrusters.
		/// </summary>
		[SerializeField]
		private NavScreen thrusterPage;
		/// <summary>
		/// The text object shown to cycle pages.
		/// </summary>
		[SerializeField]
		private TMP_Text paginationText;

		/// <summary>
		/// The dial text objects.
		/// </summary>
		[Header("Dials for Trajectory Subscreen")]
		[SerializeField]
		private NavReaderDialText[] dialTexts;
		/// <summary>
		/// The texts to display when the thrusters are engaged.
		/// </summary>
		[Header("Thrusters")]
		[SerializeField]
		private ThrusterEngagedText[] thrusterEngagedTexts;
		/// <summary>
		/// The WorkstationManager object.
		/// </summary>
		[SerializeField]
		private WorkstationManager _workstationManager;

		/// <summary>
		/// The FlightEngineer workstation.
		/// </summary>
		private FlightEngineer _flightEngineer;
		/// <summary>
		/// The screen currently displayed (thrusters or trajectories).
		/// </summary>
		private StatusScreen currentStatusScreen = StatusScreen.Trajectory;

		/// <summary>
		/// Refreshes the display to show trajectory info or thruster info based on the current status screen.
		/// </summary>
		public void RefreshDisplay()
		{
			Debug.Log("Refreshing display [FlightStatusDestinationSubScreen.cs:82]");
			if (currentStatusScreen == null || paginationText == null || trajectoryPage == null || thrusterPage == null)
			{
				Debug.LogError("Null component in screen [FlightStatusDestinationSubScreen.cs:85]");
			}
			if (currentStatusScreen == StatusScreen.Trajectory)
			{
				paginationText.text = "NEXT";
				trajectoryPage.ShowScreen();
				thrusterPage.HideScreen();
				foreach(var dt in dialTexts)
				{
					dt.RefreshDisplay();
				}
			}
			else if (currentStatusScreen == StatusScreen.Thrusters)
			{
				paginationText.text = "PREVIOUS";
				trajectoryPage.HideScreen();
				thrusterPage.ShowScreen();

				// Refresh thruster text, even if not needed
				foreach (var tet in thrusterEngagedTexts)
				{
					tet.RefreshDisplay();
				}
			}
		}

		/// <summary>
		/// Switches the screen between the current status screen and the screen not shown.
		/// </summary>
		public void SwitchScreen()
		{
			if (currentStatusScreen == StatusScreen.Thrusters)
			{
				currentStatusScreen = StatusScreen.Trajectory;
			}
			else if (currentStatusScreen == StatusScreen.Trajectory)
			{
				currentStatusScreen = StatusScreen.Thrusters;
			}
			RefreshDisplay();
		}
		
		/// <summary>
		/// Unity event function that subscribes to the trajectory lock update event.
		/// </summary>
		private void OnEnable()
		{
			ShipStateManager.OnTrajectoryLockUpdate += OnTrajectoryLockUpdated;
		}

		/// <summary>
		/// Unity event function that unsubscribes from the trajectory lock update event.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnTrajectoryLockUpdate -= OnTrajectoryLockUpdated;
		}

		/// <summary>
		/// Refreshes the display when the trajectory lock value is set.
		/// </summary>
		/// <param name="locked">Whether the trajectory lock has been updated.</param>
		private void OnTrajectoryLockUpdated(bool locked)
		{
			Debug.Log("Refreshing flight status display, Invoked OnTrajectoryLockUpdate [FlightStatusDestinationSubScreen.cs:147]");
			RefreshDisplay();
		}

		/// <summary>
		/// Sets the current status screen to display from the dials that are locked.
		/// </summary>
		/// <param name="locked">Whether or not the dials are locked.</param>
		private void SetPageFromDialsLocked(bool locked)
		{
			currentStatusScreen = locked ? StatusScreen.Thrusters : StatusScreen.Trajectory;
		}

		/// <summary>
		/// Shows this screen.
		/// </summary>
		public override void ShowScreen()
		{
			GetFlightEngineer();
			
			base.ShowScreen();
			RefreshDisplay();
		}

		/// <summary>
		/// Gets the FlightEngineer workstation object.
		/// </summary>
		private void GetFlightEngineer()
		{
			if (_flightEngineer == null)
			{
				_flightEngineer = _workstationManager.GetWorkstation(WorkstationID.FlightEngineer) as FlightEngineer;
			}
		}
	}
}
