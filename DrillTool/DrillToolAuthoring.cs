using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;

namespace DrillTool;

public static class DrillToolAuthoring
{
    public static PrefabInfo Info { get; private set; } = PrefabInfo
        .WithTechType("DrillTool", "Drill tool", "Enables agile mining of resources.")
        .WithIcon(SpriteManager.Get(TechType.Transfuser))
        .WithSizeInInventory(new Vector2int(2,2));
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);

        SetupRecipe(prefab);
        SetupObj(prefab);

        //PDAScanner.EntryData entry = PDAScanner.GetEntryData(TechType.ExosuitDrillArmFragment);
        
        //ErrorMessage.AddMessage(entry != null);
        
        prefab.SetUnlock(TechType.ExosuitDrillArmModule, 2);
        
        
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.Register();
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        //use the template of a tool to reuse the battery stuff and rigidbody stuff
        var cloneTemplate = new CloneTemplate(Info, TechType.Terraformer);

        cloneTemplate.ModifyPrefab += obj =>
        {
            //fields
            PlayerTool oldTool = obj.GetComponent<PlayerTool>();
            DrillTool drillTool = obj.AddComponent<DrillTool>();
            drillTool.CopyComponent(oldTool);
            Object.DestroyImmediate(oldTool);
            
            
            //setup renderer
            //Object.Destroy(oldRender.gameObject);
            /*Transform thing1 = obj.transform.GetChild(0);
            Transform thing2 = obj.transform.GetChild(1);
            Transform oldToolModel = obj.transform.GetChild(2);

            GameObject drillModel = GetModel();
            drillModel.transform.SetParent(obj.transform);
            drillModel.transform.localPosition = oldToolModel.localPosition;
            
            //set the new renderers
            drillTool.renderers = drillModel.GetComponentsInChildren<Renderer>(true);
            
            thing1.gameObject.SetActive(false);
            thing2.gameObject.SetActive(false);
            oldToolModel.gameObject.SetActive(false);*/
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
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Diamond, 4),
                new Ingredient(TechType.Titanium, 1),
            },
        };
        prefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Tools");
    }

    private static GameObject LoadedBundleObj;
    private static GameObject GetModel()
    {
        if (LoadedBundleObj)
        {
            return Object.Instantiate(LoadedBundleObj);
        }
        
        //LoadedBundleObj = Plugin.DrillToolBundle.LoadAsset<GameObject>("HandMiner");
        if (LoadedBundleObj == null)
        {
            //Debug.LogError("Failed to load HandMiner from asset bundle?");
            return null;
        }
        Object.DontDestroyOnLoad(LoadedBundleObj);
        
        //PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(LoadedBundleObj);

        return Object.Instantiate(LoadedBundleObj);
    }
}