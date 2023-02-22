/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using Entities.Workstations;
using Entities.Workstations.PowerRouting;
using Managers;
using UI.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// The subscreen used to render the power status of all workstations needed for launch.
	/// </summary>
	public class PowerDestinationSubScreen : NavScreen, IRefreshableUI
	{
		/// <summary>
		/// The prefab object used to instantiate power status text objects.
		/// </summary>
		[SerializeField]
		private PowerStatusText powerStatusTextPrefab;
		/// <summary>
		/// The list of power status text objects.
		/// </summary>
		private readonly List<PowerStatusText> powerStatuses = new List<PowerStatusText>();
		/// <summary>
		/// The parent object displaying power status.
		/// </summary>
		[SerializeField]
		private Transform powerStatusParent;
		/// <summary>
		/// The workstation manager, used to retrieve the PowerRouting workstation.
		/// </summary>
		[SerializeField]
		private WorkstationManager _workstationManager;
		/// <summary>
		/// The PowerRouting workstation.
		/// </summary>
		private PowerRouting _powerRouting;

		/// <summary>
		/// Unity event function which retrieves the parent object displaying power statuses.
		/// </summary>
		private void Awake()
		{
			if (powerStatusParent == null)
			{
				powerStatusParent = transform;
			}

			if (powerStatusParent.GetComponent<LayoutGroup>() == null)
			{
				Debug.LogWarning("Power Destination Subscreen doesn't have layout group for text prefabs.");
			}
		}

		/// <summary>
		/// Generates child objects to represent launch workstations.
		/// </summary>
		public void GenerateChildren()
		{			
			foreach (var status in powerStatuses)
			{
				if (status != null)
				{
					Destroy(status.gameObject);
				}
			}
			powerStatuses.Clear();

			foreach (var workstation in _workstationManager.GetLaunchWorkstations())
			{
				if (workstation is NavReader)
				{
					continue;
				}
				
				var t = Instantiate(powerStatusTextPrefab, powerStatusParent);
				t.SetWorkstation(workstation);
				powerStatuses.Add(t);
			}
		}

		/// <summary>
		/// Refreshes the display of all powered screens required for launch.
		/// </summary>
		public void RefreshDisplay()
		{
			foreach (var text in powerStatuses)
			{
				text.RefreshDisplay();
			}

			if (_powerRouting != null)
			{
				bool allPoweredForLaunch = _powerRouting.GetAllPoweredForLaunch();
			}
		}

		/// <summary>
		/// Displays this subscreen.
		/// </summary>
		public override void ShowScreen()
		{
			_powerRouting = _workstationManager.GetWorkstation(WorkstationID.PowerRouting) as PowerRouting;
			GenerateChildren();
			
			base.ShowScreen();
			
			RefreshDisplay();
		}
	}
}
