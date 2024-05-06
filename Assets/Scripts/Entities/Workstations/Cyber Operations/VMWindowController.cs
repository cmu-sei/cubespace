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
using System;
using System.Runtime.InteropServices;

namespace Entities.Workstations.CyberOperationsParts
{
    /// <summary>
    /// The controller for the VM window.
    /// <para>
    /// Importantly: a URL passed here should not be directly input by a user, as Application.OpenURL, while necessary, is not safe.
    /// Ensure that your URLs are sanitized and non-malicious before passing them into a method within this class.
    /// </para>
    /// </summary>
    public class VMWindowController : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// External method from OpenWindowPlugin.jslib that opens a VM inside the game.
        /// </summary>
        /// <param name="link">The link to the VM.</param>
        /// <param name="stationID">The ID of the station where the embedded window should open.</param>
        [DllImport("__Internal")]
        private static extern void OpenEmbeddedWindow(string link, int stationID);
        /// <summary>
        /// External method from OpenWindowPlugin.jslib that opens a VM in a new tab.
        /// </summary>
        /// <param name="link">The link to the VM.</param>
        /// <param name="stationID">The ID of the station where the VM is accessed.</param>
        /// <param name="windowName">The name of the VM displayed in the window.</param>
        [DllImport("__Internal")]
        private static extern void OpenWindow(string link, int stationID, string windowName);
        /// <summary>
        /// External method from OpenWindowPlugin.jslib that closes a window.
        /// </summary>
        /// <param name="stationID">The ID of the workstation where the window is being closed.</param>
        [DllImport("__Internal")]
        private static extern void CloseWindow(int stationID);
        #endregion

        #region Methods
        /// <summary>
        /// Checks whether a URL is valid to open and uses HTTP/HTTPS.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>Whether the URL is valid.</returns>
        private bool IsURLClean(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($"URL provided for VM is not a valid URL. URL provided: {url}");
                return false;
            }

            Uri uriResult;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
            {
                Debug.LogWarning($"URL provided for VM is not a valid URL. URL provided: {url}");
                return false;
            }
            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                Debug.LogError($"URL provided for VM does not use HTTP or HTTPS. URL provided: {url}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens a window in an embedded window. As with all URL methods, do not allow arbitrary input to this function.
        /// </summary>
        /// <param name="url">The VM URL to open in the window.</param>
        /// <param name="stationID">The ID of the workstation opening this window. This is used to send an OnClose message to fire a callback.</param>
        public void OpenWindowInFrame(string url, WorkstationID stationID)
        {
            if (!IsURLClean(url))
            {
                return;
            }

            #if (!UNITY_EDITOR && UNITY_WEBGL)
            OpenEmbeddedWindow(url, (int)stationID);
            #endif
        }

        /// <summary>
        /// Opens a window in a new tab. As with all URL methods, do not allow arbitrary input to this function.
        /// </summary>
        /// <param name="url">The VM URL to open in the window.</param>
        /// <param name="stationID">The ID of the workstation opening this window.</param>
        /// <param name="windowName">The name of the window to display.</param>
        public void OpenWindowInTab(string url, WorkstationID stationID, string windowName)
        {
            if (!IsURLClean(url))
            {
                return;
            }

            #if (!UNITY_EDITOR && UNITY_WEBGL)
            OpenWindow(url, (int) stationID, windowName);
            #else
            Application.OpenURL(url);
            #endif
        }

        /// <summary>
        /// Closes a VM window.
        /// </summary>
        /// <param name="stationID">The ID of the workstation opening this window. This is used to send an OnClose message to fire a callback.</param>
        public void CloseVM(WorkstationID stationID)
        {
            // This works but isn't being used
            // we can't prevent players from just copying the URL and opening a new tab and therefore can't enforce comp rules with this method,
            // so we'd rather not enable this behavior at all
            /*
            #if (!UNITY_EDITOR && UNITY_WEBGL)
            CloseWindow((int) stationID);
            #endif
            */
        }
        #endregion
    }
}

