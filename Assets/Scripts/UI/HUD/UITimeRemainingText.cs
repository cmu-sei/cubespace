using Managers;
using Mirror;
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
    /// UI that pops up when players run out of time.
    /// </summary>
    [SerializeField] private GameObject gameOverUIPanel;

    /// <summary>
    /// Unity event function that gets the text component when this object first starts.
    /// </summary>
    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
        gameOverUIPanel.SetActive(false);

        StartCoroutine(UpdateTimeDisplay());
    }

    /// <summary>
    /// Coroutine that updates the time display text.
    /// </summary>
    private IEnumerator UpdateTimeDisplay()
    {
        yield return new WaitUntil(() => ShipStateManager.Instance && ShipStateManager.Instance.Session != null);
        if (ShipStateManager.Instance.Session.timerTitle != null)
        {
            // Set the title of the timer
            string title = ShipStateManager.Instance.Session.timerTitle;
            timerTitle.text = title;

            // Set session DateTimes
            DateTime gameStartDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameStartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime gameCurrentDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameCurrentTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime gameEndDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameEndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

            // Set the timer to show the duration of the game, but don't start updating the timer until the start time
            timerText.text = string.Format("{0:hh\\:mm\\:ss}", gameEndDateTime - gameStartDateTime);

            yield return new WaitUntil(() => gameStartDateTime <= gameCurrentDateTime);

            // Set timespan as provided GameBrain
            TimeSpan span = gameEndDateTime - gameCurrentDateTime;
            // Create a variable to track when currentTime gets updated
            TimeSpan prevSpan = span;
            // Create an internal timer to interpolate the countdown between polls to GameBrain
            float internalTimer = Time.deltaTime;

            // Update timer based on GameBrain's clock as provided by gameCurrentTime until gameCurrentTime is beyond gameEndTime
            while (span.TotalMilliseconds >= 0)
            {
                // Update the display
                gameCurrentDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameCurrentTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                gameEndDateTime = DateTime.Parse(ShipStateManager.Instance.Session.gameEndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                span = gameEndDateTime - gameCurrentDateTime;

                // Check to see if gameCurrentUpdateTime has been updated by a poll to GameBrain
                if (span != prevSpan)
                {
                    // If so use that time directly and reset the timer used for interpolation
                    timerText.text = string.Format("{0:hh\\:mm\\:ss}", span);
                    internalTimer = 0.0f;
                    prevSpan = span;
                }
                else
                {
                    // Else use the last time polled plus whatever has accumulated on the internal time since the last poll
                    internalTimer -= Time.deltaTime;
                    // If timer crosses 0 in between polls, show 00:00:00 but don't break out of loop until GameBrain actually say currentTime has passed endTime
                    if (span + TimeSpan.FromSeconds(internalTimer) < TimeSpan.Zero)
                    {
                        timerText.text = "00:00:00";
                    }
                    else
                    {
                        timerText.text = string.Format("{0:hh\\:mm\\:ss}", span + TimeSpan.FromSeconds(internalTimer));
                    }
                }

                // Update timer title displayed if it changes
                if (ShipStateManager.Instance.Session.timerTitle != null && title != ShipStateManager.Instance.Session.timerTitle)
                {
                    title = ShipStateManager.Instance.Session.timerTitle;
                    timerTitle.text = title;
                }

                yield return null;
            }

            // Set timer to show 00:00:00 once gameCurrentTime is beyond gameEndTime
            timerText.text = "00:00:00";

            // Quit the game, stopping the session for everyone connected, and show some UI
            QuitGame();
        }
    }

    /// <summary>
    /// Quits the game. This is called when GameBrain says the gameCurrentTime is beyond gameEndTime
    /// </summary>
    public void QuitGame()
    {
        // TODO: Should disconnect players/load a new scene/something better than this
        gameOverUIPanel.SetActive(true);
    }
}
