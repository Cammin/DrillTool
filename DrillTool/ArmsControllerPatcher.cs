using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Nautilus.Utility;
using UnityEngine;

namespace DrillTool;

[HarmonyPatch(typeof(ArmsController))]
public class ArmsControllerPatcher
{
    private static AssetBundle ArmsControllerAnimationBundle;
    private static Dictionary<string, AnimationClip> ClipsToPatch;
    
    public static void Initialize()
    {
        ArmsControllerAnimationBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "firstpersonanimations");
        var clips = ArmsControllerAnimationBundle.LoadAllAssets<AnimationClip>();

        ClipsToPatch = new Dictionary<string, AnimationClip>(clips.Length, StringComparer.Ordinal);
        foreach (var c in clips)
            ClipsToPatch.Add(c.name, c);
    } 
    
    [HarmonyPatch(nameof(ArmsController.Start))]
    [HarmonyPostfix]
    public static void Start(ArmsController __instance)
    {
        __instance.animator.runtimeAnimatorController = CreateOverrideController(__instance.animator.runtimeAnimatorController);
    }

    private static AnimatorOverrideController CreateOverrideController(RuntimeAnimatorController controller)
    {
        AnimatorOverrideController overrideController = new AnimatorOverrideController(controller);
        overrideController.runtimeAnimatorController = controller;
        overrideController.name = "DrillToolOverride";

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overrides);

        for (var i = 0; i < overrides.Count; i++)
        {
            TryReplace("palyer_view_terraformer_idle", "drill_tool_idle");
            
            TryReplace("palyer_view_terraformer_equip", "drill_tool_equip");
            TryReplace("player_view_terraformer_unequip", "drill_tool_unequip");
            
            TryReplace("player_view_terraformer_bash", "drill_tool_bash");
            
            TryReplace("player_view_terraformer_panel_open_start", "drill_tool_use_start");
            TryReplace("plalyer_view_terraformer_panel_open_loop", "drill_tool_use_loop");
            TryReplace("player_view_terraformer_panel_open_end", "drill_tool_use_end");
            
            
            void TryReplace(string original, string replacement)
            {
                if (overrides[i].Key.name != original) return;
                
                if (ClipsToPatch.TryGetValue(replacement, out AnimationClip replacementClip))
                {
                    Plugin.Logger.LogInfo($"Setup animation replacement for \"{original}\" with \"{replacement}\"");
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, replacementClip);
                    return; 
                }
                Plugin.Logger.LogError($"failed to setup animation replacement for \"{original}\" with \"{replacement}\"");
            }
        }
        overrideController.ApplyOverrides(overrides);
        return overrideController;
    }
}