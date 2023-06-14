using Managers;
using UnityEngine;

namespace Entities.Workstations.PowerRouting
{
    /// <summary>
    /// A script which powers all workstations of the specified mode when the object it is attached to is clicked.
    /// </summary>
    public class PowerRoutingLightButton : MonoBehaviour
    {
        /// <summary>
        /// The power routing workstation.
        /// </summary>
        [SerializeField]
        private PowerRouting powerRouting;

        /// <summary>
        /// Whether this button is used to power workstation explorations.
        /// </summary>
        [SerializeField]
        private bool powerForExploration = false;

        /// <summary>
        /// The manager used to control workstation information.
        /// </summary>
        [SerializeField]
        private WorkstationManager _workstationManager;

        /// <summary>
        /// Unity event function that powers on or off the stations used in the specified mode all at once.
        /// </summary>
        private void OnMouseDown()
        {
            bool wasAllPoweredForExploration = powerRouting.GetAllPoweredForExploration();
            bool wasAllPoweredForLaunch = powerRouting.GetAllPoweredForLaunch();

            // Turn everything off
            DepowerAll();

            if (powerForExploration)
            {
                if (!wasAllPoweredForExploration || wasAllPoweredForLaunch)
                {
                    ToggleAllInExplorationMode();
                }
            }
            else
            {
                if (!wasAllPoweredForLaunch)
                {
                    ToggleAllInLaunchMode();
                }
            }
        }

        /// <summary>
        /// Powers off all workstations.
        /// </summary>
        private void DepowerAll()
        {
            foreach (Workstation w in _workstationManager.GetWorkstations())
            {
                if (!w.AlwaysHasPower && w.IsPowered)
                {
                    powerRouting.workstationButtonDict[w.StationID].OnButtonClick();
                }
            }
        }

        /// <summary>
        /// Toggles the power state of all launch mode workstations.
        /// </summary>
        private void ToggleAllInLaunchMode()
        {
            foreach (Workstation w in _workstationManager.GetLaunchWorkstations())
            {
                if (!w.AlwaysHasPower)
                {
                    powerRouting.workstationButtonDict[w.StationID].OnButtonClick();
                }
            }
        }

        /// <summary>
        /// Toggles the power state of all exploration mode workstations.
        /// </summary>
        private void ToggleAllInExplorationMode()
        {
            foreach (Workstation w in _workstationManager.GetExplorationWorkstations())
            {
                if (!w.AlwaysHasPower)
                {
                    powerRouting.workstationButtonDict[w.StationID].OnButtonClick();
                }
            }
        }
    }
}
