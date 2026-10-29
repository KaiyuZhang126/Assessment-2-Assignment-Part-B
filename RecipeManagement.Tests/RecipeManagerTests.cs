using System;
using System.Collections.Generic;
using Xunit;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

public sealed class RecipeManagerTests
{
    // =====================================================
    // Part A: Recipe Management Tests (10 Tests)
    // =====================================================

    [Fact]
    public void Constructor_ValidRecipes_SetsRecipeCount()
    {
        var manager = CreateManager();

        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void FindRecipe_ExistingId_ReturnsRecipe()
    {
        var manager = CreateManager();

        var result = manager.FindRecipe(10);

        Assert.NotNull(result);
        Assert.Equal("Recipe A", result.Title);
    }

    [Fact]
    public void FindRecipe_NonExistingId_ReturnsNull()
    {
        var manager = CreateManager();

        var result = manager.FindRecipe(999);

        Assert.Null(result);
    }

    [Fact]
    public void AddRecipe_ValidRecipe_ReturnsTrue()
    {
        var manager = CreateManager();

        bool result = manager.AddRecipe(new Recipe
        {
            Id = 30,
            Title = "Recipe C"
        });

        Assert.True(result);
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        var manager = CreateManager();

        bool result = manager.AddRecipe(new Recipe
        {
            Id = 10,
            Title = "Duplicate Recipe"
        });

        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_ExistingId_ReturnsTrue()
    {
        var manager = CreateManager();

        bool result = manager.RemoveRecipe(10);

        Assert.True(result);
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_NonExistingId_ReturnsFalse()
    {
        var manager = CreateManager();

        bool result = manager.RemoveRecipe(999);

        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void AddIngredientsToShoppingList_ExistingRecipe_AddsIngredients()
    {
        var manager = CreateManager();

        int result = manager.AddIngredientsToShoppingList(10);

        Assert.Equal(1, result);
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Contains("1 apple", manager.GetShoppingList());
    }

    [Fact]
    public void AddRecipeToCookingPlan_ExistingRecipe_ReturnsTrue()
    {
        var manager = CreateManager();

        bool result = manager.AddRecipeToCookingPlan(10);

        Assert.True(result);
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void StartCooking_ExistingRecipe_LoadsInstructions()
    {
        var manager = CreateManager();

        bool result = manager.StartCooking(10);

        Assert.True(result);
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.PeekNextInstruction());
    }

    // =====================================================
    // Part B1: Search By Title (3 Tests)
    // =====================================================

    [Fact]
    public void SearchByTitle_NoMatch_ReturnsEmptyList()
    {
        var manager = CreateManager();

        var result = manager.SearchByTitle("Pizza");

        Assert.Empty(result);
    }

    [Fact]
    public void SearchByTitle_CaseInsensitive_FindsRecipe()
    {
        var manager = CreateManager();

        var result = manager.SearchByTitle("recipe a");

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
    }

    [Fact]
    public void SearchByTitle_PartialMatch_ReturnsTwoRecipes()
    {
        var manager = CreateManager();

        var result = manager.SearchByTitle("Recipe");

        Assert.Equal(2, result.Count);
    }

    // =====================================================
    // Part B2: Search By Ingredient (3 Tests)
    // =====================================================

    [Fact]
    public void SearchByIngredient_ExistingIngredient_FindsRecipe()
    {
        var manager = CreateManager();

        var result = manager.SearchByIngredient("apple");

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
    }

    [Fact]
    public void SearchByIngredient_NoMatch_ReturnsEmptyList()
    {
        var manager = CreateManager();

        var result = manager.SearchByIngredient("banana");

        Assert.Empty(result);
    }

    [Fact]
    public void SearchByIngredient_CaseInsensitive_FindsRecipe()
    {
        var manager = CreateManager();

        var result = manager.SearchByIngredient("APPLE");

        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
    }

    // =====================================================
    // Part B3: Highest Protein Recipes (6 Tests)
    // =====================================================

    [Fact]
    public void GetHighestProteinRecipes_TwoRecipes_ReturnsDescendingOrder()
    {
        var manager = CreateManager();

        var result = manager.GetHighestProteinRecipes(2);

        Assert.Equal(2, result.Count);
        Assert.Equal(20, result[0].Id);
        Assert.Equal(10, result[1].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_CountOne_ReturnsHighestProteinRecipe()
    {
        var manager = CreateManager();

        var result = manager.GetHighestProteinRecipes(1);

        Assert.Single(result);
        Assert.Equal(20, result[0].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_CountZero_ReturnsEmptyList()
    {
        var manager = CreateManager();

        var result = manager.GetHighestProteinRecipes(0);

        Assert.Empty(result);
    }

    [Fact]
    public void GetHighestProteinRecipes_NegativeCount_ReturnsEmptyList()
    {
        var manager = CreateManager();

        var result = manager.GetHighestProteinRecipes(-1);

        Assert.Empty(result);
    }

    [Fact]
    public void GetHighestProteinRecipes_MissingNutrition_ExcludesRecipe()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "No Nutrition"
            },
            new Recipe
            {
                Id = 2,
                Title = "With Protein",
                Nutrition = new NutritionInfo
                {
                    ProteinG = 30
                }
            }
        };

        var manager = new RecipeManager(recipes);

        var result = manager.GetHighestProteinRecipes(2);

        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public void GetHighestProteinRecipes_MissingProteinValue_ExcludesRecipe()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Unknown Protein",
                Nutrition = new NutritionInfo
                {
                    ProteinG = null
                }
            },
            new Recipe
            {
                Id = 2,
                Title = "Known Protein",
                Nutrition = new NutritionInfo
                {
                    ProteinG = 25
                }
            }
        };

        var manager = new RecipeManager(recipes);

        var result = manager.GetHighestProteinRecipes(2);

        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    // =====================================================
    // Part B4: Saved Recipes (7 Tests)
    // =====================================================

    [Fact]
    public void AddSavedRecipe_ExistingRecipe_ReturnsTrue()
    {
        var manager = CreateManager();

        bool result = manager.AddSavedRecipe(10);

        Assert.True(result);
        Assert.True(manager.IsRecipeSaved(10));
        Assert.Contains(10, manager.GetSavedRecipes());
    }

    [Fact]
    public void AddSavedRecipe_DuplicateRecipe_ReturnsFalse()
    {
        var manager = CreateManager();

        bool firstResult = manager.AddSavedRecipe(10);
        bool secondResult = manager.AddSavedRecipe(10);

        Assert.True(firstResult);
        Assert.False(secondResult);
        Assert.Single(manager.GetSavedRecipes());
    }

    [Fact]
    public void AddSavedRecipe_NonExistingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();

        bool result = manager.AddSavedRecipe(999);

        Assert.False(result);
        Assert.Empty(manager.GetSavedRecipes());
    }

    [Fact]
    public void RemoveSavedRecipe_ExistingSavedRecipe_ReturnsTrue()
    {
        var manager = CreateManager();

        manager.AddSavedRecipe(10);

        bool result = manager.RemoveSavedRecipe(10);

        Assert.True(result);
        Assert.False(manager.IsRecipeSaved(10));
        Assert.Empty(manager.GetSavedRecipes());
    }

    [Fact]
    public void RemoveSavedRecipe_NotSaved_ReturnsFalse()
    {
        var manager = CreateManager();

        bool result = manager.RemoveSavedRecipe(10);

        Assert.False(result);
        Assert.Empty(manager.GetSavedRecipes());
    }

    [Fact]
    public void IsRecipeSaved_SavedAndUnsavedRecipes_ReturnsCorrectStatus()
    {
        var manager = CreateManager();

        manager.AddSavedRecipe(10);

        Assert.True(manager.IsRecipeSaved(10));
        Assert.False(manager.IsRecipeSaved(20));
    }

    [Fact]
    public void GetSavedRecipes_TwoSavedRecipes_ReturnsBothIds()
    {
        var manager = CreateManager();

        manager.AddSavedRecipe(10);
        manager.AddSavedRecipe(20);

        var result = manager.GetSavedRecipes();

        Assert.Equal(2, result.Count);
        Assert.Contains(10, result);
        Assert.Contains(20, result);
    }

    // =====================================================
    // Part B5: Integration Test (1 Test)
    // =====================================================

    [Fact]
    public void SearchSaveAndRemoveRecipe_IntegrationTest()
    {
        // Arrange: Create a RecipeManager
        var manager = CreateManager();

        // Step 1: Search for Recipe A by title
        var searchResults = manager.SearchByTitle("Recipe A");

        // Verify that exactly one recipe was found
        Assert.Single(searchResults);

        // Get the ID of the found recipe
        int recipeId = searchResults[0].Id;

        // Step 2: Save the recipe
        bool added = manager.AddSavedRecipe(recipeId);

        // Verify the recipe was saved successfully
        Assert.True(added);
        Assert.True(manager.IsRecipeSaved(recipeId));

        // Step 3: Remove the saved recipe
        bool removed = manager.RemoveSavedRecipe(recipeId);

        // Verify the recipe was removed successfully
        Assert.True(removed);
        Assert.False(manager.IsRecipeSaved(recipeId));
    }

    // =====================================================
    // Test Data: CreateManager()
    // =====================================================

    private static RecipeManager CreateManager()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new()
                {
                    "1 apple"
                },
                Instructions = new()
                {
                    "First step",
                    "Second step"
                },
                Nutrition = new NutritionInfo
                {
                    ProteinG = 20
                }
            },

            new Recipe
            {
                Id = 20,
                Title = "Recipe B",
                Nutrition = new NutritionInfo
                {
                    ProteinG = 45
                }
            }
        };

        return new RecipeManager(recipes);
    }
}