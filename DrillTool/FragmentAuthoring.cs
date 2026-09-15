using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace DrillTool;

public static class FragmentAuthoring
{
    public static PrefabInfo Info { get; private set; }
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("DrillToolFragment", null, null);
        SetupWorldEntityInfo();
        
        CustomPrefab prefab = new(Info);

        SetupScanningGadget(prefab);
        SetupObj(prefab);
        
        prefab.Register();
    }

    private static void SetupWorldEntityInfo()
    {
        //PrefabInfo.SetSpawns naturally gives the prefab WorldEntityInfo, but since this is spawning in a crate instead, we have to AddCustomInfo
        WorldEntityDatabaseHandler.AddCustomInfo(Info.ClassID, new WorldEntityInfo
        {
            classId = Info.ClassID,
            techType = Info.TechType,
            slotType = EntitySlot.Type.Small,
            prefabZUp = false,
            cellLevel = LargeWorldEntity.CellLevel.Near,
            localScale = Vector3.one,
        });
    }

    private static void SetupScanningGadget(CustomPrefab prefab)
    {
        TechType drillTech = DrillToolAuthoring.Info.TechType;
        prefab.CreateFragment(drillTech, 4, ConfigFileLoader.NormalScans(), drillTech.ToString());
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        CloneTemplate fragmentTemplate = new(Info, TechType.PropulsionCannonFragment)
        {
            ModifyPrefabAsync = DoModifyPrefabAsync
        };

        prefab.SetGameObject(fragmentTemplate);
        return;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            Object.Destroy(obj.transform.Find("model").gameObject);
            Object.Destroy(obj.transform.Find("collision").gameObject);
            
            //loading this asset only because of laziness to get a hitbox
            IPrefabRequest fragmentHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Tools/Terraformer_damaged.prefab");
            yield return fragmentHandle;
            if (fragmentHandle.TryGetPrefab(out var damagedPrefab))
            {
                GameObject damagedObj = Object.Instantiate(damagedPrefab);

                //Cube is the hitbox
                Transform cube = damagedObj.transform.Find("Cube");
                cube.SetParent(obj.transform);
                cube.transform.localPosition = new Vector3(0, 0.2116f, -0.34f);
                Object.DestroyImmediate(damagedObj);
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the broken terraformer model");
            }
            
            IPrefabRequest drillToolHandle = PrefabDatabase.GetPrefabAsync(DrillToolAuthoring.Info.ClassID);
            yield return drillToolHandle;
            
            //copy the drilltool mesh and make it ours
            //terraformer_anim
            if (drillToolHandle.TryGetPrefab(out var drillToolObj))
            {
                var drillToolModel = drillToolObj.transform.Find("terraformer_anim").gameObject;
                
                GameObject drillToolModelObj = Object.Instantiate(drillToolModel);
                Transform drillToolModelTransform = drillToolModelObj.transform;
                
                drillToolModelTransform.SetParent(obj.transform);
                drillToolModelTransform.localPosition = new Vector3(0, 0.15f, -0.35f);
                drillToolModelTransform.localEulerAngles = new Vector3(-3, 0, 0);

                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                obj.GetComponent<SkyApplier>().renderers = renderers;

                EnergyEffect originalEnergy = drillToolObj.GetComponent<EnergyEffect>();

                EnergyEffect energyEffect = obj.AddComponent<EnergyEffect>();
                originalEnergy.CopyFields(energyEffect);
                energyEffect.modelsWithEmissive = new[] { drillToolModelObj };

                DrillToolFragment fragment = obj.AddComponent<DrillToolFragment>();
                fragment.energyEffect = energyEffect;
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the drilltool model");
            }
        }
    }
}