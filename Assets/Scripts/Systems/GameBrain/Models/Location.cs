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

namespace Systems.GameBrain
{
	/// <summary>
	/// General structure for an in-game location.
	/// </summary>
	[Serializable]
	public class Location : IDeserializableFromJSON<Location>
	{
		// The identifier of the location
		public string locationID;
		// The name of this location
		public string name;
		// The identifier of the location's image
		public string imageID;
		// The identifier of the location's backdrop image
		public string backdropID;
		// Whether the team has already visited this location
		public bool visited;
		// Whether the team has already scanned this location
		public bool scanned;
		// The code to unlock this location
		public string unlockCode;
		// The launch angle for this location
		public int trajectoryLaunch;
		// The correction angle for this location
		public int trajectoryCorrection;
		// The cube angle for this location
		public int trajectoryCube;

		/// <summary>
		/// Checks for equality between two locations. This is an auto-generated method.
		/// </summary>
		/// <param name="obj">Another location object.</param>
		/// <returns>Whether one location object is equal to another.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Location) obj);
		}

		/// <summary>
		/// Checks references and properties of this Location object. Used as a helper method of the overriden Equals method.
		/// </summary>
		/// <param name="other">The Location object to compare equalit with.</param>
		/// <returns>Whether the two Location objects are equal.</returns>
		public bool Equals(Location other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return locationID == other.locationID
				&& name == other.name
				&& imageID == other.imageID
				&& visited == other.visited
				&& scanned == other.scanned
				&& unlockCode == other.unlockCode
				&& trajectoryLaunch == other.trajectoryLaunch
				&& trajectoryCorrection == other.trajectoryCorrection
				&& trajectoryCube == other.trajectoryCube
				&& backdropID == other.backdropID;
		}

		/// <summary>
		/// Gets the hash code of this Location object based on specific properties.
		/// </summary>
		/// <returns>The hash code of the Location object.</returns>
		public override int GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(locationID);
			hashCode.Add(name);
			hashCode.Add(imageID);
			// hashCode.Add(unlocked);
			hashCode.Add(visited);
			hashCode.Add(scanned);
			hashCode.Add(unlockCode);
			// hashCode.Add(networkEstablished);
			// hashCode.Add(networkName);
			hashCode.Add(trajectoryLaunch);
			hashCode.Add(trajectoryCorrection);
			hashCode.Add(trajectoryCube);
			hashCode.Add(backdropID);
			return hashCode.ToHashCode();
		}

		/// <summary>
		/// Checks this location against another location object to see if they're the same.
		/// </summary>
		/// <param name="obj">The other location object to compare against this one.</param>
		/// <returns>Whether the locations are equal.</returns>
		public bool IsEquivalent(Location obj) {
			// Not all properties necessarily need to be compared
			return (locationID == obj.locationID)
				&& (name == obj.name)
				&& (imageID == obj.imageID)
				// && (unlocked == loc.unlocked)
				&& (visited == obj.visited)
				&& (scanned == obj.scanned)
				&& (unlockCode == obj.unlockCode)
				// && (networkEstablished == loc.networkEstablished)
				// && (networkName == loc.networkName)
				&& (trajectoryLaunch == obj.trajectoryLaunch)
				&& (trajectoryCorrection == obj.trajectoryCorrection)
				&& (trajectoryCube == obj.trajectoryCube)
				&& (backdropID == obj.backdropID);
		}

		/// <summary>
		/// General method used to perform additional logic as needed.
		/// </summary>
		public void Initiate()
		{
			// Blank, as there's no logic needed
		}
	}
}
