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
using UnityEngine.SceneManagement;
using Managers;

namespace Utilities
{
    /// <summary>
    /// A simple class that destroys its object if the main gameplay scene is loaded. This should be used on editor objects.
    /// </summary>
    public class DestroyObjectOnMainSceneLoad : MonoBehaviour
    {
        // The SceneController object
        public SceneController sceneController;

        /// <summary>
        /// Unity event function that destroys its item if the Workstation scene is loaded from another scene.
        /// </summary>
        protected virtual void Awake()
        {
            // Check if the main gameplay scene is loaded, and destroy this object if it is.
            string mainPath = sceneController.GameplayScene;
            Scene main = SceneManager.GetSceneByPath(mainPath);
            if (main.isLoaded) 
            {
                Destroy(gameObject);
            }
        }
    }
}


