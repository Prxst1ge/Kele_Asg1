using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollectionManager : MonoBehaviour
{
    [Header("References")]
    public ImageTracker imageTracker;     // AR tracking source
    public DatabaseManager dbManager;     // Data saving destination

    // --- Data structure for the Inspector List ---
    [System.Serializable]
    public struct IngredientButtonLink
    {
        // MUST match the AR Reference Image name (e.g., "Butter")
        public string cardName;
        // The specific Button UI GameObject
        public Button specificButton;
    }



    // --- This function must be linked to the OnClick event of ALL collection buttons. ---
    // The button passes its specific ingredientName via the Inspector/dynamic link.
    public void OnUnlockSpecificIngredient(string ingredientName)
    {
        // IMPORTANT: Define the main card this ingredient is for.
        string mainCardName = "PineappleTart";

        if (!string.IsNullOrEmpty(ingredientName))
        {
            Debug.Log($"Collection Event Triggered for: {ingredientName} under {mainCardName}");

            // Send the ingredient name and its parent card to the DatabaseManager
            dbManager.AddIngredientToDatabase(mainCardName, ingredientName);
        }
    }
}