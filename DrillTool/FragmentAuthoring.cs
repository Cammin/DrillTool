using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Extensions;
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
        
        CustomPrefab prefab = new(Info);

        SetupScanningGadget(prefab);
        SetupObj(prefab);
        SetupLootSpawns(prefab);
        
        prefab.Register();
    }

    private static void SetupScanningGadget(CustomPrefab prefab)
    {
        TechType drillTech = DrillToolAuthoring.Info.TechType;
        prefab.CreateFragment(drillTech, 4, 2, drillTech.ToString());
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

            IPrefabRequest fragmentHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Tools/Terraformer_damaged.prefab");
            yield return fragmentHandle;
            if (fragmentHandle.TryGetPrefab(out var damagedPrefab))
            {
                GameObject damagedObj = Object.Instantiate(damagedPrefab);

                Transform cube = damagedObj.transform.Find("Cube");
                cube.SetParent(obj.transform);
                cube.transform.localPosition = new Vector3(0, 0.2116f, -0.34f);

                Transform damagedModel = damagedObj.transform.Find("terraformer_damaged");
                damagedModel.SetParent(obj.transform);
                damagedModel.transform.localPosition = new Vector3(0, 0.121f, 0);
                damagedModel.transform.localEulerAngles = new Vector3(-10, 0, 0);
                
                //pass over skyapplier renderers
                SkyApplier mainSky = obj.GetComponent<SkyApplier>();
                SkyApplier damagedSky = damagedObj.GetComponent<SkyApplier>();
                mainSky.renderers = damagedSky.renderers;
                Object.DestroyImmediate(damagedObj);
            }
            else
            {
                Plugin.Logger.LogError($"Failed loading the broken terraformer model");
            }
        }
    }
    
    private static void SetupLootSpawns(CustomPrefab prefab)
    {
        prefab.SetSpawns(new[]
        {
            Fragment(BiomeType.KooshZone_TechSite_Scatter, 0.03f),
            Fragment(BiomeType.KooshZone_TechSite, 0.1f),
            Fragment(BiomeType.KooshZone_TechSite_Barrier, 0.2f),
            
            Fragment(BiomeType.Dunes_TechSite_Scatter, 0.03f),
            Fragment(BiomeType.Dunes_TechSite, 0.1f),
            Fragment(BiomeType.Dunes_TechSite_Barrier, 0.2f),
            
            Fragment(BiomeType.SeaTreaderPath_TechSite_Scatter, 0.03f),
            Fragment(BiomeType.SeaTreaderPath_TechSite, 0.1f),
            Fragment(BiomeType.SeaTreaderPath_TechSite_Barrier, 0.2f),
        });
    }

    private static LootDistributionData.BiomeData Fragment(BiomeType biome, float probability)
    {
        return new LootDistributionData.BiomeData()
        {
            biome = biome,
            count = 1,
            probability = probability
        };
    }
}