using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace DrillTool;

public static class FragmentCrateAuthoring
{
    public static PrefabInfo Info { get; private set; }
    
    public static void Register()
    {
        Info = PrefabInfo.WithTechType("DrillToolFragmentCrate");
        
        CustomPrefab prefab = new(Info);
        
        SetupObj(prefab);
        SetupLootSpawns(prefab);
        
        prefab.Register();
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        prefab.SetGameObject(MakePrefab);
        IEnumerator MakePrefab(IOut<GameObject> objOut)
        {
            GameObject obj = new GameObject("DrillToolFragment_InCrate");
            
            IPrefabRequest crateHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo_damaged_opened_01.prefab");
            IPrefabRequest fragmentHandle = PrefabDatabase.GetPrefabAsync(FragmentAuthoring.Info.ClassID);
            
            yield return crateHandle;
            if (!crateHandle.TryGetPrefab(out var cratePrefab))
            {
                Plugin.Logger.LogError($"Failed loading the crate prefab");
                yield break;
            }
            
            GameObject crateObj = Object.Instantiate(cratePrefab, obj.transform);
            crateObj.transform.localPosition = Vector3.zero;
            crateObj.transform.localRotation = Quaternion.identity;
            crateObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            
            yield return fragmentHandle;
            if (!fragmentHandle.TryGetPrefab(out var fragmentPrefab))
            {
                Plugin.Logger.LogError($"Failed loading the fragment prefab");
                yield break;
            }
            
            GameObject fragmentObj = Object.Instantiate(fragmentPrefab, obj.transform);
            fragmentObj.transform.localPosition = new Vector3(0.043f, 0.0769f, -0.04f);
            fragmentObj.transform.localEulerAngles = new Vector3(0f, -30f, 0f);
            fragmentObj.transform.localScale = Vector3.one;
            
            PrefabUtils.AddBasicComponents(crateObj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
            objOut.Set(obj);
        }
    }
    
    private static void SetupLootSpawns(CustomPrefab prefab)
    {
        prefab.SetSpawns(new[]
        {
            Fragment(BiomeType.KooshZone_TechSite, 0.17f),
            Fragment(BiomeType.KooshZone_TechSite_Barrier, 0.3f),
            
            Fragment(BiomeType.MushroomForest_TechSite, 0.2f),
            Fragment(BiomeType.MushroomForest_TechSite_Barrier, 0.3f),
            
            Fragment(BiomeType.SparseReef_Techsite, 0.17f),
            Fragment(BiomeType.SparseReef_Techsite_Barrier, 0.3f),
            
            Fragment(BiomeType.CrashZone_Sand, 0.08f),
            Fragment(BiomeType.CrashZone_TrenchSand, 0.06f),
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