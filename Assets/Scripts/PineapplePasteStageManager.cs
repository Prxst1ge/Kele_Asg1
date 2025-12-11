using UnityEngine;
using TMPro;

public class PineapplePasteStageManager : MonoBehaviour
{
    public static PineapplePasteStageManager Instance { get; private set; }

    [Header("Ingredient IDs (must match reference image names)")]
    [SerializeField] private string pineappleId = "Pineapple_Ingredient";
    [SerializeField] private string sugarId     = "Sugar_Ingredient";
    [SerializeField] private string lemonId     = "Lemon_Ingredient";

    [Header("Required amounts (Add to Recipe step)")]
    [SerializeField] private int requiredPineapples = 2;
    [SerializeField] private int requiredSugar      = 1;
    [SerializeField] private int requiredLemon      = 1;

    [Header("Stage UI")]
    [SerializeField] private TMP_Text stageStatusText;   // counter in the top-right card
    [SerializeField] private GameObject mixBowlUI;       // e.g. “Drag ingredients into bowl” panel

    [Header("Invalid Card Popup")]
    [SerializeField] private GameObject invalidCardPanel;
    [SerializeField] private TMP_Text invalidCardText;
    [SerializeField] private float invalidPopupDuration = 2f;

    [Header("Mixing Bowl")]
    [SerializeField] private GameObject bowlObject;       // bowl model in scene
    [SerializeField] private TMP_Text bowlStatusText;     // text near the bowl
    [SerializeField] private GameObject finalPasteObject; // optional “finished paste” model

    // 🔹 NEW: Stage-complete UI
    [Header("Stage Complete UI")]
    [SerializeField] private GameObject stageCompletePanel; // panel with "Pineapple Paste Complete!"
    [SerializeField] private TMP_Text stageCompleteText;    // optional text on that panel

    // Counts for ingredients that have been ADDED (button press)
    int pineappleCount;
    int sugarCount;
    int lemonCount;

    // Counts for ingredients physically dropped into the bowl
    int droppedPineapple;
    int droppedSugar;
    int droppedLemon;

    Coroutine invalidPopupRoutine;
    bool mixStageStarted = false;
    bool mixingComplete  = false;
    bool stageCompleteShown = false;   // 🔹 NEW flag

    // ---------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make sure bowl and final paste start hidden
        if (bowlObject != null)       bowlObject.SetActive(false);
        if (finalPasteObject != null) finalPasteObject.SetActive(false);
        if (mixBowlUI != null)        mixBowlUI.SetActive(false);

        // 🔹 NEW: hide stage-complete panel at start
        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(false);

        UpdateStatusUI();
    }

    // ---------------------------------------------------------
    //  Called by ImageTracker BEFORE spawning a prefab
    // ---------------------------------------------------------
    public bool ValidateCard(string cardId)
    {
        bool valid =
            cardId == pineappleId ||
            cardId == sugarId     ||
            cardId == lemonId;

        if (!valid)
        {
            ShowInvalidCardPopup(
                "This ingredient cannot be used in the Pineapple Paste stage."
            );
        }

        return valid;
    }

    // ---------------------------------------------------------
    //  Called by IngredientController.AddToRecipe()
    //  Returns true = accepted (update its UI), false = reject
    // ---------------------------------------------------------
    public bool RegisterIngredient(string ingredientId)
    {
        // Only allow the 3 valid IDs
        if (!ValidateCard(ingredientId))
            return false;

        // Prevent over-counting the same type beyond requirement
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

    // ---------------------------------------------------------
    //  INVALID CARD POPUP (for wrong cards)
    // ---------------------------------------------------------
    void ShowInvalidCardPopup(string msg)
    {
        if (invalidCardPanel == null) return;

        if (invalidCardText != null)
            invalidCardText.text = msg;

        if (invalidPopupRoutine != null)
            StopCoroutine(invalidPopupRoutine);

        invalidPopupRoutine = StartCoroutine(InvalidPopupRoutine());
    }

    System.Collections.IEnumerator InvalidPopupRoutine()
    {
        invalidCardPanel.SetActive(true);
        yield return new WaitForSeconds(invalidPopupDuration);
        invalidCardPanel.SetActive(false);
    }

    // ---------------------------------------------------------
    //  STATUS + COMPLETION (Add to Recipe step)
    // ---------------------------------------------------------
    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{pineappleCount}/{requiredPineapples}\n" +
            $"{sugarCount}/{requiredSugar}\n" +
            $"{lemonCount}/{requiredLemon}";
    }

    bool IsStageComplete()
    {
        return
            pineappleCount >= requiredPineapples &&
            sugarCount      >= requiredSugar &&
            lemonCount      >= requiredLemon;
    }

    void CheckStageComplete()
    {
        if (mixStageStarted || !IsStageComplete())
            return;

        mixStageStarted = true;
        Debug.Log("[PineappleStage] All ingredients collected! You can now mix them.");

        // Show bowl and UI prompts
        if (mixBowlUI != null)   mixBowlUI.SetActive(true);
        if (bowlObject != null)  bowlObject.SetActive(true);

        if (bowlStatusText != null)
            bowlStatusText.text = "Drag the ingredients into the bowl!";
    }

    // ---------------------------------------------------------
    //  MIXING STEP – called by MixBowl.OnTriggerEnter
    // ---------------------------------------------------------
    public void OnIngredientDroppedInBowl(string ingredientId, GameObject ingredientGO)
    {
        if (!mixStageStarted || mixingComplete)
            return; // ignore if not in mixing phase

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
                // not part of this stage
                return;
        }

        // Hide the ingredient model once it falls into the bowl
        if (ingredientGO != null)
            ingredientGO.SetActive(false);

        UpdateBowlStatusUI();
        CheckMixingComplete();
    }

    void UpdateBowlStatusUI()
    {
        if (bowlStatusText == null) return;

        bowlStatusText.text =
            $"In bowl:\n" +
            $"Pineapple {droppedPineapple}/{requiredPineapples}\n" +
            $"Sugar {droppedSugar}/{requiredSugar}\n" +
            $"Lemon {droppedLemon}/{requiredLemon}";
    }

    void CheckMixingComplete()
    {
        if (droppedPineapple >= requiredPineapples &&
            droppedSugar      >= requiredSugar &&
            droppedLemon      >= requiredLemon)
        {
            mixingComplete = true;
            Debug.Log("[PineappleStage] Mixing complete – pineapple paste ready!");

            if (bowlStatusText != null)
                bowlStatusText.text = "Pineapple paste ready!";

            if (finalPasteObject != null)
                finalPasteObject.SetActive(true);

            // 🔹 Show the final stage-complete UI
            ShowStageCompleteUI();
        }
    }

    // ---------------------------------------------------------
    //  🔹 PUBLIC: called when mixing is finished
    // ---------------------------------------------------------
    public void ShowStageCompleteUI()
    {
        if (stageCompleteShown) return;
        stageCompleteShown = true;

        if (stageCompletePanel != null)
            stageCompletePanel.SetActive(true);

        if (stageCompleteText != null)
            stageCompleteText.text = "Pineapple Paste Complete!";
    }
}
