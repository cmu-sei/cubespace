using Managers;
using UI.HUD;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.WorkstationUI.NavReaderScreens
{
    /// <summmary>
    /// A simple script that controls whether the galaxy map can be opened or not.
    /// </summary>
    public class NavReaderGalaxyDisplay : MonoBehaviour
    {
        /// <summary>
        /// The collider of the galaxy display that the mouse can click to open the galaxy map.
        /// </summary>
        [SerializeField]
        private SphereCollider _collider;

        /// <summary>
        /// Unity event function enables the galaxy map display collider when this script is enabled.
        /// </summary>
        private void OnEnable()
        {
            if (_collider)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Unity event function disables the galaxy map display collider when this script is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (_collider)
            {
                _collider.enabled = false;
            }
        }

        /// <summary>
        /// Unity event function that opens the galaxy map whenever this object is clicked.
        /// </summary>
        private void OnMouseDown()
        {
            if (ShipStateManager.Instance && ShipStateManager.Instance.Session != null && ShipStateManager.Instance.Session.useGalaxyDisplayMap)
            {
                HUDController.Instance.SetMenuState(HUDController.MenuState.GalaxyMap);
            }
        }
    }
}
