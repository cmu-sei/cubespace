using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using UnityEngine;
using Entities.Workstations.CyberOperationsParts;

public class UIChallengeVmScreenController : MonoBehaviour
{
    [SerializeField] private CyberOperations workstation;
    [SerializeField] private CyberOperationsScreenController controller;

    private void Awake()
    {
        Deactivate();
    }

    public void Activate()
    {

    }

    public void Deactivate()
    {

    }

    public void Initialize(MissionVMs vms)
    {

    }

    public void OnSelectVM(ChallengeVM vm)
    {
        //workstation.SetSelectedVM(vm);
        controller.OpenConfirmationWindow(vm.vmName, vm.vmURL);
    }
}
