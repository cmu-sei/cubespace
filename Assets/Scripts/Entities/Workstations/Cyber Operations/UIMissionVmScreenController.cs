using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public void Activate()
        {
            missionButtonGridParent.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            DestroyButtons();
            missionButtonGridParent.gameObject.SetActive(false);
        }

        public void InitializeButtons(MissionVMs[] missionVMs)
        {
            DestroyButtons();

            if (missionVMs == null) return;

            for (int i = 0; i < missionVMs.Length; i++)
            {
                UIMissionVmButton button = Instantiate(missionVmButtonPrefab, missionButtonGridParent).GetComponent<UIMissionVmButton>();

                // TODO: This isn't very pretty or efficient, we aught to have a missionID->missionIcon map
                MissionData missionData = ShipStateManager.Instance.MissionDatas.Find((m) => { return m.missionID == missionVMs[i].missionID; });

                if (missionData == null)
                {
                    Debug.LogWarning("Creating vm button for a mission that can't be found in local array! Icon will be a default");
                    button.SetMissionVmButton(controller, missionVMs[i], null);
                }
                else
                {
                    button.SetMissionVmButton(controller, missionVMs[i], imageMap.GetImage(missionData.missionIcon, "default_vm_image"));
                }    
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

