using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Managers;
using System.Text.RegularExpressions;

namespace Entities.Workstations.FlightEngineerParts
{
    public class DialTextInputFormatter : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField textInputField;

        [SerializeField]
        private TrajectoryDial trajectoryDial;

        [SerializeField]
        private WorkstationManager _workstationManager;

        private void Start()
        {
            textInputField.onEndEdit.AddListener(FormatDialInputText);

            FlightEngineer.OnLock += DisableTextInputField;
            _workstationManager.GetWorkstation(WorkstationID.FlightEngineer).OnResetState += EnableTextInputField;
        }

        public void FormatDialInputText(string newInput)
        {
            // Check to see if the new string is empty. If it is, treat it like a 0
            if (newInput == null || newInput == "")
            {
                newInput = "0";
            }
            // Check to see if non numerics were entered. If any were, treat the input as a 0 (this check should be unnecessary, you can't enter non-numbers to begin with)
            Match m = Regex.Match(newInput, "[^0-9]");
            if (m.Success)
            {
                newInput = "0";
            }

            int angleInt = int.Parse(newInput);
            textInputField.text = WorkstationDial.FormatAngle(angleInt).ToString("000");
            trajectoryDial.ManuallyRotateDial(angleInt);
        }

        public void EnableTextInputField()
        {
            textInputField.enabled = true;
        }

        public void DisableTextInputField()
        {
            Debug.Log("Disabling TextInputs. Invoked from OnLock. [DialTextInputFormatter.cs:55]");
            if (textInputField == null)
            {
                Debug.LogError("NULL textInputField [DialTextInputFormatter.cs:58]");
            }
            textInputField.enabled = false;
        }
    }
}
