using UnityEngine;
using UnityEngine.UI;
using UI;
using Systems.GameBrain;
using TMPro;

namespace Entities.Workstations.CyberOperationsParts
{
    public enum CyberOpsWindowState
    {
        HomeScreen, // Default screen when entering cyber ops station
        MissionSelect, // Screen for selecting between different missions available at given location
        ChallengeSelect // Screen for selecting a specific VM associated with a mission (usually one of X duplicates, X = # of players on team)
    }

    /// <summary>
    /// Responsible for the state of the UI at the cyber ops stations
    /// </summary>
    public class CyberOperationsScreenController : MonoBehaviour
    {
        private CyberOpsWindowState curState = CyberOpsWindowState.HomeScreen;

        public bool usingOldStructure = false;

        [SerializeField] private CyberOperations workstation;
        [SerializeField] private UIMissionVmScreenController missionVmScreenController;
        [SerializeField] private UIChallengeVmScreenController challengeVmScreenController;
        [SerializeField] private Button homeScreenButton;
        [SerializeField] private Button backButton;

        [SerializeField] private GameObject bottomScreenPanel;
        [SerializeField] private TextMeshProUGUI bottomScreenText;

        private ModalWindowContent confirmationScreenContent;
        private readonly string confirmationText = "Do you want to launch this VM in a new tab<br>or embedded into this page?";

        private void Start()
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButton);
            homeScreenButton.onClick.RemoveAllListeners();
            homeScreenButton.onClick.AddListener(OnClickHomeScreen);
            ResetState();
        }

        public void ResetState()
        {
            SetState(CyberOpsWindowState.HomeScreen);
        }

        // Destroys and creates buttons for each VM at each cyber ops station everytime new data comes in from game brain
        // TODO: This should only happen when the VMs being sent change and only the relevent buttons should get touched
        public void OnShipDataChanged(ShipData data)
        {
            missionVmScreenController.InitializeButtons(data.challengeURLs);
        }

        private void SetState(CyberOpsWindowState newState)
        {
            switch (newState)
            {
                case CyberOpsWindowState.HomeScreen:
                    missionVmScreenController.Deactivate();
                    challengeVmScreenController.Deactivate();
                    homeScreenButton.interactable = true;
                    backButton.gameObject.SetActive(false);
                    bottomScreenPanel.SetActive(false);
                    break;
                case CyberOpsWindowState.MissionSelect:
                    homeScreenButton.interactable = false;
                    missionVmScreenController.Activate();
                    challengeVmScreenController.Deactivate();
                    backButton.gameObject.SetActive(true);
                    bottomScreenPanel.SetActive(true);
                    SetBottomScreenText("");
                    break;
                case CyberOpsWindowState.ChallengeSelect:
                    homeScreenButton.interactable = false;
                    missionVmScreenController.Deactivate();
                    challengeVmScreenController.Activate();
                    backButton.gameObject.SetActive(true);
                    bottomScreenPanel.SetActive(false);
                    break;
            }
            curState = newState;
        }

        public void OpenConfirmationWindow(string vmName, string vmURL)
        {
            workstation.vmURL = vmURL;
            confirmationScreenContent = new ModalWindowContent(vmName, confirmationText, "New Tab", "Embedded Window", workstation.OpenVMWindowNewTab, workstation.OpenVMWindowEmbedded, CloseConfirmationWindow);
            ModalPanel.Instance.OpenWindow(confirmationScreenContent);
        }

        private void CloseConfirmationWindow()
        {

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
            if (curState != CyberOpsWindowState.HomeScreen) return;

            // Old structure for VM workstations has no intermediate menus, just open the confirmation window immediatly
            if (usingOldStructure)
            {
                if (string.IsNullOrEmpty(workstation.vmURL))
                {
                    return;
                }
                OpenConfirmationWindow(workstation.staticVmName, workstation.vmURL);
            }
            else
            {
                // New structure gives every station access to every available VM for each mission at this location
                OpenMissionSelectScreen();
            }
        }

        // Clicking the physical mouse is just the same as clicking on the home screen
        public void OnMouseModelClick()
        {
            OnClickHomeScreen();
        }

        public void OnBackButton()
        {
            switch (curState)
            {
                case CyberOpsWindowState.HomeScreen:
                    break;
                case CyberOpsWindowState.MissionSelect:
                    SetState(CyberOpsWindowState.HomeScreen);
                    break;
                case CyberOpsWindowState.ChallengeSelect:
                    SetState(CyberOpsWindowState.MissionSelect);
                    break;
            }
        }

        public void SetBottomScreenText(string text)
        {
            bottomScreenText.text = text;
        }
    }
}