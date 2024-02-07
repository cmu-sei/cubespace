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
using Entities.Workstations.FlightEngineerParts;
using Managers;
using TMPro;
using UI.ColorPalettes;
using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// The text on the NavReader used to display information about the dial.
	/// </summary>
	public class NavReaderDialText : MonoBehaviour, IRefreshableUI
	{
		/// <summary>
		/// The ID of the dial used for this dial text.
		/// </summary>
		[Header("Configuration")]
		[SerializeField]
		public DialID dialID;
		/// <summary>
		/// The total character width of the dots.
		/// </summary>
		[SerializeField]
		private int totalCharWidth = 22;

		/// <summary>
		/// The label denoting a specific dial.
		/// </summary>
		[Header("Text Components")]
		[SerializeField]
		private TMP_Text labelText;
		/// <summary>
		/// The space between the label and the status.
		/// </summary>
		[SerializeField]
		private TMP_Text spacingText;
		/// <summary>
		/// The angle text of the dial.
		/// </summary>
		[SerializeField]
		private TMP_Text statusText;

		/// <summary>
		/// The workstation manager, used to retrieve the Flight Engineer.
		/// </summary>
		[Header("Setup")]
		[SerializeField]
		private WorkstationManager _workstationManager;
		
		/// <summary>
		/// The FlightEngineer workstation.
		/// </summary>
		private FlightEngineer _flightEngineer;

		/// <summary>
		/// Unity event function that gets the Flight Engineer on startup.
		/// </summary>
        private void Start()
        {
			GetFlightEngineer();
        }

		/// <summary>
		/// Refreshes the dial text display by updating text on angles.
		/// </summary>
        public void RefreshDisplay()
		{
			bool on = _flightEngineer.GetDialInfo(dialID).IsValueAtTarget();
			
			statusText.text = _flightEngineer.GetDialInfo(dialID).target.ToString();

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

		/// <summary>
		/// Gets the Flight Engineer workstation.
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
