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
using UnityEngine.UI;
using Entities.Workstations;

/// <summary>
/// Component used to reset the state of the ship, interrupting the launch sequence.
/// </summary>
[RequireComponent(typeof(Animator))]
public class AbortButton : MonoBehaviour
{
    #region Variables
    /// <summary>
    /// The clip that plays when the button cover closes.
    /// </summary>
    [SerializeField]
    private AnimationClip _coverCloseClip;
    /// <summary>
    /// The clip that plays when the button cover opens.
    /// </summary>
    [SerializeField]
    private AnimationClip _coverOpenClip;
    /// <summary>
    /// The clip that plays when the button is pressed.
    /// </summary>
    [SerializeField]
    private AnimationClip _buttonPressClip;
    /// <summary>
    /// The UI button covering the physical button.
    /// </summary>
    [SerializeField]
    private Button _button;
    /// <summary>
    /// The light on the physical button object.
    /// </summary>
    [SerializeField]
    private WorkstationLight _buttonLight;
    /// <summary>
    /// The workstation the abort button is located on.
    /// </summary>
    [SerializeField]
    private Workstation _workstation;

    /// <summary>
    /// The animator which moves the button cover.
    /// </summary>
    private Animator _animator;
    /// <summary>
    /// The camera used to zoom out after the button is pressed.
    /// </summary>
    private Camera _cam;
    /// <summary>
    /// The content within the confirmation screen.
    /// </summary>
    private UI.ModalWindowContent _confirmationScreenContent;

    /// <summary>
    /// The animation coroutine currently playing.
    /// </summary>
    private Coroutine _animationCoroutine = null;
    /// <summary>
    /// Whether the button is active. This is always true.
    /// </summary>
    private bool _active = false;
    /// <summary>
    /// Whether the button is currently covered.
    /// </summary>
    private bool _isCovered = true;
    #endregion

    #region Unity event functions
    /// <summary>
    /// Unity event function that gets the animator and camera components, and subscribes to the OnEnter and OnExit events.
    /// </summary>
    void Start()
    {
        _animator = GetComponent<Animator>();

        _button.onClick.AddListener(PressButton);

        _workstation.OnEnter += () => _active = true;
        _workstation.OnExit += () => _active = false;

        _cam = Camera.main;

        _confirmationScreenContent = new UI.ModalWindowContent("", "Are you sure you want to \nreset the launch sequence?", "CONFIRM", "CANCEL", ConfirmedReset, CanceledReset, CanceledReset);
    }

    /// <summary>
    /// Unity event function that toggles the cover on the button if the mouse clicks on the cover.
    /// </summary>
    private void Update()
    {
        // TODO: This is bad. Should be in OnMouseEnter
        if (_active && _animationCoroutine == null && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 5f))
            {
                if (hit.transform.tag == "AbortButtonCover")
                {
                    ToggleCover();
                }
            }
        }
    }
    #endregion

    #region Button/cover methods
    /// <summary>
    /// Toggles the cover over the workstation button.
    /// </summary>
    private void ToggleCover()
    {
        if (_isCovered && Entities.Player.LocalCanInput)
        {
            _animator.SetTrigger("OpenCover");
            _animationCoroutine = StartCoroutine(CoverAnimationCoroutine(true));
        }
        else
        {
            _animator.SetTrigger("CloseCover");
            _button.interactable = false;
            _animationCoroutine = StartCoroutine(CoverAnimationCoroutine(false));
        }
    }

    /// <summary>
    /// Triggers the button animation which also closes the button cover.
    /// </summary>
    private void PressButton()
    {
        if (_active && _animationCoroutine == null && Entities.Player.LocalCanInput)
        {
            _button.interactable = false;
            UI.ModalPanel.Instance.OpenWindow(_confirmationScreenContent);
        }
    }
    #endregion

    #region Reset functions
    /// <summary>
    /// Sets the button as uninteractable and resets the launch workstations.
    /// </summary>
    private void ConfirmedReset()
    {
        _button.interactable = false;
        _animator.SetTrigger("PressButton");
        _workstation.ResetLaunchWorkstations();
        _animationCoroutine = StartCoroutine(ButtonAnimationCoroutine());
    }

    /// <summary>
    /// Sets the button as interactable. Used when the reset call is aborted.
    /// </summary>
    private void CanceledReset()
    {
        _button.interactable = true;
    }
    #endregion

    #region Button animations
    /// <summary>
    /// Sets whether the button is covered and the light is pulsing.
    /// </summary>
    /// <param name="isOpening">Whether the button cover is opening.</param>
    /// <returns>A yield statement while waiting for the cover to open or close.</returns>
    private IEnumerator CoverAnimationCoroutine(bool isOpening)
    {
        if (isOpening)
        {
            yield return new WaitForSeconds(_coverOpenClip.length);
            _isCovered = false;
            _button.interactable = true;
            _buttonLight.Pulsing = true;
        }
        else
        {
            yield return new WaitForSeconds(_coverCloseClip.length);
            _isCovered = true;
            _buttonLight.Pulsing = false;
        }
        _animationCoroutine = null;
    }

    /// <summary>
    /// Sets whether the button is covered and the light is pulsing, and nullifies the current animation coroutine.
    /// </summary>
    /// <returns>A yield statement while waiting until the end of the button press.</returns>
    private IEnumerator ButtonAnimationCoroutine()
    {
        yield return new WaitForSeconds(_buttonPressClip.length);
        _isCovered = true;
        _buttonLight.Pulsing = false;
        _animationCoroutine = null;
    }
    #endregion
}

