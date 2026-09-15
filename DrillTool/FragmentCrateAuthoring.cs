using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;

namespace DrillTool;

public static class FragmentCrateAuthoring
{
    public static PrefabInfo Info { get; private set; }

    //WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo_damaged_opened_01.prefab
    private const string PrefabClassIdCrate = "8c3d54c0-4330-4949-91ad-f046cfd67c7c";
    private static string PrefabClassIdFragment => FragmentAuthoring.Info.ClassID;
    
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
        GameObject MakePrefab()
        {
            GameObject obj = new GameObject("DrillToolFragment_InCrate");

            GameObject placeholderCrateObj = new GameObject("Starship_cargo_damaged_opened_01(Placeholder)");
            placeholderCrateObj.transform.SetParent(obj.transform);
            placeholderCrateObj.transform.localPosition = Vector3.zero;
            placeholderCrateObj.transform.localRotation = Quaternion.identity;
            placeholderCrateObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            PrefabPlaceholder placeholderCrate = placeholderCrateObj.AddComponent<PrefabPlaceholder>();
            placeholderCrate.prefabClassId = PrefabClassIdCrate;
            
            GameObject placeholderToolObj = new GameObject("DrillToolFragment(Placeholder)");
            placeholderToolObj.transform.SetParent(obj.transform);
            placeholderToolObj.transform.localPosition = new Vector3(0.1f, 0.0769f, -0.04f);
            placeholderToolObj.transform.localEulerAngles = new Vector3(0f, 325f, 0f);
            placeholderToolObj.transform.localScale = Vector3.one;
            PrefabPlaceholder placeholderTool = placeholderToolObj.AddComponent<PrefabPlaceholder>();
            placeholderTool.prefabClassId = PrefabClassIdFragment;
            
            PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
            obj.AddComponent<PrefabPlaceholdersGroup>().prefabPlaceholders = new [] { placeholderCrate, placeholderTool };
            return obj;
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