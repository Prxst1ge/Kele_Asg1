using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections.Generic;
using System; // Required for Action<bool> callback

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userID;

    [Header("Collection UI")]
    public GameObject[] collectionIcons;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            // Load data for existing users after login
            LoadUserCollection();
        }
        else
        {
            Debug.LogError("No user logged in. Database operations will fail.");
        }
    }

    // --- NEW MASTER RECIPE STRUCTURE ---
    // Defines the full hierarchy: Component -> List of Ingredients
    private static readonly Dictionary<string, List<string>> MasterRecipeStructure =
        new Dictionary<string, List<string>>
    {
        // Component 1
        {"PineapplePaste", new List<string> {"Pineapple", "Lemon", "Sugar"}},
        // Component 2
        {"Biscuit", new List<string> {"Butter", "Flour", "Water"}}
    };


    // --- FUNCTION 1: INITIALIZE PROFILE (Builds the Nested Structure) ---
    public void CreateNewUserProfile(string userId, string initialMainCard)
    {
        Dictionary<string, object> cardChecklist = new Dictionary<string, object>();

        // Loop through each component (PineapplePaste, Biscuit)
        foreach (var componentEntry in MasterRecipeStructure)
        {
            Dictionary<string, object> ingredientChecklist = new Dictionary<string, object>();

            // Loop through ingredients in the component
            foreach (string ingredient in componentEntry.Value)
            {
                ingredientChecklist.Add(ingredient, false);
            }

            // Add the component and its ingredient list (all false) to the main checklist
            cardChecklist.Add(componentEntry.Key, ingredientChecklist);
        }

        // The entire path where the components will be saved
        DatabaseReference pathReference = dbReference.Child("users").Child(userId)
                                                    .Child("collection")
                                                    .Child(initialMainCard); // PineppleTart -> Components

        // Write the entire nested structure in one go
        pathReference.SetValueAsync(cardChecklist);

        Debug.Log($"Created full nested profile for user {userId}.");
    }

    // --- FUNCTION 2: PROFILE EXISTENCE CHECK (unchanged) ---
    public void CheckIfProfileExists(string userId, Action<bool> callback)
    {
        // Check for the existence of the 'collection' node under the UserID
        dbReference.Child("users").Child(userId).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                // ... [Existing check logic] ...
                bool profileExists = task.Result.Exists;
                callback(profileExists);
            });
    }

    // --- FUNCTION 3: WRITE INGREDIENT (Writes to the Correct Component) ---
    public void AddIngredientToDatabase(string mainCardName, string ingredientName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string componentToSaveTo = "";

        // Determine the component based on the ingredient name
        if (MasterRecipeStructure["PineapplePaste"].Contains(ingredientName))
        {
            componentToSaveTo = "PineapplePaste";
        }
        else if (MasterRecipeStructure["Biscuit"].Contains(ingredientName))
        {
            componentToSaveTo = "Biscuit";
        }
        else
        {
            Debug.LogError($"Ingredient '{ingredientName}' not found in any component! Cannot save.");
            return;
        }

        // Saves a single ingredient as a simple boolean (true = collected)
        dbReference.Child("users").Child(userID)
                   .Child("collection")
                   .Child(mainCardName) // PineappleTart
                   .Child(componentToSaveTo) // PineapplePaste OR Biscuit
                   .Child(ingredientName)
                   .SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Success! Saved {ingredientName} under {componentToSaveTo}.");
                    LoadUserCollection();
                }
            });
    }

    // --- FUNCTION 4: READ FUNCTION (Reads from Multiple Components) ---
    public void LoadUserCollection()
    {
        if (string.IsNullOrEmpty(userID)) return;

        dbReference.Child("users").Child(userID).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.Result.Exists) return;

                DataSnapshot collectionSnapshot = task.Result;
                DataSnapshot mainCardSnapshot = collectionSnapshot.Child("PineappleTart");

                // Loop through the Master Recipe Structure to find all possible ingredients
                foreach (var componentEntry in MasterRecipeStructure)
                {
                    string componentName = componentEntry.Key;
                    DataSnapshot ingredientsSnapshot = mainCardSnapshot.Child(componentName);

                    // Loop through every ingredient name defined in the C# code (e.g., "Butter", "Flour")
                    foreach (string ingredientName in componentEntry.Value)
                    {
                        // 1. DYNAMICALLY FIND THE UI ICON IN THE SCENE BY NAME
                        // This requires the UI GameObject in the Hierarchy to be named exactly "Butter", "Flour", etc.
                        GameObject icon = GameObject.Find(ingredientName);

                        if (icon == null)
                        {
                            // Warning if the UI icon isn't present in the scene
                            Debug.LogWarning($"UI Icon not found for ingredient: {ingredientName}. Skipping UI update.");
                            continue;
                        }

                        // 2. CHECK DATABASE STATUS
                        bool isCollected = false;

                        DataSnapshot ingredientValue = ingredientsSnapshot.Child(ingredientName);
                        if (ingredientValue.Exists && (bool)ingredientValue.Value)
                        {
                            isCollected = true;
                        }

                        // 3. CONTROL THE GREY LOCK PANEL
                        // Finds the child object named "GreyLockPanel" inside the found icon GameObject
                        Transform lockPanel = icon.transform.Find("GreyLockPanel");
                        if (lockPanel != null)
                        {
                            // Set the lock panel to ACTIVE if NOT collected (false), and INACTIVE if collected (true).
                            lockPanel.gameObject.SetActive(!isCollected);
                        }
                    }
                }
                // --- NEW LOGIC END ---
            });
    }
}