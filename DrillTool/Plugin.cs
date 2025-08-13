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
    [Slider("Drill Energy Cost", 0, 1, Format = "{0:F2}", Step = 0.01f, DefaultValue = 0.09f, Tooltip = "Battery drain per resource deposit \"hit\".\nIt takes up to 320 hits to fully destroy a deposit.")]
    public float DrillToolEnergyCost = 0.09f;
    [Slider("Hit Interval", 0, 0.5f, Format = "{0:F2}", Step = 0.01f, DefaultValue = 0.13f, Tooltip = "Hit interval on a resource deposit.\nUse the default to match the mining speed of a Prawn suit drill arm.")]
    public float DrillToolHitInterval = 0.13f;
}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.cammin.drilltool";
    private const string PluginName = "DrillTool";
    private const string PluginVersion = "1.0.0";
    
    public new static ManualLogSource Logger { get; private set; }
    //public static AssetBundle DrillToolBundle { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions ModConfig { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();

    private void Awake()
    {
        Logger = base.Logger;

        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(DrillablePatcher.RestoreDrillable), typeof(DrillablePatcher), nameof(DrillablePatcher.RestoreDrillable), null);
        Harmony.CreateAndPatchAll(Assembly, PluginGuid);
        
        //DrillToolBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "handminer");
        
        DrillToolAuthoring.Register();
        
    }
}