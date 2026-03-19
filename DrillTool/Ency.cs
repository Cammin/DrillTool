using Nautilus.Handlers;

namespace DrillTool;

public static class Ency
{
    public const string EncyKeyItem = "Ency_DrillTool";
    
    public static void Register()
    {
        //Sprite popupIcon = ImageUtils.LoadSpriteFromFile("Assets/Fragments/Exosuit/DrillArm.png");

        //PDAEncyclopedia.mapping["ExosuitDrillArm"].popup;
        
        
        PDAHandler.AddEncyclopediaEntry(EncyKeyItem, "Tech/Equipment", null, null);
    }
}