/*
 * Author: Javier
 * Date: 13 December 2025
 * Description:
 * Controls the start flow of an AR stage.
 * Handles hiding the stage introduction UI and starting the stopwatch
 * when the player presses the Start button.
 */

using UnityEngine;

/// <summary>
/// Controls the initial start of a stage.
/// Responsible for hiding the introduction panel and
/// starting the gameplay timer when the user begins the stage.
/// </summary>
public class StageStartController : MonoBehaviour
{
    /// <summary>
    /// UI panel shown at the beginning of the stage that explains objectives.
    /// This panel is hidden once the player presses the Start button.
    /// </summary>
    [SerializeField] GameObject stageIntroPanel;

    /// <summary>
    /// Reference to the stopwatch UI that tracks how long
    /// the player takes to complete the stage.
    /// </summary>
    [SerializeField] StopwatchUI stopwatch;

    /// <summary>
    /// Called when the Start button is pressed.
    /// Hides the intro panel and begins the stage timer.
    /// </summary>
    public void PressStart()
    {
        if (stageIntroPanel != null)
            stageIntroPanel.SetActive(false);

        if (stopwatch != null)
            stopwatch.StartTimer();
    }
}
