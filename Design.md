# Recipe Management System Design

## Overview
This project is a recipe management system written in C#. The main class is `RecipeManager`, which implements the `IRecipeManager` interface. The program allows users to manage recipes, create shopping lists, organise cooking plans, follow cooking instructions, search for recipes and save recipes. Part B adds new features to the system developed in Part A.

## Data Structures
### Dictionary
I used `Dictionary<int, Recipe>` to store the recipe catalogue. Each recipe has a unique ID, which is used as the key. This makes it easy to add, find and remove recipes without searching through the whole collection.

### List
I used `List<string>` for the shopping list. Ingredients from different recipes can be added to the list in order. I chose List because it allows duplicate items, which is useful when different recipes need the same ingredient.

### LinkedList
I used `LinkedList<int>` to manage the cooking plan. It stores recipe IDs in the order they are added. Recipes can be added to the end of the plan or removed when needed. LinkedList is useful because adding and removing nodes is efficient once the correct node is found.

### Stack
I used `Stack<int>` to keep track of recipes removed from the cooking plan. Stack follows the Last In, First Out (LIFO) rule. This means the most recently removed recipe is restored first, which is useful for an undo feature.

### Queue
I used `Queue<string>` to manage cooking instructions. Queue follows the First In, First Out (FIFO) rule, so instructions are completed in the same order they were added. This helps users follow the cooking steps in the correct sequence.

### HashSet
I used `HashSet<int>` to store saved recipe IDs. HashSet does not allow duplicate values, so the same recipe cannot be saved twice. It also makes checking whether a recipe is saved quick and simple.

## Part B Features
### Search by Title
The `SearchByTitle()` method uses LINQ to find recipes by their titles. The search ignores uppercase and lowercase differences and supports partial matches. For example, searching for "recipe" can return both "Recipe A" and "Recipe B".

### Search by Ingredient
The `SearchByIngredient()` method searches through the ingredients of each recipe. It also ignores uppercase and lowercase differences and supports partial matches. This allows users to find recipes using ingredients they already have.

### Highest Protein Recipes
The `GetHighestProteinRecipes()` method sorts recipes by protein content from highest to lowest using LINQ. Recipes without protein information are excluded. The method returns the requested number of recipes, and a count of zero or less returns an empty list.

### Saved Recipes
The saved recipes feature uses four methods. `AddSavedRecipe()` adds a recipe ID to the saved collection, while `RemoveSavedRecipe()` removes it. `IsRecipeSaved()` checks whether a recipe is saved, and `GetSavedRecipes()` returns the saved recipe IDs. I used HashSet because it prevents duplicates and supports fast lookup.

## Integration
Part B uses the same `RecipeManager` and recipe catalogue created in Part A. I did not create another collection to store all the recipes. Instead, the search and sorting methods use the existing Dictionary. The saved recipes feature only stores recipe IDs in a HashSet, so complete recipe objects do not need to be copied. I also added an integration test that searches for a recipe, saves it, checks its saved status, removes it and checks the status again. This test shows that the different features work together.

## Basic Complexity
### Dictionary Lookup — Average O(1)
Dictionary uses keys to find recipes quickly. On average, the lookup time stays constant even when more recipes are added.

### LinkedList Traversal — O(n)
Finding a specific recipe in a LinkedList may require checking every node. The time needed increases as the number of nodes grows.

### Queue Dequeue — O(1)
Removing the first instruction from a Queue takes constant time because the first element can be accessed directly.

### LINQ Search — O(n)
Searching for recipes with LINQ may require checking every recipe in the catalogue. The search time increases with the number of recipes.

### HashSet Contains — Average O(1)
HashSet uses hashing to check whether a recipe ID exists. This operation takes constant time on average.

Here, n represents the number of elements in the collection.

## Testing
I used xUnit to test the program. The tests cover recipe management, searching by title and ingredient, protein sorting, saved recipes and integration between features. I also tested cases such as duplicate recipe IDs, missing recipes and empty results. I ran `dotnet test`, and all 30 tests passed.

## Conclusion
This project helped me understand how different data structures can be used for different tasks. Dictionary is useful for finding recipes, List stores shopping items, LinkedList manages the cooking plan, Stack supports undo operations, Queue keeps cooking instructions in order, and HashSet stores saved recipes without duplicates. I also used LINQ for searching and sorting. The tests helped me check that the methods work correctly and that the new features work with the existing system.