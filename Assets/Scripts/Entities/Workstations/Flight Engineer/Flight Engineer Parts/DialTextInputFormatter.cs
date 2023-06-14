using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Managers;

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
            textInputField.enabled = false;
        }
    }
}
