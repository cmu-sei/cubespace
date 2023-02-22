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
using Mirror;

namespace Entities
{
    /// <summary>
    /// Component defining the structure of an object that a client player can interact with.
    /// </summary>
    public class Interactable : NetworkBehaviour
    {
        #region Variables
        /// <summary>
        /// Whether the player is in range of this object.
        /// </summary>
        protected bool isInRange = false;
        /// <summary>
        /// The point where player will walk to when they click on this object.
        /// </summary>
        public Transform interactionPoint;
        #endregion

        #region Interaction methods
        /// <summary>
        /// Performs logic when the player interacts with this object. This base function just performs 
        /// logging, and so should be overridden.
        /// </summary>
        /// <param name="player">The Player object interacting with this object.</param>
        public virtual void OnInteract(Player player = null)
        {
            if (player == null)
            {
                Debug.LogWarning("Interactable being interacted with by null interactor", this);
            }
            else
            {
                if (!player.isLocalPlayer)
                {
                    Debug.LogError("Non-local player attempted to interact with interactable", player);
                }
            }
        }

        /// <summary>
        /// Enables the flag specifying a player as in range of this object.
        /// </summary>
        /// <param name="player">The Player object entering this Interactable object.</param>
        public virtual void LocalPlayerEnters(Player player)
        {
            isInRange = true;
        }

        /// <summary>
        /// Disables the flag specifying a player as in range of this object.
        /// </summary>
        /// <param name="player">The player object exiting this Interactable object.</param>
        public virtual void LocalPlayerExits(Player player)
        {
            isInRange = false;
        }
        #endregion
    }
}

