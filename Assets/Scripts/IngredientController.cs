using System.Collections;
using UnityEngine;
using TMPro;

public class IngredientController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] string ingredientId = "pineapple";
    [SerializeField] Transform ingredientModel;
    [SerializeField] TMP_Text statusText;          // per-ingredient status (optional)
    [SerializeField] GameObject addedTickIcon;     // per-ingredient tick (optional)

    [Header("Global Progress UI")]
    [SerializeField] TMP_Text globalStatusText;    // shared popup text (e.g. "1/3 ingredients added")
    [SerializeField] int totalIngredients = 3;
    [SerializeField] float popupDuration = 1.5f;   // seconds

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 45f;
    bool isRotating = false;

    bool isAdded = false;
    Coroutine popupRoutine;

    void Update()
    {
        if (isRotating && ingredientModel != null)
        {
            ingredientModel.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    // Called by Rotate button
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

    // Called by Add to Recipe button
    public void AddToRecipe()
    {
        // If already added, just show "already added" popup
        if (isAdded)
        {
            ShowGlobalMessage("Ingredient already added.");
            Debug.Log($"[Ingredient] {ingredientId} already added.");
            return;
        }

        // Mark as added first time
        isAdded = true;
        IngredientProgressManager.MarkAdded(ingredientId);

        // Optional per-ingredient UI
        if (statusText != null)
            statusText.text = "Added to recipe!";

        if (addedTickIcon != null)
            addedTickIcon.SetActive(true);

        // Stop rotation when added (optional)
        isRotating = false;

        // Little pop effect on the model
        if (ingredientModel != null)
            StartCoroutine(PopEffect(ingredientModel));

        // Global message: X / total ingredients
        int addedCount = IngredientProgressManager.GetAddedCount();
        string msg = $"{addedCount}/{totalIngredients} ingredients added.";
        ShowGlobalMessage(msg);

        Debug.Log($"[Ingredient] {ingredientId} added to recipe. Now {addedCount}/{totalIngredients}");
    }

    void ShowGlobalMessage(string message)
    {
        if (globalStatusText == null) return;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PopupRoutine(message));
    }

    IEnumerator PopupRoutine(string message)
    {
        globalStatusText.gameObject.SetActive(true);
        globalStatusText.text = message;
        yield return new WaitForSeconds(popupDuration);
        globalStatusText.gameObject.SetActive(false);
    }

    IEnumerator PopEffect(Transform target)
    {
        Vector3 original = target.localScale;
        target.localScale = original * 1.2f;
        yield return new WaitForSeconds(0.1f);
        target.localScale = original;
    }
}
