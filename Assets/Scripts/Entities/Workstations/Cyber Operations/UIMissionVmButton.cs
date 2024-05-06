using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems.GameBrain;
using UnityEngine.EventSystems;

namespace Entities.Workstations.CyberOperationsParts
{
    /// <summary>
    /// A single button associated with a mission on the mission select screen of the cyber ops stations
    /// </summary>
    public class UIMissionVmButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            if (button == null)
            {
                button = gameObject.GetComponent<Button>();
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (vms != null && screenController != null)
            {
                screenController.SetBottomScreenText(vms.missionName);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (screenController != null)
            {
                screenController.SetBottomScreenText("");
            }
        }
    }
}

