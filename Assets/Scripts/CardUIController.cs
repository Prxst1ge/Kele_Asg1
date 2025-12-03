using UnityEngine;
using TMPro; // only needed if you want to change button text

public class CardUIController : MonoBehaviour
{
    [SerializeField] GameObject translationPanel;  // the panel that contains the translation text
    [SerializeField] TMP_Text buttonLabel;         // optional: assign the button's Text (TMP) here
    private bool isVisible = false;                // track whether the panel is currently showing

    // Calls Button's OnClick()
    public void ToggleTranslation()
    {
        // Flip the visibility state
        isVisible = !isVisible;

        // Show/hide the panel
        if (translationPanel != null)
            translationPanel.SetActive(isVisible);

        // Optional: update the button text
        if (buttonLabel != null)
            buttonLabel.text = isVisible ? "Hide Details" : "Show Details";

    }
}


