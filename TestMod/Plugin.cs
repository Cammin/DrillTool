using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using UnityEngine;

namespace TestMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        Logger = base.Logger;

        ConsoleCommandsHandler.RegisterConsoleCommand(nameof(DrillablePatcher.RestoreDrillable), typeof(DrillablePatcher), nameof(DrillablePatcher.RestoreDrillable), null);
        
        InitializePrefabs();

        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
    }

    private void InitializePrefabs()
    {
        AssetBundles.Load();
        
        //Coal.Register();
        //YeetKnifePrefab.Register();
        DrillToolAuthoring.Register();
    }
}