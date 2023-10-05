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

namespace UI.HUD
{
    /// <summary>
    /// The display class for a mission item in the HUD.
    /// </summary>
    public class UIHudMissionItem : MonoBehaviour
    {
        /// <summary>
        /// The map between missions and icons.
        /// </summary>
        [SerializeField]
        private IDToImageMap missionIconMap;
        /// <summary>
        /// The background of the selected mission.
        /// </summary>
        [SerializeField]
        private Image backgroundSelectedObj;
        /// <summary>
        /// The background of the mission when it has been fully completed.
        /// </summary>
        [SerializeField]
        private Image backgroundCompletedObj;

        /// <summary>
        /// The mission icon used.
        /// </summary>
        [SerializeField]
        private Image missionIcon;
        /// <summary>
        /// The background icon for a completed mission.
        /// </summary>
        [SerializeField]
        private Image missionIconCompletedBG;
        /// <summary>
        /// The text used for the title of a mission.
        /// </summary>
        [SerializeField]
        private TMP_Text titleText;
        /// <summary>
        /// The description of a mission.
        /// </summary>
        [SerializeField]
        private TMP_Text missionDescription;
        /// <summary>
        /// The centered position an item should have. Derives from a private variable.
        /// </summary>
        public Transform ItemCenterPosition => itemCenterPosition;

        /// <summary>
        /// The centered position an item should have.
        /// </summary>
        [SerializeField]
        private Transform itemCenterPosition;
        /// <summary>
        /// The button selecting this item.
        /// </summary>
        [SerializeField]
        private Button selectButton;
        /// <summary>
        /// The canvas group used to change visibility of this mission.
        /// </summary>
        [SerializeField]
        private CanvasGroup group;

        /// <summary>
        /// The time to fade the mission.
        /// </summary>
        private float fadeTime = 0.1f;
        /// <summary>
        /// The operation fading the mission.
        /// </summary>
        private Coroutine fadeRoutine;
        /// <summary>
        /// Whether to show this mission item as selected.
        /// </summary>
        private bool showAsSelected = false;

        /// <summary>
        /// The manager used to manage the children mission of the UI HUD.
        /// </summary>
        private UIHudMissionManager manager;

        /// <summary>
        /// The mission data associated with this item, cached in SetMissionData
        /// </summary>
        public MissionData CachedMissionData { get; private set; }

        /// <summary>
        /// Sets the information for this mission.
        /// </summary>
        /// <param name="data">The data of the mission.</param>
        public void SetMissionData(MissionData data)
        {
            group.interactable = true;
            group.alpha = 1;

            missionIcon.sprite = missionIconMap.GetImage(data.missionIcon);
            missionIconCompletedBG.enabled = data.complete;

            if (data.isSpecial && !showAsSelected)
            {
                backgroundSelectedObj.color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UISpecialMissionUnselected);
                backgroundSelectedObj.enabled = true;
            }
            else if (data.isSpecial && showAsSelected)
            {
                backgroundSelectedObj.color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UISpecialMissionSelected);
                backgroundSelectedObj.enabled = true;
            }
            else if (!data.isSpecial && showAsSelected)
            {
                backgroundSelectedObj.color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UIAccentColorOne);
                backgroundSelectedObj.enabled = true;
            }
            else
            {
                backgroundSelectedObj.enabled = false;
            }

            CachedMissionData = data;
            SetCompleted(data.complete);
            titleText.text = data.title;
            missionDescription.text = data.summaryShort;
        }

        /// <summary>
        /// Sets this mission to initially use default selection, completion, interaction, and visibility values.
        /// </summary>
        public void Awake()
        {
            SetSelected(false, true);
            SetCompleted(false);
            selectButton.onClick.AddListener(SelectButtonClick);
            manager = GetComponentInParent<UIHudMissionManager>();
            group.interactable = false;
            group.alpha = 0;
            titleText.text = "";
            missionDescription.text = "";
        }

        /// <summary>
        /// Marks the mission as complete or incomplete.
        /// </summary>
        /// <param name="isCompleted">Whether the mission is completed.</param>
        public void SetCompleted(bool isCompleted)
        {
            backgroundCompletedObj.enabled = isCompleted;
        }

        /// <summary>
        /// Changes the mission's visibility based on whether it is selected.
        /// </summary>
        /// <param name="isSelected">Whehter the mission is selected.</param>
        /// <param name="instant">Whether to instantly set this mission's visibility or fade it through a coroutine.</param>
        public void SetSelected(bool isSelected, bool instant = false)
        {
            if (!instant && !showAsSelected && isSelected)
            {
                StopFade();
                fadeRoutine = StartCoroutine(FadeImage(backgroundSelectedObj, 1, fadeTime));
            }
            else if (!instant && showAsSelected && !isSelected)
            {
                StopFade();
                fadeRoutine = StartCoroutine(FadeImage(backgroundSelectedObj, 0, fadeTime));
            }

            if (instant)
            {
                backgroundSelectedObj.enabled = isSelected;
            }

            if (isSelected)
            {
                selectButton.Select();
            }
            showAsSelected = isSelected;
        }

        /// <summary>
        /// Stops a fade process.
        /// </summary>
        void StopFade()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
        }

        /// <summary>
        /// Selects the mission from the mission manager.
        /// </summary>
        public void SelectButtonClick()
        {
            manager.SelectMission(this);
        }

        /// <summary>
        /// Gradually fades the image opacity.
        /// </summary>
        /// <param name="image">The image of the mission.</param>
        /// <param name="finalAlpha">The final opacity value this image should have.</param>
        /// <param name="timeToFade">The time to fade the mission out.</param>
        /// <returns>A yield statement while waiting for </returns>
        IEnumerator FadeImage(Image image, float finalAlpha, float timeToFade)
        {
            image.enabled = true;
            Color start = image.color;

            Color end;

            if (CachedMissionData != null && CachedMissionData.isSpecial && finalAlpha == 1)
            {
                end = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UISpecialMissionSelected);
            }
            else if (CachedMissionData != null && CachedMissionData.isSpecial && finalAlpha == 0)
            {
                end = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UISpecialMissionUnselected);
            }
            else
            {
                end = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UIAccentColorOne);
                end.a = finalAlpha;
            }

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / timeToFade;
                image.color = Color.Lerp(start, end, t);
                yield return null;
            }

            image.color = end;

            fadeRoutine = null;
        }
    }
}

