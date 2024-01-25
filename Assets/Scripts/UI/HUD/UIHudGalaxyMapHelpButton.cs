using Audio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHudGalaxyMapHelpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UIHudGalaxyMapHelpPanel helpPanel;

    private TextMeshProUGUI mouseoverText; // Not really a "button", just text to mouseover that activates the panel

    private void Awake()
    {
        if (helpPanel == null)
        {
            Debug.LogError("UIHudGalaxyMapHelpPanel Missiing!!!");
        }

        mouseoverText = GetComponent<TextMeshProUGUI>();
        helpPanel.Deactivate();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        helpPanel.Activate();
        AudioPlayer.Instance.UIMouseover(0);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        helpPanel.Deactivate();
        AudioPlayer.Instance.UIMouseover(1);
    }
}
