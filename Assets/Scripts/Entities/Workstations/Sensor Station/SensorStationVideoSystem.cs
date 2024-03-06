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
using UnityEngine;
using UI.HUD;
using Managers;

namespace Entities.Workstations.SensorStationParts
{
    /// <summary>
    /// The interface between the sensor station and the video player manager
    /// </summary>
    public class SensorStationVideoSystem : MonoBehaviour
    {
        #region Variables
        [SerializeField] private RenderTexture tex;
        private SensorStation station;
        #endregion

        #region Unity event functions
        /// <summary>
        /// Unity event function that gets the VideoPlayer and CustomNetworkManager and sets attributes of the video player.
        /// </summary>
        private void Start()
        {
            station = GetComponent<SensorStation>();
        }
        #endregion

        #region Video playback methods

        /// <summary>
        /// Initializes the video player and begins preparing the video at the given url
        /// </summary>
        /// <param name="url">The URL of the video that should be prepared</param>
        public bool ReadyVideo(string url)
        {
            if (!VideoPlayerManager.Instance)
            {
                Debug.LogError("Couldn't Find video player manager!");
                return false;
            }

            if (!VideoPlayerManager.Instance.InitializeVideo(url, tex, false))
            {
                // This message was getting printed after videos succesuflly played and I can't see why it would be. Commenting out for now until something starts breaking
                //Debug.LogWarning("Video initialization failed!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Plays a video which should have been initialized previously
        /// </summary>
        /// <param name="url">The URL of the video that should play.</param>
        /// <param name="callback">The callback method to invoke within a coroutine upon finishing the video.</param>
        public void PlayVideo(string url)
        {
            if (!VideoPlayerManager.Instance)
            {
                Debug.LogError("Couldn't find video player manager!");
                return;
            }

            // Check to make sure that the VideoPlayerManager still has the proper video prepared
            if (VideoPlayerManager.Instance.preparedVideoURL != url)
            {
                if (!ReadyVideo(url))
                {
                    Debug.LogError("Sensor station transmission video failed!");
                    return;
                }
            }

            VideoPlayerManager.Instance.PlayVideo();
            UIExitWorkstationButton.Instance.SetHiddenByVideo(true);

            VideoPlayerManager.OnVideoCompleted += OnVideoEnd;
        }

        /// <summary>
        /// Stops playing the video.
        /// </summary>
        public void InterruptVideo()
        {
            if (!VideoPlayerManager.Instance)
            {
                Debug.LogError("Couldn't find video player manager!");
                return;
            }

            VideoPlayerManager.Instance.StopVideo();
            UIExitWorkstationButton.Instance.SetHiddenByVideo(false);
        }

        private void OnVideoEnd(string url, bool videoCompleted)
        {
            if (videoCompleted)
            {
                station.OnVideoFinished(url);
            }
            UIExitWorkstationButton.Instance.SetHiddenByVideo(false);
            VideoPlayerManager.OnVideoCompleted -= OnVideoEnd;
        }
        #endregion
    }
}

