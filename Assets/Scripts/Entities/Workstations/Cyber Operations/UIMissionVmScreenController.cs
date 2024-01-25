using UnityEngine;
using UnityEngine.UI;
using Entities.Workstations.CyberOperationsParts;
using Systems.GameBrain;
using Managers;

namespace Entities.Workstations.CyberOperationsParts
{
    public class UIMissionVmScreenController : MonoBehaviour
    {
        [SerializeField] private CyberOperationsScreenController controller;

        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject missionVmButtonPrefab;
        [SerializeField] private Transform missionButtonGridParent;
        [SerializeField] private IDToImageMap imageMap;

        private Image background;

        private void Awake()
        {
            background = GetComponent<Image>();
        }

        public void Activate()
        {
            panel.SetActive(true);
            background.enabled = true;
        }

        public void Deactivate()
        {
            panel.SetActive(false);
            background.enabled = false;
        }

        public void InitializeButtons(MissionVMs[] missionVMs)
        {
            DestroyButtons();

            if (missionVMs == null) return;

            for (int i = 0; i < missionVMs.Length; i++)
            {
                UIMissionVmButton button = Instantiate(missionVmButtonPrefab, missionButtonGridParent).GetComponent<UIMissionVmButton>();
                button.SetMissionVmButton(controller, missionVMs[i], imageMap.GetImage(missionVMs[i].missionIcon, false, "default_vm_image"));
            }
        }

        private void DestroyButtons()
        {
            foreach (Transform c in missionButtonGridParent.transform)
            {
                Destroy(c.gameObject);
            }
        }
    }
}

