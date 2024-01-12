using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHudGalaxyMapHelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void Activate()
    {
        // TODO: Should be setting colors for system examples programtically here
        // TODO: Example systems should be interactable with tooltips too
        panel.SetActive(true);
    }

    public void Deactivate()
    {
        panel.SetActive(false);
    }
}
