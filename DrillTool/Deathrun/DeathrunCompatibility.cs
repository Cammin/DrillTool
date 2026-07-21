using System;
using System.Linq;
using BepInEx.Bootstrap;
using DeathrunRemade;
using DeathrunRemade.Objects.Enums;

namespace DrillTool;

//MAIN GOAL: the VanillaRecipeChanges needs to have it's _scanNumberCache and _recipeCache given new data.
//this data is loaded by our own config files in our own location
public static class DeathrunCompatibility
{
    public static bool HasDeathrun;

    public static void Register()
    {
        HasDeathrun = Chainloader.PluginInfos.ContainsKey(Plugin.DeathrunGuid);
        Plugin.Logger.LogInfo("Deathrun Compatibility: " + (HasDeathrun ? "Enabled" : "Disabled"));
        
        if (HasDeathrun)
        {
            RemoveBatteryFromNormalRecipe();
            PatchTheJsonLists();
        }
    }

    private static void RemoveBatteryFromNormalRecipe()
    {
        var ingredients = ConfigFileLoader.RecipeJson[nameof(Difficulty4.Normal)].ingredients;
        SerialIngredient battery = ingredients.Find(p => p.techType == TechType.Battery);
        if (battery != null)
        {
            ingredients.Remove(battery);
        }
    }

    private static void PatchTheJsonLists()
    {
        foreach (var pair in ConfigFileLoader.RecipeJson)
        {
            //if normal, then it adds to the "remove battery" list instead
            string key = pair.Key == nameof(Difficulty4.Normal) ? "RemoveBatteries" : $"ToolCosts.{pair.Key}";
            
            if (DeathrunInit._recipeChanges._recipeJson.ContainsKey(key))
            {
                DeathrunInit._recipeChanges._recipeJson[key].Add(ToDeathrunVersion(pair.Value));
            }
            else
            {
                Plugin.Logger.LogError($"_recipeJson doesnt have key {key}?");
            }
        }
        
        foreach (var pair in ConfigFileLoader.ScansJson)
        {
            //skip normal; it doesn't utilize that
            if (pair.Key == nameof(Difficulty4.Normal)) continue;
            
            if (DeathrunInit._recipeChanges._fragmentJson.ContainsKey(pair.Key))
            {
                DeathrunInit._recipeChanges._fragmentJson[pair.Key].Add(ToDeathrunVersion(pair.Value));
            }
            else
            {
                Plugin.Logger.LogError($"_fragmentJson doesnt have key {pair.Key}?");
            }
        }
        Plugin.Logger.LogInfo("Inserted DrillTool recipes & scans into DeathRun");

        DisplayDeathrunChanges();
    }

    private static void DisplayDeathrunChanges()
    {
        foreach (var pair in DeathrunInit._recipeChanges._recipeJson)
        {
            Plugin.Logger.LogInfo($"Displaying {pair.Value.Count} {pair.Key} recipes:\n{string.Join("\n", pair.Value.Select(p => $"{p.techType}: {string.Join(", ", p.ingredients.Select(p => $"{p.amount} {p.techType}"))}"))}\n");
        }
        foreach (var pair in DeathrunInit._recipeChanges._fragmentJson)
        {
            Plugin.Logger.LogInfo($"Displaying {pair.Value.Count} {pair.Key} scans:\n{string.Join("\n", pair.Value.Select(p => $"{p.techType}: {p.amount}"))}\n");
        }
    }

    private static DeathrunRemade.Objects.SerialScanData ToDeathrunVersion(SerialScanData data)
    {
        TechType type = (TechType)Enum.Parse(typeof(TechType), data.techType);
        return new DeathrunRemade.Objects.SerialScanData(type, data.amount);
    }

    private static DeathrunRemade.Objects.SerialTechData ToDeathrunVersion(SerialTechData data)
    {
        return new DeathrunRemade.Objects.SerialTechData()
        {
            techType = data.techType,
            craftAmount = data.craftAmount,
            ingredients =  data.ingredients.Select(ToDeathrunVersion).ToList(),
        };
    }

    private static DeathrunRemade.Objects.SerialIngredient ToDeathrunVersion(SerialIngredient data)
    {
        return new DeathrunRemade.Objects.SerialIngredient(data.techType, data.amount);
    }
}