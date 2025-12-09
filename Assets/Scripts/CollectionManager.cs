using UnityEngine;
using UnityEngine.UI; // Still needed for the OnAddButtonPressed definition
using System.Collections.Generic;

public class CollectionManager : MonoBehaviour
{
    [Header("References")]
    public ImageTracker imageTracker;
    public DatabaseManager dbManager;

    // No Start() or Update() methods needed here anymore.

    // --- LINK ALL YOUR BUTTONS TO THIS SAME FUNCTION ---
    public void OnAddButtonPressed()
    {
        // Get the ingredient name that is currently visible in the ImageTracker
        string ingredientName = imageTracker.currentVisibleCardName;

        if (!string.IsNullOrEmpty(ingredientName))
        {
            // IMPORTANT: You must define the main card this ingredient belongs to.
            // Assuming for now all scanned ingredients belong to "PineappleTart".
            string mainCardName = "PineappleTart";

            Debug.Log($"Button Pressed! Requesting save for: {ingredientName} under {mainCardName}");

            // Send the ingredient name and its parent card to the DatabaseManager
            dbManager.AddIngredientToDatabase(mainCardName, ingredientName);
        }
        else
        {
            Debug.LogWarning("Tried to save, but no card is currently visible!");
        }
    }
}