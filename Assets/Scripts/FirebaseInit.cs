using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        // 1. Check that Firebase dependencies are present
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;

            if (status == DependencyStatus.Available)
            {
                Debug.Log("Firebase READY!");

                // 2. Optional: write a test value to Realtime Database
                TestWriteToDatabase();
            }
            else
            {
                Debug.LogError($"Firebase dependencies not resolved: {status}");
            }
        });
    }

    void TestWriteToDatabase()
    {
        // Make sure Realtime Database is enabled in Firebase console first
        DatabaseReference rootRef = FirebaseDatabase.DefaultInstance.RootReference;

        string path = "testConnection/lastCheck";
        string value = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        rootRef.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"✅ Wrote test value to Firebase at '{path}': {value}");
            }
            else
            {
                Debug.LogError("❌ Failed to write test value to Firebase: " + task.Exception);
            }
        });
    }
}
