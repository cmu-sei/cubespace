/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using NaughtyAttributes;
using System;
using Systems.GameBrain;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.HUD
{
	/// <summary>
	/// The pog used for a mission.
	/// </summary>
	public class UIMissionPog : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		/// <summary>
		/// The sprite to show when the mission is completed.
		/// </summary>
		public Sprite completedBgSprite;
		/// <summary>
		/// The sprite to show when the mission is incomplete.
		/// </summary>
		public Sprite emptyBgSprite;
		/// <summary>
		/// The image used to show the sprites.
		/// </summary>
		[SerializeField]
		private Image _iconImage;
		/// <summary>
		/// The background image which uses the background sprites.
		/// </summary>
		[SerializeField]
		private Image _bgImage;

		private int _missionIndex = -1;

		[Header("Tooltip")]
        /// <summary>
        /// whether or not tooltips should be shown for this pog when moused over. False if the pog is empty and displayEmptyPogs is true in container
        /// </summary>
        private bool _shouldShowTooltip = false;
        /// <summary>
        /// whether or not the tooltip is being displayed right now 
        /// </summary>
        private bool _tooltipShowing = false;

        [SerializeField]
        private GameObject _tooltipObject;
		[SerializeField]
		private TextMeshProUGUI _tooltipTitleText;
        [SerializeField]
        private TextMeshProUGUI _tooltipStatusText;

        public void UpdatePog(bool missionComplete, int missionScore, Sprite missionIcon, string missionName, int missionIndex)
        {
			_tooltipTitleText.text = missionName; 
            if (!missionComplete && missionScore == 0)
            {
                _tooltipStatusText.text = "Incomplete";
            }
            else if (!missionComplete && missionScore > 0)
            {
                _tooltipStatusText.text = "Partially completed";
            }
            else if (missionComplete)
            {
                _tooltipStatusText.text = "Completed";
            }

			_missionIndex = missionIndex;

            SetSprite(missionIcon);
			_shouldShowTooltip = true;
        }

        /// <summary>
        /// Sets the sprite used in the icon image and the background sprite.
        /// </summary>
        /// <param name="s">The sprite to use for the icon.</param>
        private void SetSprite(Sprite s)
		{
			_iconImage.sprite = s;
			_iconImage.enabled = true;
			_bgImage.sprite = completedBgSprite;
			_bgImage.enabled = true;
        }

		/// <summary>
		/// Clears the appearance of the background sprite.
		/// </summary>
		public void SetEmpty()
        {
			_iconImage.enabled = false;
			_bgImage.sprite = emptyBgSprite;
			_bgImage.enabled = true;

            _shouldShowTooltip = false;
			DisableTooltip();
        }

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
            if (_shouldShowTooltip && !_tooltipShowing)
            {
                EnableTooltip();
            }
        }

		public void OnPointerExit(PointerEventData pointerEventData)
		{
            if (_tooltipShowing)
            {
                DisableTooltip();
            }
        }

		public void OnPointerClick(PointerEventData pointerEventData)
		{
			if (_shouldShowTooltip && _missionIndex >= 0 && HUDController.Instance && UIHudMissionManager.Instance)
			{
                HUDController.Instance.OpenMissionLog();
                UIHudMissionManager.Instance.SelectMission(_missionIndex, true);
				DisableTooltip();
            }
        }

		private void EnableTooltip()
		{
			_tooltipObject.SetActive(true);
			_tooltipShowing = true;
		}

		private void DisableTooltip()
		{
            _tooltipObject.SetActive(false);
            _tooltipShowing = false;
		}
	}
}

