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

    [Header("Managers")]
    [SerializeField] private CollectionManager collectionManager;
    [SerializeField] private PineapplePasteStageManager pineapplePasteStageManager;   // NEW

    // imageName -> prefab template
    private Dictionary<string, GameObject> prefabLookup =
        new Dictionary<string, GameObject>();

    // each tracked image (unique ID) -> spawned instance
    private Dictionary<TrackableId, GameObject> spawnedByTrackableId =
        new Dictionary<TrackableId, GameObject>();

    public string currentVisibleCardName = "";

    void Awake()
    {
        if (trackedImageManager == null)
            trackedImageManager = GetComponent<ARTrackedImageManager>();

        prefabLookup.Clear();
        foreach (GameObject prefab in placeablePrefabs)
        {
            if (prefab == null) continue;

            string key = prefab.name;
            if (!prefabLookup.ContainsKey(key))
                prefabLookup.Add(key, prefab);
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
        foreach (ARTrackedImage trackedImage in args.added)
            CreateInstanceFor(trackedImage);

        foreach (ARTrackedImage trackedImage in args.updated)
            UpdateInstanceFor(trackedImage);

        foreach (ARTrackedImage trackedImage in args.removed)
            RemoveInstanceFor(trackedImage);
    }

    void CreateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;

        // -----------------------------------------------------
        // UPDATED — Validate only, do NOT count yet.
        // -----------------------------------------------------
        if (pineapplePasteStageManager != null)
        {
            bool allowed = pineapplePasteStageManager.ValidateCard(imageName);
            if (!allowed)
            {
                Debug.Log($"[ImageTracker] Card '{imageName}' is not valid for this stage.");
                return;
            }
        }

        if (!prefabLookup.TryGetValue(imageName, out GameObject prefab))
        {
            Debug.LogWarning($"[ImageTracker] No prefab found for image '{imageName}'.");
            return;
        }

        GameObject instance = Instantiate(
            prefab,
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );

        instance.transform.SetParent(trackedImage.transform);
        spawnedByTrackableId[trackedImage.trackableId] = instance;

        // -----------------------------------------------------
        // UPDATED — Link AddToRecipe button to IngredientController,
        // NOT directly to CollectionManager.
        // -----------------------------------------------------
        LinkAddButtonToIngredientController(instance);

        currentVisibleCardName = imageName;
    }

    void UpdateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        if (!spawnedByTrackableId.TryGetValue(trackedImage.trackableId, out GameObject instance))
            return;

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

        if (currentVisibleCardName == trackedImage.referenceImage.name)
            currentVisibleCardName = "";
    }

    // -----------------------------------------------------
    // UPDATED — New linking method
    // Finds IngredientController → finds AddToRecipe button → connects everything.
    // -----------------------------------------------------
    void LinkAddButtonToIngredientController(GameObject spawnedPrefab)
    {
        IngredientController controller = spawnedPrefab.GetComponentInChildren<IngredientController>();

        if (controller == null)
        {
            Debug.LogWarning("[ImageTracker] No IngredientController found in prefab.");
            return;
        }

        Button[] buttons = spawnedPrefab.GetComponentsInChildren<Button>(true);

        foreach (Button b in buttons)
        {
            if (b.name.Contains("Add"))
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(controller.AddToRecipe);
                return;
            }
        }

        Debug.LogWarning("[ImageTracker] Could not find 'Add to Recipe' button in prefab.");
    }
}
