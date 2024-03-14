/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using System.Collections.Generic;
using Entities.Workstations;
using UnityEngine;
using System.Linq;

namespace Systems.GameBrain
{
	/// <summary>
	/// The complete data of the ship received from a polling request to GameBrain.
	/// </summary>
	[Serializable]
	public class GameData : IDeserializableFromJSON<GameData>
	{
		// Basic team information
		public Session session;
		// URLs for VM workstations and the Codex workstation
		public ShipData ship;
		// The missions available to the team
		public MissionData[] missions;
		// The list of locations this team has unlocked
		public Location[] locations;
		// Infomation on the ship's status at the given location
		public CurrentLocationGameplayData currentStatus;
		
		// A map of the unlocked locations
		public Dictionary<string, Location> locationMap;

		/// <summary>
		/// Creates a new GameData structure from a JSON provided by GameBrain.
		/// </summary>
		/// <param name="jsonShipData">A JSON structure representing ship data.</param>
		/// <returns>GameData which has been deserialized from JSON.</returns>
		public static GameData CreateFromJSON(string jsonShipData)
		{
			return JsonUtility.FromJson<GameData>(jsonShipData);
		}

		/// <summary>
		/// Sets the power state, transmission event type, and dictionary of locations.
		/// </summary>
		public void Initiate()
		{
			// Set whether the ship is in launch mode, exploration mode, or standby (neither)
			currentStatus.powerState = CurrentLocationGameplayData.GetPoweredState(currentStatus.powerStatus);
			// If there's an incoming transmission object, set the type of comm event accordingly
			if (currentStatus.incomingTransmissionObject != null && currentStatus.incomingTransmissionObject.commTemplate != null)
			{
				currentStatus.incomingTransmissionObject.template = CommEvent.GetEventTemplate(currentStatus.incomingTransmissionObject.commTemplate);
			}

			// Build the location dictionary
			BuildLocationMap();
		}

		/// <summary>
		/// Builds the dictionary of unlocked locations.
		/// </summary>
		private void BuildLocationMap()
		{
			// Instantiate the dictionary, then add all received locations to it
			locationMap = new Dictionary<string, Location>();

			if (locations == null) return;
			foreach (var location in locations)
			{
				locationMap.Add(location.locationID, location);
			}
		}
	}

	/// <summary>
	/// Basic information about the session where this game occurs.
	/// </summary>
	[Serializable]
	public class Session
	{
		// The name of the team
		public string teamInfoName;
		// The number of codexes found by this team so far
		public int teamCodexCount;
		// The URL of the jump cutscene
		public string jumpCutsceneURL;
		// The timestamp when the game starts
		public string gameStartTime;
        // The timestamp on the Gamebrain server
        public string gameCurrentTime;
        // The timestamp when the game ends
        public string gameEndTime;
		// The DateTime when the game starts
		public DateTime gameStartDateTime;
		// The DateTime when the game ends
		public DateTime gameEndDateTime;
		// The current time on the Gamebrain server; ensure this name matches the one in the JSON sent by Gamebrain
		public DateTime gameCurrentDateTime;
		// The title to give the timer
		public string timerTitle;
		// Whether to use the galaxy display map
		public bool useGalaxyDisplayMap;
		// Whether to display codices (mission pogs)
		public bool useCodices;
		// Whether to show empty pogs for incomplete but visable missions
		public bool displayIncompleteMissionPogs;
    }

	/// <summary>
	/// Class providing URLs for each VM workstation and the Codex workstation.
	/// </summary>
	[Serializable]
	public class ShipData
	{
		// Codex workstation URL; This is from a previous years competition but remains for backwards compatibility
		// If this is present the codex station will use it, otherwise if it null the codex station will be *TBD*
		public string codexURL;
		// Old structure for VM workstation URLs; each one of these maps to one Cyber Operations station
		public string workstation1URL;
		public string workstation2URL;
		public string workstation3URL;
		public string workstation4URL;
		public string workstation5URL;
        // New data structure for VM URLs available for each mission at CyberOps stations
        public MissionVMs[] challengeURLs;

