/*
 * Author: Javier
 * Date: 10 November 2025
 * Description:
 * ImageTracker handles AR image detection and spawning of corresponding
 * ingredient prefabs. It validates cards against the current stage,
 * manages multiple tracked image instances, and wires UI buttons to
 * ingredient logic and Firebase collection updates.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

/// <summary>
/// Manages AR image tracking, prefab spawning, and interaction wiring.
/// Supports multiple tracked copies of the same image and stage-based
/// validation for Pineapple Paste and Biscuit scenes.
/// </summary>
public class ImageTracker : MonoBehaviour
{
    /// <summary>
    /// AR Foundation manager responsible for tracked image detection.
    /// </summary>
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    /// <summary>
    /// Prefabs that will be spawned when their corresponding reference
    /// images are detected. Prefab names must match reference image names.
    /// </summary>
    [Header("Prefabs to spawn (name must match reference image name)")]
    [SerializeField] private GameObject[] placeablePrefabs;

    /// <summary>
    /// Handles database updates when ingredients are added to the recipe.
    /// </summary>
    [Header("Managers")]
    [SerializeField] private CollectionManager collectionManager;

    /// <summary>
    /// Stage manager for the Pineapple Paste AR scene.
    /// Only one stage manager should be assigned per scene.
    /// </summary>
    [SerializeField] private PineapplePasteStageManager pineapplePasteStageManager;

    /// <summary>
    /// Stage manager for the Biscuit AR scene.
    /// Only one stage manager should be assigned per scene.
    /// </summary>
    [SerializeField] private BiscuitStageManager biscuitStageManager;

    /// <summary>
    /// Maps reference image names to prefab templates.
    /// </summary>
    private readonly Dictionary<string, GameObject> prefabLookup =
        new Dictionary<string, GameObject>();

    /// <summary>
    /// Tracks spawned prefab instances per unique tracked image ID.
    /// Allows multiple physical copies of the same image to exist.
    /// </summary>
    private readonly Dictionary<TrackableId, GameObject> spawnedByTrackableId =
        new Dictionary<TrackableId, GameObject>();

    /// <summary>
    /// Name of the currently visible card (used by other systems if needed).
    /// </summary>
    public string currentVisibleCardName = "";

    /// <summary>
    /// Builds the prefab lookup table and ensures ARTrackedImageManager exists.
    /// </summary>
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

    /// <summary>
    /// Subscribes to AR image tracking events.
    /// </summary>
    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    /// <summary>
    /// Unsubscribes from AR image tracking events.
    /// </summary>
    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    /// <summary>
    /// Ensures event handlers are removed if the object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    /// <summary>
    /// Handles AR tracked image lifecycle events.
    /// </summary>
    /// <param name="args">Added, updated, and removed tracked images.</param>
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            CreateInstanceFor(img);

        foreach (var img in args.updated)
            UpdateInstanceFor(img);

        foreach (var img in args.removed)
            RemoveInstanceFor(img);
    }

    /// <summary>
    /// Creates and attaches a prefab instance for a newly detected tracked image.
    /// Performs stage validation before spawning.
    /// </summary>
    /// <param name="trackedImage">The detected AR tracked image.</param>
    void CreateInstanceFor(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;

        // Stage validation
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
            return;
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

        LinkAddButton(instance, imageName);
        currentVisibleCardName = imageName;
    }

    /// <summary>
    /// Updates an existing prefab instance based on tracking state.
    /// </summary>
    /// <param name="trackedImage">The updated tracked image.</param>
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

    /// <summary>
    /// Removes and destroys a prefab instance when its tracked image is lost.
    /// </summary>
    /// <param name="trackedImage">The removed tracked image.</param>
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

    /// <summary>
    /// Links the "Add to Recipe" button in the spawned prefab to both
    /// the IngredientController logic and the CollectionManager (Firebase).
    /// </summary>
    /// <param name="spawnedPrefab">The instantiated ingredient prefab.</param>
    /// <param name="cardName">The reference image / ingredient identifier.</param>
    void LinkAddButton(GameObject spawnedPrefab, string cardName)
    {
        IngredientController controller =
            spawnedPrefab.GetComponentInChildren<IngredientController>();

        Button addButton = null;
        Button[] buttons = spawnedPrefab.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.name.Contains("Add"))
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

        addButton.onClick.RemoveAllListeners();

        if (controller != null)
        {
            addButton.onClick.AddListener(controller.AddToRecipe);
        }

        if (collectionManager != null)
        {
            addButton.onClick.AddListener(
                () => collectionManager.OnUnlockSpecificIngredient(cardName)
            );
        }
    }
}
