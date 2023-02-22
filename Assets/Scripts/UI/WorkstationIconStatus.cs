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
using TMPro;
using UnityEngine.UI;

namespace UI{
    /// <summary>
    /// A component that displays a workstation icon and whether it's occupied or not.
    /// </summary>
    public class WorkstationIconStatus : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The text object displaying whether this object is available or occupied.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI text;
        /// <summary>
        /// The image representing the workstation.
        /// </summary>
        [SerializeField]        private Image image;
        #endregion
        #region Unity event functions
        /// <summary>
        /// Unity event function that initially disables this object and the opacity of its text and image.
        /// </summary>
        void Start()
        {
            SetAlpha(0);
            gameObject.SetActive(false);
        }


        #endregion
        #region Main methods
        /// <summary>
        /// Sets the transparency of the text and image.
        /// </summary>
        /// <param name="alpha">The transparency of the text and image.</param>
        public void SetAlpha(float alpha)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        /// <summary>
        /// Sets whether the GameObject is active.
        /// </summary>
        /// <param name="active">Whether the GameObject is active.</param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        /// <summary>
        /// Checks whether the GameObject is active within the Scene.
        /// </summary>
        /// <returns>Whether the GameObject is active within the Scene.</returns>
        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }
        #endregion
    }
}

