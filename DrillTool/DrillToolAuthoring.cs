using System.Collections;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using UnityEngine;
using UnityEngine.U2D;
using UWE;

namespace DrillTool;

public static class DrillToolAuthoring
{
    public static PrefabInfo Info { get; private set; }

    public static AssetBundle DrillArmAnimationsBundle; 
    public static AssetBundle UiBundle; 
    
    public static void Register()
    {
        DrillArmAnimationsBundle = Plugin.LoadBundle("drillarmanimations");
        UiBundle = Plugin.LoadBundle("ui");

        SpriteAtlas atlas = UiBundle.LoadAsset<SpriteAtlas>("DrillTool_UI");
        Texture2D encyImage = UiBundle.LoadAsset<Texture2D>("DrillTool_DataBankHeader");
        Sprite popupSprite = atlas.GetSprite("DrillTool_DataBankPopup");
        Sprite iconSprite = atlas.GetSprite("DrillTool_Icon");
        
        if (encyImage == null) Plugin.Logger.LogError($"Failed loading the encyImage");
        if (popupSprite == null) Plugin.Logger.LogError($"Failed loading the popupSprite");
        if (iconSprite == null) Plugin.Logger.LogError($"Failed loading the iconSprite");

        //Register PrefabInfo here so it executes based on the current language
        Info = PrefabInfo
            .WithTechType("DrillTool")
            .WithIcon(iconSprite)
            .WithSizeInInventory(new Vector2int(2, 2));
        
        CustomPrefab prefab = new(Info);

        SetupRecipe(prefab);
        
        prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools)
            .WithAnalysisTech(popupSprite, null, null)
            .WithEncyclopediaEntry("Tech/Equipment", popupSprite, encyImage, null, null);
        
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
            PlayerTool terraformerScript = obj.GetComponent<PlayerTool>();
            DrillTool drillTool = obj.AddComponent<DrillTool>();
            drillTool.CopyComponent(terraformerScript);
            Object.DestroyImmediate(terraformerScript);
            
            //turn off the front of the terraformer
            obj.transform.Find("terraformer_anim/Terraformer_Export_Geo/Terraformer_body/Terraformer_front").gameObject.SetActive(false);

            //fabrication
            VFXFabricating fabricating = obj.GetComponentInChildren<VFXFabricating>();
            fabricating.posOffset = new Vector3(-0.4f, 0.17f, 0f);
            fabricating.eulerOffset = new Vector3(0, 90f, 0);
            fabricating.localMaxY = 0.15f;
            fabricating.localMinY = -0.2f;

            //load the drill arm and orient it in a specific location
            IPrefabRequest exosuitHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Tools/Exosuit.prefab");
            
            yield return exosuitHandle;
            
            if (exosuitHandle.TryGetPrefab(out var exosuitPrefabObj))
            {
                Exosuit exosuitPrefab = exosuitPrefabObj.GetComponent<Exosuit>();
                GameObject drillPrefab = exosuitPrefab.GetArmPrefab(TechType.ExosuitDrillArmModule);
                
                //the parent has to be under the terraformer model because the VFXCrafting component is there
                GameObject drillArmObj = Object.Instantiate(drillPrefab, obj.transform.Find("terraformer_anim"), true);
                Transform drillTransform = drillArmObj.transform;
                
                //destroy unneeded objs
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/exosuit_arm_torpedoLauncher_geo").gameObject);
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/exosuit_grapplingHook_geo").gameObject);
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/exosuit_grapplingHook_hand_geo").gameObject);
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/exosuit_hand_geo").gameObject);
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/exosuit_propulsion_geo").gameObject);
                Object.DestroyImmediate(drillTransform.Find("exosuit_01_armRight/ArmRig/grapplingHook").gameObject);
                
                //setup all transformations so that we end up with a simple drill bit
                drillTransform.localPosition = new Vector3(0f, -0.066f, 0.125f);
                drillTransform.localRotation = Quaternion.Euler(0,0,60);
                drillTransform.localScale = Vector3.one * 0.5f;
                
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localPosition = Vector3.zero;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localRotation = Quaternion.Euler(0, 90, 0);
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle").localScale = Vector3.one * 0.1f;
                
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder").localPosition = Vector3.zero;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder").localRotation = Quaternion.identity;
                
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot").localPosition = Vector3.zero;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot").localRotation = Quaternion.identity;
                
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow").localPosition = Vector3.zero;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow").localRotation = Quaternion.identity;
                
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localPosition = Vector3.zero;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localRotation = Quaternion.identity;
                drillTransform.Find("exosuit_01_armRight/ArmRig/clavicle/shoulder/bicepPivot/elbow/drill").localScale = Vector3.one * 10f;
                
                ExosuitDrillArm drillArmScript = drillArmObj.GetComponent<ExosuitDrillArm>();
                drillTool.hasBashAnimation = true;
                drillTool.DrillAnimator = drillArmScript.animator;
                drillTool.fxSpawnPoint = drillArmScript.fxSpawnPoint;
                drillTool.fxControl = drillArmScript.fxControl;
                drillTool.Loop = drillArmScript.loop;
                drillTool.LoopHit = drillArmScript.loopHit;
                Object.DestroyImmediate(drillArmScript);
                
                //change the animator to a custom one where there is no arm swinging movements
                var armAnimsHandle = DrillArmAnimationsBundle.LoadAssetAsync<RuntimeAnimatorController>("drill_OnlyFingers");
                yield return armAnimsHandle;
                drillTool.DrillAnimator.runtimeAnimatorController = armAnimsHandle.asset as RuntimeAnimatorController;
                drillTool.DrillAnimator.avatar = null;
                
                //pass over the skyapplier renderers from the drill to the terraformer's and discard the skyapplier on the drill arm
                SkyApplier mainSky = obj.GetComponent<SkyApplier>();
                SkyApplier drillArmSky = drillArmObj.GetComponent<SkyApplier>();
                mainSky.renderers = mainSky.renderers.Concat(drillArmSky.renderers.Where(p => p)).ToArray();
                Object.DestroyImmediate(drillArmSky);
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
                new Ingredient(TechType.Titanium, 5),
                new Ingredient(TechType.Battery, 1),
            },
        };
        prefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Tools")
            .WithCraftingTime(5);
    }
}