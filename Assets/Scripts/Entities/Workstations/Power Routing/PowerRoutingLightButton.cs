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
        private bool isExplorationButton = false;

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
            if (isExplorationButton && powerRouting.GetAllPoweredForExploration())
            {
                SetExplorationMode(false);
                return;
            }
            else if (!isExplorationButton && powerRouting.GetAllPoweredForLaunch())
            {
                SetLaunchMode(false);
                return;
            }

            DepowerUnneededStations();

            if (isExplorationButton)
            {
                SetExplorationMode(true);
            }
            else
            {
                SetLaunchMode(true);
            }
        }

        /// <summary>
        /// Powers off all workstations except
        /// </summary>
        private void DepowerUnneededStations()
        {
            foreach (Workstation w in _workstationManager.GetWorkstations())
            {
                if (isExplorationButton)
                {
                    if (!w.AlwaysHasPower && !w.UsedInExplorationMode && w.IsPowered)
                    {
                        powerRouting.TogglePowerState(w.StationID);
                    }
                }
                else
                {
                    if (!w.AlwaysHasPower && !w.UsedInLaunchMode && w.IsPowered)
                    {
                        powerRouting.TogglePowerState(w.StationID);
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the power state of all launch mode workstations that are currently unpowered
        /// </summary>
        private void SetLaunchMode(bool power)
        {
            foreach (Workstation w in _workstationManager.GetLaunchWorkstations())
            {
                if (!w.AlwaysHasPower && w.IsPowered != power)
                {
                    powerRouting.TogglePowerState(w.StationID);
                }
            }
        }

        /// <summary>
        /// Toggles the power state of all exploration mode workstations that are currently unpowered
        /// </summary>
        private void SetExplorationMode(bool power)
        {
            foreach (Workstation w in _workstationManager.GetExplorationWorkstations())
            {
                if (!w.AlwaysHasPower && w.IsPowered != power)
                {
                    powerRouting.TogglePowerState(w.StationID);
                }
            }
        }
    }
}
