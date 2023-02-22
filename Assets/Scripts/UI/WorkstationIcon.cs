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
using Entities;
using Entities.Workstations;
using TMPro;

namespace UI 
{
    /// <summary>
    /// The icon associated with a workstation. This class displays the icon as well as whether the workstation is in use or not.
    /// </summary>
    public class WorkstationIcon : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// The Terminal object using this WorkstationIcon.
        /// </summary>
        [Header("GameObject References")]
        [SerializeField]
        private Terminal terminal;
        /// <summary>
        /// The TextMeshPro objects that should have an assigned sprite index aligning with this workstation and be colored.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI[] textMeshes;
        /// <summary>
        /// The display to render when the associated workstation is occupied.
        /// </summary>
        [SerializeField]
        private WorkstationIconStatus occupiedStatus;
        /// <summary>
        /// The display to render when the associated workstation is available.
        /// </summary>
        [SerializeField]
        private WorkstationIconStatus availableStatus;
        /// <summary>
        /// The offset to use for better display of the workstation icon.
        /// </summary>
        [Header("Icon Variables")]
        [SerializeField]
        private Vector3 offset = new Vector3(0, 0, 0);

        /// <summary>
        /// The camera used for distance calculation.
        /// </summary>
        private Camera cam;
        /// <summary>
        /// The current animation fading in the color on text or disabling the text.
        /// </summary>
        private Coroutine currentAnimation;
        /// <summary>
        /// The occupied or unoccupied workstation icon displays.
        /// </summary>
        private List<WorkstationIconStatus> statuses = new List<WorkstationIconStatus>();
        /// <summary>
        /// Whether the player is in range. Note that this does not need to be synchronized because it is local to the player.
        /// </summary>
        private bool isInRange = false;
        /// <summary>
        /// Whether to use an exclamation point as the icon (such as in case of an incoming transmission) or to use the stationID 
        /// to set the icon (as is the case with most workstations).
        /// </summary>
        public bool isNotificationIcon = false;
        #endregion

        #region Unity event functions
        /// <summary>
        /// A Unity event function that sets the sprite index string within and colors all given TextMeshPro objects.
        /// </summary>
        void Start()
        {
            cam = Camera.main;
        
            textMeshes[0].text = FormatText();
            foreach(TextMeshProUGUI text in textMeshes) 
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            }

            statuses.Add(occupiedStatus);
            statuses.Add(availableStatus);

            UpdatePosition();
        }

        /// <summary>
        /// A Unity event function that constantly updates the position of the icon, in case the screen resizes.
        /// </summary>
        void Update() 
        {
            UpdatePosition();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Sets the position of the WorkstationIcon.
        /// </summary>
        public void UpdatePosition() 
        {
            Vector3 screenPos = cam.WorldToScreenPoint(terminal.transform.position);
            transform.position = screenPos + offset;
        }

        /// <summary>
        /// Formats the text displayed on this WorkstationIcon by returning a tag containing the sprite index.
        /// </summary>
        /// <returns>A string formatted as a tag containing the sprite index.</returns>
        private string FormatText()
        {
            if (isNotificationIcon)
            {
                return "!";
            }

            string num;
            switch (terminal.StationID)
            {
                case WorkstationID.CubeStation:
                    {
                        num = "0";
                        break;
                    }
                case WorkstationID.Sensor:
                    {
                        num = "1";
                        break;
                    }
                case WorkstationID.Antenna:
                    {
                        num = "2";
                        break;
                    }
                case WorkstationID.PowerRouting:
                    {
                        num = "3";
                        break;
                    }
                case WorkstationID.FlightEngineer:
                    {
                        num = "4";
                        break;
                    }
                case WorkstationID.NavReader:
                    {
                        num = "5";
                        break;
                    }
                default:
                case WorkstationID.WorkstationVM1:
                    {
                        num = "6";
                        break;
                    }
                case WorkstationID.ThrustersAB:
                    {
                        num = "10";
                        break;
                    }
                case WorkstationID.ThrustersCD:
                    {
                        num = "11";
                        break;
                    }
                case WorkstationID.Codex:
                    {
                        num = "9";
                        break;
                    }
            }

            return "<sprite index=" + num + ">";
        }
        #endregion

        #region Status methods
        /// <summary>
        /// Enables or disables the occupied and available displays.
        /// </summary>
        /// <param name="occupied">Whether the workstation is currently occupied.</param>
        public void SetOccupied(bool occupied)
        {
            occupiedStatus.SetActive(occupied);
            if (occupied)
            {
                availableStatus.SetActive(false);
            }
            else
            {
                availableStatus.SetActive(isInRange);
            }
        }

        /// <summary>
        /// Enables or disables the available status based on whether the player is in range.
        /// </summary>
        /// <param name="inRange">Whether the player is in range.</param>
        public void SetInRange(bool inRange)
        {
            isInRange = inRange;
            if(!occupiedStatus.IsActive())
            {
                availableStatus.SetActive(inRange);
            }
        }
        #endregion

        #region Icon enable/disable methods
        /// <summary>
        /// Enables the icon.
        /// </summary>
        /// <param name="fade">Whether to fade this icon.</param>
        public void EnableIcon(bool fade = true) 
        {  
            if (currentAnimation != null) 
            {
                StopCoroutine(currentAnimation);
            }

            Color newColor = new Color(textMeshes[0].color.r, textMeshes[0].color.g, textMeshes[0].color.b, 1);
            if (fade)
            {
                currentAnimation = StartCoroutine(FadeToColor(newColor));
            } 
            else 
            {
                foreach(TextMeshProUGUI text in textMeshes) 
                {
                    text.color = newColor;
                }
            }
        }

        /// <summary>
        /// Disables the icon.
        /// </summary>
        /// <param name="fade">Whether to fade this icon.</param>
        public void DisableIcon(bool fade = true) 
        {
            if (currentAnimation != null) 
            {
                StopCoroutine(currentAnimation);
            }

            Color newColor = new Color(textMeshes[0].color.r, textMeshes[0].color.g, textMeshes[0].color.b, 0);
            if (fade)
            {
                currentAnimation = StartCoroutine(FadeToColor(newColor, true));
            } 
            else 
            {
                foreach(TextMeshProUGUI text in textMeshes) 
                {
                    text.color = newColor;
                }
            }
        }
        /// <summary>
        /// Fades the current color of each text object given to a different color.
        /// </summary>
        /// <param name="color">The color to fade the text to.</param>
        /// <param name="disable">Whether to disable the text objects.</param>
        /// <param name="fadeTime">The time to fade the color.</param>
        /// <returns>A yield statement while waiting for the color to fade.</returns>
        IEnumerator FadeToColor(Color color, bool disable = false, float fadeTime = 0.5f) 
        {
            if (!disable) 
            {
                foreach(TextMeshProUGUI text in textMeshes) 
                {
                    text.enabled = true;
                }
            }

            float timeElapsed = 0f;
            Color originalColor = textMeshes[0].color;
            while (timeElapsed < fadeTime) 
            {
                Color newColor = Color.Lerp(originalColor, color, timeElapsed / fadeTime);
                foreach (TextMeshProUGUI text in textMeshes) 
                {
                    text.color = newColor;
                }
                foreach (WorkstationIconStatus status in statuses) 
                {
                    status.SetAlpha(newColor.a);
                }
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            foreach (WorkstationIconStatus status in statuses) 
            {
                status.SetAlpha(color.a);
            }

            if (disable)
            {
                foreach (TextMeshProUGUI text in textMeshes) 
                {
                    text.enabled = false;
                }
            }
        }
        #endregion
    }
}

