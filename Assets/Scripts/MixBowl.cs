/*
 * Author: Javier
 * Date: 9 December 2025
 * Description:
 * Detects when ingredient objects are dropped into the mixing bowl.
 * Communicates with the active stage manager (Pineapple Paste or Biscuit)
 * to validate and process ingredient mixing.
 */

using UnityEngine;

/// <summary>
/// Handles collision detection for the mixing bowl.
/// When an ingredient enters the bowl, it notifies the
/// appropriate stage manager to process the ingredient.
/// </summary>
public class MixBowl : MonoBehaviour
{
    /// <summary>
    /// Called automatically by Unity when a collider enters the bowl trigger.
    /// Checks if the collider belongs to an ingredient and forwards
    /// the event to the active stage manager.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Ensure the object belongs to an ingredient
        var controller = other.GetComponentInParent<IngredientController>();
        if (controller == null) return;

        // Retrieve the ingredient identifier from the controller
        string ingredientId = controller.IngredientId;

        // Forward the drop event to the active stage manager
        if (PineapplePasteStageManager.Instance != null)
        {
            PineapplePasteStageManager.Instance.OnIngredientDroppedInBowl(
                ingredientId,
                other.gameObject
            );
        }
        else if (BiscuitStageManager.Instance != null)
        {
            BiscuitStageManager.Instance.OnIngredientDroppedInBowl(
                ingredientId,
                other.gameObject
            );
        }
    }
}