        /// <summary>
        /// Gets the URL for a virtual machine workstation given the workstation's ID. This is the old structure, which remains for backwards compatibility
        /// </summary>
        /// <param name="stationID">The identifier of the VM/Codex workstation.</param>
        /// <returns>A URL for the provided workstation.</returns>
        public string GetURLForStation(WorkstationID stationID)
		{
			switch (stationID)
			{
				case WorkstationID.WorkstationVM1:
					return workstation1URL;
				case WorkstationID.WorkstationVM2:
					return workstation2URL;
				case WorkstationID.WorkstationVM3:
					return workstation3URL;
				case WorkstationID.WorkstationVM4:
					return workstation4URL;
				case WorkstationID.WorkstationVM5:
					return workstation5URL;
				case WorkstationID.Codex:
					return codexURL;
				default:
					return "";
			}
		}

		// If the new data structure is present, use it
		public bool IsMissionVMsStructureInUse()
		{
			// If we have a non-empty list of challengeURLs, use the new structure. Otherwise, use the old structure unless the workstation1URL is null or empty
			return (challengeURLs != null && challengeURLs.Length > 0) || string.IsNullOrEmpty(workstation1URL);
        }
	}

    /// <summary>
    /// New data structure for VM URLs available at CyberOps stations for a given mission
    /// </summary>
    [Serializable]
	public class MissionVMs
	{
		// ID of the mission; maps to an icon, mission in MissionData, etc
		public string missionID;
		// Display name of this mission
		public string missionName;
		// The id of the icon for this vm
		public string missionIcon;
		// All the vms/challenges available for this mission
		public ChallengeVM[] vmURLs;
	}

    /// <summary>
    /// An individual VM that a user can open from a CyberOps station to complete a challenge
    /// </summary>
    [Serializable]
	public class ChallengeVM
	{
		// The display name of this vm
		public string vmName;
		// The url to open to connect to this vm
		public string vmURL;
	}

    /// <summary>
    /// A class storing information for a mission, with that data being rendered in the Mission Log.
    /// </summary>
    [Serializable]
	public class MissionData
	{
		// The ID of the mission
		public string missionID;
		// The possible number of points that can be achieved (base + full bonus)
		public int possibleMaximumScore;
		// The base number of points this challenge has at its base
		public int baseSolveValue;
		// The number of bonus points remaining
		public int bonusRemaining;
		// The currentScore attained for this mission
		public int currentScore;
		// All challenges (aka cache challenges) associated with this one which will be unlocked when the player completes this mission
		public AssociatedChallengeData[] associatedChallenges;
        // The number of teams who have attempted the challenge
        public int totalTeams;
		// The number of teams who have solved the challenge
		public int solveTeams;
		// Boolean that must be true for the mission to appear in the Mission Log
		public bool visible;
		// If the mission has been completed
		public bool complete;
		// If this mission should be marked differently than other missions
		public bool isSpecial;
		// The title of the mission
		public string title;
		// The icon of the mission, used also for the mission's codex piece
		public string missionIcon;
		// A short one-line description in the mission list
		public string summaryShort;
		// A longer description when the mission is specifically clicked
		public string summaryLong;
		// A list of the NICE roles used in this mission
		public string[] roleList;
		// A list of the different tasks used in this mission
		public TaskData[] taskList;
		// The x position of the mission on the galaxy map. Allowable range: [-540, 540]. Give each a circle with diameter 125 t0 avoid overlap
		public float galaxyMapXPos;
        // The y position of the mission on the galaxy map. Allowable range: [-320, 320]
        public float galaxyMapYPos;
        // The x position of the target of the mission on the galaxy map. Allowable range: [-540, 540]
        public float galaxyMapTargetXPos;
        // The y position of the target of the mission on the galaxy map. Allowable range: [-320, 320]
        public float galaxyMapTargetYPos;

