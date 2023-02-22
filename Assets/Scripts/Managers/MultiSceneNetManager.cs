/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// Class that addtively loads multiple scenes on clients and the server.
/// Extends Mirror's NetworkManager and is extended by CustomNetworkManager.
/// Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
/// API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
/// </summary>
namespace Managers
{
    [AddComponentMenu("")]
    public class MultiSceneNetManager : NetworkManager
    {
        // The external scene controller object needed to unload additive scenes
        public SceneController _sceneController; 
        // A flag set true after the server loads all subscene instances
        private bool additionalScenesLoaded;
        // A list of all additive scenes loaded in the game; additional scenes are added as they are loaded
        private readonly List<Scene> loadedScenes = new List<Scene>();

        #region Server System Callbacks
        /// <summary>
        /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }

        /// <summary>
        /// <para>This delay is mostly for the host player that loads too fast for the
        /// server to have subscenes async loaded from OnStartServer ahead of it.</para>
        /// </summary>
        /// <param name="conn">The NetworkConnection of the client connecting.</param>
        /// <returns>A yield while waiting for the additive scenes to load.</returns>
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // Wait for server to async load all subscenes for game instances
            while (!additionalScenesLoaded)
            {
                yield return null;
            }              

            // Send scene message to client to additively load the other scenes
            foreach (var scene in _sceneController.GetAdditionalScenes())
            {
                // Make sure we're not trying to load the container scene
                if (scene != gameObject.scene.path)
                {
                    conn.Send(new SceneMessage 
                    {
                        sceneName = scene,
                        sceneOperation = SceneOperation.LoadAdditive
                    });
                }
            }

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            // Call the base functionality, since we're overriding a Mirror function
            base.OnServerAddPlayer(conn);
        }
        #endregion

        #region Start & Stop Callbacks
        /// <summary>
        /// Begins loading additive scenes on the server.
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            StartCoroutine(ServerLoadSubScenes());
        }

        /// <summary>
        /// Loads additive scenes on the server.
        /// </summary>
        /// <returns>A yield while waiting for additive scenes to load.</returns>
        IEnumerator ServerLoadSubScenes()
        {
            // Wait for the online scene to be loaded before loading additional scenes
            while (SceneManager.GetActiveScene().path != onlineScene)
            {
                yield return null;
            }

            // Loop through the specified additive scenes
            foreach (var scene in _sceneController.GetAdditionalScenes())
            {
                // Make sure we're not trying to load the container scene
                if (scene != onlineScene)
                {
                    // Load the additive scene asynchronously, then wait for it to finish
                    AsyncOperation sceneLoadingOp = SceneManager.LoadSceneAsync(scene, new LoadSceneParameters 
                    {
                        loadSceneMode = LoadSceneMode.Additive,
                        localPhysicsMode = LocalPhysicsMode.None
                    });
                    while (!sceneLoadingOp.isDone)
                    {
                        yield return null;
                    }
                }
            }

            yield return null;

            // Add the container scene to the list and mark the operation as finished
            Scene newScene = SceneManager.GetSceneByPath(onlineScene);
            loadedScenes.Add(newScene);
            additionalScenesLoaded = true;
        }

        /// <summary>
        /// Starts unloading additive scenes on the server and asks all clients to do the same.
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            // Ask each client to unload all additive scenes
            NetworkServer.SendToAll(new SceneMessage 
            {
                sceneName = onlineScene,
                sceneOperation = SceneOperation.UnloadAdditive 
            });
            // Unload all additive scenes on the server
            StartCoroutine(ServerUnloadSubScenes());
        }

        /// <summary>
        /// Unloads the subScenes and unused assets, and clears the subScenes list.
        /// </summary>
        /// <returns></returns>
        IEnumerator ServerUnloadSubScenes()
        {
            // Loop through the additive scenes and unload each one
            foreach (var scene in _sceneController.GetAdditionalScenes())
            {
                // Make sure we're not trying to unload the container scene
                if (scene != onlineScene)
                {
                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }

            yield return null;

            // Clear the list and reset the scenes loaded flag
            loadedScenes.Clear();
            additionalScenesLoaded = false;

            // Wait for unused assets to be unloaded
            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Starts unloading additive scenes on a client.
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            // Make sure we're not in host mode before unloading additive scenes
            if (mode == NetworkManagerMode.ClientOnly)
            {
                StartCoroutine(ClientUnloadSubScenes());
            }    
        }

        /// <summary>
        /// Unloads all scenes except the active one, which is the "container" scene.
        /// </summary>
        /// <returns>A yield waiting a frame while the scenes unload.</returns>
        IEnumerator ClientUnloadSubScenes()
        {
            // Loop through all additive scenes and unload them
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                // Make sure we're not trying to unload the container scene
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                {
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
                }  
                yield return null;
            }
        }
        #endregion
    }
}

