/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using TMPro;
using UI.ColorPalettes;
using UI.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NavScreen.NavScreenComponents
{
	/// <summary>
	/// The button used within the destination display screen.
	/// </summary>
	public class DestinationDisplayButton : MonoBehaviour, IRefreshableUI
	{	
		/// <summary>
		/// Whether to show the button as selected.
		/// </summary>
		private bool showSelected;
		/// <summary>
		/// Whether to show the button as complete.
		/// </summary>
		private bool showComplete;
		
		//component references
		/// <summary>
		/// The button the user clicks.
		/// </summary>
		private Button button;
		/// <summary>
		/// The image whose color should be changed.
		/// </summary>
		private Image image;
		/// <summary>
		/// The text of this button.
		/// </summary>
		private TMP_Text buttonText;
		/// <summary>
		/// The destination display screen on the NavReader.
		/// </summary>
		private DestinationDisplayNavScreen destinationDisplayNavScreen;

		/// <summary>
		/// Gets the image, button, and text components, and adds a listener to the button when it is clicked.
		/// </summary>
		private void Awake()
		{
			image = GetComponent<Image>();
			button = GetComponent<Button>();
			buttonText = button.GetComponentInChildren<TMP_Text>();
			button.onClick.AddListener(OnButtonClick);
		}

		/// <summary>
		/// Sets the destination display screen on the NavReader to be the one provided.
		/// </summary>
		/// <param name="navScreen">The navigation screen on the NavReader.</param>
		public void SetDestinationDisplayNavScreen(DestinationDisplayNavScreen navScreen)
		{
			destinationDisplayNavScreen = navScreen;
		}

		/// <summary>
		/// Refreshes the display of this button when the screen is complete.
		/// </summary>
		/// <param name="isComplete">Whether this button's screen has been completed.</param>
		public void SetIsComplete(bool isComplete)
		{
			if (isComplete != showComplete)
			{
				showComplete = isComplete;
				RefreshDisplay();
			}
		}

		/// <summary>
		/// Refreshes the display of this button when the screen is selected.
		/// </summary>
		/// <param name="isSelected">Whether this button's screen is selected.</param>
		public void SetIsSelected(bool isSelected)
		{
			if (isSelected != showSelected)
			{
				showSelected = isSelected;
				RefreshDisplay();
			}
		}

		/// <summary>
		/// Refreshes the button color.
		/// </summary>
		public void RefreshDisplay()
		{
			if (showComplete)
			{
                image.color = ColorPalette.GetColor(PaletteColor.NavItemComplete);
            }
			else if (showSelected)
			{
                image.color = ColorPalette.GetColor(PaletteColor.NavButtonWarning);
            }
			else
			{
                image.color = ColorPalette.GetColor(PaletteColor.NavButtonBase);
            }
		}
		
		/// <summary>
		/// Makes this button the one selected on the display screen when clicked.
		/// </summary>
		private void OnButtonClick()
		{
			if (destinationDisplayNavScreen != null)
			{
				destinationDisplayNavScreen.ButtonSelected(this);
			}
			else
			{
				Debug.LogWarning("navreader Button not properly set up!",this);
			}
		}

		/// <summary>
		/// Whether to make the button interactable.
		/// </summary>
		/// <param name="interactable">Whether the button is interactable.</param>
		public void SetInteractable(bool interactable)
		{
			button.interactable = interactable;
		}

		/// <summary>
		/// Sets the text of the button.
		/// </summary>
		/// <param name="text">The text to give the button.</param>
		public void SetButtonText(string text)
		{
			buttonText.text = text;
		}
	}
}