		/// <summary>
		/// Checks this mission against another mission object to see if they're the same.
		/// </summary>
		/// <param name="obj">The other mission object to compare against this one.</param>
		/// <returns>Whether the missions are equal.</returns>
		public bool IsEquivalentTo(MissionData obj)
		{
			// Return if the different attributes between the two missions are equal
			return missionID == obj.missionID
				//first compare bools and ints
				&& complete == obj.complete
				&& totalTeams == obj.totalTeams
				&& solveTeams == obj.solveTeams
				&& possibleMaximumScore == obj.possibleMaximumScore
				&& baseSolveValue == obj.baseSolveValue
				&& bonusRemaining == obj.bonusRemaining
				&& currentScore == obj.currentScore
				&& visible == obj.visible
				&& isSpecial == obj.isSpecial

				// then compare strings and arrays
				&& title == obj.title
				&& missionIcon == obj.missionIcon
				&& summaryShort == obj.summaryShort
				&& summaryLong == obj.summaryLong
				&& roleList.Length == obj.roleList.Length
				&& roleList.All(obj.roleList.Contains)

                && IsAssociatedChallengeEquivalent(associatedChallenges, obj.associatedChallenges)
				&& IsTaskDataEquivalent(taskList, obj.taskList)

				// then compare things we know don't update often
				&& Math.Abs(galaxyMapXPos - obj.galaxyMapXPos) < Mathf.Epsilon
				&& Math.Abs(galaxyMapYPos - obj.galaxyMapYPos) < Mathf.Epsilon
				&& Math.Abs(galaxyMapTargetXPos - obj.galaxyMapTargetXPos) < Mathf.Epsilon
				&& Mathf.Abs(galaxyMapTargetYPos - obj.galaxyMapTargetYPos) < Mathf.Epsilon;
        }

		/// <summary>
		/// Helper method for comparison between two task lists.
		/// </summary>
		/// <param name="l">The first task list.</param>
		/// <param name="l2">The second task list.</param>
		/// <returns>Whether the two task lists are equal.</returns>
		private bool IsTaskDataEquivalent(TaskData[] l, TaskData[] l2)
        {
			// If both are null, return true
			if (l == null && l2 == null)
			{
				return true;
			}
			// if only one is null or the lists are different lengths, return false
			else if (l == null || l2 == null || l.Length != l2.Length)
			{
				return false;
			}

			TaskData[] ol = l.OrderBy(t => t.taskID).ToArray();
            TaskData[] ol2 = l2.OrderBy(t => t.taskID).ToArray();

            // Loop through all tasks in the two lists and compare them; return false if any two differ
            for (int i = 0; i < ol.Length; i++)
            {
				if (!ol[i].IsEquivalentTo(ol2[i]))
				{
					return false;
				}
            }
			return true;
        }

        private bool IsAssociatedChallengeEquivalent(AssociatedChallengeData[] l, AssociatedChallengeData[] l2)
        {
            // If both are null, return true
            if (l == null && l2 == null)
            {
                return true;
            }
            // if only one is null or the lists are different lengths, return false
            else if (l == null || l2 == null || l.Length != l2.Length)
            {
                return false;
            }

            AssociatedChallengeData[] ol = l.OrderBy(t => t.missionID).ToArray();
            AssociatedChallengeData[] ol2 = l2.OrderBy(t => t.missionID).ToArray();

            // Loop through all tasks in the two lists and compare them; return false if any two differ
            for (int i = 0; i < ol.Length; i++)
            {
                if (!ol[i].IsEquivalentTo(ol2[i]))
                {
                    return false;
                }
            }
            return true;
        }

