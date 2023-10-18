using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities.Workstations.CyberOperationsParts;
using UnityEngine.UI;
using TMPro;
using Systems.GameBrain;

namespace Entities.Workstations.CyberOperationsParts
{
    public class UIMissionVmButton : MonoBehaviour
    {
        private MissionVMs vms;
        CyberOperationsScreenController screenController;

        [SerializeField] private Image missionIcon;
        [SerializeField] private TextMeshProUGUI missionTitleText;
        private Button button;

        [SerializeField] private Sprite defaultSprite;

        public void SetMissionVmButton(CyberOperationsScreenController controller, MissionVMs missionVMs, Sprite missionIconSprite)
        {
            if (missionVMs == null || controller == null) 
            {
                Destroy(gameObject);
                return;
            }

            vms = missionVMs;
            screenController = controller;

            if (missionIconSprite == null)
            {
                missionIcon.sprite = defaultSprite;
            }
            else
            {
                missionIcon.sprite = missionIconSprite;
            }
            missionTitleText.text = missionVMs.missionName;

            button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OpenChallengeSelectScreen);
        }

        public void OpenChallengeSelectScreen()
        {
            if (screenController == null || vms == null)
            {
                return;
            }
            screenController.OpenChallengeSelectScreen(vms);
        }
    }
}

