using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengeVmButton : MonoBehaviour
{
    private ChallengeVM vm;
    CyberOperationsScreenController screenController;

    [SerializeField] private TextMeshProUGUI buttonText;
    private Button button;

    public void SetChallengeVmButton(CyberOperationsScreenController controller, ChallengeVM challengeVM)
    {
        if (challengeVM == null || controller == null)
        {
            Destroy(gameObject);
            return;
        }
        if (button == null)
        {
            button = gameObject.GetComponent<Button>();
        }

        vm = challengeVM;
        screenController = controller;

        buttonText.text = vm.vmName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenConfirmationWindow);
    }

    public void OpenConfirmationWindow()
    {
        if (screenController == null || vm == null)
        {
            return;
        }
        screenController.OpenConfirmationWindow(vm.vmName, vm.vmURL);
    }
}
