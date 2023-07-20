using System;
using Entities.Workstations;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.HUD
{
	public class UICurrentWorkstationPowerStatusDisplay : Managers.ConnectedSingleton<UICurrentWorkstationPowerStatusDisplay>
	{
		[SerializeField] private TMP_Text _uiText;
		[SerializeField] private GameObject _textContainer;
		private Workstation _currentWorkstation;
		

		//wrapped up as static so we can remove the element without instantly breaking things, without messing with code elsewhere.
		public static void EnterWorkstation(Workstation workstation)
		{
			if (Instance != null)
			{
				Instance._currentWorkstation = workstation;
			}
		}

		public static void ExitWorkstation()
		{
			if (Instance != null)
			{
				Instance._currentWorkstation = null;
				Instance.UpdateWarningText();
			}
		}

		public override void Awake()
		{
			base.Awake();
			_textContainer.SetActive(false);
		}

		public void Update()
		{
			if (_currentWorkstation != null)
			{
				UpdateWarningText();
			}
		}

		private void UpdateWarningText()
		{
			if (_currentWorkstation == null)
			{
				_textContainer.SetActive(false);
				return;
			}

			_textContainer.SetActive(!_currentWorkstation.IsPowered );
			_uiText.text = $"{_currentWorkstation.name} does not have power. <sprite index=6>";
		}
	}
}