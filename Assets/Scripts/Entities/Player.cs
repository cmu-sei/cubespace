/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using Entities.Workstations;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Audio;
using Systems;
using Systems.CredentialRequests;

namespace Entities
{
    /// <summary>
    /// A class defining the possible behavior and actions of a player.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        /// <summary>
        /// An action that occurs when a player is created.
        /// </summary>
        public static Action<Player> NewLocalPlayerEvent;
        /// <summary>
        /// The NavMeshAgent component of the player.
        /// </summary>
        private NavMeshAgent agent;
        /// <summary>
        /// The NavMesh layer mask the player has.
        /// </summary>
        [SerializeField]
        private LayerMask navMeshLayerMask;
        /// <summary>
        /// The interactable layer mask component of the player.
        /// </summary>
        [SerializeField]
        private LayerMask interactableLayerMask;

        /// <summary>
        /// The camera used to view the player and get mouse events to direct their movement.
        /// </summary>
        private Camera _camera;
        
        /// <summary>
        /// The workstation currently occupied by the player. Derives from a private variable.
        /// </summary>
        public WorkstationID OccupiedWorkstation => _occupiedWorkstation;
        /// <summary>
        /// The workstation currently occupied by the player.
        /// </summary>
        [SyncVar] 
        private WorkstationID _occupiedWorkstation = WorkstationID.NULL;

        /// <summary>
        /// Whether the player can provide input to move on the server.
        /// </summary>
        [SyncVar]
        private bool _canInput = true;

        /// <summary>
        /// Whether the player can provide input to move on the client. Derives from a private variable.
        /// </summary>
        public static bool LocalCanInput => localCanInput;
        /// <summary>
        /// Whether the player can provide input to move on the client.
        /// </summary>
        private static bool localCanInput = true;

        /// <summary>
        /// The default priority of the player's NavMeshAgent.
        /// </summary>
        private readonly int navDefaultPriority = 50;
        /// <summary>
        /// The priority used for players to avoid jostling workstations. If high, players don't try to push the workstation agent, and so fail.
        /// </summary>
        private readonly int navWorkstationPriority = 10;
        
