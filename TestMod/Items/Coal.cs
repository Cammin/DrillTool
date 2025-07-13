using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;

namespace TestMod.Items;

public class Coal
{
    // To access the TechType anywhere in the project
    public static PrefabInfo Info { get; private set; }

    public static void Register()
    {
        Info = PrefabInfo
            .WithTechType("Coal", "Coal", "Coal that makes me go yes.")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(SpriteManager.Get(TechType.Nickel));
        
        var coalPrefab = new CustomPrefab(Info);

        var coalObj = new CloneTemplate(Info, TechType.Nickel);
        coalPrefab.SetGameObject(coalObj);
        
        // register the coal to the game
        coalPrefab.Register();
    }
}