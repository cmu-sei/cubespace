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
using System.Linq;
using Entities;
using Entities.Workstations;
using UnityEngine;

namespace Managers
{
	/// <summary>
	/// A ScriptableObject acting as a middleman for information on the workstations.
	/// </summary>
	[CreateAssetMenu(fileName = "Workstation Manager", menuName = "Game Data/Workstation Manager", order = 0)]
	public class WorkstationManager : ScriptableObject
	{
        #region Variables
        /// <summary>
        /// The layer mask for the main ship scene, used to enable or disable the objects visible when entering or exiting a workstation.
        /// Derives from a private variable.
        /// </summary>
        public LayerMask MainShipViewLayerMask => _mainShipViewLayerMask;
		/// <summary>
		/// The layer mask for the main ship scene, used to enable or disable the objects visible when entering or exiting a workstation.
		/// </summary>
		[SerializeField]
		private LayerMask _mainShipViewLayerMask;
		/// <summary>
		/// The layer mask for the workstation scene, used to enable or disable the objects visible when entering or exiting a workstation.
		/// </summary>
		[SerializeField]
		private LayerMask _workstationViewLayerMask;
		/// <summary>
		/// The layer mask for the workstation scene, used to enable or disable the objects visible when entering or exiting a workstation.
		/// Derives from a private variable.
		/// </summary>
		public LayerMask WorkstationViewLayerMask => _workstationViewLayerMask;

		/// <summary>
		/// Whether workstations have been added to the dictionary.
		/// </summary>
		public bool HasWorkstations => workstations.Count > 0;

		/// <summary>
		/// A dictionary used to look up workstations. Workstations are the player-enterable entities within the Workstations scene.
		/// </summary>
		private Dictionary<WorkstationID, Workstation> workstations = new Dictionary<WorkstationID, Workstation>();
		/// <summary>
		/// A dictionary used to look up workstations. Terminals are the entities with mouse events within the Main scene.
		/// </summary>
		private Dictionary<WorkstationID, Terminal> terminals = new Dictionary<WorkstationID, Terminal>();

		/// <summary>
		/// The list of workstations used in exploration mode.
		/// </summary>
		private List<Workstation> explorationModeWorkstations = new List<Workstation>();
		/// <summary>
		/// The list of workstations used in launch mode.
		/// </summary>
		private List<Workstation> launchModeWorkstations = new List<Workstation>();
        #endregion

        #region Workstation methods

        
        /// <summary>
        /// Clears the lists to prevent editor only bugs with scriptableObjects not being reset. Called once during game launch.
        /// </summary>
        public void Init()
        {
	        workstations.Clear();
	        terminals.Clear();
	        
	        explorationModeWorkstations.Clear();
	        launchModeWorkstations.Clear();
        }
        
        /// <summary>
        /// Places a workstation under a player's authority and moves their camera into that workstation.
        /// This workstation corresponds to whatever Terminal object the client clicks on.
        /// </summary>
        /// <param name="player">The Player object representing the local client player.</param>
        /// <param name="workstationID">The ID corresponding with the workstation to access.</param>
        public void AccessWorkstation(Player player, WorkstationID workstationID)
		{
			// If the terminal's GameObject exists and hasn't been destroyed, place the player at the station
			if (workstations.TryGetValue(workstationID, out var station) && station != null)
			{
				// Places a player within a workstation
				station.Activate(player, CameraManager.Instance.mainVCam);
			}
		}

		/// <summary>
		/// Checks to see if a workstation with the given ID exists within the workstaions dictionary.
		/// </summary>
		/// <param name="workstationID">The ID of the workstation to check.</param>
		/// <returns>Whether the workstation exists within the dictionary.</returns>
		public bool CheckForWorkstation(WorkstationID workstationID)
        {
			return workstations.ContainsKey(workstationID);
		}

