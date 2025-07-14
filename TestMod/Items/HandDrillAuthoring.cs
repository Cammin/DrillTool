using System.Collections;
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
    public static PrefabInfo Info { get; private set; } = PrefabInfo
        .WithTechType("HandDrill", "Drill tool", "Enables agile mining of resources.")
        .WithIcon(SpriteManager.Get(TechType.StasisRifle));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);

        SetupRecipe(prefab);
        SetupObj(prefab);

        prefab.SetUnlock(TechType.ExosuitDrillArmModule);
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.Register();
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        //use the template of a tool to reuse the battery stuff and rigidbody stuff
        var cloneTemplate = new CloneTemplate(Info, TechType.Welder);

        cloneTemplate.ModifyPrefab += obj =>
        {
            //fields
            PlayerTool welder = obj.GetComponent<PlayerTool>();
            HandDrill drill = obj.AddComponent<HandDrill>();
            drill.CopyComponent(welder);
            Object.DestroyImmediate(welder);
            
            
            //setup renderer
            //Object.Destroy(oldRender.gameObject);
            Transform welderModel = obj.transform.GetChild(2);

            GameObject model = GetModel();
            model.transform.SetParent(obj.transform);
            model.transform.localPosition = welderModel.localPosition;

            //welderModel.gameObject.SetActive(false);
            obj.transform.GetChild(2).gameObject.SetActive(false);
            obj.transform.GetChild(1).gameObject.SetActive(false);
            obj.transform.GetChild(0).gameObject.SetActive(false);
            //Object.Destroy();
            //Object.Destroy();
            //model
            
        };
        prefab.SetGameObject(cloneTemplate);
    }

    private static void SetupRecipe(CustomPrefab prefab)
    {
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
    }

    private static GameObject GetModel()
    {
        GameObject obj = AssetBundles.MyAssetBundle.LoadAsset<GameObject>("HandMiner");
        //PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj);
        
        //pickupable
        //obj.AddComponent<EnergyMixin>();
        

        //obj.transform.localScale *= 2.5f;
        //obj.transform.localScale *= 2.5f;

        

        //obj.AddComponent<Pickupable>();
        
        //obj.AddComponent<HandDrill>();
        
        //todo figure out battery thing later
        //EnergyMixin energy = obj.AddComponent<EnergyMixin>();
        //energy.

        //myCoolPrefab.AddComponent<>();
        //obj.AddComponent<HandDrill>();
        //drillarm
        //Drillable drillable = obj.GetComponent<Drillable>();
        
        //obj.AddComponent<Pickupable>
        
        
        return obj;
    }
}