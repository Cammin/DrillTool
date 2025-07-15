using HarmonyLib;
using UnityEngine;

namespace TestMod;

[HarmonyPatch(typeof(Drillable))]
public class DrillablePatcher
{
    [HarmonyPatch(nameof(Drillable.HoverDrillable))]
    [HarmonyPrefix]
    public static bool HoverDrillable(Drillable __instance)
    {
        Inventory inventory = Inventory.Get();
        if (!inventory) return true;
        
        var drill = inventory.GetHeldTool() as DrillTool;
        if (drill)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<string>("DrillResource", Language.main.Get(__instance.primaryTooltip)), false, GameInput.Button.RightHand);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, __instance.secondaryTooltip, true, GameInput.Button.None);
            HandReticle.main.SetIcon(HandReticle.IconType.Drill, 1f);
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(Drillable.OnDrill))]
    [HarmonyPrefix]
    public static void OnDrill(Drillable __instance, Vector3 position, Exosuit exo, out GameObject hitObject)
    {
        hitObject = null;

        //return to skip the rest of the fucntion
        //return;
    }
    
    
}
