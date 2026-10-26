using System;
using System.Collections.Generic;
using Xunit;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

public sealed class RecipeManagerTests
{
    // =========================
    // Part A Tests
    // =========================

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

        Assert.Null(manager.FindRecipe(999));
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

        Assert.True(manager.RemoveRecipe(10));
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_NonExistingId_ReturnsFalse()
    {
        var manager = CreateManager();

        Assert.False(manager.RemoveRecipe(999));
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

        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void StartCooking_ExistingRecipe_LoadsInstructions()
    {
        var manager = CreateManager();

        Assert.True(manager.StartCooking(10));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("First step", manager.PeekNextInstruction());
    }

    // =========================
    // Part B1: SearchByTitle
    // =========================

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

    // =========================
    // Part B2: SearchByIngredient
    // =========================

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

    // =========================
    // Part B3: GetHighestProteinRecipes
    // =========================

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

    // =========================
    // Test Data
    // =========================

    private static RecipeManager CreateManager()
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" },
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