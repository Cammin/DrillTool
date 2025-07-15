using System.Reflection;
using BepInEx;
using Nautilus.Utility;
using UnityEngine;

namespace TestMod;

internal class AssetBundles
{
    // Usually this is done in your Plugin script but technically you can do it wherever
    public static AssetBundle DrillToolBundle { get; private set; }

    public static void Load()
    {
        DrillToolBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "handminer");

        // This name needs to be the exact same name as the prefab you put in the bundle
        //GameObject mirrorVariant1 = MyAssetBundle.LoadAsset<GameObject>("HandMiner");
    }
}