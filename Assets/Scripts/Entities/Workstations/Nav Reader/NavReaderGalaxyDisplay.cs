using Managers;
using UI.HUD;
using UnityEngine;

namespace UI.WorkstationUI.NavReaderScreens
{
    public class NavReaderGalaxyDisplay : MonoBehaviour
    {
        [SerializeField]
        private SphereCollider collider;

        private void OnEnable()
        {
            if (collider)
            {
                collider.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (collider)
            {
                collider.enabled = false;
            }
        }

        /// <summary>
        /// Unity event function that opens the galaxy map whenever this object is clicked.
        /// </summary>
        private void OnMouseDown()
        {
            if (ShipStateManager.Instance && ShipStateManager.Instance.Session != null && ShipStateManager.Instance.Session.useGalaxyDisplayMap)
            {
                HUDController.Instance.OpenGalaxyMap();
            }
        }
    }
}
