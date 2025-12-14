/*
 * Author: Javier
 * Date: 10 November 2025
 * Description:
 * Controls the toggle behaviour for ingredient card UI details.
 * Handles showing and hiding a translation/details panel and
 * optionally updates the button label text to reflect the current state.
 */

using UnityEngine;
using TMPro; // Used for TextMeshPro button labels

/// <summary>
/// Manages the visibility of a card's translation/details panel.
/// Allows toggling UI content via a button press.
/// </summary>
public class CardUIController : MonoBehaviour
{
    /// <summary>
    /// Panel GameObject that contains the translation or details text.
    /// </summary>
    [SerializeField] GameObject translationPanel;

    /// <summary>
    /// Optional TextMeshPro label for the toggle button.
    /// Updates between "Show Details" and "Hide Details".
    /// </summary>
    [SerializeField] TMP_Text buttonLabel;

    /// <summary>
    /// Tracks whether the translation panel is currently visible.
    /// </summary>
    private bool isVisible = false;

    /// <summary>
    /// Toggles the visibility of the translation/details panel.
    /// Intended to be called from a UI Button's OnClick event.
    /// </summary>
    public void ToggleTranslation()
    {
        // Flip the visibility state
        isVisible = !isVisible;

        // Show or hide the translation panel
        if (translationPanel != null)
            translationPanel.SetActive(isVisible);

        // Update button label text if assigned
        if (buttonLabel != null)
            buttonLabel.text = isVisible ? "Hide Details" : "Show Details";
    }
}
