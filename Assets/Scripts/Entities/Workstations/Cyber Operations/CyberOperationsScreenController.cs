using Entities.Workstations.CyberOperationsParts;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Systems.GameBrain;

public enum CyberOpsWindowState
{
    HomeScreen,
    MissionSelect,
    ChallengeSelect,
    ConfirmationWindow
}

public class CyberOperationsScreenController : MonoBehaviour
{
    public CyberOpsWindowState curState = CyberOpsWindowState.HomeScreen;

    public bool usingOldStructure = false;

    [SerializeField] private UIMissionVmScreenController missionVmScreenController;
    [SerializeField] private UIChallengeVmScreenController challengeVmScreenController;

    [SerializeField] private Button openMissionScreenButton;

    [SerializeField] private Button openConfirmationWindowButton;
    private ModalWindowContent confirmationScreenContent;
    private readonly string confirmationText = "Do you want to launch this VM in a new tab<br>or embedded into this page?";

    private void Awake()
    {
        ResetState();
    }

    public void ResetState()
    {
        missionVmScreenController.Deactivate();
        challengeVmScreenController.Deactivate();
        openMissionScreenButton.enabled = true;
    }

    private void SetState(CyberOpsWindowState newState)
    {
        switch (newState)
        {
            case CyberOpsWindowState.HomeScreen:
                ResetState();
                break;
            case CyberOpsWindowState.MissionSelect:
                openMissionScreenButton.enabled = false;
                missionVmScreenController.Activate();
                challengeVmScreenController.Deactivate();
                break;
            case CyberOpsWindowState.ChallengeSelect:
                openMissionScreenButton.enabled = false;
                missionVmScreenController.Deactivate();
                challengeVmScreenController.Activate();
                break;
            case CyberOpsWindowState.ConfirmationWindow:
                break;
        }
    }

    public void InitializeMissionScreen(MissionVMs[] missionVMs)
    {
        missionVmScreenController.InitializeButtons(missionVMs);
    }

    public void OpenConfirmationWindow(string vmName, string vmURL)
    {
        //confirmationScreenContent = new ModalWindowContent(vmName, confirmationText, "New Tab", "Embedded Window", OpenVMWindowNewTab, OpenVMWindowEmbedded, CloseConfirmationWindow);
    }

    public void OpenChallengeSelectScreen(MissionVMs vms)
    {
        SetState(CyberOpsWindowState.ChallengeSelect);
        challengeVmScreenController.Initialize(vms);
    }

    public void OpenMissionSelectScreen()
    {
        SetState(CyberOpsWindowState.MissionSelect);
    }

    public void OnClickHomeScreen()
    {
        if (usingOldStructure)
        {

        }
        else
        {

        }
    }

    public void OnMouseModelClick()
    {

    }
}
