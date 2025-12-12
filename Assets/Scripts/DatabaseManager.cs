using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Linq;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userID;
    // Cache for counts loaded from Firebase so UI can query instantly
    private Dictionary<string, Dictionary<string, int>> cachedCounts =
        new Dictionary<string, Dictionary<string, int>>();

    // --- MASTER RECIPE STRUCTURE ---
    // Component -> Ingredients list
    private static readonly Dictionary<string, List<string>> MasterRecipeStructure =
        new Dictionary<string, List<string>>
    {
        {"PineapplePaste", new List<string> {"Pineapple_Ingredient", "Lemon_Ingredient", "Sugar_Ingredient"}},
        {"Biscuit",        new List<string> {"Butter_Ingredient", "Flour_Ingredient", "Water_Ingredient"}}
    };

    // --- REQUIRED COUNTS ---
    // Component -> (Ingredient -> required count)
    private static readonly Dictionary<string, Dictionary<string, int>> RequiredCounts =
        new Dictionary<string, Dictionary<string, int>>
    {
        { "PineapplePaste", new Dictionary<string, int>
            {
                { "Pineapple_Ingredient", 2 },
                { "Sugar_Ingredient", 1 },
                { "Lemon_Ingredient", 1 }
            }
        },
        { "Biscuit", new Dictionary<string, int>
            {
                { "Flour_Ingredient", 2 },
                { "Butter_Ingredient", 1 },
                { "Sugar_Ingredient", 1 },  // if you use sugar in biscuit stage
                { "Water_Ingredient", 1 }
            }
        }
    };

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            LoadUserCollection();
        }
        else
        {
            Debug.LogError("No user logged in. Database operations will fail.");
        }
    }

    // ----------------------------
    // PROFILE CREATION (ints = 0)
    // ----------------------------
    public void CreateNewUserProfile(string userId, string initialMainCard)
    {
        Dictionary<string, object> cardChecklist = new Dictionary<string, object>();

        foreach (var componentEntry in MasterRecipeStructure)
        {
            Dictionary<string, object> ingredientChecklist = new Dictionary<string, object>();

            foreach (string ingredient in componentEntry.Value)
            {
                // IMPORTANT: store as COUNT
                ingredientChecklist.Add(ingredient, 0);
            }

            // timer node (optional, but keeps structure consistent)
            ingredientChecklist["timer"] = new Dictionary<string, object>
            {
                {"startTime", null},
                {"finishTime", null},
                {"durationSeconds", null},
                {"isComplete", false}
            };

            cardChecklist.Add(componentEntry.Key, ingredientChecklist);
        }

        DatabaseReference pathReference = dbReference.Child("users").Child(userId)
            .Child("collection")
            .Child(initialMainCard);

        pathReference.SetValueAsync(cardChecklist);

        Debug.Log($"Created full nested profile (COUNT-based) for user {userId}.");
    }

    public void CheckIfProfileExists(string userId, Action<bool> callback)
    {
        dbReference.Child("users").Child(userId).Child("collection").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                bool profileExists = task.Result != null && task.Result.Exists;
                callback(profileExists);
            });
    }

    // -----------------------------------------
    // ADD INGREDIENT (increment with transaction)
    // -----------------------------------------
    public void AddIngredientToDatabase(string mainCardName, string ingredientName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string componentToSaveTo = GetComponentForIngredient(ingredientName);
        if (string.IsNullOrEmpty(componentToSaveTo))
        {
            Debug.LogError($"Ingredient '{ingredientName}' not found in any component! Cannot save.");
            return;
        }

        int required = GetRequiredCount(componentToSaveTo, ingredientName);

        DatabaseReference ingredientRef = dbReference.Child("users").Child(userID)
            .Child("collection")
            .Child(mainCardName)
            .Child(componentToSaveTo)
            .Child(ingredientName);

        ingredientRef.RunTransaction(mutableData =>
        {
            int current = ConvertDbValueToInt(mutableData.Value);

            // Clamp so spam-click can’t exceed requirement (extra safe)
            if (required > 0 && current >= required)
            {
                return TransactionResult.Success(mutableData);
            }

            current += 1;
            mutableData.Value = current;

            return TransactionResult.Success(mutableData);
        })
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[DB] Failed increment for {ingredientName}: {task.Exception}");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"[DB] Incremented {ingredientName} under {componentToSaveTo}.");
                CheckComponentCompletion(mainCardName, componentToSaveTo);
                LoadUserCollection();
            }
        });
    }

    // -----------------------------------------
    // TIMER START = ALSO RESET COUNTS TO 0
    // -----------------------------------------
    public void StartComponentTimer(string componentName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string componentPath = $"users/{userID}/collection/PineappleTart/{componentName}";
        string timerPath = $"{componentPath}/timer";

        dbReference.Child(componentPath).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result == null || !task.Result.Exists)
                {
                    Debug.LogError($"Error retrieving component data for timer: {componentName}");
                    return;
                }

                DataSnapshot timerSnapshot = task.Result.Child("timer");
                bool timerExists = timerSnapshot.Child("startTime").Exists && timerSnapshot.Child("startTime").Value != null;
                bool isAlreadyComplete = timerSnapshot.Child("isComplete").Exists && timerSnapshot.Child("isComplete").Value != null
                    && (bool)timerSnapshot.Child("isComplete").Value;

                // If running and NOT complete, do nothing
                if (timerExists && !isAlreadyComplete)
                {
                    Debug.Log($"Timer for {componentName} is already running. Action skipped.");
                    return;
                }

                // ✅ Reset ingredient counts when starting/resetting the stage
                ResetComponentCounts("PineappleTart", componentName);

                string startTime = DateTime.UtcNow.ToString("o");

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    {"startTime", startTime},
                    {"finishTime", null},
                    {"isComplete", false},
                    {"durationSeconds", null}
                };

                dbReference.Child(timerPath).UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                            Debug.Log($"Timer STARTED/RESET for {componentName} at {startTime}");
                    });
            });
    }

    private void ResetComponentCounts(string mainCardName, string componentName)
    {
        if (!MasterRecipeStructure.ContainsKey(componentName)) return;

        string componentPath = $"users/{userID}/collection/{mainCardName}/{componentName}";

        Dictionary<string, object> updates = new Dictionary<string, object>();
        foreach (string ing in MasterRecipeStructure[componentName])
        {
            updates[ing] = 0;
        }

        dbReference.Child(componentPath).UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsCompleted)
                    Debug.Log($"[DB] Reset counts to 0 for {componentName}");
            });
    }

    // -----------------------------------------
    // COMPLETION CHECK (counts vs required)
    // -----------------------------------------
    private void CheckComponentCompletion(string mainCardName, string componentName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string componentPath = $"users/{userID}/collection/{mainCardName}/{componentName}";

        dbReference.Child(componentPath).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result == null || !task.Result.Exists) return;

                DataSnapshot componentData = task.Result;

                // If already completed, skip
                if (componentData.Child("timer").Child("isComplete").Exists
                    && componentData.Child("timer").Child("isComplete").Value != null
                    && (bool)componentData.Child("timer").Child("isComplete").Value)
                {
                    Debug.Log($"{componentName} already completed. Skipping check.");
                    return;
                }

                // Check required counts
                bool allCollected = true;

                foreach (string ingredient in MasterRecipeStructure[componentName])
                {
                    int required = GetRequiredCount(componentName, ingredient);
                    int current = 0;

                    var snap = componentData.Child(ingredient);
                    if (snap.Exists)
                        current = ConvertDbValueToInt(snap.Value);

                    if (required > 0 && current < required)
                    {
                        allCollected = false;
                        break;
                    }
                }

                if (!allCollected) return;

                DataSnapshot timerSnapshot = componentData.Child("timer");
                if (!timerSnapshot.Child("startTime").Exists || timerSnapshot.Child("startTime").Value == null) return;

                string startTimeStr = timerSnapshot.Child("startTime").Value.ToString();

                DateTime startTime = DateTime.Parse(startTimeStr).ToUniversalTime();
                DateTime finishTime = DateTime.UtcNow;

                TimeSpan duration = finishTime.Subtract(startTime);
                long durationSeconds = (long)duration.TotalSeconds;


                Dictionary<string, object> results = new Dictionary<string, object>
                {
                    {"finishTime", finishTime.ToString("o")},
                    {"durationSeconds", durationSeconds},
                    {"isComplete", true}
                };

                dbReference.Child(componentPath).Child("timer").UpdateChildrenAsync(results);
                Debug.Log($"--- {componentName} Completed! Time: {durationSeconds} seconds ---");
            });
    }

    // -----------------------------------------
    // READ (treat any >0 as collected for UI lock)
    // -----------------------------------------
