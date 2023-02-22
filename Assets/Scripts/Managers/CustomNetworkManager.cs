/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Entities;
using Entities.Workstations;
using Entities.Workstations.CubeStationParts;
using Audio;

namespace Managers
{
    /// <summary>
    /// A class further customizing behavior from the network manager extension that handles loading multiple scenes.
    /// </summary>
    public class CustomNetworkManager : MultiSceneNetManager
    {
        // The WorkstationManager ScriptableObject that tracks all workstations
        public WorkstationManager workstationManager = null;
        // Objects that get put in DontDestroyOnLoad need to be destroyed manually when a player disconnects
        private List<GameObject> objectsToDestroyOnDisconnect = new List<GameObject>();
        // Configurations to run this app in dev mode or debug mode (set externally)
        public bool isInDevMode;
        public bool isInDebugMode;

        // Developer hotkeys, used only when the game runs in dev mode
        [Header("Developer Hotkeys - dev mode only")]
        public KeyCode hideHUDKeyCode = KeyCode.H;
        public KeyCode skipVideoKeyCode = KeyCode.V;
        public KeyCode jumpKeyCode = KeyCode.J;
        public KeyCode abortKeyCode = KeyCode.A;
        public KeyCode launchModeKeyCode = KeyCode.L;
        public KeyCode explorationModeKeyCode = KeyCode.E;
        public KeyCode standbyModeKeyCode = KeyCode.S;

        // The AudioManager/AudioPlayer prefab
        [Header("Non-networked prefabs to instantiate in the first scene")]
        [SerializeField]
        private GameObject audioPrefab;

        #region Unity events
        /// <summary>
        /// Unity event function that instantiates the AudioManager before the first frame in case it does not exist (it should exist, however).
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (AudioManager.Instance == null)
            {
                Instantiate(audioPrefab);
            }
        }

        /// <summary>
        /// Unity event function that instantiates the AudioManager on a given frame in case it does not exist (it should exist, however).
        /// </summary>
        private void Update()
        {
            if (AudioManager.Instance == null || AudioPlayer.Instance == null)
            {
                Instantiate(audioPrefab);
            }
        }
        #endregion

        #region Mirror events
        /// <summary>
        /// Called on the server when a client disconnects. The override is used to decide what should happen when a disconnection is detected.
        /// </summary>
        /// <param name="conn">The network connection to the client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // Get the Player component of the object with the network connection representing the player
            Debug.Log($"Player with connection id {conn.connectionId} disconnected");
            Player player = conn.identity.GetComponent<Player>();
            
            // Remove the cube from that player if they were holding it
            if (ShipStateManager.Instance.PlayerIsHoldingCube(player)) 
            {
                Debug.Log("Player that disconnected was holding the cube! Reseting cube at Nav Reader");
                ShipStateManager.Instance.SetCubeState(CubeState.InNavReader);
            }
            // If the player was at a workstation, remove them from that workstation before they disconnect
            if (player.OccupiedWorkstation != WorkstationID.NULL)
            {
                Debug.Log("Player that disconnected was at a workstation! Making that station's terminal usable again...");
                workstationManager.ExitTerminal(player.OccupiedWorkstation);
            }

            // Remove client authority from all workstation objects to prevent them from being destroyed
            NetworkIdentity[] ownedObjects = new NetworkIdentity[conn.clientOwnedObjects.Count];
            conn.clientOwnedObjects.CopyTo(ownedObjects);
            foreach (NetworkIdentity networkIdentity in ownedObjects)
            {
                if (networkIdentity.GetComponent<Workstation>() != null)
                {
                    networkIdentity.RemoveClientAuthority();
                }
            }
            
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// Called on a client when they begin to disconnect from the server.
        /// </summary>
        public override void OnStopClient()
        {
            base.OnStopClient();

            if (isInDebugMode)
            {
                Debug.Log("In OnStopClient");
            }

            // Destroy the objects registered to be destroyed on disconnect
            DestroyRegisteredObjects();
        }

        /// <summary>
        /// Called on a client when they disconnect from the server.
        /// </summary>
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            if (isInDebugMode)
            {
                Debug.Log("In OnClientDisconnect");
            }

            // Reset the AudioSource pool to avoid errors when disconnecting
            AudioManager.Instance.ResetPool();

            // Destroy the objects registered to be destroyed on disconnect
            // This is a failsafe call in case this method is not called after OnStopClient
            DestroyRegisteredObjects();
        }
        #endregion

        #region Destruction helper functions
        /// <summary>
        /// Adds a given object to a list of objects to destroy when a player disconnects.
        /// </summary>
        /// <param name="obj">The object to destroy when a player disconnects (along with other objects).</param>
        public void RegisterObjectToDestroyOnDisconnect(GameObject obj)
        {
            if (isInDebugMode)
            {
                Debug.Log($"Registered {obj.name} to be destroyed on disconnect.");
            }

            // Add the object to the list of objects to destroy
            objectsToDestroyOnDisconnect.Add(obj);
        }

        /// <summary>
        /// Destroys any objects in DontDestroyOnLoad that have been registered to prevent issues on reconnect.
        /// </summary>
        private void DestroyRegisteredObjects()
        {
            if (isInDebugMode)
            {
                Debug.Log("Destroying objects registered in CustomNetworkManager.");
            }

            // Loop through all objects to destroy when a player disconnects and destroy them
            foreach (GameObject obj in objectsToDestroyOnDisconnect)
            {
                // As long as the object still exists, destroy it
                if (obj)
                {
                    if (isInDebugMode)
                    {
                        Debug.Log($"Destroying {obj.name} in CustomNetworkManager.");
                    }
                    Destroy(obj);
                }
            }

            // Clear the list of objects to destroy
            objectsToDestroyOnDisconnect.Clear();
        }
        #endregion
    }
}


