using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollectionManager : MonoBehaviour
{
    [Header("References")]
    public ImageTracker imageTracker;     // Drag your AR Session Origin here
    public DatabaseManager dbManager;     // Drag the GameObject this script is on

    [System.Serializable]
    public struct IngredientButtonLink
    {
        public string cardName;      // e.g. "PineappleTart"
        public Button specificButton; // Drag the specific UI button here
    }

    public List<IngredientButtonLink> allButtons;

    void Start()
    {
        // Hide ALL buttons at start
        foreach (var item in allButtons)
        {
            if (item.specificButton != null)
                item.specificButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        string currentCard = "";

        if (imageTracker != null)
        {
            currentCard = imageTracker.currentVisibleCardName;
        }
        // 1. Check if the AR Camera sees a card
        // We read the variable 'currentVisibleCardName' from your ImageTracker script
        foreach (var item in allButtons)
        {
            if (item.specificButton == null) continue;

            // If this button matches the card we are looking at...
            if (!string.IsNullOrEmpty(currentCard) && item.cardName == currentCard)
            {
                // SHOW IT!
                item.specificButton.gameObject.SetActive(true);
                item.specificButton.interactable = true;
            }
            else
            {
                // HIDE IT! (Because we aren't looking at this specific ingredient)
                item.specificButton.gameObject.SetActive(false);
            }
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
