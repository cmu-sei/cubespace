/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Customization
{
	/// <summary>
	/// Class that stores information for local player customization.
	/// </summary>
	public class PlayerCustomization : NetworkBehaviour
	{
		// The Scriptable Object storing all customization options
		[SerializeField]
		private CustomizationOptions options;
		// The color selected by the client; uses magenta as a default
		[SyncVar(hook = nameof(SetColorChoice))]
		private ColorChoice colorChoice = new ColorChoice();
		// The icon selected by the client
		[SyncVar(hook = nameof(SetIconChoice))]
		private string iconChoice = "";
		
		// The MeshRenderer on the player object (the main peg body)
		[SerializeField]
		private Renderer _playerColorRenderer;
		// The particle system circling the player
		[SerializeField]
		private ParticleSystem _playerParticleSystem;
		// The UI icon used on the player object (the icon at the base of the peg)
		[SerializeField]
		private Image _playerIconDisplayImage;

		/// <summary>
		/// Sets the color and icon selected in the offline scene on the player object.
		/// Mirror's docs state that the state of SyncVars is applied to game objects on clients before OnStartClient is called.
		/// Therefore, the state of the object is always up-to-date inside OnStartClient.
		/// </summary>
		public override void OnStartClient()
		{
			// Synchronize the variable for this player by sending commands
			if (isLocalPlayer)
			{
				CmdSetColorChoice(options.GetLocalSelectedColorChoice());
				CmdSetIconChoice(options.GetLocalSelectedIconChoice().GetID());
			}

			// Update the color and icon on this client to match the selections made
			UpdateColor(colorChoice);
			UpdateIcon(iconChoice);
		}

		#region Commands
		/// <summary>
		/// Command to change the player's color on the server, so it populates across all clients.
		/// </summary>
		/// <param name="color">The color to set the player's color to on the server.</param>
		[Command]
		public void CmdSetColorChoice(ColorChoice color)
		{
			colorChoice = color;
		}
		
		/// <summary>
		/// Command to change the player's icon on the server, so it populates across all clients.
		/// </summary>
		/// <param name="icon">The icon to set the player's icon to on the server.</param>
		[Command] 
		private void CmdSetIconChoice(string icon)
		{
			iconChoice = icon;
		}
        #endregion

        #region SyncVar Hooks
		/// <summary>
		/// Callback to set the player's color.
		/// </summary>
		/// <param name="oldColor">The player's previous color. Unused, but necessary.</param>
		/// <param name="newColor">The player's new color.</param>
        private void SetColorChoice(ColorChoice oldColor, ColorChoice newColor)
		{
			UpdateColor(newColor);
		}

		/// <summary>
		/// Callback to set the player's icon.
		/// </summary>
		/// <param name="oldIcon">The player's previous icon. Unused, but necessary.</param>
		/// <param name="newIcon">The player's new icon.</param>
		private void SetIconChoice(string oldIcon, string newIcon)
		{
			UpdateIcon(newIcon);
		}
        #endregion

		/// <summary>
		/// Sets the color displayed on the player to the given color.
		/// </summary>
		/// <param name="color"></param>
        private void UpdateColor(ColorChoice color)
		{
			// Set the color of the player's body's renderer and the player's particle system
			_playerColorRenderer.material.color = color.color;

			// Note that the main module is a struct, but this still works
			var module = _playerParticleSystem.main;
			module.startColor = color.color;
		}

		/// <summary>
		/// Sets the icon displayed on the player to the given icon.
		/// </summary>
		/// <param name="iconID">The ID of the icon to set the player's icon sprite to</param>
		private void UpdateIcon(string iconID)
		{
			// If the icon exists as an option, set it
			if (options.GetIconFromID(iconID, out var icon))
			{
				_playerIconDisplayImage.enabled = true;
				_playerIconDisplayImage.sprite = icon.Icon;
			}
			// Otherwise, just don't display the icon
			else
			{
				_playerIconDisplayImage.enabled = false;
				
			}
		}
	}
}
