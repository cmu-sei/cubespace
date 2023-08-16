using System;
using System.Linq;
using UnityEngine;

namespace UI.HUD
{
	public class UIHudMenuPanelActiveWithState : MonoBehaviour
	{
		[Tooltip("Menu States to activate this gameObject with")]
		public HUDController.MenuState[] _states;

		public void Awake()
		{
			HUDController.Instance.OnMenuStateChange += OnMenuStateChange;
		}

		private void OnMenuStateChange(HUDController.MenuState state)
		{
			bool active = _states.Contains(state);
			gameObject.SetActive(active);
		}

		public void OnDestroy()
		{
			HUDController.Instance.OnMenuStateChange -= OnMenuStateChange;

		}
	}
}