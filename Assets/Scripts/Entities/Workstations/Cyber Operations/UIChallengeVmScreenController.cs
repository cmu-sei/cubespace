using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using UnityEngine;
using Entities.Workstations.CyberOperationsParts;

public class UIChallengeVmScreenController : MonoBehaviour
{
    [SerializeField] private CyberOperations workstation;
    [SerializeField] private CyberOperationsScreenController controller;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform listParent;

    [SerializeField] private GameObject buttonPrefab;

    private void Awake()
    {
        Deactivate();
    }

    public void Activate()
    {
        panel.SetActive(true);
    }

    public void Deactivate()
    {
        panel.SetActive(false);
    }

    public void Initialize(MissionVMs missionVMs)
    {
        DestroyButtons();

        if (missionVMs == null || missionVMs.vmURLs == null || missionVMs.vmURLs.Length == 0) return;

        for (int i = 0; i < missionVMs.vmURLs.Length; i++)
        {
            UIChallengeVmButton button = Instantiate(buttonPrefab, listParent).GetComponent<UIChallengeVmButton>();
            button.SetChallengeVmButton(controller, missionVMs.vmURLs[i]);
        }
    }

    private void DestroyButtons()
    {
        foreach (Transform c in listParent.transform)
        {
            Destroy(c.gameObject);
        }
    }
}
