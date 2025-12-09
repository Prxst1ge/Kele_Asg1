using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class ImageTracker : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Prefabs to spawn (name must match reference image name)")]
    [SerializeField] private GameObject[] placeablePrefabs;

    [SerializeField] private CollectionManager collectionManager;
    // imageName -> prefab template
    private Dictionary<string, GameObject> prefabLookup =
        new Dictionary<string, GameObject>();

    // each tracked image (unique ID) -> spawned instance
    private Dictionary<TrackableId, GameObject> spawnedByTrackableId =
        new Dictionary<TrackableId, GameObject>();

    // optional: for other scripts that want to know which card was last seen
    public string currentVisibleCardName = "";

    void Awake()
    {
        if (trackedImageManager == null)
            trackedImageManager = GetComponent<ARTrackedImageManager>();

        // build lookup from prefab name -> prefab
        prefabLookup.Clear();
        foreach (GameObject prefab in placeablePrefabs)
        {
            if (prefab == null) continue;

            string key = prefab.name;
            if (!prefabLookup.ContainsKey(key))
                prefabLookup.Add(key, prefab);
            else
                Debug.LogWarning($"[ImageTracker] Duplicate prefab name '{key}' in placeablePrefabs.");
        }
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // New images detected
        foreach (ARTrackedImage trackedImage in args.added)
        {
            CreateInstanceFor(trackedImage);
        }

        // Existing images updated (pose / tracking state)
        foreach (ARTrackedImage trackedImage in args.updated)
        {
            UpdateInstanceFor(trackedImage);
        }

        // Images lost
        foreach (ARTrackedImage trackedImage in args.removed)
        {
            RemoveInstanceFor(trackedImage);
        }
    }

    void CreateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;

        if (!prefabLookup.TryGetValue(imageName, out GameObject prefab))
        {
            Debug.LogWarning($"[ImageTracker] No prefab found for image '{imageName}'.");
            return;
        }

        // spawn a NEW instance for this specific detected image
        GameObject instance = Instantiate(prefab,
            trackedImage.transform.position,
            trackedImage.transform.rotation);

        // parent to the tracked image so it follows the card
        instance.transform.SetParent(trackedImage.transform);

        spawnedByTrackableId[trackedImage.trackableId] = instance;

        currentVisibleCardName = imageName;
    }

    void UpdateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        if (!spawnedByTrackableId.TryGetValue(trackedImage.trackableId, out GameObject instance))
            return;

        // enable / disable based on tracking state
        bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
        instance.SetActive(isTracking);

        if (isTracking)
            currentVisibleCardName = trackedImage.referenceImage.name;
    }

    void RemoveInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        if (spawnedByTrackableId.TryGetValue(trackedImage.trackableId, out GameObject instance))
        {
            Destroy(instance);
            spawnedByTrackableId.Remove(trackedImage.trackableId);
        }

        // if we just lost the last card of that type, you can optionally clear
        if (currentVisibleCardName == trackedImage.referenceImage.name)
            currentVisibleCardName = "";
    }

    void LinkButtonToManager(GameObject spawnedPrefab, string cardName)
    {
        // 1. Find the button component inside the spawned prefab
        // We use GetComponentInChildren to find the button nested deep inside your Prefab.
        // NOTE: The button must be named exactly "Add to Recipe_Button" for this to work.
        Button addButton = spawnedPrefab.GetComponentInChildren<Button>();

        if (addButton != null && collectionManager != null)
        {
            // 2. Remove any old events (crucial for cleaning up prefabs)
            addButton.onClick.RemoveAllListeners();

            // 3. Link the button's OnClick event to the CollectionManager function
            // The CollectionManager reads the currently visible card name, so we just call the function.
            addButton.onClick.AddListener(collectionManager.OnAddButtonPressed);
        }
        else
        {
            // This warning helps diagnose if the button name is wrong or the reference is missing.
            Debug.LogWarning($"Failed to link button in prefab for {cardName}. Check button name or CollectionManager reference.");
        }
    }

}
