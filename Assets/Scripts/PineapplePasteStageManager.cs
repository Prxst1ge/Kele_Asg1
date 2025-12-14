/*
 * Author: Javier
 * Date: 8 December 2025
 * Description:
 * Manages the Pineapple Paste AR stage flow.
 * Handles ingredient validation, add-to-recipe tracking, mixing logic,
 * bowl interactions, UI feedback, stopwatch control, and database updates
 * when the stage is completed.
 */

using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the full gameplay flow for the Pineapple Paste stage.
/// This includes ingredient validation, progress tracking,
/// mixing interactions, UI updates, and stage completion handling.
/// </summary>
public class PineapplePasteStageManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the PineapplePasteStageManager.
    /// Ensures only one instance exists at runtime.
    /// </summary>
    public static PineapplePasteStageManager Instance { get; private set; }

    [Header("Ingredient IDs (must match reference image names)")]

    /// <summary>
    /// Reference image / database key for pineapple ingredient.
    /// </summary>
    [SerializeField] private string pineappleId = "Pineapple_Ingredient";

    /// <summary>
    /// Reference image / database key for sugar ingredient.
    /// </summary>
    [SerializeField] private string sugarId = "Sugar_Ingredient";

    /// <summary>
    /// Reference image / database key for lemon ingredient.
    /// </summary>
    [SerializeField] private string lemonId = "Lemon_Ingredient";

    [Header("Required amounts (Add to Recipe step)")]

    /// <summary>
    /// Required number of pineapples to progress.
    /// </summary>
    [SerializeField] private int requiredPineapples = 2;

    /// <summary>
    /// Required number of sugar units to progress.
    /// </summary>
    [SerializeField] private int requiredSugar = 1;

    /// <summary>
    /// Required number of lemons to progress.
    /// </summary>
    [SerializeField] private int requiredLemon = 1;

    [Header("Stage UI")]

    /// <summary>
    /// UI text displaying ingredient progress counters.
    /// </summary>
    [SerializeField] private TMP_Text stageStatusText;

    /// <summary>
    /// UI prompt shown when mixing stage becomes available.
    /// </summary>
    [SerializeField] private GameObject mixBowlUI;

    [Header("Invalid Card Popup")]

    /// <summary>
    /// Panel shown when an invalid ingredient card is scanned.
    /// </summary>
    [SerializeField] private GameObject invalidCardPanel;

    /// <summary>
    /// Text displayed on the invalid card popup.
    /// </summary>
    [SerializeField] private TMP_Text invalidCardText;

    /// <summary>
    /// Duration (in seconds) the invalid card popup remains visible.
    /// </summary>
    [SerializeField] private float invalidPopupDuration = 2f;

    [Header("Mixing Bowl")]

    /// <summary>
    /// Bowl object used for ingredient mixing.
    /// </summary>
    [SerializeField] private GameObject bowlObject;

    /// <summary>
    /// Text displayed near the bowl during mixing.
    /// </summary>
    [SerializeField] private TMP_Text bowlStatusText;

    /// <summary>
    /// Final pineapple paste model shown after mixing completes.
    /// </summary>
    [SerializeField] private GameObject finalPasteObject;

    /// <summary>
    /// UI stopwatch used to track stage completion time.
    /// </summary>
    [SerializeField] private StopwatchUI stopwatchUI;

    /// <summary>
    /// Reference to DatabaseManager for saving stage timing data.
    /// </summary>
    [SerializeField] private DatabaseManager dbManager;

    [Header("Stage Complete UI")]

    /// <summary>
    /// Panel displayed when the stage is fully completed.
    /// </summary>
    [SerializeField] private GameObject stageCompletePanel;

    /// <summary>
    /// Text displayed on the stage completion panel.
    /// </summary>
    [SerializeField] private TMP_Text stageCompleteText;

    /// <summary>
    /// Count of ingredients added via Add to Recipe button.
    /// </summary>
    int pineappleCount, sugarCount, lemonCount;

    /// <summary>
    /// Count of ingredients physically dropped into the bowl.
    /// </summary>
    int droppedPineapple, droppedSugar, droppedLemon;

    Coroutine invalidPopupRoutine;
    bool mixStageStarted = false;
    bool mixingComplete = false;
    bool stageCompleteShown = false;

    /// <summary>
    /// Initializes the singleton instance and hides all stage UI elements.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (bowlObject != null) bowlObject.SetActive(false);
        if (finalPasteObject != null) finalPasteObject.SetActive(false);
        if (mixBowlUI != null) mixBowlUI.SetActive(false);
        if (stageCompletePanel != null) stageCompletePanel.SetActive(false);

        UpdateStatusUI();
    }

    /// <summary>
    /// Validates whether a scanned card is allowed in this stage.
    /// </summary>
    /// <param name="cardId">Scanned card identifier.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public bool ValidateCard(string cardId)
    {
        bool valid =
            cardId == pineappleId ||
            cardId == sugarId ||
            cardId == lemonId;

        if (!valid)
        {
            ShowInvalidCardPopup("This ingredient cannot be used in the Pineapple Paste stage.");
        }

        return valid;
    }

    /// <summary>
    /// Registers an ingredient when the Add to Recipe button is pressed.
    /// </summary>
    /// <param name="ingredientId">Ingredient identifier.</param>
    /// <returns>True if accepted, otherwise false.</returns>
    public bool RegisterIngredient(string ingredientId)
    {
        if (!ValidateCard(ingredientId))
            return false;

        switch (ingredientId)
        {
            case var _ when ingredientId == pineappleId:
                if (pineappleCount >= requiredPineapples) return false;
                pineappleCount++;
                break;

            case var _ when ingredientId == sugarId:
                if (sugarCount >= requiredSugar) return false;
                sugarCount++;
                break;

            case var _ when ingredientId == lemonId:
                if (lemonCount >= requiredLemon) return false;
                lemonCount++;
                break;
        }

        UpdateStatusUI();
        CheckStageComplete();
        return true;
    }

    /// <summary>
    /// Displays a temporary popup for invalid ingredient cards.
    /// </summary>
    void ShowInvalidCardPopup(string msg)
    {
        if (invalidCardPanel == null) return;

        if (invalidCardText != null)
            invalidCardText.text = msg;

        if (invalidPopupRoutine != null)
            StopCoroutine(invalidPopupRoutine);

        invalidPopupRoutine = StartCoroutine(InvalidPopupRoutine());
    }

    IEnumerator InvalidPopupRoutine()
    {
        invalidCardPanel.SetActive(true);
        yield return new WaitForSeconds(invalidPopupDuration);
        invalidCardPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the ingredient progress counter UI.
    /// </summary>
    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{pineappleCount}/{requiredPineapples}\n" +
            $"{sugarCount}/{requiredSugar}\n" +
            $"{lemonCount}/{requiredLemon}";
    }

    /// <summary>
    /// Checks whether all ingredients have been added via Add to Recipe.
    /// </summary>
    bool IsStageComplete()
    {
        return
            pineappleCount >= requiredPineapples &&
            sugarCount >= requiredSugar &&
            lemonCount >= requiredLemon;
    }

    /// <summary>
    /// Enables the mixing stage once all required ingredients are collected.
    /// </summary>
    void CheckStageComplete()
    {
        if (mixStageStarted || !IsStageComplete())
            return;

        mixStageStarted = true;

        if (mixBowlUI != null) mixBowlUI.SetActive(true);
        if (bowlObject != null) bowlObject.SetActive(true);

        if (bowlStatusText != null)
            bowlStatusText.text = "Drag the ingredients into the bowl!";
    }

    /// <summary>
    /// Called when an ingredient is dropped into the mixing bowl.
    /// </summary>
    public void OnIngredientDroppedInBowl(string ingredientId, GameObject ingredientGO)
    {
        if (!mixStageStarted || mixingComplete)
            return;

        switch (ingredientId)
        {
            case var _ when ingredientId == pineappleId:
                if (droppedPineapple >= requiredPineapples) return;
                droppedPineapple++;
                break;

            case var _ when ingredientId == sugarId:
                if (droppedSugar >= requiredSugar) return;
                droppedSugar++;
                break;

            case var _ when ingredientId == lemonId:
                if (droppedLemon >= requiredLemon) return;
                droppedLemon++;
                break;

            default:
                return;
        }

        if (ingredientGO != null)
            ingredientGO.SetActive(false);

        UpdateBowlStatusUI();
        CheckMixingComplete();
    }

    /// <summary>
    /// Updates the bowl UI to show current mixing progress.
    /// </summary>
    void UpdateBowlStatusUI()
    {
        if (bowlStatusText == null) return;

        bowlStatusText.text =
            $"In bowl:\n" +
            $"Pineapple {droppedPineapple}/{requiredPineapples}\n" +
            $"Sugar {droppedSugar}/{requiredSugar}\n" +
            $"Lemon {droppedLemon}/{requiredLemon}";
    }

    /// <summary>
    /// Checks whether all required ingredients have been mixed.
    /// </summary>
    void CheckMixingComplete()
    {
        if (droppedPineapple >= requiredPineapples &&
            droppedSugar >= requiredSugar &&
            droppedLemon >= requiredLemon)
        {
            mixingComplete = true;

            if (bowlStatusText != null)
                bowlStatusText.text = "Pineapple paste ready!";

            if (finalPasteObject != null)
                finalPasteObject.SetActive(true);

            ShowStageCompleteUI();
        }
    }

    /// <summary>
    /// Displays the stage completion UI and stops timers.
    /// </summary>
    public void ShowStageCompleteUI()
    {
        if (stageCompleteShown) return;
        stageCompleteShown = true;

        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(true);

        if (stopwatchUI != null)
            stopwatchUI.StopTimer();

        if (dbManager != null)
            dbManager.StopComponentTimer("PineappleTart", "PineapplePaste");

        if (stageCompleteText != null)
            stageCompleteText.text = "Pineapple Paste Complete!";
    }
}
