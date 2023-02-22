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
    /// An extension of the custom mouse cursor that changes the appearance of the cursor when dragged.
    /// </summary>
    public class OnMouseoverAndDrag : OnMouseover
    {
        /// <summary>
        /// Whether the cursor is currently clicking something.
        /// </summary>
        protected bool clicked = false;
        /// <summary>
        /// Whether the cursor is hovering over an object.
        /// </summary>
        protected bool hovering = false;
        /// <summary>
        /// Whether this cursor should respect when input is locked.
        /// </summary>
        public bool respectsPlayerInputLock = false;

        /// <summary>
        /// Sets the mouse state as hovering when the mouse enters this object.
        /// </summary>
        protected override void OnMouseEnter()
        {
            if (respectsPlayerInputLock && !Entities.Player.LocalCanInput)
            {
                return;
            }
            base.OnMouseEnter();
            hovering = true;
        }

        /// <summary>
        /// Disables the hovering flag when the mouse leaves this object.
        /// </summary>
        protected override void OnMouseExit()
        {
            if (respectsPlayerInputLock && !Entities.Player.LocalCanInput)
            {
                return;
            }
            if (!clicked) 
            {
                base.OnMouseExit();
            }
            hovering = false;
        }

        /// <summary>
        /// Enables the flag saying the mouse is down.
        /// </summary>
        protected virtual void OnMouseDown() 
        {
            if (respectsPlayerInputLock && !Entities.Player.LocalCanInput)
            {
                return;
            }
            clicked = true;
        }

        /// <summary>
        /// Disables the flag saying the mouse is down and resets the appearance of the cursor.
        /// </summary>
        protected virtual void OnMouseUp()
        {
            clicked = false;
            if (!hovering) 
            {
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
            }
        }
    }
}
