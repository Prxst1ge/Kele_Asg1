/*
 * Author: Javier
 * Date: 11 December 2025
 * Description:
 * Manages the Biscuit stage of the KELE AR experience.
 * Handles ingredient validation, recipe progression, bowl mixing,
 * stopwatch control, Firebase timer stopping, and stage completion UI.
 */

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the Biscuit AR stage logic, including ingredient validation,
/// recipe progress tracking, mixing bowl interaction, stopwatch handling,
/// database updates, and stage completion UI.
/// </summary>
public class BiscuitStageManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static BiscuitStageManager Instance { get; private set; }

    [Header("Ingredient IDs (must match reference image names / prefab names)")]

    /// <summary>Ingredient ID for flour.</summary>
    [SerializeField] private string flourId  = "Flour_Ingredient";

    /// <summary>Ingredient ID for butter.</summary>
    [SerializeField] private string butterId = "Butter_Ingredient";

    /// <summary>Ingredient ID for water.</summary>
    [SerializeField] private string waterId  = "Water_Ingredient";

    [Header("Required amounts (Add to Recipe step)")]

    /// <summary>Number of flour units required.</summary>
    [SerializeField] private int requiredFlour  = 2;

    /// <summary>Number of butter units required.</summary>
    [SerializeField] private int requiredButter = 1;

    /// <summary>Number of water units required.</summary>
    [SerializeField] private int requiredWater  = 1;

    [Header("Stage UI")]

    /// <summary>Text displaying ingredient progress.</summary>
    [SerializeField] private TMP_Text stageStatusText;

    /// <summary>UI shown when mixing stage becomes available.</summary>
    [SerializeField] private GameObject mixBowlUI;

    [Header("Invalid Card Popup")]

    /// <summary>Popup panel shown when an invalid card is scanned.</summary>
    [SerializeField] private GameObject invalidCardPanel;

    /// <summary>Text displayed inside the invalid card popup.</summary>
    [SerializeField] private TMP_Text invalidCardText;

    /// <summary>Duration the invalid popup stays visible.</summary>
    [SerializeField] private float invalidPopupDuration = 2f;

    [Header("Mixing Bowl")]

    /// <summary>The bowl GameObject used for mixing ingredients.</summary>
    [SerializeField] private GameObject bowlObject;

    /// <summary>Status text shown during bowl mixing.</summary>
    [SerializeField] private TMP_Text bowlStatusText;

    /// <summary>Final dough object shown after successful mixing.</summary>
    [SerializeField] private GameObject finalDoughObject;

    [Header("Stopwatch UI")]

    /// <summary>Reference to the stopwatch UI controller.</summary>
    [SerializeField] private StopwatchUI stopwatchUI;

    [Header("Database")]

    /// <summary>Reference to the database manager for timer updates.</summary>
    [SerializeField] private DatabaseManager dbManager;

    [Header("Stage Complete UI")]

    /// <summary>Panel shown when the stage is completed.</summary>
    [SerializeField] private GameObject levelCompletePanel;

    /// <summary>Text displayed on stage completion.</summary>
    [SerializeField] private TMP_Text levelCompleteText;

    // Ingredient counts from "Add to Recipe"
    int flourCount;
    int butterCount;
    int waterCount;

    // Ingredient counts from bowl drops
    int bowlFlourCount;
    int bowlButterCount;
    int bowlWaterCount;

    Coroutine invalidPopupRoutine;

    bool mixStageStarted = false;
    bool mixingComplete  = false;
    bool stageCompleteShown = false;

    /// <summary>
    /// Initializes singleton instance and sets initial UI states.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (bowlObject != null)       bowlObject.SetActive(false);
        if (finalDoughObject != null) finalDoughObject.SetActive(false);
        if (mixBowlUI != null)        mixBowlUI.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        UpdateStatusUI();
    }

    /// <summary>
    /// Validates whether a scanned card belongs to the Biscuit stage.
    /// Called by ImageTracker before spawning the prefab.
    /// </summary>
    public bool ValidateCard(string cardId)
    {
        bool valid = (cardId == flourId || cardId == butterId || cardId == waterId);

        if (!valid)
            ShowInvalidCardPopup("This ingredient cannot be used in the Biscuit stage.");

        return valid;
    }

    /// <summary>
    /// Registers an ingredient when the "Add to Recipe" button is pressed.
    /// Prevents over-collection beyond required amounts.
    /// </summary>
    public bool RegisterIngredient(string ingredientId)
    {
        if (!ValidateCard(ingredientId))
            return false;

        if (ingredientId == flourId)
        {
            if (flourCount >= requiredFlour) return false;
            flourCount++;
        }
        else if (ingredientId == butterId)
        {
            if (butterCount >= requiredButter) return false;
            butterCount++;
        }
        else if (ingredientId == waterId)
        {
            if (waterCount >= requiredWater) return false;
            waterCount++;
        }

        UpdateStatusUI();
        CheckStageComplete();
        return true;
    }

    /// <summary>
    /// Handles ingredient interaction when dropped into the mixing bowl.
    /// </summary>
    public void OnIngredientDroppedInBowl(string ingredientId, GameObject ingredientGO)
    {
        if (!mixStageStarted || mixingComplete)
            return;

        if (ingredientId == flourId)
        {
            if (bowlFlourCount >= requiredFlour) return;
            bowlFlourCount++;
        }
        else if (ingredientId == butterId)
        {
            if (bowlButterCount >= requiredButter) return;
            bowlButterCount++;
        }
        else if (ingredientId == waterId)
        {
            if (bowlWaterCount >= requiredWater) return;
            bowlWaterCount++;
        }
        else
        {
            return;
        }

        if (ingredientGO != null)
            ingredientGO.SetActive(false);

        UpdateBowlStatusUI();
        CheckBowlComplete();
    }

    /// <summary>
    /// Checks if all required ingredients have been added to start mixing.
    /// </summary>
    void CheckStageComplete()
    {
        bool complete =
            flourCount  >= requiredFlour &&
            butterCount >= requiredButter &&
            waterCount  >= requiredWater;

        if (complete && !mixStageStarted)
        {
            mixStageStarted = true;

            if (mixBowlUI != null)  mixBowlUI.SetActive(true);
            if (bowlObject != null) bowlObject.SetActive(true);

            if (bowlStatusText != null)
                bowlStatusText.text = "Drag the ingredients into the bowl!";
        }
    }

    /// <summary>
    /// Checks if all required ingredients have been placed into the bowl.
    /// </summary>
    void CheckBowlComplete()
    {
        bool complete =
            bowlFlourCount  >= requiredFlour &&
            bowlButterCount >= requiredButter &&
            bowlWaterCount  >= requiredWater;

        if (complete && !mixingComplete)
        {
            mixingComplete = true;

            if (bowlStatusText != null)
                bowlStatusText.text = "Biscuit dough ready!";

            if (finalDoughObject != null)
                finalDoughObject.SetActive(true);

            ShowStageCompleteUI();
        }
    }

    /// <summary>
    /// Displays the stage completion UI, stops stopwatch,
    /// and updates the database timer.
    /// </summary>
    public void ShowStageCompleteUI()
    {
        if (stageCompleteShown) return;
        stageCompleteShown = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (stopwatchUI != null)
            stopwatchUI.StopTimer();

        if (dbManager != null)
            dbManager.StopComponentTimer("PineappleTart", "Biscuit");

        if (levelCompleteText != null)
            levelCompleteText.text = "Biscuit Completed!";
    }

    /// <summary>
    /// Updates the ingredient progress UI.
    /// </summary>
    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{flourCount}/{requiredFlour}\n" +
            $"{butterCount}/{requiredButter}\n" +
            $"{waterCount}/{requiredWater}";
    }

    /// <summary>
    /// Updates the bowl mixing progress UI.
    /// </summary>
    void UpdateBowlStatusUI()
    {
        if (bowlStatusText == null) return;

        bowlStatusText.text =
            $"In bowl:\n" +
            $"Flour {bowlFlourCount}/{requiredFlour}\n" +
            $"Butter {bowlButterCount}/{requiredButter}\n" +
            $"Water {bowlWaterCount}/{requiredWater}";
    }

    /// <summary>
    /// Shows an invalid ingredient popup for a short duration.
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

    /// <summary>
    /// Coroutine that controls invalid popup visibility duration.
    /// </summary>
    IEnumerator InvalidPopupRoutine()
    {
        invalidCardPanel.SetActive(true);
        yield return new WaitForSeconds(invalidPopupDuration);
        invalidCardPanel.SetActive(false);
    }
}