        /// <summary>
        /// Adds a workstation with the given identifier to the dictionary.
        /// </summary>
        /// <param name="stationID">The ID associated with the workstation that should be registered.</param>
        /// <param name="workstation">The Workstation object to register.</param>
        public void RegisterWorkstation(WorkstationID stationID, Workstation workstation)
		{
			// Add to the exploration and launch workstation lists if the workstation is in either mode
			if (!CheckForWorkstation(stationID))
            {
				if (workstation.UsedInExplorationMode)
				{
					explorationModeWorkstations.Add(workstation);
				}

				if (workstation.UsedInLaunchMode)
				{
					launchModeWorkstations.Add(workstation);
				}
			}

			// Add the workstation to the dictionary
			workstations[stationID] = workstation;
		}

        /// <summary>
        /// Removes the workstation from reference. Because WorkstationManager is a scriptableObject, it will hold onto objects when entering/exiting play mode, so we reset cleanly here. See registerWorkstation for parameter info.
        /// </summary>
        public void DeregisterWorkstation(WorkstationID stationID, Workstation workstation)
        {
	        if (workstations.ContainsKey(stationID))
	        {
		        workstations.Remove(stationID);
	        }

	        if (explorationModeWorkstations.Contains(workstation))
	        {
		        explorationModeWorkstations.Remove(workstation);
	        }

	        if (launchModeWorkstations.Contains(workstation))
	        {
		        launchModeWorkstations.Remove(workstation);
	        }
        }
        

        #endregion

        #region Terminal methods
        /// <summary>
        /// Adds a terminal to the dictionary of possible terminals.
        /// </summary>
        /// <param name="stationID">The ID of the terminal to add.</param>
        /// <param name="terminal">The Terminal script object.</param>
        public void RegisterTerminal(WorkstationID stationID, Terminal terminal)
		{
			terminals[stationID] = terminal;
		}

		/// <summary>
		/// Detaches a player from a workstation by asking the workstation to mark itself as not in use.
		/// </summary>
		/// <param name="workstationID">The ID of the workstation to pull the player out of.</param>
		public void ExitTerminal(WorkstationID workstationID)
		{
			// If the terminal's GameObject exists and hasn't been destroyed, leave the terminal
			if (terminals.TryGetValue(workstationID, out var terminal) && terminal != null)
			{
				terminal.ExitTerminal();
			}
		}
		#endregion

		#region Getter methods
		/// <summary>
		/// Gets a Workstation object with the ID specified.
		/// </summary>
		/// <param name="workstationID">The ID of the Workstation object we want to find.</param>
		/// <returns>The workstation with the ID specified.</returns>
		public Workstation GetWorkstation(WorkstationID workstationID)
		{
			if (workstations.TryGetValue(workstationID, out var station))
			{
				return station;
			}

			return null;
		}

		/// <summary>
		/// Gets a Terminal object with the ID specified.
		/// </summary>
		/// <param name="workstationID">The ID of the Terminal object we want to find.</param>
		/// <returns>The terminal with the ID specified.</returns>
		public Terminal GetTerminal(WorkstationID workstationID)
		{
			if (terminals.TryGetValue(workstationID, out var terminal))
			{
				return terminal;
			}

			return null;
		}
		#endregion

		#region Workstation list methods
		/// <summary>
		/// Gets all workstations.
		/// </summary>
		/// <returns>A list of all the workstations.</returns>
		public List<Workstation> GetWorkstations()
		{
			return workstations.Values.ToList();
		}

		/// <summary>
		/// Gets a list of workstations used in exploration mode.
		/// </summary>
		/// <returns>All workstations used in exploration mode.</returns>
		public List<Workstation> GetExplorationWorkstations()
		{
			return explorationModeWorkstations;
		}

		/// <summary>
		/// Gets a list of workstations used in launch mode.
		/// </summary>
		/// <returns>All workstations used in launch mode.</returns>
		public List<Workstation> GetLaunchWorkstations()
		{
			return launchModeWorkstations;
		}
		#endregion

		#region Context menu methods (not used in-game)
		/// <summary>
		/// Clears the workstations dictionary through an additional context menu.
		/// </summary>
		[ContextMenu("Clear Workstations Dictionary")]
		public void ClearWorkstationsDictionary()
		{
			workstations.Clear();
		}

		/// <summary>
		/// Clears the terminals dictionary through an additional context menu.
		/// </summary>
		[ContextMenu("Clear Terminals Dictionary")]
		public void ClearTerminalsDictionary()
		{
			terminals.Clear();
		}
		#endregion

		
	}
}
