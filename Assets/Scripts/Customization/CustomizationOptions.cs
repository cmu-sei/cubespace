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

namespace Customization
{
	/// <summary>
	/// The ScriptableObject defining all customization options available to the player on the offline scene.
	/// </summary>
	[CreateAssetMenu(fileName = "Customization Options", menuName = "Game Data/Customization Options", order = 0)]
	public class CustomizationOptions : ScriptableObject
	{
		// The identifiers for the local color and icon choices
		private const string PlayerColorPrefID = "PlayerColorSelection";
		private const string PlayerIconPrefID = "PlayerIconSelection";

		// The local color and icon choices made by the client
		private ColorChoice localSelectedColorChoice;
		private IconChoice localSelectedIconChoice;
		// An icon map for quick icon lookup
		private Dictionary<string, IconChoice> _iconMap = new Dictionary<string, IconChoice>();

		// The list of possible color and icon choices
		[SerializeField] private ColorChoice[] _colorChoices;
		[SerializeField] private IconChoice[] _iconChoices;

		/// <summary>
		/// Instantiates the icon dictionary.
		/// </summary>
		public void InitiateIconMap()
		{
			// Clear the icon map
			_iconMap.Clear();
			// Loop through all icons within the list of possibilities and add them to the dictionary
			foreach (var icon in _iconChoices)
			{
				if (!_iconMap.ContainsKey(icon.GetID()))
				{
					_iconMap.Add(icon.GetID(), icon);
				}
			}
		}

		/// <summary>
		/// Gets all possible color choices.
		/// </summary>
		/// <returns>The array of all possible color choices.</returns>
		public ColorChoice[] GetColorChoices()
		{
			return _colorChoices;
		}

		/// <summary>
		/// Gets all possible icon choices.
		/// </summary>
		/// <returns>The array of all possible icon choices.</returns>
		public IconChoice[] GetIconChoices()
		{
			return _iconChoices;
		}

		/// <summary>
		/// Sets a player color and stores it in the local PlayerPrefs.
		/// </summary>
		/// <param name="colorChoice">The color selected.</param>
		public void ChooseLocalColor(ColorChoice colorChoice)
		{
			// Set the color chosen by the player
			localSelectedColorChoice = colorChoice;
			// Store the color in the PlayerPrefs so it can be accessed later
			PlayerPrefs.SetString(PlayerColorPrefID,colorChoice.colorName);
		}

		/// <summary>
		/// Sets a player icon and stores it in the local PlayerPrefs.
		/// </summary>
		/// <param name="icon">The icon selected.</param>
		public void ChooseLocalIcon(IconChoice icon)
		{
			// Set the icon chosen by the player
			localSelectedIconChoice = icon;
			// Store the icon in the PlayerPrefs so it can be accessed later
			PlayerPrefs.SetString(PlayerIconPrefID, icon.GetID());
		}

		/// <summary>
		/// Gets the color chosen by the player.
		/// </summary>
		/// <returns>The color chosen by the local client.</returns>
		public ColorChoice GetLocalSelectedColorChoice()
		{
			return localSelectedColorChoice;
		}

		/// <summary>
		/// Gets the icon chosen by the player.
		/// </summary>
		/// <returns>The icon chosen by the local client.</returns>
		public IconChoice GetLocalSelectedIconChoice()
		{
			return localSelectedIconChoice;
		}

		/// <summary>
		/// Looks up a given icon ID for quick retrieval.
		/// </summary>
		/// <param name="iconID">The ID of the icon to look up.</param>
		/// <param name="icon">The icon to return. This is an out parameter.</param>
		/// <returns>Whether the icon exists. Also returns the icon if it exists.</returns>
		public bool GetIconFromID(string iconID, out IconChoice icon)
		{
			// If by chance there's no icon map, initiate it
			if (_iconMap.Count == 0)
			{
				InitiateIconMap();
			}
			
			// Retrieve the value
			return _iconMap.TryGetValue(iconID, out icon);
		}
	}
}
