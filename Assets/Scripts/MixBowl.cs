using UnityEngine;

public class MixBowl : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Make sure this is an ingredient
        var controller = other.GetComponentInParent<IngredientController>();
        if (controller == null) return;

        string ingredientId = controller.IngredientId;   // uses the public getter we added
        
    if (PineapplePasteStageManager.Instance != null)
    {
        PineapplePasteStageManager.Instance.OnIngredientDroppedInBowl(ingredientId, other.gameObject);
    }
    else if (BiscuitStageManager.Instance != null)
    {
        BiscuitStageManager.Instance.OnIngredientDroppedInBowl(ingredientId, other.gameObject);
    }

    }
}