		public override string ToString()
		{
            string res = $"missionID: {missionID}\n" +
				  $"possibleMaximumScore: {possibleMaximumScore}\n" +
				  $"baseSolveValue: {baseSolveValue}\n" +
				  $"bonusRemaining: {bonusRemaining}\n" +
				  $"currentScore: {currentScore}\n" +
				  $"totalTeams: {totalTeams}\n" +
				  $"solveTeams: {solveTeams}\n" +
				  $"visible: {visible}\n" +
				  $"complete: {complete}\n" +
				  $"isSpecial: {isSpecial}\n" +
				  $"title: {title}\n" +
				  $"missionIcon: {missionIcon}\n" +
				  $"summaryShort: {summaryShort}\n" +
				  $"summaryLong: {summaryLong}\n" +
				  $"galaxyMapXPos: {galaxyMapXPos}\n" +
				  $"galaxyMapYPos: {galaxyMapYPos}\n" +
				  $"galaxyMapTargetXPos: {galaxyMapTargetXPos}\n" +
				  $"galaxyMapTargetYPos: {galaxyMapTargetYPos}\n";
			res += "roleList: [";
			foreach (string r in roleList)
			{
				res += $"{r}, ";
			}
			res += "]\n";

			res += "---------------------------------------------";
			res += "AssociatedChallenges:\n";
			res += $"    Count: {associatedChallenges.Count()}\n";
			res += "     [\n";
			foreach (AssociatedChallengeData acd in associatedChallenges)
			{
				res += $"       (missionID: {acd.missionID}, unlockCode: {acd.unlockCode}, complete: {acd.complete})\n";
			}
			res += "     ]\n";
            res += "---------------------------------------------";
            res += "TaskDatas:\n";
            res += $"    Count: {taskList.Count()}\n";
            res += "     [\n";
            foreach (TaskData td in taskList)
            {
                res += $"       (taskID: {td.taskID}, missionID: {td.missionID}, complete: {td.complete}, infoPresent: {td.infoPresent}, videoPresent: {td.videoPresent}, videoURL: {td.videoURL}, infoTest: {td.infoText}, descriptionText: {td.descriptionText})\n";
            }
            res += "     ]";

            return res;
		}
    }

	/// <summary>
	/// Information on a specific task within a mission.
	/// </summary>
	[Serializable]
	public class TaskData
	{
		// The ID of the task
		public string taskID;
		// The ID of the mission this task is associated with
		public string missionID;
		// The short description for the task rendered in the UI
		public string descriptionText;
		// Whether this task is visible to players
		public bool visible;
		// Whether this task is marked as complete
		public bool complete;
		// Whether additional information is available for this task
		public bool infoPresent;
		// The information accessible under the task's information icon (only selectable in UI if infoPresent is true)
		public string infoText;
		// Whether a video is accessible for this task
		public bool videoPresent;
		// The URL of the video that should play for this task (only selectable in UI if videoPresent is true)
		public string videoURL;

		/// <summary>
		/// Checks this task against another task object to see if they're the same.
		/// </summary>
		/// <param name="obj">The other task object to compare against this one.</param>
		/// <returns>Whether the tasks are equal.</returns>
		public bool IsEquivalentTo(TaskData obj)
		{
			bool tasksAreEqual = taskID == obj.taskID
				&& missionID == obj.missionID
				&& descriptionText == obj.descriptionText
				&& visible == obj.visible
				&& complete == obj.complete
				&& infoPresent == obj.infoPresent
				&& infoText == obj.infoText
				&& videoPresent == obj.videoPresent
				&& videoURL == obj.videoURL;
			return tasksAreEqual;
		}
	}

	/// <summary>
	/// Information on a mission that is linked to another mission (cache missions)
	/// </summary>
	[Serializable]
	public class AssociatedChallengeData
	{
		// The id of the associated mission
		public string missionID;
		// The coordinates of the location the mission is at, shown to player in the galaxy map when the mission this is associated with is complete
		public string unlockCode;
		// Whether or not the associated mission is complete
		public bool complete;

        public bool IsEquivalentTo(AssociatedChallengeData obj)
        {
            return missionID == obj.missionID && unlockCode == obj.unlockCode && complete == obj.complete;
        }
    }

    /// <summary>
    /// A class holding information on a communication that takes place at the sensor station.
    /// </summary>
    [Serializable]
	public class CommEvent
	{
		// The ID of the comm event
		public string commID;
		// The URL of the video for this comm event
		public string videoURL;
		// The string representation of a CommEventTemplate
		public string commTemplate;
		// The actual enum representing a comm event, obtained through data conversion
		public CommEventTemplate template;
		// The message initially displayed when trying to translate the comm event on the sensor station screen
		public string translationMessage;
		// The information displayed on the sensor screen before choosing to scan
		public string scanInfoMessage;
		// Whether this is a first contact comm event
		public bool firstContact;

		/// <summary>
		/// The general type of comm event.
		/// </summary>
		public enum CommEventTemplate
		{
			None,
			Incoming,
			BadTranslation,
			Probe
		}

