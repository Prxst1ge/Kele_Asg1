using System.Collections;
using UnityEngine;
using TMPro;

public class BiscuitStageManager : MonoBehaviour
{
    public static BiscuitStageManager Instance { get; private set; }

    [Header("Ingredient IDs (must match reference image names / prefab names)")]
    [SerializeField] private string flourId  = "Flour_Ingredient";
    [SerializeField] private string butterId = "Butter_Ingredient";
    [SerializeField] private string waterId  = "Water_Ingredient";

    [Header("Required amounts (Add to Recipe step)")]
    [SerializeField] private int requiredFlour  = 2;
    [SerializeField] private int requiredButter = 1;
    [SerializeField] private int requiredWater  = 1;

    [Header("Stage UI")]
    [SerializeField] private TMP_Text stageStatusText;
    [SerializeField] private GameObject mixBowlUI;

    [Header("Invalid Card Popup")]
    [SerializeField] private GameObject invalidCardPanel;
    [SerializeField] private TMP_Text invalidCardText;
    [SerializeField] private float invalidPopupDuration = 2f;

    [Header("Mixing Bowl")]
    [SerializeField] private GameObject bowlObject;
    [SerializeField] private TMP_Text bowlStatusText;
    [SerializeField] private GameObject finalDoughObject;

    [Header("Stopwatch UI")]
    [SerializeField] private StopwatchUI stopwatchUI;

    [Header("Database")]
    [SerializeField] private DatabaseManager dbManager;

    [Header("Stage Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text levelCompleteText;

    // counts from "Add to Recipe"
    int flourCount;
    int butterCount;
    int waterCount;

    // counts from bowl drops
    int bowlFlourCount;
    int bowlButterCount;
    int bowlWaterCount;

    Coroutine invalidPopupRoutine;

    bool mixStageStarted = false;
    bool mixingComplete  = false;
    bool stageCompleteShown = false;

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

    // ------------------ CARD VALIDATION (ImageTracker can call this before spawning) ------------------

    public bool ValidateCard(string cardId)
    {
        bool valid = (cardId == flourId || cardId == butterId || cardId == waterId);

        if (!valid)
            ShowInvalidCardPopup("This ingredient cannot be used in the Biscuit stage.");

        return valid;
    }

    // ------------------ WHEN BUTTON "ADD TO RECIPE" IS PRESSED ------------------

    public bool RegisterIngredient(string ingredientId)
    {
        if (!ValidateCard(ingredientId))
            return false;

        // Prevent over-counting beyond requirement
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

    // ------------------ WHEN DROPPED INTO BOWL ------------------

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
            // ignore anything else
            return;
        }

        if (ingredientGO != null)
            ingredientGO.SetActive(false);

        UpdateBowlStatusUI();
        CheckBowlComplete();
    }

    // ------------------ CHECKS ------------------

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

    public void ShowStageCompleteUI()
    {
        if (stageCompleteShown) return;
        stageCompleteShown = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        if (stopwatchUI != null)
            stopwatchUI.StopTimer();

        // stop Firebase timer for Biscuit stage
        if (dbManager != null)
            dbManager.StopComponentTimer("PineappleTart", "Biscuit");

        if (levelCompleteText != null)
            levelCompleteText.text = "Biscuit Completed!";
    }

    // ------------------ UI HELPERS ------------------

    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{flourCount}/{requiredFlour}\n" +
            $"{butterCount}/{requiredButter}\n" +
            $"{waterCount}/{requiredWater}";
    }

    void UpdateBowlStatusUI()
    {
        if (bowlStatusText == null) return;

        bowlStatusText.text =
            $"In bowl:\n" +
            $"Flour {bowlFlourCount}/{requiredFlour}\n" +
            $"Butter {bowlButterCount}/{requiredButter}\n" +
            $"Water {bowlWaterCount}/{requiredWater}";
    }

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
}
