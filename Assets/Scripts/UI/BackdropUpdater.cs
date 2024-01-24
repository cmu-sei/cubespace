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
using Managers;
using Mirror;
using Systems.GameBrain;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A class used to switch the backdrop from an old image to a new one.
    /// </summary>
    public class BackdropUpdater : NetworkBehaviour
    {
        /// <summary>
        /// The image to use.
        /// </summary>
        private Image _image;
        /// <summary>
        /// The ID to image map to look up backdrops.
        /// </summary>
        [SyncVar(hook = nameof(UpdateBackdrop))] private string currentBackdropID = "N/A";
        [SerializeField]
        private IDToImageMap _backdropLookup;
        /// <summary>
        /// The fallback background to render if none is provided.
        /// </summary>
        public string backdropIdFallback = "bkd_stars";

        /// <summary>
        /// Unity event function that initiates the dictionary.
        /// </summary>
        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = Color.black;
            _backdropLookup.InitiateDictionary();
        }

        /// <summary>
        /// Unity event function that updates the backdrop.
        /// </summary>
        private void Start()
        {
            if (currentBackdropID != null)
            {
                UpdateBackdrop("", currentBackdropID);
            }
        }

        /// <summary>
        /// Updates the backdrop when the client starts.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateBackdrop("", currentBackdropID);
        }

        /// <summary>
        /// Unity event function that subscribes to the current location change event.
        /// </summary>
        private void OnEnable()
        {
            ShipStateManager.ServerOnCurrentLocationChange += OnCurrentLocationChange;
        }

        /// <summary>
        /// Unity event function that unsubscribes from the current location change event.
        /// </summary>
        private void OnDisable()
        {
            ShipStateManager.ServerOnCurrentLocationChange -= OnCurrentLocationChange;
        }

        /// <summary>
        /// Event function that swaps the backdrop when the ship's location changes.
        /// </summary>
        /// <param name="location">The location the ship has jumped to.</param>
        private void OnCurrentLocationChange(Location location)
        {
            if (isServer)
            {
                StartCoroutine(DelayBackdropChange(1.5f, location.backdropID));
            }
        }

        /// <summary>
        /// Delays switching the background to a new one.
        /// </summary>
        /// <param name="delay">The delay </param>
        /// <param name="backdropID">The ID of the backdrop to switch to.</param>
        /// <returns>A yield return while waiting to swap the backdrop.</returns>
        private IEnumerator DelayBackdropChange(float delay, string backdropID) 
        {
            yield return new WaitForSeconds(delay);
            currentBackdropID = backdropID;
        }

        /// <summary>
        /// Updates the backdrop form an old backdrop to a new one.
        /// </summary>
        /// <param name="old">The old backdrop ID.</param>
        /// <param name="id">The new backdrop ID.</param>
        void UpdateBackdrop(string old, string id)
        {
            if (old == id)
            {
                return;
            }

            _image.sprite = _backdropLookup.GetImage(id, false, backdropIdFallback);

            if (_image.sprite == null)
            {
                _image.color = Color.black;
            }
            else
            {
                StopCoroutine(FadeIn());
                StartCoroutine(FadeIn());
            }
        }

        /// <summary>
        /// Gradually fades in the color on the image.
        /// </summary>
        /// <returns>A yield return while waiting for the color to fade.</returns>
        private IEnumerator FadeIn()
        {
            _image.color = Color.black;
            float timer = 0f;

            while (timer < 0.5f)
            {
                _image.color = Color.Lerp(Color.black, Color.white, timer / 0.5f);
                timer += Time.deltaTime;
                yield return null;
            }

            _image.color = Color.white;
        }
    }
}

