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

namespace Entities.Workstations {
    /// <summary>
    /// A sprite that renders if the workstation is powered.
    /// </summary>
    public class PoweredSprite : PoweredComponent
    {
        #region Variables
        /// <summary>
        /// The SpriteRenderer component to enable or change color.
        /// </summary>
        private SpriteRenderer spriteRenderer;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the sprite renderer object.
        /// </summary>
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// A Unity event function that sets the color of the SpriteRenderer depending on the power state.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (workstation.IsPowered)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
            }
            else
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            }
        }
        #endregion

        #region Power functions
        /// <summary>
        /// Changes the color of the SpriteRenderer when the workstation is powered on.
        /// </summary>
        protected override void TurnOn()
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        }

        /// <summary>
        /// Changes the color of the SpriteRenderer when the workstation is powered off.
        /// </summary>
        protected override void TurnOff()
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
        #endregion
    }
}

