using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimeRemainingText : MonoBehaviour
{
    /// <summary>
    /// The title of the timer.
    /// </summary>
    [SerializeField]
    private TMP_Text timerTitle;
    /// <summary>
    /// The text object which shows the current time remaining.
    /// </summary>
    private TMP_Text timerText;

    /// <summary>
    /// Unity event function that gets the text component when this object first starts.
    /// </summary>
    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();

        StartCoroutine(UpdateTimeDisplay());
    }

    private IEnumerator UpdateTimeDisplay()
    {
        yield return new WaitUntil(() => ShipStateManager.Instance && ShipStateManager.Instance.Session != null);
        if (ShipStateManager.Instance.Session.timerTitle != null)
        {
            // Set the title of the timer
            timerTitle.text = ShipStateManager.Instance.Session.timerTitle;

            // Set session DateTimes
            DateTime gameStartDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameStartTime);
            DateTime gameEndDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameEndTime);

            yield return new WaitUntil(() => gameStartDateTime <= DateTime.Now);

            // Set timespan
            TimeSpan span = gameEndDateTime - DateTime.Now;

            while (span.TotalMilliseconds >= 0)
            {
                // Update the display
                gameStartDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameStartTime);
                gameEndDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameEndTime);
                span = gameEndDateTime - DateTime.Now;

                timerText.text = string.Format("{0:hh\\:mm\\:ss}", span);

                yield return null;
            }
        }
    }
}
