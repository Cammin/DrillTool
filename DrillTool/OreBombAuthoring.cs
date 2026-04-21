using System.Collections;
using System.Collections.Generic;
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

public class OreBombAuthoring
{
    
    
    
    public static PrefabInfo Info { get; private set; }

    public static void Register()
    {
        //Register PrefabInfo here so it executes based on the current language
        Info = PrefabInfo.WithTechType("OreBomb", "Ore Bomb", "Explodes resources", unlockAtStart: true)
            .WithIcon(SpriteManager.Get(TechType.MercuryOre));
        
        CustomPrefab prefab = new(Info);

        SetupRecipe(prefab);

        
        
        PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/CrashSite/xCrash_Xplo");
        /*prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Tools)
            .WithAnalysisTech(popupSprite, null, null)
            .WithEncyclopediaEntry("Tech/Equipment", popupSprite, encyImage, null, null);*/
        
        prefab.SetEquipment(EquipmentType.Hand);
        
        SetupObj(prefab);
        prefab.Register();
    }

    private static void SetupObj(CustomPrefab prefab)
    {
        CloneTemplate cloneTemplate = new(Info, TechType.Flare)
        {
            ModifyPrefabAsync = DoModifyPrefabAsync
        };

        prefab.SetGameObject(cloneTemplate);
        return;

        IEnumerator DoModifyPrefabAsync(GameObject obj)
        {
            //fields
            PlayerTool flare = obj.GetComponent<Flare>();
            OreBomb bomb = obj.AddComponent<OreBomb>();
            bomb.CopyComponent(flare);
            Object.DestroyImmediate(flare);
            
            //load the explosion
            //todo could use the xSeaDragon_MeteorImpact or xLavaLizard_MeteorImpact
            IPrefabRequest crashHomeHandle = PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/CrashHome.prefab");
            yield return crashHomeHandle;
            if (!crashHomeHandle.TryGetPrefab(out GameObject crashHomePrefab))
            {
                Plugin.Logger.LogError($"Failed loading the crash home");
                yield break;
            }
            GameObject crashPrefab = crashHomePrefab.GetComponent<CrashHome>().crashPrefab;
            GameObject explosionPrefab = crashPrefab.GetComponent<Crash>().detonateParticlePrefab;
            bomb.detonateParticlePrefab = explosionPrefab;
            
            //fabrication
            VFXFabricating fabricating = obj.GetComponentInChildren<VFXFabricating>();
            fabricating.posOffset = new Vector3(-0.4f, 0.17f, 0f);
            fabricating.eulerOffset = new Vector3(0, 90f, 0);
            fabricating.localMaxY = 0.15f;
            fabricating.localMinY = -0.2f;

            yield break;
        }
    }

    private static void SetupRecipe(CustomPrefab prefab)
    {
        RecipeData recipe = new RecipeData
        {
            craftAmount = 2,
            Ingredients =
            {
                new Ingredient(TechType.Diamond, 1),
                new Ingredient(TechType.CrashPowder, 1),
            },
        };
        prefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Tools")
            .WithCraftingTime(2);
    }
}