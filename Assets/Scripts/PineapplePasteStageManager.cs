using UnityEngine;
using TMPro;

public class PineapplePasteStageManager : MonoBehaviour
{
    // ---------------------------------------------------------
    // SINGLETON ACCESS
    // ---------------------------------------------------------
    public static PineapplePasteStageManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ---------------------------------------------------------
    // INGREDIENT SETTINGS
    // ---------------------------------------------------------
    [Header("Ingredient IDs (must match reference image names)")]
    [SerializeField] private string pineappleId = "Pineapple_Ingredient";
    [SerializeField] private string sugarId = "Sugar_Ingredient";
    [SerializeField] private string lemonId = "Lemon_Ingredient";

    [Header("Required amounts")]
    public int requiredPineapples = 2;
    public int requiredSugar = 1;
    public int requiredLemon = 1;

    [Header("Stage UI")]
    [SerializeField] private TMP_Text stageStatusText;
    [SerializeField] private GameObject mixBowlUI;

    [Header("Invalid Card Popup")]
    [SerializeField] private GameObject invalidCardPanel;
    [SerializeField] private TMP_Text invalidCardText;
    [SerializeField] private float invalidPopupDuration = 2f;

    int pineappleCount;
    int sugarCount;
    int lemonCount;

    Coroutine invalidPopupRoutine;

    // ---------------------------------------------------------
    // IMAGE TRACKER VALIDATION
    // ---------------------------------------------------------
    public bool ValidateCard(string cardId)
    {
        bool valid =
            (cardId == pineappleId) ||
            (cardId == sugarId) ||
            (cardId == lemonId);

        if (!valid)
            ShowInvalidCardPopup("This ingredient cannot be used in the Pineapple Paste stage.");

        return valid;
    }

    // ---------------------------------------------------------
    // CALLED WHEN "ADD TO RECIPE" BUTTON IS PRESSED
    // ---------------------------------------------------------
    public bool RegisterIngredient(string ingredientId)
    {
        // Not part of this stage → reject
        if (ingredientId != pineappleId && ingredientId != sugarId && ingredientId != lemonId)
        {
            ShowInvalidCardPopup("This ingredient cannot be used in the Pineapple Paste stage.");
            return false;
        }

        // Correct card type → increment counts manually
        if (ingredientId == pineappleId) pineappleCount++;
        else if (ingredientId == sugarId) sugarCount++;
        else if (ingredientId == lemonId) lemonCount++;

        UpdateStatusUI();
        CheckStageComplete();

        return true; // ingredient accepted
    }


    // ---------------------------------------------------------
    // INVALID POPUP
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
    // STATUS UI + COMPLETION
    // ---------------------------------------------------------
    void UpdateStatusUI()
    {
        if (stageStatusText == null) return;

        stageStatusText.text =
            $"{pineappleCount}/{requiredPineapples}\n" +
            $"{sugarCount}/{requiredSugar}\n" +
            $"{lemonCount}/{requiredLemon}";
    }

    void CheckStageComplete()
    {
        bool complete =
            pineappleCount >= requiredPineapples &&
            sugarCount >= requiredSugar &&
            lemonCount >= requiredLemon;

        if (complete)
        {
            Debug.Log("[PineappleStage] All ingredients collected! You can now mix them.");
            if (mixBowlUI != null)
                mixBowlUI.SetActive(true);
        }
    }
}
