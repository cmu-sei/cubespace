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
using UI.ColorPalettes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace UI.HUD
{
	/// <summary>
	/// A definition for the button which toggles the display of the UI HUD.
	/// </summary>
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(Audio.ButtonAudio))]
	[RequireComponent(typeof(Button))]
	public class UIHudDisplayMenuButton : MonoBehaviour
	{
		
		
		/// <summary>
		/// An event firing when the controller is opened.
		/// </summary>
		[HideInInspector] public UnityEvent controllerOpenFunction;

		/// <summary>
		/// An event firing when the controller is closed.
		/// </summary>
		[HideInInspector] public UnityEvent controllerCloseFunction;

		/// <summary>
		/// The image used on the button.
		/// </summary>
		private Image _image;

		/// <summary>
		/// The button audio.
		/// </summary>
		private Audio.ButtonAudio _buttonAudio;

		/// <summary>
		/// The button used.
		/// </summary>
		private Button _button;

		[SerializeField] private HUDController.MenuState menuState;
		
		/// <summary>
		/// The palette color to use on this button while the HUD is active.
		/// </summary>
		[SerializeField] private PaletteColor onColor;

		/// <summary>
		/// The palette color to use on this button while the HUD is inactive.
		/// </summary>
		[SerializeField] private PaletteColor offColor;

		/// <summary>
		/// The full color palette.
		/// </summary>
		[SerializeField] private ColorPalette palette;


		/// <summary>
		/// The text used when the HUD display can be toggled off.
		/// Leave empty to disable toggle text behaviour.
		/// </summary>
		[Tooltip("When left empty, the text will not change.")]
		[SerializeField] private string toggledText;

		/// <summary>
		/// The text used when the HUD display can be toggled on.
		/// </summary>
		private string normalText;

		/// <summary>
		/// Whether to use different text when the button is toggled.
		/// </summary>
		private bool isDifferentTextWhenToggled;

		/// <summary>
		/// The text on the button.
		/// </summary>
		private TextMeshProUGUI buttonText;

		/// <summary>
		/// The UI component of the mission log button that flashes if the player has not clicked it yet.
		/// </summary>
		private FlashBox _flashBox;
		

		/// <summary>
		/// Unity event function that primarily grabs references to different UI objects and subscribes the button to the OnClick function.
		/// </summary>
		private void Awake()
		{
			_image = GetComponent<Image>();
			_buttonAudio = GetComponent<Audio.ButtonAudio>();
			_button = GetComponent<Button>();
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OnClick);
			_flashBox = GetComponent<FlashBox>();
			
			if (_button.GetComponentInChildren<TextMeshProUGUI>() != null)
			{
				buttonText = _button.GetComponentInChildren<TextMeshProUGUI>();
				isDifferentTextWhenToggled = !string.IsNullOrEmpty(toggledText);
				normalText = buttonText.text;
			}

			_buttonAudio.activated = false;
			_image.color = palette.GetPaletteColor(offColor);
		}

		private void OnEnable()
		{
			HUDController.Instance.OnMenuStateChange += OnMenuStateChange;
		}

		private void OnDisable()
		{
			HUDController.Instance.OnMenuStateChange -= OnMenuStateChange;
		}

		public void SetVisuals(bool open)
		{
			if (open)
			{
				_image.color = palette.GetPaletteColor(onColor);
				// Swap the text back to normal
				if (isDifferentTextWhenToggled)
				{
					buttonText.text = normalText;
				}
			}
			else
			{
				_image.color = palette.GetPaletteColor(offColor);
				// Swap the text to the toggled text
				if (isDifferentTextWhenToggled)
				{
					buttonText.text = toggledText;
				}
			}
		}

		public void OnMenuStateChange(HUDController.MenuState state)
		{
			SetVisuals(state != menuState);
			_buttonAudio.activated = false;
		}

		/// <summary>
		/// Function that swaps text and the panel state when opened.
		/// </summary>
		public void OnClick()
		{
			//play audio on click or just open
			_buttonAudio.activated = true;

			//open
			HUDController.Instance.ToggleMenuState(menuState);

			//stop mission log button from flashing.
			if (_flashBox != null)
			{
				_flashBox.stopFlashing = true;
			}
		}
	}
}

