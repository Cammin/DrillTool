using HarmonyLib;
using UnityEngine;

namespace DrillTool;

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
    public static bool OnDrill(Drillable __instance, Vector3 position, Exosuit exo, out GameObject hitObject)
    {
        hitObject = null;
        
        //prevent adding to the pinata if an exosuit isn't involved, so that the resources don't automatically go into the suit if entered at a later time 
        __instance.lootPinataOnSpawn = exo != null;
        return true;
    }
    
    public static void RestoreDrillable()
    {
        Drillable drillable = FindNearestDrillable();
        if (drillable)
        {
            drillable.Restore();
            ErrorMessage.AddMessage($"Drillable \"{drillable.GetDominantResourceType()}\" restored");
        }
    }
    private static Drillable FindNearestDrillable(float maxDistance = 10f)
    {
        Drillable nearestDrillable = null;
        float closestDistance = maxDistance;
    
        // Find all drillables in range
        Drillable[] drillables = Object.FindObjectsOfType<Drillable>();
    
        foreach (Drillable drillable in drillables)
        {
            float distance = Vector3.Distance(Player.main.transform.position, drillable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestDrillable = drillable;
            }
        }
    
        return nearestDrillable;
    }

}
