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

namespace UI.CustomCursor
{
    /// <summary>
    /// A component that defines mouse cursor behavior, and enables/disables display of that custom mouse cursor.
    /// </summary>
    public class OnMouseover : MonoBehaviour
    {
        /// <summary>
        /// The custom mouse cursor to use when hovering over something.
        /// </summary>
        public Texture2D cursorTexture;
        /// <summary>
        /// The mode specifying how to swtich the cursors.
        /// </summary>
        public CursorMode cursorMode = CursorMode.Auto;
        /// <summary>
        /// The hotspot of the cursor.
        /// </summary>
        public Vector2 hotSpot = Vector2.zero;
        /// <summary>
        /// Whether to actively set this cursor.
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Sets the cursor mode to use the custom texture provided.
        /// </summary>
        protected virtual void OnMouseEnter()
        {
            if (active)
            {
                Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            }
        }

        /// <summary>
        /// Resets the cursor mode to default.
        /// </summary>
        protected virtual void OnMouseExit()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }
}
