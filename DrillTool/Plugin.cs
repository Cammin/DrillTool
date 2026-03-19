using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace DrillTool;

[Menu("Drill Tool")]
public class ModOptions : ConfigFile
{    
    [Slider(0, 1, DefaultValue = 0.09f, Step = 0.01f, Format = "{0:F2}", LabelLanguageId = DrillToolLanguage.EnergyCostLabel, TooltipLanguageId = DrillToolLanguage.EnergyCostTooltip)]
    public float DrillToolEnergyCost = 0.09f;
    [Slider(0, 0.5f, DefaultValue = 0.13f, Step = 0.01f, Format = "{0:F2}", LabelLanguageId = DrillToolLanguage.HitIntervalLabel, TooltipLanguageId = DrillToolLanguage.HitIntervalTooltip)]
    public float DrillToolHitInterval = 0.13f;
}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.cammin.drilltool";
    private const string PluginName = "Drill Tool";
    private const string PluginVersion = "0.6.0"; 
    
    public new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions ModConfig { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();

    private void Awake()
    {
        Logger = base.Logger;
        LanguageHandler.RegisterLocalizationFolder();

        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(DrillablePatcher.RestoreDrillable), typeof(DrillablePatcher), nameof(DrillablePatcher.RestoreDrillable), null);
        Harmony.CreateAndPatchAll(Assembly, PluginGuid);
        
        ArmsControllerPatcher.Initialize();
        DrillToolAuthoring.Register();
        
        FragmentAuthoring.Register();
        Ency.Register();
    }
}