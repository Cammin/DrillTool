using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using UnityEngine;

namespace TestMod;

[Menu("Drill Tool")]
public class ModOptions : ConfigFile
{            
    [Slider("Drill Energy Cost", 0, 100, Tooltip = "Battery drain per resource deposit hit on the drill tool")]
    public int DrillToolEnergyCost = 10;
}

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "com.cammin.testmod";
    public const string PLUGIN_NAME = "TestMod";
    public const string PLUGIN_VERSION = "1.0.0";
    
    public new static ManualLogSource Logger { get; private set; }
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static ModOptions Config { get; } = OptionsPanelHandler.RegisterModOptions<ModOptions>();

    private void Awake()
    {
        Logger = base.Logger;

        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(DrillablePatcher.RestoreDrillable), typeof(DrillablePatcher), nameof(DrillablePatcher.RestoreDrillable), null);
        
        AssetBundles.Load();
        
        //Coal.Register();
        //YeetKnifePrefab.Register();
        DrillToolAuthoring.Register();

        Harmony.CreateAndPatchAll(Assembly, PLUGIN_GUID);
    }
}