using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using Nautilus.Utility;
using UnityEngine;

namespace DrillTool;

[Menu("Drill Tool")]
public class ModOptions : ConfigFile
{    
    [Slider(0, 1, DefaultValue = 0.07f, Step = 0.01f, Format = "{0:F2}", LabelLanguageId = DrillToolLanguage.EnergyCostLabel, TooltipLanguageId = DrillToolLanguage.EnergyCostTooltip)]
    public float DrillToolEnergyCost = 0.09f;
    [Slider(0, 0.5f, DefaultValue = 0.13f, Step = 0.01f, Format = "{0:F2}", LabelLanguageId = DrillToolLanguage.HitIntervalLabel, TooltipLanguageId = DrillToolLanguage.HitIntervalTooltip)]
    public float DrillToolHitInterval = 0.13f;
    [Toggle(LabelLanguageId = DrillToolLanguage.AutoCollectLabel, TooltipLanguageId = DrillToolLanguage.AutoCollectTooltip)]
    public bool DrillToolAutoCollect = true;
}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency(DeathrunGuid, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.cammin.drilltool";
    private const string PluginName = "Drill Tool";
    private const string PluginVersion = "1.2.0";
    public const string DeathrunGuid = "com.github.tinyhoot.DeathrunRemade"; 
    
    public new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions ModConfig { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();
    public static string ModPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    private void Awake()
    {
        Logger = base.Logger;
        LanguageHandler.RegisterLocalizationFolder();

        DrillablePatcher.RegisterConsoleCommands();
        Harmony.CreateAndPatchAll(Assembly, PluginGuid);
        
        ArmsControllerPatcher.Initialize();
        DrillToolAuthoring.Register();
        
        FragmentAuthoring.Register();
        FragmentCrateAuthoring.Register();
        
        DeathrunCompatibility.Register();
    }

    private void OnDestroy()
    {
        DrillToolAuthoring.Unregister();
    }

    public static AssetBundle LoadBundle(string bundleName)
    {
        return AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly, bundleName);
    }
}