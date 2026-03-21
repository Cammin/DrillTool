using System.Collections.Generic;
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
        
        //pinata is the list of loot that is primed to enter the exosuit.
        //prevent adding to the pinata if an exosuit isn't involved, so that the resources don't automatically go into the suit if entered at a later time 
        __instance.lootPinataOnSpawn = exo != null || Plugin.ModConfig.DrillToolAutoCollect;
        invalidatedAutoCollect = false;
        return true;
    }

    private static bool invalidatedAutoCollect = false;
    [HarmonyPatch(nameof(Drillable.ManagedUpdate))]
    [HarmonyPrefix]
    public static bool ManagedUpdate(Drillable __instance)
    {
        if (invalidatedAutoCollect) return true;
        if (!Plugin.ModConfig.DrillToolAutoCollect) return true;
        if (__instance.drillingExo) return true;
        if (__instance.lootPinataObjects.Count <= 0) return true;
        
        Player player = Player.main;
        if (!player) return true;
        
        Inventory inventory = Inventory.Get();
        if (!inventory) return true;

        //if the player quickly unequips the drill or enters a vehicle, then invalidate the pinata
        if (inventory.GetHeldTool() is not DrillTool || player.GetVehicle())
        {
            invalidatedAutoCollect = true;
            __instance.lootPinataObjects.Clear();
            return true;
        }
        
        List<GameObject> removalList = new List<GameObject>();
        foreach (GameObject lootObj in __instance.lootPinataObjects)
        {
            if (lootObj == null)
            {
                removalList.Add(lootObj);
                continue;
            }

            Vector3 deliverPos = player.transform.position + new Vector3(0f, 0.6f, 0f);
            lootObj.transform.position = Vector3.Lerp(lootObj.transform.position, deliverPos, Time.deltaTime * 5f);

            float dist = Vector3.Distance(lootObj.transform.position, deliverPos);
            if (dist > 2f) continue;
            
            Pickupable pickup = lootObj.GetComponentInChildren<Pickupable>();
            if (!pickup) continue;
                
            if (!inventory.HasRoomFor(pickup))
            {
                ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
            }
            else
            {
                ErrorMessage.AddMessage($"{Language.main.Get(pickup.GetTechName())} {Language.main.Get("AddedToInventory")}");
                uGUI_IconNotifier.main.Play(pickup.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
                pickup.Initialize();
                InventoryItem inventoryItem = new InventoryItem(pickup);
                inventory.container.UnsafeAdd(inventoryItem);
                pickup.PlayPickupSound();
            }
            removalList.Add(lootObj);
        }
        
        if (removalList.Count > 0)
        {
            foreach (GameObject remove in removalList)
            {
                __instance.lootPinataObjects.Remove(remove);
            }
        }

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
