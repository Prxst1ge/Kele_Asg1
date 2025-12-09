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

    private static readonly List<string> MasterIngredientList = new List<string>
    {
        "Butter",
        "Flour",
        "Pineapple",
    };
    // --- FUNCTION 1: INITIALIZE PROFILE (CALLED BY AUTHMANAGER) ---
    public void CreateNewUserProfile(string userId, string initialMainCard)
    {
        Dictionary<string, object> ingredientChecklist = new Dictionary<string, object>();

        // Use the Master List to create the checklist with FALSE values
        foreach (string ingredient in MasterIngredientList)
        {
            ingredientChecklist.Add(ingredient, false);
        }

        // The entire path where the ingredients will be saved
        DatabaseReference pathReference = dbReference.Child("users").Child(userId)
                                                    .Child("collection")
                                                    .Child(initialMainCard)
                                                    .Child("ingredients");

        // Write the entire checklist Dictionary in one go
        pathReference.SetValueAsync(ingredientChecklist);

        Debug.Log($"Created profile checklist for user {userId}. Initial ingredients set to FALSE.");
    }

    // --- FUNCTION 2: PROFILE EXISTENCE CHECK (CALLED BY AUTHMANAGER) ---
    public void CheckIfProfileExists(string userId, Action<bool> callback)
    {
        // Check for the existence of the 'collection' node under the UserID
        dbReference.Child("users").Child(userId).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Database check failed: " + task.Exception);
                    callback(false);
                    return;
                }

                bool profileExists = task.Result.Exists;
                callback(profileExists);
            });
    }

    // --- FUNCTION 3: WRITE INGREDIENT (Called by CollectionManager) ---
    public void AddIngredientToDatabase(string mainCardName, string ingredientName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        // Saves a single ingredient as a simple boolean (true = collected)
        dbReference.Child("users").Child(userID)
                   .Child("collection")
                   .Child(mainCardName)
                   .Child("ingredients")
                   .Child(ingredientName)
                   .SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Success! Saved {ingredientName} under {mainCardName}.");
                    LoadUserCollection();
                }
            });
    }

    // --- FUNCTION 4: READ FUNCTION (Updates the Collection UI) ---
    public void LoadUserCollection()
    {
        if (string.IsNullOrEmpty(userID)) return;

        // Go to the user's specific collection folder
        dbReference.Child("users").Child(userID).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.Result.Exists) return;

                DataSnapshot collectionSnapshot = task.Result;

                // Assuming your UI icons are named: PineappleTart_Flour, PineappleTart_Butter, etc.

                // Loop through your UI Icons to check which ones are unlocked
                foreach (GameObject icon in collectionIcons)
                {
                    // Assuming "PineappleTart" is the main card name for simplicity
                    DataSnapshot ingredientsSnapshot = collectionSnapshot.Child("PineappleTart").Child("ingredients");

                    // Check if the database has this specific ingredient name and if its value is 'true'
                    if (ingredientsSnapshot.HasChild(icon.name) && (bool)ingredientsSnapshot.Child(icon.name).Value)
                    {
                        // UNLOCK IT! (Value is true)
                        var img = icon.GetComponent<Image>();
                        if (img != null) img.color = Color.white;
                    }
                    else
                    {
                        // LOCK IT! (Either missing, or value is false)
                        var img = icon.GetComponent<Image>();
                        if (img != null) img.color = Color.black;
                    }
                }
            });
    }


}