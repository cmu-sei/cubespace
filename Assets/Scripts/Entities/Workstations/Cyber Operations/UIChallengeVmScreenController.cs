using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using UnityEngine;
using Entities.Workstations.CyberOperationsParts;
using UnityEngine.UI;

public class UIChallengeVmScreenController : MonoBehaviour
{
    [SerializeField] private CyberOperations workstation;
    [SerializeField] private CyberOperationsScreenController controller;

    [SerializeField] private GameObject panel;
    private Image background;
    [SerializeField] private Transform listParent;

    [SerializeField] private GameObject buttonPrefab;

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
