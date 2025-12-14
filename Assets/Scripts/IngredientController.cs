/*
 * Author: Javier
 * Date: 10 November 2025
 * Description:
 * Controls interaction logic for an ingredient AR object.
 * Handles rotation, add-to-recipe flow, UI feedback, and
 * communication with stage managers and global progress systems.
 */

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles user interaction with an ingredient AR prefab.
/// Includes rotation control, add-to-recipe logic, UI feedback,
/// and validation with stage managers (Pineapple Paste / Biscuit).
/// </summary>
public class IngredientController : MonoBehaviour
{
    /// <summary>
    /// Public read-only access to this ingredient's identifier.
    /// Must match the reference image / database key.
    /// </summary>
    public string IngredientId => ingredientId;

    [Header("Setup")]

    /// <summary>
    /// Unique identifier for this ingredient.
    /// Should match AR reference image and database entry.
    /// </summary>
    [SerializeField] string ingredientId = "pineapple";

    /// <summary>
    /// Transform of the 3D ingredient model to be rotated.
    /// </summary>
    [SerializeField] Transform ingredientModel;

    /// <summary>
    /// Optional per-ingredient status text (e.g. "Added to recipe").
    /// </summary>
    [SerializeField] TMP_Text statusText;

    /// <summary>
    /// Optional tick/check icon displayed when ingredient is added.
    /// </summary>
    [SerializeField] GameObject addedTickIcon;

    [Header("Global Progress UI")]

    /// <summary>
    /// Shared UI text used to display global ingredient progress.
    /// </summary>
    [SerializeField] TMP_Text globalStatusText;

    /// <summary>
    /// Total number of ingredients required for this stage.
    /// </summary>
    [SerializeField] int totalIngredients = 3;

    /// <summary>
    /// Duration (in seconds) that the popup message remains visible.
    /// </summary>
    [SerializeField] float popupDuration = 1.5f;

    [Header("Rotation")]

    /// <summary>
    /// Rotation speed (degrees per second) for the ingredient model.
    /// </summary>
    [SerializeField] float rotationSpeed = 45f;

    /// <summary>
    /// Whether the ingredient model is currently rotating.
    /// </summary>
    bool isRotating = false;

    /// <summary>
    /// Whether this ingredient has already been added to the recipe.
    /// Prevents duplicate additions.
    /// </summary>
    bool isAdded = false;

    /// <summary>
    /// Reference to the currently running popup coroutine.
    /// </summary>
    Coroutine popupRoutine;

    /// <summary>
    /// Continuously rotates the ingredient model while rotation is enabled.
    /// </summary>
    void Update()
    {
        if (isRotating && ingredientModel != null)
        {
            ingredientModel.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    // ------------------- ROTATE -------------------

    /// <summary>
    /// Toggles continuous rotation of the ingredient model.
    /// Called by the Rotate button.
    /// </summary>
    public void ToggleRotate()
    {
        if (ingredientModel == null)
        {
            Debug.LogWarning("[Ingredient] ingredientModel is NULL for " + ingredientId);
            return;
        }

        isRotating = !isRotating;
        Debug.Log($"[Ingredient] Rotate toggle for {ingredientId}: {isRotating}");
    }

    // ------------------ ADD TO RECIPE ------------------

    /// <summary>
    /// Adds this ingredient to the recipe if valid for the current stage.
    /// Handles validation, UI updates, progress tracking, and disables
    /// further interaction once added.
    /// </summary>
    public void AddToRecipe()
    {
        // If already added, show popup and stop
        if (isAdded)
        {
            ShowGlobalMessage("Ingredient already added.");
            Debug.Log($"[Ingredient] {ingredientId} already added.");
            return;
        }

        // Pineapple Paste stage validation
        if (PineapplePasteStageManager.Instance != null)
        {
            bool accepted = PineapplePasteStageManager.Instance.RegisterIngredient(ingredientId);

            if (!accepted)
            {
                Debug.Log($"[Ingredient] Stage rejected ingredient: {ingredientId}");
                return;
            }
        }

        // Biscuit stage validation
        if (BiscuitStageManager.Instance != null)
        {
            bool acceptedBiscuit = BiscuitStageManager.Instance.RegisterIngredient(ingredientId);
            if (!acceptedBiscuit)
            {
                Debug.Log($"[Ingredient] Biscuit stage rejected ingredient: {ingredientId}");
                return;
            }
        }

        // Mark as added (first time only)
        isAdded = true;
        IngredientProgressManager.MarkAdded(ingredientId);

        if (statusText != null)
            statusText.text = "Added to recipe!";

        if (addedTickIcon != null)
            addedTickIcon.SetActive(true);

        isRotating = false;

        if (ingredientModel != null)
            StartCoroutine(PopEffect(ingredientModel));

        int addedCount = IngredientProgressManager.GetAddedCount();
        string msg = $"{addedCount}/{totalIngredients} ingredients added.";
        ShowGlobalMessage(msg);

        Debug.Log($"[Ingredient] {ingredientId} added to recipe.");

        // Disable Add button after use
        var btn = GetComponentInChildren<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;
    }

    // ---------------- UI POPUP --------------------

    /// <summary>
    /// Displays a temporary popup message in the global UI.
    /// </summary>
    /// <param name="message">Message to display.</param>
    void ShowGlobalMessage(string message)
    {
        if (globalStatusText == null) return;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PopupRoutine(message));
    }

    /// <summary>
    /// Coroutine that shows and hides the popup message after a delay.
    /// </summary>
    IEnumerator PopupRoutine(string message)
    {
        globalStatusText.gameObject.SetActive(true);
        globalStatusText.text = message;
        yield return new WaitForSeconds(popupDuration);
        globalStatusText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Applies a short scale "pop" animation to the ingredient model.
    /// </summary>
    /// <param name="target">Transform to animate.</param>
    IEnumerator PopEffect(Transform target)
    {
        Vector3 original = target.localScale;
        target.localScale = original * 1.2f;
        yield return new WaitForSeconds(0.1f);
        target.localScale = original;
    }
}