		/// <summary>
		/// Gets the type of a comm event in the data type it should be read as, given the string of that type.
		/// </summary>
		/// <param name="template">The string representation of the comm event type.</param>
		/// <returns>The type of comm event in the CommEventTemplate data type.</returns>
		public static CommEventTemplate GetEventTemplate(string template)
		{
			switch (template)
			{
				case "incoming":
					return CommEventTemplate.Incoming;
				case "badTranslation":
					return CommEventTemplate.BadTranslation;
				case "probe":
					return CommEventTemplate.Probe;
				case "":
					return CommEventTemplate.None;
				default:
					Debug.LogWarning("Unknown Comm Event Template:"+template);
					return CommEventTemplate.None;
			}
		}

		/// <summary>
		/// Gets the string representation of a comm event from the CommEventTemplate data type.
		/// </summary>
		/// <param name="template">The CommEventTemplate representation of the comm event type.</param>
		/// <returns>The type of comm event as a string.</returns>
		public static string GetEventString(CommEventTemplate template)
		{
			switch (template)
			{
				case CommEventTemplate.Incoming:
					return "incoming";
				case CommEventTemplate.BadTranslation:
					return "badTranslation";
				case CommEventTemplate.Probe:
					return "probe";
				default:
					return "";
			}
		}

		/// <summary>
		/// Checks this comm event against another comm event object to see if they're the same.
		/// </summary>
		/// <param name="obj">The other comm event object to compare against this one.</param>
		/// <returns>Whether the comm events are equal.</returns>
		public bool IsEquivalentTo(CommEvent obj)
		{
			return template == obj.template
			       && firstContact == obj.firstContact
			       && commID == obj.commID
			       && scanInfoMessage == obj.scanInfoMessage
			       && videoURL == obj.videoURL;
		}
	}

	/// <summary>
	/// Class containing the current gameplay information at the given scene.
	/// </summary>
	[Serializable]
	public class CurrentLocationGameplayData
	{
		// The name of the location the team is currently at
		public string currentLocation;
		// Whether the antenna is extended at this location
		public bool antennaExtended;
		// Whether the team has connected to the network at this location (unused)
		public bool networkConnected;
		// The name of the network joined
		public string networkName;
		// Whether the team has already had a first contact event at the Sensor workstation at this location
		public bool firstContactComplete;
		// The power state of the ship
		public string powerStatus;
		// Whether there is an incoming transmission at this location
		public bool incomingTransmission;
		// Whether this location has been scanned by the team
		public bool currentLocationScanned;
		// Flavor text for the location displayed when there's a comm event at the Sensor station
		public string currentLocationSurroundings;
		// The comm event being sent at this location, if there is one (only shown if incomingTransmission is true)
		public CommEvent incomingTransmissionObject;
		// The current power mode
		public PoweredState powerState; // TODO: Cubespace is the authority on this, so it shouldn't really need to be getting this var from gamebrain

		/// <summary>
		/// The power mode of the ship, given its power configuration.
		/// </summary>
		public enum PoweredState
		{
			Standby,
			LaunchMode,
			ExplorationMode,
		}

		/// <summary>
		/// Retrieves a given power state as the PoweredState data type.
		/// </summary>
		/// <param name="powerMode">The power state of the ship (exploration mode, launch mode, or standby - no mode) as a string.</param>
		/// <returns>A PoweredState representation of the power state.</returns>
		public static PoweredState GetPoweredState(string powerMode)
		{
			switch (powerMode)
			{
				case "standby":
					return PoweredState.Standby;
				case "launchMode":
					return PoweredState.LaunchMode;
				case "explorationMode":
					return PoweredState.ExplorationMode;
				default:
					if (!string.IsNullOrEmpty(powerMode))
					{
						Debug.LogWarning("Unknown Power State: " + powerMode);
					}

					return PoweredState.Standby;
			}
		}

		/// <summary>
		/// Retrieves a given power state as a string.
		/// </summary>
		/// <param name="powerMode">The power state of the ship (exploration mode, launch mode, or standby - no mode).</param>
		/// <returns>A string representation of the power state.</returns>
		public static string PoweredStateAsString(PoweredState powerMode)
		{
			switch (powerMode)
			{
				case PoweredState.Standby:
					return "standby";
				case PoweredState.ExplorationMode:
					return "explorationMode";
				case PoweredState.LaunchMode:
					return "launchMode";
				default:
					return "";
			}
		}
	}
}
