using Managers;
using System.Collections;
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

        // Used to disable buttons for breif period after clicking, which is a bad but effective hack
        [SerializeField]
        private PowerRoutingLightButton otherButton;
        private bool isPressable = true;

        /// <summary>
        /// Unity event function that powers on or off the stations used in the specified mode all at once.
        /// </summary>
        private void OnMouseDown()
        {
            if (!isPressable) return;

            // If we're in the mode the player hit the button for, turn off everything (this will turn off stations for the other mode if they're on as well, oh well)
            if ((isExplorationButton && powerRouting.GetAllPoweredForExploration()) || (!isExplorationButton && powerRouting.GetAllPoweredForLaunch()))
            {
                powerRouting.SetPowerStateToMode(PoweredState.Standby);
            }
            else if (isExplorationButton)
            {
                powerRouting.SetPowerStateToMode(PoweredState.ExplorationMode);
            }
            else
            {
                powerRouting.SetPowerStateToMode(PoweredState.LaunchMode);
            }

            StartCoroutine(Co_DisableButtonForQuarterSecond());
            StartCoroutine(otherButton.Co_DisableButtonForQuarterSecond());
        }

        // This is a hack to prevent many bugs arising from quickly pressing these buttons
        // A better solution would be to rewrite PowerRouting and handle powered state in a sane way, but this works too
        public IEnumerator Co_DisableButtonForQuarterSecond()
        {
            isPressable = false;
            yield return new WaitForSeconds(0.25f);
            isPressable = true;
        }
    }
}
