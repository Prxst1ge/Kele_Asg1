/*
 * Author: Javier
 * Date: 12 December 2025
 * Description:
 * Handles scene transitions and application exit.
 * Includes a one-frame delay when loading scenes to allow
 * AR systems or simulation components to clean up properly
 * before switching scenes.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene loading and application quitting.
/// Used for navigating between different scenes in the application.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a new scene by name with a short delay.
    /// The delay allows AR or XR systems to safely clean up
    /// before the scene transition occurs.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        // Small delay gives AR / Simulation a frame to clean up
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    /// <summary>
    /// Coroutine that waits one frame before loading the scene.
    /// This prevents errors caused by destroying AR-related objects mid-frame.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene to load.</param>
    /// <returns>IEnumerator used for coroutine execution.</returns>
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return null;              // wait 1 frame
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the application.
    /// Has no effect when running in the Unity Editor,
    /// but works in standalone or mobile builds.
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
    }
}
