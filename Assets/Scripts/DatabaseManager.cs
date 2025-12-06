using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userID;

    [Header("Collection UI")]
    // Drag your "Inventory/Collection" UI Images here. 
    // IMPORTANT: Name these GameObjects EXACTLY the same as your tracking cards (e.g. "PineappleTart")
    public GameObject[] collectionIcons;

    void Start()
    {
        // 1. Initialize Database Reference
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // 2. Get the Current User's ID from the Auth system
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            Debug.Log("Database initialized for User: " + userID);

            // 3. Check the database immediately to update the UI
            LoadUserCollection();
        }
        else
        {
            Debug.LogError("No user logged in. Database operations will fail.");
        }
    }

    // --- WRITE FUNCTION (Saves data & Creates Folder) ---
    // This is called when you press the "Add to Collection" button
    public void AddCardToDatabase(string cardName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        // DEFINING THE PATH:
        // users -> [UserID] -> collection -> [CardName]
        // Example: users/AbCd123/collection/PineappleTart

        dbReference.Child("users").Child(userID).Child("collection").Child(cardName).SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Save failed: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Success! Saved " + cardName + " to Firebase.");

                    // Reload the UI so the item turns white immediately
                    LoadUserCollection();
                }
            });
    }

    // --- READ FUNCTION (Updates the Collection UI) ---
    public void LoadUserCollection()
    {
        if (string.IsNullOrEmpty(userID)) return;

        // Go to the user's specific folder
        dbReference.Child("users").Child(userID).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Load failed: " + task.Exception);
                    return;
                }

                // "Snapshot" contains all the data found at that path
                DataSnapshot snapshot = task.Result;

                // Loop through your UI Icons to check which ones are unlocked
                foreach (GameObject icon in collectionIcons)
                {
                    // Check if the database has this specific card name
                    if (snapshot.HasChild(icon.name))
                    {
                        // UNLOCK IT! (Set Color to White/Full)
                        var img = icon.GetComponent<Image>();
                        if (img != null) img.color = Color.white;
                    }
                    else
                    {
                        // LOCK IT! (Set Color to Dark Gray/Black)
                        var img = icon.GetComponent<Image>();
                        if (img != null) img.color = Color.black;
                    }
                }
            });
    }
}