/*
 * Author: Javier
 * Date: 10 December 2025
 * Description:
 * Static manager that tracks ingredient collection progress across AR stages.
 * Uses an in-memory HashSet to prevent duplicate ingredient additions and
 * provides utility methods for checking progress state.
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages global ingredient collection progress.
/// Tracks which ingredients have been added to the recipe
/// and prevents duplicate additions.
/// </summary>
public static class IngredientProgressManager
{
    /// <summary>
    /// Stores the unique identifiers of ingredients that have been added.
    /// HashSet ensures no duplicates are counted.
    /// </summary>
    static HashSet<string> addedIngredients = new HashSet<string>();

    /// <summary>
    /// Marks an ingredient as added to the recipe.
    /// Duplicate ingredient IDs are ignored automatically.
    /// </summary>
    /// <param name="ingredientId">Unique identifier of the ingredient.</param>
    public static void MarkAdded(string ingredientId)
    {
        if (string.IsNullOrEmpty(ingredientId)) return;

        addedIngredients.Add(ingredientId);
        Debug.Log($"[Progress] Added ingredient: {ingredientId}. Total = {addedIngredients.Count}");
    }

    /// <summary>
    /// Checks whether a specific ingredient has already been added.
    /// </summary>
    /// <param name="ingredientId">Unique identifier of the ingredient.</param>
    /// <returns>True if the ingredient has been added, otherwise false.</returns>
    public static bool IsAdded(string ingredientId)
    {
        if (string.IsNullOrEmpty(ingredientId)) return false;
        return addedIngredients.Contains(ingredientId);
    }

    /// <summary>
    /// Gets the total number of unique ingredients added so far.
    /// </summary>
    /// <returns>The count of added ingredients.</returns>
    public static int GetAddedCount()
    {
        return addedIngredients.Count;
    }
}
