/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Entities.Workstations.SensorStationParts;
using UnityEngine;
using UnityEngine.UI;
using Systems;

/// <summary>
/// A slider which changes the volume used in transmissions.
/// </summary>
public class UITransmissionVolumeSlider : MonoBehaviour
{
    /// <summary>
    /// The video system affected by the volume change.
    /// </summary>
    private SensorStationVideoSystem videoSystem;
    /// <summary>
    /// The slider used by the player to change the volume.
    /// </summary>
    [SerializeField]
    private Slider slider;

    /// <summary>
    /// Unity event function that gets the video system and sets its volume.
    /// </summary>
    public void Start()
    {
        videoSystem = GameObject.Find("SensorStation").GetComponent<SensorStationVideoSystem>();
        SetVolume();
    }

    /// <summary>
    /// Sets the volume of the video system.
    /// </summary>
    public void SetVolume() 
    {
        if (videoSystem)
        {
            videoSystem.SetVideoVolume(slider.value);
        }
        else
        {
            Debug.Log("Can't find Sensor System's Video Player");
        }

        if (CutsceneSystem.Instance)
        {
            CutsceneSystem.Instance.SetVideoVolume(slider.value);
        }
        else
        {
            Debug.Log("Can't find HUD's Video Player");
        }
    }
}

