using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeathrunRemade.Objects.Enums;
using Newtonsoft.Json;

namespace DrillTool;

/// <summary>
/// the recipe changes are applied when a game save is started/loaded.
/// So all we have to do is insert the recipe change into VanillaRecipeChanges
/// </summary>
public static class ConfigFileLoader
{
    public const string RecipesFileName = "DeathRunRecipes.json";
    public const string ScansFileName = "DeathRunScans.json";
    
    private static string _configDir;
    
    private const string FragmentName = "DrillToolFragment";
    
    public static Dictionary<string, SerialScanData> DefaultScans = new Dictionary<string, SerialScanData>()
    {
        { nameof(Difficulty4.Normal), new SerialScanData(FragmentName, 2) },
        { nameof(Difficulty4.Hard), new SerialScanData(FragmentName, 4) },
        { nameof(Difficulty4.Deathrun), new SerialScanData(FragmentName, 6) },
        { nameof(Difficulty4.Kharaa), new SerialScanData(FragmentName, 6) }
    };
    
    public static Dictionary<string, List<SerialIngredient>> DefaultRecipes = new()
    {
        {
            nameof(Difficulty4.Normal), new List<SerialIngredient>
            {
                new SerialIngredient(TechType.Diamond, 3),
                new SerialIngredient(TechType.Titanium, 2),
                new SerialIngredient(TechType.WiringKit, 1),
                new SerialIngredient(TechType.Battery, 1),
            }
        },
        {
            nameof(Difficulty4.Hard), new List<SerialIngredient>
            {
                new SerialIngredient(TechType.Diamond, 3),
                new SerialIngredient(TechType.Magnetite, 1),
                new SerialIngredient(TechType.WiringKit, 1),
            }
        },
        {
            nameof(Difficulty4.Deathrun), new List<SerialIngredient>
            {
                new SerialIngredient(TechType.Diamond, 3),
                new SerialIngredient(TechType.Magnetite, 2),
                new SerialIngredient(TechType.AdvancedWiringKit, 1),
            }
        },
        {
            nameof(Difficulty4.Kharaa), new List<SerialIngredient>
            {
                new SerialIngredient(TechType.Diamond, 3),
                new SerialIngredient(TechType.Magnetite, 3),
                new SerialIngredient(TechType.AdvancedWiringKit, 1),
            }
        }
    };
    
    //the keys are Difficulty4
    public static Dictionary<string, SerialTechData> RecipeJson;
    public static Dictionary<string, SerialScanData> ScansJson;
    
    /// <summary>
    /// ScansJson and RecipeJson will always be filled; if failed to load json, will use the hardcoded fallbacks
    /// </summary>
    /// <param name="configFolderName"></param>
    public static void LoadConfigFolder(string configFolderName = "Config")
    {
        _configDir = Path.Combine(Plugin.ModPath, configFolderName);
        if (!Directory.Exists(_configDir))
        {
            Plugin.Logger.LogError($"Directory '{_configDir}' does not exist");
            return;
        }
        
        ScansJson = LoadConfigFile<Dictionary<string, SerialScanData>>(ScansFileName);
        ScansJson ??= DefaultScans;
        
        var ingredients = LoadConfigFile<Dictionary<string, List<SerialIngredient>>>(RecipesFileName);
        ingredients ??= DefaultRecipes;
        
        //this asserts that DrillToolAuthoring.Info is registered
        RecipeJson = ingredients.ToDictionary(
            pair => pair.Key,
            pair => new SerialTechData(DrillToolAuthoring.Info.TechType, 1, pair.Value));
    }

    private static T LoadConfigFile<T>(string file) where T : class
    {
        string path = Path.Combine(_configDir, file);
        if (!File.Exists(path))
        {
            Plugin.Logger.LogError($"File does not exist at '{path}' ");
            return null;
        }

        Plugin.Logger.LogInfo($"Loading config file: {file}");
        T content;
        try
        {
            string txt = File.ReadAllText(path);
            content = JsonConvert.DeserializeObject<T>(txt);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Exception caught while trying to deserialize config file at path: '{path}'. Exception: {e}");
            return null;
        }
            
        if (content == null)
        {
            Plugin.Logger.LogError($"Config file '{file}' is empty");
            return null;
        }

        return content;
    }
    
    public static List<Ingredient> NormalIngredients()
    {
        return RecipeJson[nameof(Difficulty4.Normal)].ingredients.Select(p => p.ToIngredient()).ToList();
    }
    public static int NormalScans()
    {
        return ScansJson[nameof(Difficulty4.Normal)].amount;
    }
}