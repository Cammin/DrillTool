using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace TestMod.Items;

internal static class HandDrillAuthoring
{
    public static PrefabInfo MyPrefabInfo { get; private set; }

    public static void Register()
    {
        MyPrefabInfo = PrefabInfo
            .WithTechType("HandDrill", "Drill tool", "Enables agile mining of resources.")
            .WithIcon(SpriteManager.Get(TechType.StasisRifle));

        var prefab = new CustomPrefab(MyPrefabInfo);

        RecipeData recipe = new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new CraftData.Ingredient(TechType.AdvancedWiringKit, 1),
                new CraftData.Ingredient(TechType.Diamond, 4),
                new CraftData.Ingredient(TechType.Titanium, 1),
            },
        };
        
        prefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Tools");

        //AirBladder
        /*var cloneTemplate = new CloneTemplate(MyPrefabInfo, TechType.LaserCutter);
        cloneTemplate.ModifyPrefab += obj =>
        {
            LaserCutter laserCutter = obj.GetComponent<LaserCutter>();
            
            var drill = obj.AddComponent<HandDrill>().CopyComponent(laserCutter);
            
            //lc.renderers
            
            DestroyImmediate(laserCutter);
            
        };*/
        
        //prefab.
        prefab.SetGameObject(GetModel());

        prefab.SetUnlock(TechType.ExosuitDrillArmModule);
            
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);

        prefab.SetEquipment(EquipmentType.Hand);
        
        prefab.Register();
    }

    private static GameObject GetModel()
    {
        GameObject obj = AssetBundles.MyAssetBundle.LoadAsset<GameObject>("HandMiner");
        PrefabUtils.AddBasicComponents(obj, MyPrefabInfo.ClassID, MyPrefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
        MaterialUtils.ApplySNShaders(obj);
        
        //pickupable
        
        //obj.AddComponent<EnergyMixin>();
        

        //obj.transform.localScale *= 2.5f;
        //obj.transform.localScale *= 2.5f;

        

        obj.AddComponent<Pickupable>();
        
        obj.AddComponent<HandDrill>();
        //myCoolPrefab.AddComponent<>();
        //obj.AddComponent<HandDrill>();
        //drillarm
        //Drillable drillable = obj.GetComponent<Drillable>();
        
        //obj.AddComponent<Pickupable>
        
        
        return obj;
    }
}

internal class HandDrill : PlayerTool
{
    public override void OnToolUseAnim(GUIHand hand)
    {
        base.OnToolUseAnim(hand);
        
        Plugin.Logger.LogWarning("Use drill!!");
    }

    public override bool GetUsedToolThisFrame()
    {
        return base.GetUsedToolThisFrame();
    }

    public override string GetCustomUseText()
    {
        return "Use this drill";
    }

    

    public override void OnToolActionStart()
    {
        base.OnToolActionStart();

        Vector3 zero = Vector3.zero;
        GameObject obj = null;
        UWE.Utils.TraceFPSTargetPosition(Player.mainObject, 5f, ref obj, ref zero, true);

        if (obj)
        {
            Drillable drillable = obj.FindAncestor<Drillable>();
            
            if (!drillable)
            {
                obj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
                return;
            }
            
            //drillable.OnDrill(zero, null, out hitObj);
        }
    }
}