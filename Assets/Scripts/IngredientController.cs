using System.Collections;
using UnityEngine;
using TMPro;

public class IngredientController : MonoBehaviour
{
    [Header("Setup")]
    public string IngredientId => ingredientId;
    [SerializeField] string ingredientId = "pineapple";
    
    [SerializeField] Transform ingredientModel;
    [SerializeField] TMP_Text statusText;          // per-ingredient status (optional)
    [SerializeField] GameObject addedTickIcon;     // per-ingredient tick (optional)

    [Header("Global Progress UI")]
    [SerializeField] TMP_Text globalStatusText;    
    [SerializeField] int totalIngredients = 3;
    [SerializeField] float popupDuration = 1.5f;

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

    // ------------------- ROTATE -------------------
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
    public void AddToRecipe()
    {
        // If already added, show popup and stop
        if (isAdded)
        {
            ShowGlobalMessage("Ingredient already added.");
            Debug.Log($"[Ingredient] {ingredientId} already added.");
            return;
        }

        // 🔥 NEW PART — ask stage manager if allowed
        if (PineapplePasteStageManager.Instance != null)
        {
            bool accepted = PineapplePasteStageManager.Instance.RegisterIngredient(ingredientId);

            if (!accepted)
            {
                // Stage rejected this ingredient → do NOT add anything here
                Debug.Log($"[Ingredient] Stage rejected ingredient: {ingredientId}");
                return;
            }
        }

        // Biscuit stage support (only does something in Biscuit_AR scene)
        if (BiscuitStageManager.Instance != null)
        {
            bool acceptedBiscuit = BiscuitStageManager.Instance.RegisterIngredient(ingredientId);
            if (!acceptedBiscuit)
            {
                Debug.Log($"[Ingredient] Biscuit stage rejected ingredient: {ingredientId}");
                return;
            }
        }

        // Mark as added FIRST TIME
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

        // Optional: disable the button so it can't be spam-clicked
        var btn = GetComponentInChildren<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;

    }

    // ---------------- UI POPUP --------------------
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
