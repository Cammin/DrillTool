using System.Linq;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Handlers;

namespace DrillTool;

//[HarmonyPatch(typeof(PDAHandler))]
//[HarmonyPatch(typeof(CraftDataHandler))]
public static class LogDeathrunSets
{
    //[HarmonyPatch(typeof(PDAHandler), nameof(PDAHandler.EditFragmentsToScan))]
    //[HarmonyPrefix]
    public static void EditFragmentsToScan(TechType techType, int fragmentCount)
    {
        Plugin.Logger.LogInfo($"Setting fragments required for {techType}: {fragmentCount}");
    }
    
    //[HarmonyPatch(typeof(CraftDataHandler), nameof(CraftDataHandler.SetRecipeData))]
    //[HarmonyPrefix]
    public static void SetRecipeData(TechType techType, RecipeData recipeData)
    {
        if (recipeData != null)
        {
            string IngredientToString(Ingredient ing)
            {
                if (ing == null) return "NULL";
                return $"{ing.amount} {ing.techType}";
            }
            string things = string.Join(", ", recipeData.Ingredients.Select(IngredientToString));
            Plugin.Logger.LogInfo($"Setting recipe for {techType}: {things}");
        }
        else
        {
            Plugin.Logger.LogInfo($"Setting recipe for {techType}: <RecipeData NULL>");
        }
    }
}