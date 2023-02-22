/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using Managers;
using Systems.GameBrain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Entities.Workstations;

namespace UI.NavScreen
{
	/// <summary>
	/// The NavScreen used for entering coordinates.
	/// </summary>
	public class CoordinateEntryNavScreen : NavScreen
	{
        #region Variables
        /// <summary>
        /// The back button.
        /// </summary>
        [Header("Setup")]
		[SerializeField]
		private Button backButton;
		/// <summary>
		/// The confirm button.
		/// </summary>
		[SerializeField]
		private Button manualConfirmButton;
		/// <summary>
		/// The field where coordinates are input.
		/// </summary>
		[SerializeField]
		private TMP_InputField inputField;
		/// <summary>
		/// The prettified input text.
		/// </summary>
		[SerializeField]
		private TMP_Text prettyDisplayText;
		/// <summary>
		/// The display used for fully correct text.
		/// </summary>
		[SerializeField]
		private GameObject correctTextDisplay;
		/// <summary>
		/// Whether to reset the input field when this screen first appears.
		/// </summary>
		[SerializeField]
		private bool resetInputOnShow = true;
		/// <summary>
		/// The NavReader workstation.
		/// </summary>
		[SerializeField]
		private NavReader navReader;

		/// <summary>
		/// The screen used to select a location.
		/// </summary>
		[Header("Configuration")]
		[SerializeField]
		private LocationSelectionNavScreen locationSelectionScreen;

		/// <summary>
		/// The time to wait between flashing text.
		/// </summary>
		[Header("Timing Durations")]
		[SerializeField]
		private float timeToWaitBetweenFlashing = 0.3f;
		/// <summary>
		/// The tiem to wait before switching between screens after a location is successfully entered.
		/// </summary>
		[SerializeField]
		private float timeToWaitBeforeSwitchingBackScreens = 1.0f;

		/// <summary>
		/// The coordinates entered by the user.
		/// </summary>
		private string enteredLocationCoords = "";
		/// <summary>
		/// Whether the screen should be displayed.
		/// </summary>
		private bool active = false;
		/// <summary>
		/// Whether the unlocked locations changed.
		/// </summary>
		private bool unlockedLocationsChanged = false;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that initializes the back button's listener and adds a listener to
		/// the input field.
        /// </summary>
        private void Awake()
		{
			backButton.onClick.AddListener(OnBackButtonClick);
			
			inputField.onSubmit.AddListener(OnManualConfirmButtonClick);
			prettyDisplayText.text = ToPrettyInput("");
		}

		/// <summary>
		/// Unity event function that selects the input field automatically if this is active.
		/// </summary>
        private void Update()
        {
			if (active && !inputField.isFocused)
            {
				inputField.Select();
            }
        }

		/// <summary>
		/// Unity event function that subscribes to location events on the ShipStateManager.
		/// </summary>
        private void OnEnable()
		{
			ShipStateManager.OnTryLocationUnlockResponse += OnLocationUnlocked;
			ShipStateManager.UnlockedLocationsChangedHook += OnUnlockedLocationsChanged;
		}

		/// <summary>
		/// Unity event function that unsubscribes from location events on the ShipStateManager.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnTryLocationUnlockResponse -= OnLocationUnlocked;
			ShipStateManager.UnlockedLocationsChangedHook -= OnUnlockedLocationsChanged;
		}
        #endregion

        #region Screen methods
        /// <summary>
        /// Shows this screen and selects the input field.
        /// </summary>
        public override void ShowScreen()
		{
			base.ShowScreen();

			if (resetInputOnShow)
			{
				inputField.text = "";
			}

			active = true;
			inputField.Select();
		}

		/// <summary>
		/// Hides this screen and marks it as inactive.
		/// </summary>
        public override void HideScreen()
        {
            base.HideScreen();

			active = false;
        }

		/// <summary>
		/// Resets this screen and the text entered to it.
		/// </summary>
        public override void ResetNavScreen()
		{
			base.ResetNavScreen();
			inputField.text = "";
			enteredLocationCoords = "";
			
			correctTextDisplay.SetActive(false);
		}

		/// <summary>
		/// Asks the controller to move back to the selection screen.
		/// </summary>
		private void ReturnToSelectionScreen()
		{
			correctTextDisplay.SetActive(false);
			_navScreenController.SetNavScreen(locationSelectionScreen);
		}
        #endregion

        #region Listener functions
        /// <summary>
        /// Listener function used by the back button to return to the selection screen.
        /// </summary>
        private void OnBackButtonClick()
		{
			ReturnToSelectionScreen();
		}

		/// <summary>
		/// Sets the entered location coordinates when text is entered.
		/// Used in the OnValueChanged callback on the text input field object.
		/// </summary>
		/// <param name="text">The text entered.</param>
		public void OnEnterText(string text)
		{
			prettyDisplayText.text = ToPrettyInput(text);
			enteredLocationCoords = text.ToUpper();
		}

		/// <summary>
		/// Acts as the proper method signature for inputField.onSubmit, so the user can press Return
		/// to check the input. This is also used on the NavReader confirm button.
		/// </summary>
		/// <param name="input">The text entered.</param>
		public void OnManualConfirmButtonClick(string input)
		{
			navReader.CmdTryUnlockLocation(enteredLocationCoords);
		}
        #endregion

