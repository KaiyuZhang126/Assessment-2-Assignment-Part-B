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
    // Maps recipe ID to Recipe object for lookup by ID
    private readonly Dictionary<int, Recipe> _recipeCatalogue;
    // Shopping list (List)
    // Stores all ingredient entries to purchase
    private readonly List<string> _shoppingList;
    // Cooking plan (LinkedList)
    // Stores recipe IDs in cooking sequence
    private readonly LinkedList<int> _cookingPlan;
    // Remove / restore (Stack)
    // Stores IDs removed from cooking plan, LIFO behaviour for restore
    private readonly Stack<int> _removedRecipesStack;
    // Cooking instructions (Queue)
    // Stores step-by-step instructions for current recipe, FIFO execution
    private readonly Queue<string> _instructionQueue;

    // Initialises the manager with input validation and builds the recipe catalogue
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
                throw new ArgumentException("Recipe collection contains null item.", nameof(recipes));
            if (recipe.Id <= 0)
                throw new ArgumentException($"Recipe ID must be positive: {recipe.Id}", nameof(recipes));
            if (string.IsNullOrWhiteSpace(recipe.Title))
                throw new ArgumentException($"Recipe title cannot be blank (ID: {recipe.Id})", nameof(recipes));
            if (_recipeCatalogue.ContainsKey(recipe.Id))
                throw new ArgumentException($"Duplicate recipe ID: {recipe.Id}", nameof(recipes));

            _recipeCatalogue.Add(recipe.Id, recipe);
        }
    }

    // Read-only count properties for each collection
    public int RecipeCount => _recipeCatalogue.Count;
    public int ShoppingItemCount => _shoppingList.Count;
    public int CookingPlanCount => _cookingPlan.Count;
    public int PendingInstructionCount => _instructionQueue.Count;
    public int RemovedRecipeCount => _removedRecipesStack.Count;

    // Adds a new recipe to the catalogue
    // Returns true if added successfully; false if ID is invalid, title is blank or ID already exists
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

    // Looks up a recipe by ID
    // Returns matching recipe if found; null if ID does not exist
    public Recipe? FindRecipe(int recipeId)
    {
        _recipeCatalogue.TryGetValue(recipeId, out Recipe? recipe);
        return recipe;
    }

    // Removes a recipe from the catalogue
    // Business rule: a recipe in the cooking plan cannot be removed
    // Returns true if removed; false if ID does not exist or is in cooking plan
    public bool RemoveRecipe(int recipeId)
    {
        if (!_recipeCatalogue.ContainsKey(recipeId))
            return false;

        if (_cookingPlan.Contains(recipeId))
            return false;

        _recipeCatalogue.Remove(recipeId);
        return true;
    }

    // Adds all ingredients of a recipe to the shopping list
    // Returns number of ingredients added; 0 if recipe not found or has no ingredients
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

    // Returns a read-only view of the shopping list
    public IReadOnlyList<string> GetShoppingList()
    {
        return _shoppingList.AsReadOnly();
    }

    public void ClearShoppingList()
    {
        _shoppingList.Clear();
    }

    // Adds a recipe to the end of the cooking plan
    // Returns true if added; false if ID does not exist or is already in plan
    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (!_recipeCatalogue.ContainsKey(recipeId))
            return false;

        if (_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    // Removes a recipe from the cooking plan
    // On successful removal, pushes the ID onto the removed stack
    // Returns true if removed; false if ID is not in the plan
    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        bool removed = _cookingPlan.Remove(recipeId);
        if (!removed)
            return false;

        _removedRecipesStack.Push(recipeId);
        return true;
    }

    // Returns a read-only copy of the cooking plan in current order
    public IReadOnlyList<int> GetCookingPlan()
    {
        return _cookingPlan.ToList().AsReadOnly();
    }

    // Restores the most recently removed recipe to the end of the cooking plan
    // LIFO behaviour: last removed = first restored
    // Returns true if restored; false if stack is empty, ID not in catalogue or already in plan
    public bool RestoreLastRemovedRecipe()
    {
        if (_removedRecipesStack.Count == 0)
            return false;

        int recipeId = _removedRecipesStack.Pop();

        if (_recipeCatalogue.ContainsKey(recipeId) && !_cookingPlan.Contains(recipeId))
        {
            _cookingPlan.AddLast(recipeId);
            return true;
        }

        return false;
    }

    // Peeks the most recently removed recipe ID without popping it
    // Returns top of stack ID; null if stack is empty
    public int? PeekLastRemovedRecipe()
    {
        if (_removedRecipesStack.Count == 0)
            return null;

        return _removedRecipesStack.Peek();
    }

    // Starts cooking a recipe, clears current queue and loads instructions in order
    // Returns true if started; false if recipe not found or has no instructions
    public bool StartCooking(int recipeId)
    {
        Recipe? recipe = FindRecipe(recipeId);
        if (recipe == null)
            return false;

        if (recipe.Instructions == null || recipe.Instructions.Count == 0)
            return false;

        _instructionQueue.Clear();

        foreach (string instruction in recipe.Instructions)
        {
            _instructionQueue.Enqueue(instruction);
        }
        return true;
    }

    // Peeks the next cooking instruction without dequeuing it
    // Returns next instruction; null if queue is empty
    public string? PeekNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Peek();
    }

    // Completes and removes the next cooking instruction
    // FIFO behaviour: first added = first executed
    // Returns next instruction; null if queue is empty
    public string? CompleteNextInstruction()
    {
        if (_instructionQueue.Count == 0)
            return null;

        return _instructionQueue.Dequeue();
    }

// Part B
public IReadOnlyList<Recipe> SearchByTitle(string searchText)
{
    return _recipeCatalogue.Values
        .Where(recipe =>
            recipe.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        .ToList();
}

public IReadOnlyList<Recipe> SearchByIngredient(string searchText) =>
    throw new NotImplementedException("Part B: implement SearchByIngredient.");

public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count) =>
    throw new NotImplementedException("Part B: implement GetHighestProteinRecipes.");

public bool AddSavedRecipe(int recipeId) =>
    throw new NotImplementedException("Part B: implement AddSavedRecipe.");

public bool RemoveSavedRecipe(int recipeId) =>
    throw new NotImplementedException("Part B: implement RemoveSavedRecipe.");

public bool IsRecipeSaved(int recipeId) =>
    throw new NotImplementedException("Part B: implement IsRecipeSaved.");

public IReadOnlyList<int> GetSavedRecipes() =>
    throw new NotImplementedException("Part B: implement GetSavedRecipes.");
}
