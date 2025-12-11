using System.Collections;
using UnityEngine;
using TMPro;

public class BiscuitStageManager : MonoBehaviour
{
    // ---- Singleton (easy access like PineapplePasteStageManager.Instance) ----
    public static BiscuitStageManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make sure bowl & UI start hidden
        if (bowlObject != null)      bowlObject.SetActive(false);
        if (finalDoughObject != null) finalDoughObject.SetActive(false);
        if (mixBowlUI != null)       mixBowlUI.SetActive(false);

        UpdateStatusUI();
    }

    [Header("Ingredient IDs (match IngredientController.IngredientId)")]
    [SerializeField] private string flourId  = "flour";
    [SerializeField] private string butterId = "butter";
    [SerializeField] private string sugarId  = "sugar";
    [SerializeField] private string waterId  = "water";

    [Header("Required amounts")]
    [SerializeField] private int requiredFlour  = 2;
    [SerializeField] private int requiredButter = 1;
    [SerializeField] private int requiredSugar  = 1;
    [SerializeField] private int requiredWater  = 1;

    [Header("Stage UI")]
    [SerializeField] private TMP_Text stageStatusText; // flour/butter/sugar/water counts
    [SerializeField] private GameObject mixBowlUI;     // shown when stage complete

    [Header("Invalid Card Popup")]
    [SerializeField] private GameObject invalidCardPanel;
    [SerializeField] private TMP_Text invalidCardText;
    [SerializeField] private float invalidPopupDuration = 2f;

    [Header("Mixing Bowl")]
    [SerializeField] private GameObject bowlObject;        // bowl model in scene
    [SerializeField] private TMP_Text bowlStatusText;      // text near the bowl
    [SerializeField] private GameObject finalDoughObject;  // finished biscuit dough model

    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;

    // counts from "Add to Recipe" button
    int flourCount;
    int butterCount;
    int sugarCount;
    int waterCount;

    // counts from bowl drops
    int bowlFlourCount;
    int bowlButterCount;
    int bowlSugarCount;
    int bowlWaterCount;

    Coroutine invalidPopupRoutine;

    bool mixStageStarted = false;
    bool mixingComplete  = false;

    // ------------------  CARD VALIDATION (called from ImageTracker) ------------------

    public bool ValidateCard(string cardId)
    {
        // Only allow biscuit ingredients in this scene
        bool valid =
            cardId == flourId ||
            cardId == butterId ||
            cardId == sugarId ||
            cardId == waterId;

        if (!valid)
            ShowInvalidCardPopup("This ingredient cannot be used in the Biscuit stage.");

        return valid;
    }

    // ------------------  WHEN BUTTON "ADD TO RECIPE" IS PRESSED ------------------

    public bool RegisterIngredient(string ingredientId)
    {
        // increment counts according to ingredient
        if (ingredientId == flourId)       flourCount++;
        else if (ingredientId == butterId) butterCount++;
        else if (ingredientId == sugarId)  sugarCount++;
        else if (ingredientId == waterId)  waterCount++;
        else
        {
            Debug.Log("[BiscuitStage] Ingredient not used in this stage: " + ingredientId);
            return false;
        }

        Debug.Log($"[BiscuitStage] Added {ingredientId}. Now F={flourCount} B={butterCount} S={sugarCount} W={waterCount}");

        UpdateStatusUI();
        CheckStageComplete();

        return true;
    }

    // ------------------  WHEN DROPPED INTO BOWL ------------------

    public void OnIngredientDroppedInBowl(string ingredientId, GameObject ingredientGO)
    {
        // Only react once we are in the mixing step
        if (!mixStageStarted || mixingComplete)
            return;

        if (ingredientId == flourId)          bowlFlourCount++;
        else if (ingredientId == butterId)    bowlButterCount++;
        else if (ingredientId == sugarId)     bowlSugarCount++;
        else if (ingredientId == waterId)     bowlWaterCount++;
        else
        {
            Debug.Log("[BiscuitStage] Non-biscuit ingredient dropped in bowl: " + ingredientId);
            return;
        }

        Debug.Log($"[BiscuitStage] Bowl drop {ingredientId}. Now F={bowlFlourCount} B={bowlButterCount} S={bowlSugarCount} W={bowlWaterCount}");

        if (ingredientGO != null)
            ingredientGO.SetActive(false); // hide once dropped

        UpdateBowlStatusUI();
        CheckBowlComplete();
    }

    // ------------------  CHECKS ------------------

    void CheckStageComplete()
    {
        bool complete =
            flourCount  >= requiredFlour &&
            butterCount >= requiredButter &&
            sugarCount  >= requiredSugar &&
            waterCount  >= requiredWater;

        if (complete && !mixStageStarted)
        {
            mixStageStarted = true;
            Debug.Log("[BiscuitStage] All biscuit ingredients collected! Start mixing.");

            // Show "drag into bowl" UI and the bowl itself
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
            bowlSugarCount  >= requiredSugar &&
            bowlWaterCount  >= requiredWater;

        if (complete && !mixingComplete)
        {
            mixingComplete = true;
            Debug.Log("[BiscuitStage] Bowl complete! Showing level complete UI.");

            if (bowlStatusText != null)
                bowlStatusText.text = "Biscuit dough ready!";

            if (finalDoughObject != null)
                finalDoughObject.SetActive(true);

            ShowStageCompleteUI();
        }
    }

    public void ShowStageCompleteUI()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    // ------------------  UI HELPERS ------------------

    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{flourCount}/{requiredFlour}\n" +
            $"{butterCount}/{requiredButter}\n" +
            $"{sugarCount}/{requiredSugar}\n" +
            $"{waterCount}/{requiredWater}";
    }

    void UpdateBowlStatusUI()
    {
        if (bowlStatusText == null) return;

        bowlStatusText.text =
            $"In bowl:\n" +
            $"Flour {bowlFlourCount}/{requiredFlour}\n" +
            $"Butter {bowlButterCount}/{requiredButter}\n" +
            $"Sugar {bowlSugarCount}/{requiredSugar}\n" +
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
