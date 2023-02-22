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
using TMPro;
using UnityEngine.Events;

namespace UI
{
    /// <summary>
    /// The core modal panel displayed on a canvas. This is a singleton object.
    /// </summary>
    public class ModalPanel : Managers.ConnectedSingleton<ModalPanel>
    {
        /// <summary>
        /// The panel of the GameObject.
        /// </summary>
        [SerializeField]
        private GameObject _panel;

        /// <summary>
        /// The header text of the modal.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _header;
        /// <summary>
        /// The content text to render in the modal panel.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _content;

        /// <summary>
        /// The button on the bottom left.
        /// </summary>
        [SerializeField]
        private Button _leftFooterButton;
        /// <summary>
        /// The text of the button on the bottom left.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _leftFooterButtonText;
        /// <summary>
        /// The button on the bottom right.
        /// </summary>
        [SerializeField]
        private Button _rightFooterButton;
        /// <summary>
        /// The text of the button on the bottom right.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _rightFooterButtonText;

        /// <summary>
        /// The close button.
        /// </summary>
        [SerializeField]
        private Button _closeButton;

        /// <summary>
        /// Unity event function that initializes the text objects.
        /// </summary>
        public override void Start()
        {
            base.Start();
            Initialize();
        }

        /// <summary>
        /// Sets blank text for some TMP objects and removes listeners.
        /// </summary>
        private void Initialize()
        {
            _header.text = "";
            _content.text = "";

            _leftFooterButtonText.text = "";
            _rightFooterButtonText.text = "";
            _leftFooterButton.onClick.RemoveAllListeners();
            _rightFooterButton.onClick.RemoveAllListeners();
            _leftFooterButton.gameObject.SetActive(false);
            _rightFooterButton.gameObject.SetActive(false);

            _closeButton.onClick.RemoveAllListeners();

            _closeButton.onClick.AddListener(Close);
            _leftFooterButton.onClick.AddListener(Close);
            _rightFooterButton.onClick.AddListener(Close);
        }

        /// <summary>
        /// Closes the panel and re-initializes text information.
        /// </summary>
        private void Close()
        {
            _panel.SetActive(false);
            Initialize();
        }

        /// <summary>
        /// Opens the modal window with content.
        /// </summary>
        /// <param name="content">The content of the modal window, defined as a struct below.</param>
        public void OpenWindow(ModalWindowContent content)
        {
            _header.text = content.headerText;
            _content.text = content.contentText;

            _leftFooterButtonText.text = content.leftFooterButtonLabel;
            _rightFooterButtonText.text = content.rightFooterButtonLabel;

            if (content.rightFooterButtonListener != null)
            {
                _rightFooterButton.onClick.AddListener(content.rightFooterButtonListener);
                _rightFooterButtonText.text = content.rightFooterButtonLabel;
                _rightFooterButton.gameObject.SetActive(true);
            }
            if (content.leftFooterButtonListener != null)
            {
                _leftFooterButton.onClick.AddListener(content.leftFooterButtonListener);
                _leftFooterButtonText.text = content.leftFooterButtonLabel;
                _leftFooterButton.gameObject.SetActive(true);
            }
            if (content.closeButtonListener != null)
            {
                _closeButton.onClick.AddListener(content.closeButtonListener);
            }

            _panel.SetActive(true);
            Audio.AudioPlayer.Instance.UIMove();
        }
    }

    /// <summary>
    /// General structure for content shown in the modal window.
    /// </summary>
    public struct ModalWindowContent
    {
        public string headerText;
        public string contentText;
        public string leftFooterButtonLabel;
        public string rightFooterButtonLabel;

        public UnityAction leftFooterButtonListener;
        public UnityAction rightFooterButtonListener;
        public UnityAction closeButtonListener;

        public ModalWindowContent(string _headerText = "", string _contentText = "", string _leftFooterButtonLabel = "", string _rightFooterButtonLabel = "",
                                  UnityAction _leftFooterButtonListener = null, UnityAction _rightFooterButtonListener = null, UnityAction _closeButtonListener = null)
        {
            headerText = _headerText;
            contentText = _contentText;
            leftFooterButtonLabel = _leftFooterButtonLabel;
            rightFooterButtonLabel = _rightFooterButtonLabel;

            leftFooterButtonListener = _leftFooterButtonListener;
            rightFooterButtonListener = _rightFooterButtonListener;
            closeButtonListener = _closeButtonListener;
        }
    }
}

