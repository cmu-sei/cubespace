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
using UnityEngine.UI;

namespace UI.HUD
{
    /// <summary>
    /// The cube sprite displayed on the HUD overlay.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIHudCube : MonoBehaviour
    {
        /// <summary>
        /// The image of the cube to display.
        /// </summary>
        [SerializeField]
        private Image image;
        /// <summary>
        /// The text object to render with the cube.
        /// </summary>
        [SerializeField]
        private GameObject textObject;

        /// <summary>
        /// Unity event function that gets the image component of the cube.
        /// </summary>
        private void Start()
        {
            image = GetComponent<Image>();
        }

        /// <summary>
        /// Sets whether the cube and text display.
        /// </summary>
        /// <param name="enabled">Whether the cube and text should display.</param>
        public void SetCube(bool enabled)
        {
            image.enabled = enabled;
            textObject.SetActive(enabled);
        }
    }
}

