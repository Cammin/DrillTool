using System.Collections;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace DrillTool;

public static class DrillToolAuthoring
{
    public static PrefabInfo Info { get; private set; } = PrefabInfo
        .WithTechType("DrillTool", "Drill tool", "Enables agile mining of resources.")
        .WithIcon(SpriteManager.Get(TechType.Transfuser))
        .WithSizeInInventory(new Vector2int(2,2));

    public static AssetBundle DrillArmAnimationsBundle; 
    
    public static void Register()
    {
        DrillArmAnimationsBundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly.GetExecutingAssembly(), "drillarmanimations");
        
        CustomPrefab prefab = new(Info);

        SetupRecipe(prefab);
        
        prefab.SetUnlock(TechType.ExosuitDrillArmModule, 2);
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools);
        prefab.SetEquipment(EquipmentType.Hand);
        
        SetupObj(prefab);
        prefab.Register();
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        //use the template of a tool to reuse the battery stuff and rigidbody stuff
        CloneTemplate cloneTemplate = new(Info, TechType.Terraformer)
        {
            ModifyPrefabAsync = DoModifyPrefabAsync
        };

        prefab.SetGameObject(cloneTemplate);
        return;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            //fields
            PlayerTool oldTool = obj.GetComponent<PlayerTool>();
            DrillTool drillTool = obj.AddComponent<DrillTool>();
            drillTool.CopyComponent(oldTool);
            Object.DestroyImmediate(oldTool);
            
            //turn off the front of the terraformer
            obj.transform.Find("terraformer_anim/Terraformer_Export_Geo/Terraformer_body/Terraformer_front").gameObject.SetActive(false);
            
            //load the drill arm and orient it in a specific location
            IPrefabRequest handle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Tools/Exosuit.prefab");
            yield return handle;
            
            if (handle.TryGetPrefab(out var exosuitPrefabObj))
            {
                Exosuit exosuitPrefab = exosuitPrefabObj.GetComponent<Exosuit>();
                GameObject drillPrefab = exosuitPrefab.GetArmPrefab(TechType.ExosuitDrillArmModule);
                
                GameObject drillObj = Object.Instantiate(drillPrefab, obj.transform, true);
                Transform drillObjTransform = drillObj.transform;
                
                drillObjTransform.localPosition = new Vector3(0f, -0.066f, 0.273f);
                drillObjTransform.localRotation = Quaternion.Euler(0,0,60);
                drillObjTransform.localScale = Vector3.one * 0.35f;
                
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localPosition = Vector3.zero;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localRotation = Quaternion.Euler(0, 90, 0);
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localScale = Vector3.one * 0.1f;
                
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder").localPosition = Vector3.zero;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder").localRotation = Quaternion.identity;
                
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot").localPosition = Vector3.zero;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot").localRotation = Quaternion.identity;
                
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow").localPosition = Vector3.zero;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow").localRotation = Quaternion.identity;
                
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localPosition = Vector3.zero;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localRotation = Quaternion.identity;
                drillObjTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localScale = Vector3.one * 10f;

                ExosuitDrillArm drillArm = drillObj.GetComponent<ExosuitDrillArm>();
                drillTool.Loop = drillArm.loop;
                drillTool.LoopHit = drillArm.loopHit;
                drillTool.DrillAnimator = drillArm.animator;
                
                //change the animator to a custom one where there is no arm swinging movements
                var armAnimsHandle = DrillArmAnimationsBundle.LoadAssetAsync<RuntimeAnimatorController>("drill_OnlyFingers");
                yield return armAnimsHandle;
                drillTool.DrillAnimator.runtimeAnimatorController = armAnimsHandle.asset as RuntimeAnimatorController;
                drillTool.DrillAnimator.avatar = null;
                
                //destroy scripts on the spawned object. we only want it for it's model
                Object.DestroyImmediate(drillArm);
                Object.DestroyImmediate(drillObj.GetComponent<SkyApplier>());
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the exosuit");
            }
        }
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

    //temporary measure while i figure out a custom model. but for now, overlap the terraformer model and the prawn drill arm model
    /*private static GameObject GetFrankensteinModel()
    {
        bool loadedDrillArm = false;
        PrefabDatabase.GetPrefabAsync("exosuitdrillarm");

        
        
    }*/
}