        /// <summary>
        /// Gets the NavMeshAgent component and allows the player to move.
        /// </summary>
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            localCanInput = true;
        }

        /// <summary>
        /// Invokes an event on the client when the player joins the game and gets the camera.
        /// </summary>
        void Start()
        {
            if (isLocalPlayer)
            {
                NewLocalPlayerEvent?.Invoke(this);
                // Send client token to server to Gamebrain for verification
                Debug.LogWarning("?DEBUGGING?: Player.cs:103\nCubespace client just started loading main scenes. Sending command to server to send my token for authorization.");
#if !UNITY_EDITOR
                Debug.LogWarning("?DEBUGGING?: Player.cs:103\nCubespace client just started loading main scenes. Sending command to server to send my token for authorization.");
                CmdSendClientToken(LoadingSystem.Instance.token);
#endif
            }

            _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        /// <summary>
        /// Sends the token of the player who's joined the game using the WebGL client to Gamebrain.
        /// </summary>
        /// <param name="clientToken">The client token to send to Gamebrain.</param>
        [Command]
        void CmdSendClientToken(string clientToken)
        {
            Debug.LogWarning("?DEBUGGING?: Player.cs:120\nCubespace server just recieved command from new client with token: " + clientToken + ". Sending this token to gamebrain to authorize.");
            ClientCredentialSender.Instance.SendClientToken(clientToken);
        }

        /// <summary>
        /// Moves the player, if they are able to move.
        /// </summary>
        void Update()
        {
            if (isLocalPlayer)
            {
                if (localCanInput && _canInput && _occupiedWorkstation == WorkstationID.NULL)
                {
                    if (!InteractionTick())
                    {
                        MovementTick();
                    }
                }
            }
        }

        /// <summary>
        /// Moves the player to a specific position when the mouse button is pressed there.
        /// </summary>
        /// <returns>Whether the player has moved to the specified destination or not.</returns>
        bool MovementTick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, navMeshLayerMask))
                {
                    agent.destination = hit.point;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the player has reached an interactable object.
        /// </summary>
        /// <returns>Whether the player has reached an interactable object or not.</returns>
        bool InteractionTick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayerMask))
                {
                    Interactable interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable) 
                    {
                        interactable.OnInteract(this);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Locks input for all players.
        /// </summary>
        [Command]
        public void CmdLockAllPlayersInput()
        {
            _canInput = false;
        }

        /// <summary>
        /// Unlocks input for all players.
        /// </summary>
        [Command]
        public void CmdUnlockAllPlayersInput()
        {
            _canInput = true;
        }

        /// <summary>
        /// Locks input on the WebGL client.
        /// </summary>
        public static void LockLocalPlayerInput()
        {
            localCanInput = false;
        }

        /// <summary>
        /// Unlocks input on the WebGL client.
        /// </summary>
        public static void UnlockLocalPlayerInput()
        {
            localCanInput = true;
        }

        /// <summary>
        /// Sets the destination of the NavAgent on this player object.
        /// </summary>
        /// <param name="destination">The destination where the player should go.</param>
        public void SetNavAgentDestination(Vector3 destination)
        {
            agent.destination = destination;
        }

        /// <summary>
        /// Force-places the player at a given position.
        /// </summary>
        /// <param name="pos">The position to move the player to.</param>
        public void Teleport(Vector3 pos)
        {
            agent.Warp(pos);
        }

        /// <summary>
        /// Checks if the player has entered the area of an interactable object.
        /// </summary>
        /// <param name="other">The collider of the trigger area the player has entered.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            var overlapped = other.GetComponent<Interactable>();
            if (overlapped != null)
            {
                overlapped.LocalPlayerEnters(this);
            }
        }

        /// <summary>
        /// Checks if the player has left the area of an interactable object.
        /// </summary>
        /// <param name="other">The collider of the trigger area the player has exited.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            var overlapped = other.GetComponent<Interactable>();
            if (overlapped != null)
            {
                overlapped.LocalPlayerExits(this);
            }
        }

        /// <summary>
        /// Called when the player first joins the game.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            AudioPlayer.Instance.OnConnect(gameObject);
        }

        /// <summary>
        /// Called when the player exits the game.
        /// </summary>
        public override void OnStopLocalPlayer()
        {
            AudioPlayer.Instance.OnDisconnect();
            base.OnStopLocalPlayer();
        }

#region Command Wrappers
        /// <summary>
        /// Places the player inside the workstation view.
        /// </summary>
        /// <param name="stationID">The ID of the workstation the player is entering.</param>
        public void EnterWorkstationView(WorkstationID stationID)
        {
            CmdSetOccupiedWorkstation(stationID);
        }

        /// <summary>
        /// Moves the player outside of the workstation view.
        /// </summary>
        public void ExitWorkstationView()
        {
            CmdSetOccupiedWorkstation(WorkstationID.NULL);
        }
#endregion

#region Commands
        /// <summary>
        /// Sets the workstation the player has occupied on the server.
        /// </summary>
        /// <param name="stationID">The ID of the workstation the player is using.</param>
        [Command(requiresAuthority = false)]
        private void CmdSetOccupiedWorkstation(WorkstationID stationID)
        {
            _occupiedWorkstation = stationID;
            if (stationID == WorkstationID.NULL)
            {
                SetNavMeshAgentPriority(navDefaultPriority);
            }
            else
            {
                SetNavMeshAgentPriority(navWorkstationPriority);
            }
        }

        /// <summary>
        /// Sets the NavMeshAgent priority on the client.
        /// </summary>
        /// <param name="priority">The avoidance priority the player's NavMeshAgent has.</param>
        [ClientRpc]
        private void SetNavMeshAgentPriority(int priority)
        {
            agent.avoidancePriority = priority;
        }

#endregion
    }
}

