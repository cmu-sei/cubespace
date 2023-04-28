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
		/// The workstation buttons for the workstations this light button should power.
		/// </summary>
		[SerializeField]
        private PowerRoutingButton[] buttonsToTriggerOnClick;

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
            if (powerForExploration)
            {
                // Power off each workstation if they're all powered
                if (powerRouting.GetAllPoweredForExploration())
                {
                    foreach (PowerRoutingButton powerRoutingButton in buttonsToTriggerOnClick)
                    {
                        powerRoutingButton.OnButtonClick();
                    }
                }
                // Power on each workstation if power is available and it is not powered
                else if (powerRouting.GetPowerRemaining() >= buttonsToTriggerOnClick.Length)
                {
                    foreach (PowerRoutingButton powerRoutingButton in buttonsToTriggerOnClick)
                    {
                        Workstation ws = _workstationManager.GetWorkstation(powerRoutingButton.WorkstationID);
                        if (!ws.AlwaysHasPower && !ws.IsPowered)
                        {
                            powerRoutingButton.OnButtonClick();
                        }
                    }
                }
            }
            else
            {
                // Power off each workstation if they're all powered
                if (powerRouting.GetAllPoweredForLaunch())
                {
                    foreach (PowerRoutingButton powerRoutingButton in buttonsToTriggerOnClick)
                    {
                        powerRoutingButton.OnButtonClick();
                    }
                }
                // Power on each workstation if power is available and it is not powered
                else if (powerRouting.GetPowerRemaining() >= buttonsToTriggerOnClick.Length)
                {
                    foreach (PowerRoutingButton powerRoutingButton in buttonsToTriggerOnClick)
                    {
                        Workstation ws = _workstationManager.GetWorkstation(powerRoutingButton.WorkstationID);
                        if (!ws.AlwaysHasPower && !ws.IsPowered)
                        {
                            powerRoutingButton.OnButtonClick();
                        }
                    }
                }
            }
        }
    }
}
