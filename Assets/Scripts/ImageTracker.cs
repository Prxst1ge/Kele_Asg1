using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Prefabs to spawn (name must match reference image name)")]
    [SerializeField] private GameObject[] placeablePrefabs;

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
}
