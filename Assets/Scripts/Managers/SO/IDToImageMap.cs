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
using UnityEngine;

namespace Managers
{
	/// <summary>
	/// ScriptableObject defining a mapping between a location identifier and a background image.
	/// </summary>
	[CreateAssetMenu(fileName = "ID To Image Map", menuName = "Game Data/ID To Image Map", order = 1)]
	public class IDToImageMap : ScriptableObject
	{
		// A list of location IDs and the background images that are associated with a location
		[SerializeField]
		private List<LocationIDImagePair> pairs = new List<LocationIDImagePair>();
		// A dictionary of location IDs and the location image
		private readonly Dictionary<string, Sprite> images = new Dictionary<string, Sprite>();

		/// <summary>
		/// Creates the dictionary when the script loads in game or the pairs List is modified in the Inspector.
		/// </summary>
		void OnValidate()
		{
			InitiateDictionary();
		}

		/// <summary>
		/// Populates a dictionary with a location ID and an image.
		/// </summary>
		public void InitiateDictionary()
		{
			// Clear the existing dictionary if it exists
			images.Clear();

			// Loop through the list of pairs and make sure the pair exists and has both attributes
			foreach (LocationIDImagePair pair in pairs)
			{
				if (!images.ContainsKey(pair.GetLocationID()) && pair.GetLocationID() != "" && pair.GetImage() != null)
				{
					images.Add(pair.GetLocationID(), pair.GetImage());
				}
			}
		}

		/// <summary>
		/// Gets a sprite from this image map
		/// </summary>
		/// <param name="imageID">The ID for the image.</param>
		/// /// <param name="getRandomIfNotFound">Should it set a random image to this ID if the right image can't be found.</param>
		/// <param name="defaultID">A default to use in case the function cannot find the image with the given ID.</param>
		/// <returns>The Sprite object representing a background image.</returns>
		public Sprite GetImage(string imageID, bool getRandomIfNotFound, string defaultID = "")
		{
			if (images == null || images.Count == 0)
			{
				if (pairs.Count == 0)
				{
                    Debug.LogWarning("Tried to get image with id: " + imageID + " and defaultID: " + defaultID + "from empty image map");
                }
				else
				{
					InitiateDictionary();
				}
				return null;
			}

            Sprite image;
            if (string.IsNullOrEmpty(imageID))
			{
				if (defaultID != "" && images.TryGetValue(defaultID, out image))
				{
                    return image;
                }
			}
			else if (images.TryGetValue(imageID, out image))
			{
				return image;
			}

			if (getRandomIfNotFound)
			{
                var s = GetRandomImage();
				if (s != null && !string.IsNullOrEmpty(imageID)) images.Add(imageID, s);
                return s;
            }
			
			return null;
		}

		/// <summary>
		/// Gets a random image from the list of location ID and image pairs.
		/// </summary>
		/// <returns>A random image from the list.</returns>
		private Sprite GetRandomImage()
		{
			// Return a random image from the list of pairs
			if (pairs.Count > 0)
			{
				return pairs[Random.Range(0, pairs.Count)].GetImage();
			}
			// Otherwise return null
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// A pairing between the ID and a background image for a location.
	/// </summary>
	[System.Serializable]
	public class LocationIDImagePair
	{
		// The ID of a location
		[SerializeField]
		private string locationID;
		// The background image of a location
		[SerializeField]
		private Sprite locationImage;

		/// <summary>
		/// Gets the ID for a location.
		/// </summary>
		/// <returns>The ID representing a location.</returns>
		public string GetLocationID()
		{
			return locationID;
		}
		
		/// <summary>
		/// Gets the background image for a location.
		/// </summary>
		/// <returns>The background image for a location.</returns>
		public Sprite GetImage()
		{
			return locationImage;
		}
	}
}
