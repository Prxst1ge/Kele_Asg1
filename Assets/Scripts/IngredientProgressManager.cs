using System.Collections.Generic;
using UnityEngine;

public static class IngredientProgressManager
{
    // tracks which ingredients have been added
    static HashSet<string> addedIngredients = new HashSet<string>();

    public static void MarkAdded(string ingredientId)
    {
        if (string.IsNullOrEmpty(ingredientId)) return;

        addedIngredients.Add(ingredientId);
        Debug.Log($"[Progress] Added ingredient: {ingredientId}. Total = {addedIngredients.Count}");
    }

    public static bool IsAdded(string ingredientId)
    {
        if (string.IsNullOrEmpty(ingredientId)) return false;
        return addedIngredients.Contains(ingredientId);
    }

    public static int GetAddedCount()
    {
        return addedIngredients.Count;
    }
}
