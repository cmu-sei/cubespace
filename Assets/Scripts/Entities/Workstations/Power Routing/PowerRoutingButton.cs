/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.ColorPalettes;

namespace Entities.Workstations.PowerRouting
{
	/// <summary>
	/// A component defining button behavior on the power routing workstation.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class PowerRoutingButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
	{
		#region Variables
		/// <summary>
		/// The WorkstationID of the workstation this button should power.
		/// </summary>
		[SerializeField]
		private WorkstationID workstationToPower;
		/// <summary>
		/// The PowerRouting component of the PowerRouting workstation.
		/// </summary>
		[SerializeField]
		private PowerRouting _powerRouting;
		/// <summary>
		/// The pipe associated with this button, whose emission powers on/off.
		/// </summary>
		[SerializeField]
		private WorkstationPipe pipe;

		/// <summary>
		/// A public reference to the workstation this button powers.
		/// </summary>
		public WorkstationID WorkstationID => workstationToPower;

		/// <summary>
		/// The UI button object the client clicks.
		/// </summary>
		private Button _button;
		/// <summary>
		/// The line connecting this button to the center of the PowerRouting screen.
		/// </summary>
		private Image _connectionLineImage;
		/// <summary>
		/// The button image used.
		/// </summary>
		private Image _buttonImage;
        #endregion

        #region Unity event functions
		/// <summary>
		/// Unity event function that gets references to UI objects and adds a listener to the button.
		/// </summary>
        private void Awake()
		{
			_buttonImage = GetComponent<Image>();
			_button = GetComponent<Button>();
			_button.onClick.AddListener(OnButtonClick);
			
			_connectionLineImage = _button.transform.GetChild(0).GetComponent<Image>();
		}

		/// <summary>
		/// Plays a mouseover sound effect when the button is hovered over.
		/// </summary>
		/// <param name="pointerEventData"></param>
		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			Audio.AudioPlayer.Instance.UIMouseover(0, transform);
		}

		/// <summary>
		/// Plays a sound effect when the button is clicked.
		/// </summary>
		/// <param name="pointerEventData">The event data that occurs when this button is clicked.</param>
		public void OnPointerClick(PointerEventData pointerEventData)
		{
			if (_powerRouting.GetPowerStateForWorkstation(workstationToPower))
			{
				Audio.AudioPlayer.Instance.UIExit(0, transform);
			}
			else
			{
				if (_powerRouting.PowerIsAvailable())
				{
					Audio.AudioPlayer.Instance.UISelect(0, transform);
				}
				else
				{
					Audio.AudioPlayer.Instance.UIError(transform);
				}
			}
		}
        #endregion

        #region Main methods
        /// <summary>
        /// Toggles the power state on PowerRouting when this button is clicked.
        /// </summary>
        public void OnButtonClick()
		{
			_powerRouting.TogglePowerState(workstationToPower);
		}

		/// <summary>
		/// Switches the UI appearance of an object when this button is clicked.
		/// </summary>
		/// <param name="workstation">The workstation being powered or unpowered.</param>
		/// <param name="isPowered">Whether the workstation targeted by this button is powered.</param>
		public void TogglePower(Workstation workstation,bool isPowered)
		{
			if (isPowered)
            {
                if (workstation.UsedInLaunchMode)
                {
                    _buttonImage.color = ColorPalette.GetColor(PaletteColor.LaunchModePowered);
                    _connectionLineImage.color = ColorPalette.GetColor(PaletteColor.Powered);
                }
                else if (workstation.UsedInExplorationMode)
                {
                    _buttonImage.color = ColorPalette.GetColor(PaletteColor.ExplorationModePowered);
                    _connectionLineImage.color = ColorPalette.GetColor(PaletteColor.Powered);
                }
                else
                {
                    Debug.LogError("Tried to toggle power to a station that isn't used in launch mode or exploration mode");
                    _buttonImage.color = ColorPalette.GetColor(PaletteColor.Powered);
                    _connectionLineImage.color = ColorPalette.GetColor(PaletteColor.Powered);
                }
                
				pipe.SetEmissionPower(WorkstationPipe.ON_EMISSION_POWER);
                Audio.AudioPlayer.Instance.PowerRoutingTubeOn(workstationToPower, pipe.transform);
            }
            else
            {
                _buttonImage.color = ColorPalette.GetColor(PaletteColor.Unpowered);
                _connectionLineImage.color = ColorPalette.GetColor(PaletteColor.Unpowered);
                pipe.SetEmissionPower(WorkstationPipe.OFF_EMISSION_POWER);
                Audio.AudioPlayer.Instance.PowerRoutingTubeOff(workstationToPower);
            }
		}

		/// <summary>
		/// General function called when power cannot be set. No logic included.
		/// </summary>
		public void OnPowerFail() {
			// Placeholder for VFX, SFX not here because logic is separate to prevent firing on enter
		}
		#endregion
	}
}
