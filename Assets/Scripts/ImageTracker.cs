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

    // Assign ONE of these per scene:
    [SerializeField] private PineapplePasteStageManager pineapplePasteStageManager;
    [SerializeField] private BiscuitStageManager biscuitStageManager;

    // imageName -> prefab template
    private readonly Dictionary<string, GameObject> prefabLookup =
        new Dictionary<string, GameObject>();

    // each tracked image (unique ID) -> spawned instance
    private readonly Dictionary<TrackableId, GameObject> spawnedByTrackableId =
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
        void OnDestroy()
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

    // ------------------------------------------------------------------
    //  Spawn a prefab for this specific tracked image
    // ------------------------------------------------------------------
    void CreateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;

        // 1. Stage validation (BLOCK invalid cards for this stage)
        bool allowed = true;

        if (pineapplePasteStageManager != null)
        {
            allowed = pineapplePasteStageManager.ValidateCard(imageName);
        }
        else if (biscuitStageManager != null)
        {
            allowed = biscuitStageManager.ValidateCard(imageName);
        }

        if (!allowed)
        {
            Debug.Log($"[ImageTracker] Card '{imageName}' is not valid for this stage.");
            return; // do NOT spawn anything
        }

        // 2. Get prefab template
        if (!prefabLookup.TryGetValue(imageName, out GameObject prefab))
        {
            Debug.LogWarning($"[ImageTracker] No prefab found for image '{imageName}'.");
            return;
        }

        // 3. Instantiate and parent to tracked image
        GameObject instance = Instantiate(
            prefab,
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );
        instance.transform.SetParent(trackedImage.transform);

        spawnedByTrackableId[trackedImage.trackableId] = instance;

        // 4. Wire up the Add button → IngredientController + CollectionManager
        LinkAddButton(instance, imageName);

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

    // ------------------------------------------------------------------
    //  Link Add-to-Recipe button: IngredientController + CollectionManager
    // ------------------------------------------------------------------
    void LinkAddButton(GameObject spawnedPrefab, string cardName)
    {
        // IngredientController on this prefab
        IngredientController controller =
            spawnedPrefab.GetComponentInChildren<IngredientController>();

        // Find the "Add to Recipe" button (by name containing "Add")
        Button addButton = null;
        Button[] buttons = spawnedPrefab.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.name.Contains("Add")) // e.g. "Add to Recipe_Button"
            {
                addButton = b;
                break;
            }
        }

        if (addButton == null)
        {
            Debug.LogWarning($"[ImageTracker] Could not find 'Add' button in prefab for {cardName}.");
            return;
        }

        // Clear old listeners to avoid duplicates
        addButton.onClick.RemoveAllListeners();

        // 1) Local ingredient behaviour
        if (controller != null)
        {
            addButton.onClick.AddListener(controller.AddToRecipe);
        }
        else
        {
            Debug.LogWarning($"[ImageTracker] No IngredientController found in prefab for {cardName}.");
        }

        // 2) Collection / Firebase behaviour
        if (collectionManager != null)
        {
            // Uses cardName directly so it works even with multiple copies
            addButton.onClick.AddListener(
                () => collectionManager.OnUnlockSpecificIngredient(cardName)
            );
        }
        else
        {
            Debug.LogWarning("[ImageTracker] collectionManager reference is missing.");
        }
    }

}
