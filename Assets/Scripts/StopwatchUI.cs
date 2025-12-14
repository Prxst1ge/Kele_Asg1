/*
 * Author: Javier
 * Date: 12 December2025
 * Description:
 * Displays and manages a stopwatch-style timer UI.
 * The timer starts, updates in real-time, and stops based on
 * stage progression events.
 */

using UnityEngine;
using TMPro;

/// <summary>
/// Handles a simple stopwatch timer for tracking how long
/// the player takes to complete a stage.
/// </summary>
public class StopwatchUI : MonoBehaviour
{
    /// <summary>
    /// Text component used to display the formatted timer (MM:SS).
    /// </summary>
    [SerializeField] TMP_Text timerText;

    /// <summary>
    /// Total elapsed time in seconds.
    /// </summary>
    float elapsed;

    /// <summary>
    /// Indicates whether the stopwatch is currently running.
    /// </summary>
    bool running;

    /// <summary>
    /// Resets and starts the stopwatch timer.
    /// Called when the player begins a stage.
    /// </summary>
    public void StartTimer()
    {
        elapsed = 0f;
        running = true;
        UpdateText();
    }

    /// <summary>
    /// Stops the stopwatch timer.
    /// Typically called when the stage is completed.
    /// </summary>
    public void StopTimer()
    {
        running = false;
        UpdateText();
    }

    /// <summary>
    /// Updates the timer every frame while it is running.
    /// Uses unscaled time to remain accurate regardless of time scale changes.
    /// </summary>
    void Update()
    {
        if (!running) return;

        elapsed += Time.unscaledDeltaTime;
        UpdateText();
    }

    /// <summary>
    /// Formats the elapsed time into MM:SS and updates the UI text.
    /// </summary>
    void UpdateText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsed);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
