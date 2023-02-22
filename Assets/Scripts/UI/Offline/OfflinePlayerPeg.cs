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
using Customization;
using UI.Customization;

/// <summary>
/// The peg displayed in the offline scene.
/// </summary>
public class OfflinePlayerPeg : MonoBehaviour
{
    /// <summary>
    /// The options provided to the player for customization.
    /// </summary>
    [SerializeField]
    private CustomizationOptions options;

    /// <summary>
    /// The renderer on the peg which is colored based on what the local player selects.
    /// </summary>
    [SerializeField]
    private Renderer _playerColorRenderer;
    /// <summary>
    /// The ParticleSystem colored by the local player's selection.
    /// </summary>
    [SerializeField]
    private ParticleSystem _playerParticleSystem;
    /// <summary>
    /// The image displayed.
    /// </summary>
    [SerializeField]
    private Image _playerIconDisplayImage;
    /// <summary>
    /// The rate of rotation of the player peg.
    /// </summary>
    [SerializeField]
    private float rotationRate = 1.0f;

    /// <summary>
    /// The parent object of the connect button.
    /// </summary>
    [SerializeField]
    private GameObject connectButtonParent;
    /// <summary>
    /// The color selection panel.
    /// </summary>
    [SerializeField]
    private PlayerColorSelectionPanel colorPanel;
    /// <summary>
    /// The icon selection panel.
    /// </summary>
    [SerializeField]
    private PlayerIconSelectionPanel iconPanel;

    /// <summary>
    /// Unity event function that rotates the peg and waits to activate the connect button.
    /// </summary>
    void Start()
    {
        StartCoroutine(Rotate());
        StartCoroutine(WaitForSelections());
    }

    /// <summary>
    /// Rotates the peg.
    /// </summary>
    /// <returns>A yield statement for the peg.</returns>
    private IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * rotationRate * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Waits for the color and icon to be selected.
    /// </summary>
    /// <returns>A yield statement while no color or icon is selected.</returns>
    private IEnumerator WaitForSelections()
    {
        yield return new WaitUntil(() => colorPanel.colorSelected && iconPanel.iconSelected);
        connectButtonParent.SetActive(true);
    }
}

