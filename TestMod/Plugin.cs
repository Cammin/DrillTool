using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TestMod.Items;
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

        InitializePrefabs();

        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        
        Logger.LogWarning($"CUSTOM!!!!!!!!!!!!!!!!!!!              Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Debug.LogError("We Have Setup!");
    }

    private void InitializePrefabs()
    {
        AssetBundles.Load();
        
        Coal.Register();
        YeetKnifePrefab.Register();
        HandDrill.Patch();
    }
}