public void LoadUserCollection()
{
    if (string.IsNullOrEmpty(userID)) return;

    dbReference.Child("users").Child(userID).Child("collection").GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.Result == null || !task.Result.Exists) return;

            DataSnapshot collectionSnapshot = task.Result;
            DataSnapshot mainCardSnapshot = collectionSnapshot.Child("PineappleTart");

            // 🔹 RESET CACHE
            cachedCounts.Clear();

            foreach (var componentEntry in MasterRecipeStructure)
            {
                string componentName = componentEntry.Key;
                DataSnapshot ingredientsSnapshot = mainCardSnapshot.Child(componentName);

                // Create cache bucket for this component
                cachedCounts[componentName] = new Dictionary<string, int>();

                foreach (string ingredientName in componentEntry.Value)
                {
                    int count = 0;
                    var ingSnap = ingredientsSnapshot.Child(ingredientName);
                    if (ingSnap.Exists)
                        count = ConvertDbValueToInt(ingSnap.Value);

                    // 🔹 STORE IN CACHE
                    cachedCounts[componentName][ingredientName] = count;

                    // 🔹 EXISTING UI LOCK LOGIC (unchanged)
                    GameObject icon = GameObject.Find(ingredientName);
                    if (icon == null) continue;

                    bool isCollected = count > 0;

                    Transform lockPanel = icon.transform.Find("GreyLockPanel");
                    if (lockPanel != null)
                        lockPanel.gameObject.SetActive(!isCollected);
                }
            }

            Debug.Log("[Database] User collection loaded & cache updated.");
        });
}


    // -----------------------------------------
    // Helpers
    // -----------------------------------------
    private string GetComponentForIngredient(string ingredientName)
    {
        if (MasterRecipeStructure["PineapplePaste"].Contains(ingredientName)) return "PineapplePaste";
        if (MasterRecipeStructure["Biscuit"].Contains(ingredientName)) return "Biscuit";
        return "";
    }

    private int GetRequiredCount(string componentName, string ingredientName)
    {
        if (RequiredCounts.ContainsKey(componentName) && RequiredCounts[componentName].ContainsKey(ingredientName))
            return RequiredCounts[componentName][ingredientName];

        // default (if not specified) assume 1
        return 1;
    }

    // Converts DB values that might be bool/int/long/string into int
    private int ConvertDbValueToInt(object value)
    {
        if (value == null) return 0;

        if (value is bool b) return b ? 1 : 0;
        if (value is long l) return (int)l;
        if (value is int i) return i;

        if (int.TryParse(value.ToString(), out int parsed))
            return parsed;

        return 0;
    }
        public void StopComponentTimer(string mainCardName, string componentName)
    {
        if (string.IsNullOrEmpty(userID)) return;

        string componentPath = $"users/{userID}/collection/{mainCardName}/{componentName}";
        string timerPath = $"{componentPath}/timer";

        dbReference.Child(timerPath).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result == null || !task.Result.Exists) return;

                var timerSnap = task.Result;

                // already complete? don't overwrite
                if (timerSnap.Child("isComplete").Exists && timerSnap.Child("isComplete").Value != null
                    && (bool)timerSnap.Child("isComplete").Value)
                    return;

                if (!timerSnap.Child("startTime").Exists || timerSnap.Child("startTime").Value == null)
                {
                    Debug.LogWarning($"[DB] No startTime found for {componentName}, cannot stop timer.");
                    return;
                }

                string startTimeStr = timerSnap.Child("startTime").Value.ToString();
                DateTime startTime = DateTime.Parse(startTimeStr).ToUniversalTime();
                DateTime finishTime = DateTime.UtcNow;

                long durationSeconds = (long)(finishTime - startTime).TotalSeconds;

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    {"finishTime", finishTime.ToString("o")},
                    {"durationSeconds", durationSeconds},
                    {"isComplete", true}
                };

                dbReference.Child(timerPath).UpdateChildrenAsync(updates);
                Debug.Log($"[DB] Timer STOPPED for {componentName}. Duration: {durationSeconds}s");
            });
    }
    public bool AreAllIngredientsCollected(string mainCardName = "PineappleTart")
    {
        // Biscuit
        int butter = GetIngredientCount("Biscuit", "Butter_Ingredient", mainCardName);
        int flour  = GetIngredientCount("Biscuit", "Flour_Ingredient",  mainCardName);
        int water  = GetIngredientCount("Biscuit", "Water_Ingredient",  mainCardName);
        int sugarB = GetIngredientCount("Biscuit", "Sugar_Ingredient",  mainCardName); // only if biscuit has sugar

        // PineapplePaste
        int sugarP = GetIngredientCount("PineapplePaste", "Sugar_Ingredient",     mainCardName);
        int lemon  = GetIngredientCount("PineapplePaste", "Lemon_Ingredient",     mainCardName);
        int pine   = GetIngredientCount("PineapplePaste", "Pineapple_Ingredient", mainCardName); // needs 2

        bool sugarCollected = (sugarB + sugarP) >= 1;

        return butter >= 1 &&
            flour  >= 1 &&
            water  >= 1 &&
            sugarCollected &&
            lemon  >= 1 &&
            pine   >= 2;
    }

    public int GetIngredientCount(string componentName, string ingredientName, string mainCardName = "PineappleTart")
    {
        if (!cachedCounts.ContainsKey(componentName)) return 0;
        if (!cachedCounts[componentName].ContainsKey(ingredientName)) return 0;
        return cachedCounts[componentName][ingredientName];
    }

}