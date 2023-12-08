using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// This is strictly for using TMPro links as hypertext to open embedded links in the mission log
// It can be made more general to work on a non-screen space canvas or to fire generic events from linked text
// See this video for how to do that:
// https://www.youtube.com/watch?v=N6vYyCahLr8

// Link Format:
// <style="Clickable"><link="https://www.google.com/">Click here to go to google!</link></style>
public class LinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textBox;

    private void Awake()
    {
        textBox = GetComponent<TextMeshProUGUI>();
        if (textBox == null)
        {
            Debug.LogError("Text box null in link handler!!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);
        var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(textBox, mousePosition, null);

        if (linkTaggedText != -1)
        {
            TMP_LinkInfo linkInfo = textBox.textInfo.linkInfo[linkTaggedText];
            string URL = linkInfo.GetLinkID();
            
            if (ValidateURL(URL))
            {
                Application.OpenURL(URL);
            }
        }
    }

    private bool ValidateURL(string url)
    {
        Uri uriResult;
        if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
        {
            Debug.LogError($"URL provided in link is not a valid URL. URL provided: {url}");
            return false;
        }
        // URL does not use HTTPS or HTTP
        else if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
        {
            Debug.LogError($"URL provided in link does not use HTTP or HTTPS. URL provided: {url}");
            return false;
        }

        return true;
    }
}
