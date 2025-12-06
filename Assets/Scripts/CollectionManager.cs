using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [Header("References")]
    public ImageTracker imageTracker;     // Drag your AR Session Origin here
    public DatabaseManager dbManager;     // Drag the GameObject this script is on
    public Button addToCollectionButton;  // Drag your UI Button here

    void Update()
    {
        // 1. Check if the AR Camera sees a card
        // We read the variable 'currentVisibleCardName' from your ImageTracker script
        if (imageTracker != null && !string.IsNullOrEmpty(imageTracker.currentVisibleCardName))
        {
            // Card found: Enable the button so player can click it
            addToCollectionButton.interactable = true;
        }
        else
        {
            // No card: Disable the button
            addToCollectionButton.interactable = false;
        }
    }

    // --- LINK THIS TO YOUR BUTTON'S ON CLICK EVENT ---
    public void OnAddButtonPressed()
    {
        // 1. Get the name of the card (e.g., "PineappleTart")
        string cardName = imageTracker.currentVisibleCardName;

        if (!string.IsNullOrEmpty(cardName))
        {
            Debug.Log("Button Pressed! Requesting save for: " + cardName);

            // 2. Tell the Database Manager to save it
            dbManager.AddCardToDatabase(cardName);
        }
        else
        {
            Debug.LogWarning("Button pressed but no card name found!");
        }
    }
}
