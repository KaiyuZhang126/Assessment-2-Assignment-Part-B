using System.Collections.Generic;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

/// <summary>
/// Example tests from the assignment specification. Add your own tests as you work.
/// </summary>
public sealed class RecipeManagerTests
{
    [Fact]
    public void Constructor_BuildsRecipeDictionary()
    {
        var manager = CreateManager();
        Assert.Equal(2, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(10)?.Title);
    }

    [Fact]
    public void InstructionsAreCompletedInFileOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.PeekNextInstruction());
    }

    // Verifies LIFO behaviour of the removed recipe stack: last removed is first restored
    [Fact]
    public void RemovedRecipesAreRestoredLastInFirstOut()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }
    
    // Verifies ingredients are correctly added to the shopping list
    [Fact]
    public void AddIngredientsToShoppingList_ExistingRecipe_AddsCorrectly()
    {
        var manager = CreateManager();
        int addedCount = manager.AddIngredientsToShoppingList(10);

        Assert.Equal(1, addedCount);
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Equal("1 apple", manager.GetShoppingList()[0]);
    }

    [Fact]
    // Test for clearing the shopping list
    // Verifies that ClearShoppingList removes all items from the list
    public void ClearShoppingList_AfterAddingItems_ClearsAll()
    {
        // Arrange: create manager and add ingredients to shopping list
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        
        //clear all items from the shopping list
        manager.ClearShoppingList();
        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Empty(manager.GetShoppingList());
    }


    [Fact]
    // Test for duplicate recipe ID in the Dictionary catalogue
    // Verifies that adding a recipe with an existing ID fails and keeps count unchanged
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        // Arrange: create manager with recipe ID 10 already existing
        var manager = CreateManager();
        var duplicateRecipe = new Recipe { Id = 10, Title = "Duplicate Recipe" };

        bool result = manager.AddRecipe(duplicateRecipe);

        // Assert: add returns false, total recipe count stays at 2
        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
    }


    [Fact]
    public void AddRecipeToCookingPlan_DuplicateRecipe_ReturnsFalse()
    {
        var manager = CreateManager();

        // First attempt to add recipe 10 to cooking plan
        bool firstAdd = manager.AddRecipeToCookingPlan(10);
        // Second attempt to add the same recipe 10 again
        bool secondAdd = manager.AddRecipeToCookingPlan(10);

        // First addition should succeed
        Assert.True(firstAdd);
        // Second addition with duplicate ID should fail
        Assert.False(secondAdd);
        // Cooking plan count should remain 1
        Assert.Equal(1, manager.CookingPlanCount);
    }

        [Fact]
    public void EmptyRemovedRecipeStack_InitialState_ReturnsExpected()
    {
        var manager = CreateManager();

        // Peek on empty stack should return null
        Assert.Null(manager.PeekLastRemovedRecipe());
        // Restore on empty stack should return false
        Assert.False(manager.RestoreLastRemovedRecipe());
        // Removed recipe count should be 0
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

        [Fact]
    public void EmptyInstructionQueue_InitialState_ReturnsExpected()
    {
        var manager = CreateManager();

        // Peek on empty queue should return null
        Assert.Null(manager.PeekNextInstruction());
        // Complete on empty queue should return null
        Assert.Null(manager.CompleteNextInstruction());
        // Pending instruction count should be 0
        Assert.Equal(0, manager.PendingInstructionCount);
    }

        [Fact]
    public void RemoveRecipe_InCookingPlan_ReturnsFalseAndRemainsInCatalogue()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        bool removeResult = manager.RemoveRecipe(10);

        // Remove should fail because recipe is in cooking plan
        Assert.False(removeResult);
        // Recipe should still exist in the catalogue
        Assert.NotNull(manager.FindRecipe(10));
        // Total recipe count remains unchanged
        Assert.Equal(2, manager.RecipeCount);
    }


[Fact]
public void SearchByTitle_NoMatch_ReturnsEmptyList()
{
    // Arrange: Create a RecipeManager
    var manager = CreateManager();

    // Act: Search for a recipe that does not exist
    var result = manager.SearchByTitle("Pizza");

    // Assert: The result should be an empty list
    Assert.Empty(result);
}

[Fact]
public void SearchByTitle_CaseInsensitive_FindsRecipe()
{
    // Arrange: Create a RecipeManager
    var manager = CreateManager();

    // Act: Search using lowercase letters
    var result = manager.SearchByTitle("recipe a");

    // Assert: Only one recipe should be found
    Assert.Single(result);

    // Assert: The matching recipe should have ID 10
    Assert.Equal(10, result[0].Id);
}

[Fact]
public void SearchByTitle_PartialMatch_ReturnsTwoRecipes()
{
    // Arrange: Create a RecipeManager
    var manager = CreateManager();

    // Act: Search using a partial title
    var result = manager.SearchByTitle("Recipe");

    // Assert: Both recipes should be found
    Assert.Equal(2, result.Count);
}

    private static RecipeManager CreateManager()
    {
        return new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" }
            },
            new Recipe
            {
                Id = 20,
                Title = "Recipe B"
            }
        });
    }
}
