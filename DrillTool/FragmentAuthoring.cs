using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;
using UWE;

namespace DrillTool;

public static class FragmentAuthoring
{
    public static PrefabInfo Info { get; private set; }
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("DrillToolFragment", null, null);
        
        CustomPrefab prefab = new(Info);

        SetupScanningGadget(prefab);
        SetupObj(prefab);
        
        prefab.Register();
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
                
                //pass over skyapplier renderers
                SkyApplier mainSky = obj.GetComponent<SkyApplier>();
                SkyApplier damagedSky = drillToolModelObj.GetComponent<SkyApplier>();
                mainSky.renderers = damagedSky.renderers;
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the drilltool model");
            }
        }
    }
}