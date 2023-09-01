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
		// Codex workstation URL
		public string codexURL;
		// VM workstation URLs
		public string workstation1URL;
		public string workstation2URL;
		public string workstation3URL;
		public string workstation4URL;
		public string workstation5URL;
		
		/// <summary>
		/// Gets the URL for a virtual machine workstation given the workstation's ID.
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
		// The challenge associated with this one; for ship challenges, this should be null if there are no associated challenges
		public string[] associatedChallenges;
		// The coordinates of the associated challenges, should have same length as associatedChallenges, should have null entries until this mission is complete
		// example: missions has 1 associated challenge and is incomplete, coords should look like *[ null ]* 
		// mission has 1 associated challenge and is complete, coords should look like *[ "123456" ]*
		// mission has 0 associated challenges, coords will look like *null* or []
		public string[] associatedChallengesCoordinates;
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
		// The x position of the mission on the galaxy map
		public float galaxyMapXPos;
		// The y position of the mission on the galaxy map
		public float galaxyMapYPos;
		// The x position of the target of the mission on the galaxy map
		public float galaxyMapTargetXPos;
		// The y position of the target of the mission on the galaxy map
		public float galaxyMapTargetYPos;

		/// <summary>
		/// Checks this mission against another mission object to see if they're the same.
		/// </summary>
		/// <param name="obj">The other mission object to compare against this one.</param>
		/// <returns>Whether the missions are equal.</returns>
		public bool IsEquivalentTo(MissionData obj)
		{
			// First, order the two task lists, because they also need comparison
			TaskData[] l = taskList.OrderBy(t => t.taskID).ToArray();
			TaskData[] l2 = obj.taskList.OrderBy(t => t.taskID).ToArray();

			/*
            Debug.Log("missionID: " + missionID + " vs " + "obj.missionID: " + obj.missionID);
            Debug.Log("complete: " + complete + " vs " + "obj.complete: " + obj.complete);
            Debug.Log("totalTeams: " + totalTeams + " vs " + "obj.totalTeams: " + obj.totalTeams);
            Debug.Log("solveTeams: " + solveTeams + " vs " + "obj.solveTeams: " + obj.solveTeams);
            Debug.Log("possibleMaximumScore: " + possibleMaximumScore + " vs " + "obj.possibleMaximumScore: " + obj.possibleMaximumScore);
            Debug.Log("baseSolveValue: " + baseSolveValue + " vs " + "obj.baseSolveValue: " + obj.baseSolveValue);
            Debug.Log("bonusRemaining: " + bonusRemaining + " vs " + "obj.bonusRemaining: " + obj.bonusRemaining);
            Debug.Log("currentScore: " + currentScore + " vs " + "obj.currentScore: " + obj.currentScore);
            Debug.Log("associatedChallenges.Length: " + associatedChallenges.Length + " vs " + "obj.associatedChallenges.Length: " + obj.associatedChallenges.Length);
            Debug.Log("associatedChallengesCoordinates.Length: " + associatedChallengesCoordinates.Length + " vs " + "obj.associatedChallengesCoordinates.Length: " + obj.associatedChallengesCoordinates.Length);
            Debug.Log("galaxyMapXPos: " + galaxyMapXPos + " vs " + "obj.galaxyMapXPos: " + obj.galaxyMapXPos);
            Debug.Log("galaxyMapYPos: " + galaxyMapYPos + " vs " + "obj.galaxyMapYPos: " + obj.galaxyMapYPos);
            Debug.Log("galaxyMapTargetXPos: " + galaxyMapTargetXPos + " vs " + "obj.galaxyMapTargetXPos: " + obj.galaxyMapTargetXPos);
            Debug.Log("galaxyMapTargetYPos: " + galaxyMapTargetYPos + " vs " + "obj.galaxyMapTargetYPos: " + obj.galaxyMapTargetYPos);
            Debug.Log("visible: " + visible + " vs " + "obj.visible: " + obj.visible);
            Debug.Log("isSpecial: " + isSpecial + " vs " + "obj.isSpecial: " + obj.isSpecial);
            Debug.Log("title: " + title + " vs " + "obj.title: " + obj.title);
            Debug.Log("summaryShort: " + summaryShort + " vs " + "obj.summaryShort: " + obj.summaryShort);
            Debug.Log("summaryLong: " + summaryLong + " vs " + "obj.summaryLong: " + obj.summaryLong);
            Debug.Log("roleList.Length: " + roleList.Length + " vs " + "obj.roleList.Length: " + obj.roleList.Length);
            Debug.Log("l.Length: " + l.Length + " vs " + "l2.Length: " + l2.Length);
            Debug.LogWarning("EASY STUFF DONE!");
			*/

			/*
			Debug.Log("obj.associatedChallengesCoordinates == null: " + (obj.associatedChallengesCoordinates == null).ToString());
            Debug.Log("obj.associatedChallengesCoordinates == new string[0]: " + (obj.associatedChallengesCoordinates == new string[0]).ToString());
			Debug.Log("obj.associatedChallengesCoordinates: " + obj.associatedChallengesCoordinates.ToString());
            Debug.Log("obj.associatedChallengesCoordinates.Length: " + obj.associatedChallengesCoordinates.Length);

			for (int i = 0; i < obj.associatedChallengesCoordinates.Length; i++)
			{
				Debug.Log("coord " + i + ": " + obj.associatedChallengesCoordinates[i]);
			}
			*/

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
				&& l.Length == l2.Length

				// Technically these checks are not exhaustive because the ordering of the coords/challenges is relavent but we rely on GameBrain to keep challenge->coord mapping one-to-one
				&& ((associatedChallenges == null && obj.associatedChallenges == null) || ((associatedChallenges.Length == obj.associatedChallenges.Length) && associatedChallenges.All(obj.associatedChallenges.Contains) && obj.associatedChallenges.All(associatedChallenges.Contains)))
				&& ((associatedChallengesCoordinates == null && obj.associatedChallengesCoordinates == null) || ((associatedChallengesCoordinates.Length == obj.associatedChallengesCoordinates.Length) && associatedChallengesCoordinates.All(obj.associatedChallengesCoordinates.Contains) && obj.associatedChallengesCoordinates.All(associatedChallengesCoordinates.Contains)))

				// then compare things we know don't update often
				&& Math.Abs(galaxyMapXPos - obj.galaxyMapXPos) < Mathf.Epsilon
				&& Math.Abs(galaxyMapYPos - obj.galaxyMapYPos) < Mathf.Epsilon
				&& Math.Abs(galaxyMapTargetXPos - obj.galaxyMapTargetXPos) < Mathf.Epsilon
				&& Mathf.Abs(galaxyMapTargetYPos - obj.galaxyMapTargetYPos) < Mathf.Epsilon

				// The last step is to compare the individual elements of the two task lists
				&& IsTaskDataEquivalent(l, l2);
        }

		/// <summary>
		/// Helper method for comparison between two task lists.
		/// </summary>
		/// <param name="l">The first task list.</param>
		/// <param name="l2">The second task list.</param>
		/// <returns>Whether the two task lists are equal.</returns>
		private bool IsTaskDataEquivalent(TaskData[] l, TaskData[] l2)
        {
			// Loop through all tasks in the two lists and compare them; return false if any two differ
			for (int i = 0; i < l.Length; i++)
            {
				if (!l[i].IsEquivalentTo(l2[i]))
				{
					return false;
				}
            }
			return true;
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
		public PoweredState powerState;

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
