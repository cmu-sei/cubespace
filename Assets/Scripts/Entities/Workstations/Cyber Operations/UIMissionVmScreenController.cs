using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private GameObject missionVmButtonPrefab;
        [SerializeField] private Transform missionButtonGridParent;
        [SerializeField] private IDToImageMap imageMap;

        [SerializeField] private Image background;

        private void Awake()
        {
            background = GetComponent<Image>();
        }

        public void Activate()
        {
            missionButtonGridParent.gameObject.SetActive(true);
            background.enabled = true;
        }

        public void Deactivate()
        {
            missionButtonGridParent.gameObject.SetActive(false);
            background.enabled = false;
        }

        public void InitializeButtons(MissionVMs[] missionVMs)
        {
            DestroyButtons();

            if (missionVMs == null) return;

            Debug.Log("Creating " + missionVMs.Length + " buttons");
            for (int i = 0; i < missionVMs.Length; i++)
            {
                UIMissionVmButton button = Instantiate(missionVmButtonPrefab, missionButtonGridParent).GetComponent<UIMissionVmButton>();
                button.SetMissionVmButton(controller, missionVMs[i], imageMap.GetImage(missionVMs[i].missionIcon, "default_vm_image"));
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

