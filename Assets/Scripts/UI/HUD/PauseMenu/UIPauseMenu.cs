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

namespace UI.PauseMenu
{
    /// <summary>
    /// A class for the pause menu.
    /// </summary>
    public class UIPauseMenu : MonoBehaviour
    {
        /// <summary>
        /// An action that fires when the pause menu is opened.
        /// </summary>
        public static System.Action OnPause;

        /// <summary>
        /// Sets the state of the pause menu.
        /// </summary>
        /// <param name="state">Whether the pause menu should be active.</param>
        public void SetState(bool state)
        {
            if (state && !gameObject.activeInHierarchy)
            {
                Activate();
            }
            else if (!state && gameObject.activeInHierarchy)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Activates this GameObject, locks player input, and fires the OnPause event.
        /// </summary>
        public void Activate() 
        {
            gameObject.SetActive(true);
            Entities.Player.LockLocalPlayerInput();
            OnPause();
        }

        /// <summary>
        /// Deactivates this GameObject and unlocks player input.
        /// </summary>
        public void Deactivate() 
        {
            gameObject.SetActive(false);
            Entities.Player.UnlockLocalPlayerInput();
        }
    }
}

