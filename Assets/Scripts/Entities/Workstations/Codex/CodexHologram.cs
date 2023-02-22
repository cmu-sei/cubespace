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

namespace Entities.Workstations.CodexStationParts 
{
    /// <summary>
    /// A component for the holographic cube displayed on the Codex workstation.
    /// </summary>
    public class CodexHologram : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The reference to the Codex workstation.
        /// </summary>
        [Header("Codex Hologram References")]
        [SerializeField]
        private CodexStation codexStation;
        /// <summary>
        /// The possible codex pieces.
        /// </summary>
        [SerializeField]
        private List<MeshRenderer> codexPieces;
        /// <summary>
        /// The animator used to spin the codex hologram.
        /// <para>
        /// This variable exists because this script must be on the full model for activation.
        /// </para>
        /// </summary>
        [SerializeField]
        private Animator codexHologramAnimator;

        /// <summary>
        /// Whether the cube hologram is enabled. Derives from a private variable.
        /// </summary>
        public bool IsActive => isActive;
        /// <summary>
        /// Whether the cube hologram should be displayed.
        /// <para>
        /// This is necessary because the hologram may not yet be active when the number of pieces is updated.
        /// </para>
        /// </summary>
        private bool isActive = false;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Turns off the codex hologram and sets its number of active pieces to 0.
        /// </summary>
        private void Start()
        {
            Deactivate();
        }
        #endregion

        #region Activation and deactivation methods
        /// <summary>
        /// Activates the codex and sets its number of pieces.
        /// </summary>
        public void Activate() 
        {
            isActive = true;
            SetNumberActivePieces(codexStation.CodexPieceCount);
        }

        /// <summary>
        /// Deactivates the codex and sets its number of pieces to 0.
        /// </summary>
        public void Deactivate() 
        {
            isActive = false;
            SetNumberActivePieces(0);
        }
        #endregion

        #region Hologram manipulation methods
        /// <summary>
        /// Enables and disables the pieces on the codex hologram based on how many have been obtained.
        /// </summary>
        /// <param name="activePiecesCount">The number of codex pieces currently obtained.</param>
        public void SetNumberActivePieces(int activePiecesCount) 
        {
            for (int i = 0; i < codexPieces.Count; i++) 
            {
                if (i < activePiecesCount) 
                {
                    codexPieces[i].enabled = true;
                } 
                else 
                {
                    codexPieces[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// Sets the speed the codex should spin at by changing the animator's speed value.
        /// </summary>
        /// <param name="speed">The speed the holographic codex should spin at.</param>
        public void SetSpinSpeed(float speed) 
        {
            codexHologramAnimator.SetFloat("Speed", speed);
        }
        #endregion
    }
}


