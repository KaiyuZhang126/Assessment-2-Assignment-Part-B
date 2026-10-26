using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeManagement.Core;

/// <summary>
/// Implements Part A functionality using five collection types:
/// Dictionary, List, LinkedList, Stack and Queue.
/// </summary>
public sealed class RecipeManager : IRecipeManager
{
    // Recipe catalogue (Dictionary)
    private readonly Dictionary<int, Recipe> _recipeCatalogue;

    // Shopping list (List)
    private readonly List<string> _shoppingList;

    // Cooking plan (LinkedList)
    private readonly LinkedList<int> _cookingPlan;

    // Remove / restore (Stack)
    private readonly Stack<int> _removedRecipesStack;

    // Cooking instructions (Queue)
    private readonly Queue<string> _instructionQueue;

    // Constructor
    public RecipeManager(IEnumerable<Recipe> recipes)
    {
        if (recipes == null)
            throw new ArgumentNullException(nameof(recipes));

        _recipeCatalogue = new Dictionary<int, Recipe>();
        _shoppingList = new List<string>();
        _cookingPlan = new LinkedList<int>();
        _removedRecipesStack = new Stack<int>();
        _instructionQueue = new Queue<string>();

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null)
                throw new ArgumentException(
                    "Recipe collection contains null item.",
                    nameof(recipes));

            if (recipe.Id <= 0)
                throw new ArgumentException(
                    $"Recipe ID must be positive: {recipe.Id}",
                    nameof(recipes));

            if (string.IsNullOrWhiteSpace(recipe.Title))
                throw new ArgumentException(
                    $"Recipe title cannot be blank (ID: {recipe.Id})",
                    nameof(recipes));

            if (_recipeCatalogue.ContainsKey(recipe.Id))
                throw new ArgumentException(
                    $"Duplicate recipe ID: {recipe.Id}",
                    nameof(recipes));

            _recipeCatalogue.Add(recipe.Id, recipe);
        }
    }

    // =========================
    // Part A: Count Properties
    // =========================

    public int RecipeCount => _recipeCatalogue.Count;

    public int ShoppingItemCount => _shoppingList.Count;

    public int CookingPlanCount => _cookingPlan.Count;

    public int PendingInstructionCount => _instructionQueue.Count;

    public int RemovedRecipeCount => _removedRecipesStack.Count;

    // =========================
    // Part A: Recipe Catalogue
    // =========================

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe == null)
            throw new ArgumentNullException(nameof(recipe));

        if (recipe.Id <= 0 || string.IsNullOrWhiteSpace(recipe.Title))
            return false;

        if (_recipeCatalogue.ContainsKey(recipe.Id))
            return false;

        _recipeCatalogue.Add(recipe.Id, recipe);

        return true;
    }

    public Recipe? FindRecipe(int recipeId)
    {
        _recipeCatalogue.TryGetValue(recipeId, out Recipe? recipe);

        return recipe;
    }

    public bool RemoveRecipe(int recipeId)
    {
        if (!_recipeCatalogue.ContainsKey(recipeId))
            return false;

        if (_cookingPlan.Contains(recipeId))
            return false;

        _recipeCatalogue.Remove(recipeId);

        return true;
    }

    // =========================
    // Part A: Shopping List
    // =========================

    public int AddIngredientsToShoppingList(int recipeId)
    {
        Recipe? recipe = FindRecipe(recipeId);

        if (recipe == null || recipe.Ingredients == null)
            return 0;

        int addedCount = 0;

        foreach (string ingredient in recipe.Ingredients)
        {
            _shoppingList.Add(ingredient);
            addedCount++;
        }

        return addedCount;
    }

    public IReadOnlyList<string> GetShoppingList()
    {
        return _shoppingList.AsReadOnly();
    }

    public void ClearShoppingList()
    {
        _shoppingList.Clear();
    }

    // =========================
    // Part A: Cooking Plan
    // =========================

    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (!_recipeCatalogue.ContainsKey(recipeId))
            return false;

        if (_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.AddLast(recipeId);

        return true;
    }

    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        bool removed = _cookingPlan.Remove(recipeId);

        if (!removed)
            return false;

        _removedRecipesStack.Push(recipeId);

        return true;
    }

    public IReadOnlyList<int> GetCookingPlan()
    {
        return _cookingPlan.ToList().AsReadOnly();
    }

    public bool RestoreLastRemovedRecipe()
    {
        if (_removedRecipesStack.Count == 0)
            return false;

        int recipeId = _removedRecipesStack.Pop();

        if (_recipeCatalogue.ContainsKey(recipeId) &&
            !_cookingPlan.Contains(recipeId))
        {
            _cookingPlan.AddLast(recipeId);
            return true;
        }

        return false;
    }

    public int? PeekLastRemovedRecipe()
    {
        if (_removedRecipesStack.Count == 0)
            return null;

        return _removedRecipesStack.Peek();
    }

    // =========================
    // Part A: Cooking Instructions
    // =========================

    public bool StartCooking(int recipeId)
    {
        Recipe? recipe = FindRecipe(recipeId);

        if (recipe == null)
            return false;

        if (recipe.Instructions == null ||
            recipe.Instructions.Count == 0)
            return false;

        _instructionQueue.Clear();

        foreach (string instruction in recipe.Instructions)
        {
            _instructionQueue.Enqueue(instruction);
        }

        return true;
    }

    public string? PeekNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Peek();
    }

    public string? CompleteNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Dequeue();
    }

    // =========================
    // Part B1: Search By Title
    // =========================

    public IReadOnlyList<Recipe> SearchByTitle(string searchText)
    {
        return _recipeCatalogue.Values
            .Where(recipe =>
                recipe.Title.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // =========================
    // Part B2: Search By Ingredient
    // =========================

    public IReadOnlyList<Recipe> SearchByIngredient(string searchText)
    {
        return _recipeCatalogue.Values
            .Where(recipe =>
                recipe.Ingredients.Any(ingredient =>
                    ingredient.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    // =========================
    // Part B3: Highest Protein Recipes
    // =========================

    public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count)
    {
        // Return empty list if count is zero or negative
        if (count <= 0)
        {
            return new List<Recipe>();
        }

        // Filter recipes with valid protein information,
        // sort by protein descending, and take the top count.
        return _recipeCatalogue.Values
            .Where(recipe =>
                recipe.Nutrition != null &&
                recipe.Nutrition.ProteinG.HasValue)
            .OrderByDescending(recipe =>
                recipe.Nutrition!.ProteinG)
            .Take(count)
            .ToList();
    }

    // =========================
    // Part B4: Saved Recipes
    // Not implemented yet
    // =========================

    public bool AddSavedRecipe(int recipeId) =>
        throw new NotImplementedException(
            "Part B: implement AddSavedRecipe.");

    public bool RemoveSavedRecipe(int recipeId) =>
        throw new NotImplementedException(
            "Part B: implement RemoveSavedRecipe.");

    public bool IsRecipeSaved(int recipeId) =>
        throw new NotImplementedException(
            "Part B: implement IsRecipeSaved.");

    public IReadOnlyList<int> GetSavedRecipes() =>
        throw new NotImplementedException(
            "Part B: implement GetSavedRecipes.");
}