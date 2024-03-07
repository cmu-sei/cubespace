using Managers;
using UnityEngine;
using System.Collections.Generic;

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
            // If we're in the mode the player hit the button for, turn off everything (this will turn off stations for the other mode if they're on as well, oh well)
            if ((isExplorationButton && powerRouting.GetAllPoweredForExploration()) || (!isExplorationButton && powerRouting.GetAllPoweredForLaunch()))
            {
                powerRouting.SetPowerStateToMode(Systems.GameBrain.CurrentLocationGameplayData.PoweredState.Standby);
                return;
            }
            else if (isExplorationButton)
            {
                powerRouting.SetPowerStateToMode(Systems.GameBrain.CurrentLocationGameplayData.PoweredState.ExplorationMode);
            }
            else
            {
                powerRouting.SetPowerStateToMode(Systems.GameBrain.CurrentLocationGameplayData.PoweredState.LaunchMode);
            }
        }
    }
}
