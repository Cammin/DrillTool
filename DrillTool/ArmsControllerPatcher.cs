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
    public static AssetBundle ArmsControllerAnimationBundle;

    public static AnimatorOverrideController OverrideController;
    
    [HarmonyPatch(nameof(ArmsController.Start))]
    [HarmonyPostfix]
    public static void Start(ArmsController __instance)
    {
        RuntimeAnimatorController controller = __instance.animator.runtimeAnimatorController;
        
        Plugin.Logger.LogInfo($"TryCreateOverrideController"); 
        
        if (OverrideController != null) return;

        if (!ArmsControllerAnimationBundle)
        {
            ArmsControllerAnimationBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "firstpersonanimations");
        }
        var clips = ArmsControllerAnimationBundle.LoadAllAssets<AnimationClip>();

        var newClips = new Dictionary<string, AnimationClip>(clips.Length, StringComparer.Ordinal);
        foreach (var c in clips)
            newClips.Add(c.name, c);
        
        OverrideController = new AnimatorOverrideController(controller);
        OverrideController.runtimeAnimatorController = controller;
        OverrideController.name = "DrillToolOverride";

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(OverrideController.overridesCount);
        OverrideController.GetOverrides(overrides);

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
                
                if (newClips.TryGetValue(replacement, out AnimationClip replacementClip))
                {
                    Plugin.Logger.LogInfo($"Setup animation replacement for \"{original}\" with \"{replacement}\"");
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, replacementClip);
                    return; 
                }

                Plugin.Logger.LogError($"failed to setup animation replacement for \"{original}\" with \"{replacement}\"");
            }
        }
        OverrideController.ApplyOverrides(overrides);
        
        //set new controller
        __instance.animator.runtimeAnimatorController = OverrideController; 
        Plugin.Logger.LogInfo($"Applied all aniamtion overrides");
    }
}