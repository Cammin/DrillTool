using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using Nautilus.Utility;
using UnityEngine;

namespace TestMod;

[Menu("Drill Tool")]
public class ModOptions : ConfigFile
{            
    [Slider("Drill Energy Cost", 0, 100, Tooltip = "Battery drain per resource deposit hit on the drill tool")]
    public int DrillToolEnergyCost = 10;
}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.cammin.drilltool";
    private const string PluginName = "DrillTool";
    private const string PluginVersion = "1.0.0";
    
    public new static ManualLogSource Logger { get; private set; }
    public static AssetBundle DrillToolBundle { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions ModConfig { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();

    private void Awake()
    {
        Logger = base.Logger;

        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(DrillablePatcher.RestoreDrillable), typeof(DrillablePatcher), nameof(DrillablePatcher.RestoreDrillable), null);
        Harmony.CreateAndPatchAll(Assembly, PluginGuid);
        
        DrillToolBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "handminer");
        
        DrillToolAuthoring.Register();
        
    }
}