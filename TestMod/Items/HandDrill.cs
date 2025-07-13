using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;

namespace TestMod.Items;

internal class HandDrill : LaserCutter
{
    public static PrefabInfo MyPrefabInfo { get; private set; }

    public static void Patch()
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

        
        var yeetKnifeObj = new CloneTemplate(MyPrefabInfo, TechType.LaserCutter);
        yeetKnifeObj.ModifyPrefab += obj =>
        {
            LaserCutter lc = obj.GetComponent<LaserCutter>();
            
            var drill = obj.AddComponent<HandDrill>().CopyComponent(lc);
            
            //lc.renderers
            
            DestroyImmediate(lc);
            
        };
        

        prefab.SetGameObject(GetModel());

        prefab.SetUnlock(TechType.ExosuitDrillArmModule);
            
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);

        prefab.SetEquipment(EquipmentType.Hand);
        
        prefab.Register();
    }

    private static GameObject GetModel()
    {
        GameObject myCoolPrefab = AssetBundles.MyAssetBundle.LoadAsset<GameObject>("HandMiner");
        PrefabUtils.AddBasicComponents(myCoolPrefab, MyPrefabInfo.ClassID, MyPrefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
        MaterialUtils.ApplySNShaders(myCoolPrefab);
        //Pickupable pickup = myCoolPrefab.AddComponent<Pickupable>();
        
        
        return myCoolPrefab;
    }
}