        #region Callback event functions
		/// <summary>
		/// Starts the coroutine for an unlocked location, whether successful or failed.
		/// </summary>
		/// <param name="response">The LocationUnlockResponse received.</param>
        private void OnLocationUnlocked(LocationUnlockResponse response)
		{
			if (response.unlockResult == LocationUnlockResponse.UnlockResult.Success)
			{
				StartCoroutine(WaitForLocationUnlock(response.locationID));
			}
			else
			{
				ShowIncorrectEntryFeedback();
			}
		}

		/// <summary>
		/// Sets the unlocked locations changed status to true when the list of unlocked locations
		/// has changed.
		/// </summary>
		/// <param name="locations">The list of unlocked locations, which have just changed.</param>
		private void OnUnlockedLocationsChanged(Location[] locations)
		{
			unlockedLocationsChanged = true;
		}
		#endregion

		#region Static functions
		/// <summary>
		/// A static method that converts the provided input string to prettified text.
		/// </summary>
		/// <param name="input">The text input by the user.</param>
		/// <param name="total">The total number of characters in the string.</param>
		/// <returns>A prettified version of the input string.</returns>
		public static string ToPrettyInput(string input, int total = 6)
		{
			char underscore = '_';
			string output = input.Trim();
			
			if (input.Length > 6)
			{
				// Reduce the input to the total by trimming it if it's too long
				output = input.Substring(0, 6);
			}
			else
			{
				// Fill to the total with underscores
				for (int i = input.Length; i < total; i++)
				{
					{
						output += underscore;
					}
				}
			}

			// Insert the slash
			output = output.Insert(total / 2, "/");

			return output;
		}
        #endregion

        #region Location entry text
        /// <summary>
        /// Displays a failed location unlock attempt.
        /// </summary>
        private void ShowIncorrectEntryFeedback()
		{
			StartCoroutine(IncorrectEntryDisplay());
		}

		/// <summary>
		/// Displays the result for a failed location unlock request.
		/// </summary>
		/// <returns>A yield statement while waiting to switch from an error color to a normal color.</returns>
		private IEnumerator IncorrectEntryDisplay()
		{
			Color regularColor = prettyDisplayText.color;
			Color errorColor = Color.red;

			Audio.AudioPlayer.Instance.IncorrectCoordinates();

			prettyDisplayText.color = errorColor;
			yield return new WaitForSeconds(0.3f);
			prettyDisplayText.color = regularColor;
			inputField.text = "";
			inputField.Select();
		}

		/// <summary>
		/// Waits for the number of unlocked locations to change before displaying the text for
		/// unlocking a location.
		/// </summary>
		/// <param name="locationID">The ID of the location to attempt to unlock.</param>
		/// <returns>A yield statement while waiting for unlockedLocationsChanged to be true.</returns>
		private IEnumerator WaitForLocationUnlock(string locationID)
		{
			float maxTime = 60;
			float timeElapsed = 0f;

			// If the location is already unlocked, change the location
			if(ShipStateManager.Instance.unlockedLocations.Any(x => x.locationID == locationID))
			{
				// Set location to newly selected screen
				locationSelectionScreen.ChangeLocation(locationID);
				ReturnToSelectionScreen();
				yield break;
			}

			unlockedLocationsChanged = false;
			
			// Wait until unlockedLocationsChanged changes from the event hook firing
			while (timeElapsed < maxTime)
			{
				if (unlockedLocationsChanged)
				{
					if (ShipStateManager.Instance.unlockedLocations.Any(x => x.locationID == locationID))
					{
						locationSelectionScreen.ChangeLocation(locationID);
						StartCoroutine(CorrectEntryDisplay(ShipStateManager.Instance.GetLocation(locationID).name));
						yield break;
					}
					else
					{
						unlockedLocationsChanged = false;
						Debug.LogWarning("Locations changed while waiting but unlockedLocations still does not contain " + locationID);
					}
				}
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			Debug.LogWarning("unlocked locations change timed out");
			ReturnToSelectionScreen();
			yield return null;
		}

		/// <summary>
		/// Flashes text on a correct location entry.
		/// </summary>
		/// <param name="newLocation">The new location to display as unlocked.</param>
		/// <returns>A yield statement while waiting between flashing text.</returns>
		private IEnumerator CorrectEntryDisplay(string newLocation)
		{
			Color regularColor = prettyDisplayText.color;
			Color correctColor = Color.green;

			Audio.AudioPlayer.Instance.CorrectCoordinates();

			for (int i = 0; i < 3; i++)
			{
				prettyDisplayText.color = correctColor;
				yield return new WaitForSeconds(timeToWaitBetweenFlashing);
				prettyDisplayText.color = regularColor;
				yield return new WaitForSeconds(timeToWaitBetweenFlashing);
			}

			// Set Up the correct text
			correctTextDisplay.SetActive(true);
			newLocation = newLocation.Replace("<br>", " ");
			correctTextDisplay.GetComponent<TextMeshProUGUI>().text = "Location Unlocked:<br>"+newLocation;

			yield return new WaitForSeconds(timeToWaitBeforeSwitchingBackScreens);
			ReturnToSelectionScreen();

			inputField.text = "";
			inputField.Select();
		}
		#endregion
	}
}
