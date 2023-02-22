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
using UnityEngine;

namespace Systems.GameBrain
{
	/// <summary>
	/// A structure for a response from GameBrain upon an attempt to unlock a location.
	/// </summary>
	[Serializable]
	public struct LocationUnlockResponse : IDeserializableFromJSON<LocationUnlockResponse>
	{
		/// <summary>
		/// The result of the attempt to unlock a location - the attempt was invalid, successful, or already done.
		/// </summary>
		public enum UnlockResult
		{
			Invalid,
			Success,
			AlreadyUnlocked
		}

		// The result of trying to unlock the location
		public UnlockResult unlockResult;
		// The coordinates entered for this unlock attempt
		public string enteredCoordinates;
		// The status of the attempt to unlock a location
		public string responseStatus;
		// The ID of the location unlocked
		public string locationID;

		/// <summary>
		/// Creates an object indicating the result of an attempt to unlock a location.
		/// </summary>
		/// <param name="locationUnlockJson">The JSON representation of the location unlock response.</param>
		/// <returns>The result of the unlock attempt and additional information as the LocationUnlockResponse data type.</returns>
		public static LocationUnlockResponse CreateFromJSON(string locationUnlockJson)
		{
			return JsonUtility.FromJson<LocationUnlockResponse>(locationUnlockJson);
		}

		/// <summary>
		/// Creates the unlockResult by converting the received responseStatus into an object of type UnlockResult.
		/// </summary>
		public void Initiate()
		{
			unlockResult = UnlockResultFromResponse();
		}

		/// <summary>
		/// Converts the responseStatus string received into an object of type UnlockResult.
		/// </summary>
		/// <returns>The result of the unlock attempt as the UnlockResult data type.</returns>
		private UnlockResult UnlockResultFromResponse()
		{
			if (responseStatus == null) 
			{
				return UnlockResult.Invalid;
			}

			switch (responseStatus.ToLower())
			{
				case "success":
					return UnlockResult.Success;
				case "invalid":
					return UnlockResult.Invalid;
				case "alreadyunlocked":
					return UnlockResult.AlreadyUnlocked;
				default:
					Debug.LogWarning("bad result from location unlock.");
					return UnlockResult.Invalid;
			}
		}
	}
}
