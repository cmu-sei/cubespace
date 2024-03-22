/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;
using Entities.Workstations;
using Entities.Workstations.CyberOperationsParts;

namespace Managers
{
    /// <summary>
    /// This class solely receives messages sent by external JavaScript calls. To call a function from here from JavaScript, use "window.unityInstance.SendMessage('ExternalMessageManager', 'FunctionInThisFile')."
    /// <para>
    /// Note that only one primitive-type argument (int, string, etc.) can be provided, and messages sent cannot find objects in the root heirarchy.
    /// </para>
    /// </summary>
    public class ExternalMessageManager : ConnectedSingleton<ExternalMessageManager>
    {
        // The workstation manager object
        [SerializeField]
        private WorkstationManager _workstationManager;

        /// <summary>
        /// Unity event function that forces this object to have the name ExternalMessageManager.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            if (gameObject.name != "ExternalMessageManager")
            {
                Debug.LogError("ExternalMessageManager gameObject must have the exact name 'ExternalMessageManager'!");
                gameObject.name = "ExternalMessageManager";
            }
        }

        /// <summary>
        /// Performs logic when the close button is clicked on the embedded VM window. Note that this does not get called when a separate tab for a VM is closed, unless it is closed via VMWindowController.CloseVM.
        /// </summary>
        /// <param name="stationID"></param>
        public void OnCloseVMWindow(int stationID)
        {
            CyberOperations station = (CyberOperations) _workstationManager.GetWorkstation((WorkstationID) stationID);
            station?.OnCloseVMWindow();
        }
    }
}


