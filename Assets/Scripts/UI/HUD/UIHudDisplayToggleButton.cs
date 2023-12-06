/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UI.ColorPalettes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace UI.HUD
{
	/// <summary>
	/// A definition for the button which toggles the display of the UI HUD.
	/// This has been mostly but not entirely replaced by UIHudDisplayMenuButton during a refactor.
	/// </summary>
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(Audio.ButtonAudio))]
	[RequireComponent(typeof(Button))]
	public class UIHudDisplayToggleButton : MonoBehaviour
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
		/// </summary>
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
		/// If this button only closes the panel.
		/// </summary>
		[SerializeField] private bool closeOnly = false;

		/// <summary>
		/// Whether the HUD panel is already open.
		/// </summary>
		private bool panelOpen;

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

			if (_button.GetComponentInChildren<TextMeshProUGUI>() != null)
			{
				buttonText = _button.GetComponentInChildren<TextMeshProUGUI>();
				isDifferentTextWhenToggled = !string.IsNullOrEmpty(toggledText);
				normalText = buttonText.text;
			}

			panelOpen = closeOnly;
			_buttonAudio.activated = false;
			_image.color = palette.GetPaletteColor(offColor);
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

		/// <summary>
		/// Call this if the panel has changed state not by the onclick function, so this button will update it's state to match.
		/// Like a button click but without calling the function.
		/// </summary>
		public void OnPanelUpdatedOverride(bool open)
		{
			panelOpen = open;
			SetVisuals(open);
			_buttonAudio.activated = open;
		}

		/// <summary>
		/// Function that swaps text and the panel state when opened.
		/// </summary>
		public void OnClick()
		{
			if (panelOpen || closeOnly)
			{
				SetVisuals(true);
				controllerCloseFunction?.Invoke();
				panelOpen = false;
				_buttonAudio.activated = false;
			}
			else
			{
				SetVisuals(false);
				controllerOpenFunction?.Invoke();
				panelOpen = true;
				_buttonAudio.activated = true;
			}
		}
	}
}

