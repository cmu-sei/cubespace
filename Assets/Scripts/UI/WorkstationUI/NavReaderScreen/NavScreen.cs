/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UI.UIInterfaces;
using UnityEngine;

namespace UI.NavScreen
{
	/// <summary>
	/// A general class for a navigation screen.
	/// </summary>
	public class NavScreen : MonoBehaviour
	{
		/// <summary>
		/// The controller dictating movement between screens.
		/// </summary>
		protected NavScreenController _navScreenController;
		/// <summary>
		/// Whether this nav screen is currently being shown.
		/// </summary>
		private bool currentlyShown = false;
		/// <summary>
		/// Whether to hide this nav screen on startup.
		/// </summary>
		public bool hideOnStart = true;

		/// <summary>
		/// Unity event function that hides this screen if it is the current navigation screen.
		/// </summary>
		protected virtual void Start()
		{
			if (!_navScreenController.IsCurrentNavScreen(this))
			{
				HideScreen();
			}
		}

		/// <summary>
		/// Shows the screen by making it active and refreshing the display.
		/// </summary>
		public virtual void ShowScreen()
		{
			gameObject.SetActive(true);
			
			if (this is IRefreshableUI refreshableUI)
			{
				refreshableUI.RefreshDisplay();
			}

			if (currentlyShown)
			{
				return;
			}
			
			foreach (Transform child in transform)
			{
				transform.gameObject.SetActive(true);
			}
			
			currentlyShown = true;
		}

		/// <summary>
		/// Hides this screen.
		/// </summary>
		public virtual void HideScreen()
		{
			foreach (Transform child in transform)
			{
				transform.gameObject.SetActive(false);
			}

			currentlyShown = false;
		}

		/// <summary>
		/// Performs logic when resetting the navigation screen.
		/// </summary>
		public virtual void ResetNavScreen()
		{
			// No logic needed; override this method
		}

		/// <summary>
		/// Sets the navigation controller.
		/// </summary>
		/// <param name="controller">The controller used to move between screens.</param>
		public void SetController(NavScreenController controller)
		{
			_navScreenController = controller;
		}
	}
}
