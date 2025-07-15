using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;

namespace TestMod;

internal static class HandDrillAuthoring
{
    public static PrefabInfo Info { get; private set; } = PrefabInfo
        .WithTechType("DrillTool", "Drill tool", "Enables agile mining of resources.")
        .WithIcon(SpriteManager.Get(TechType.StasisRifle));
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);

        SetupRecipe(prefab);
        SetupObj(prefab);

        PDAScanner.EntryData entry = PDAScanner.GetEntryData(TechType.ExosuitDrillArmModule);
        prefab.SetUnlock(TechType.ExosuitDrillArmModule, entry.totalFragments);
        
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
            DrillTool drillTool = obj.AddComponent<DrillTool>();
            drillTool.CopyComponent(welder);
            Object.DestroyImmediate(welder);
            
            
            //setup renderer
            //Object.Destroy(oldRender.gameObject);
            Transform thing1 = obj.transform.GetChild(0);
            Transform thing2 = obj.transform.GetChild(1);
            Transform welderModel = obj.transform.GetChild(2);

            GameObject drillModel = GetModel();
            drillModel.transform.SetParent(obj.transform);
            drillModel.transform.localPosition = welderModel.localPosition;
            
            //set the new renderers
            drillTool.renderers = drillModel.GetComponentsInChildren<Renderer>(true);
            
            thing1.gameObject.SetActive(false);
            thing2.gameObject.SetActive(false);
            welderModel.gameObject.SetActive(false);
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
        GameObject obj = AssetBundles.DrillToolBundle.LoadAsset<GameObject>("HandMiner");
        //PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj);
        return obj;
    }
}