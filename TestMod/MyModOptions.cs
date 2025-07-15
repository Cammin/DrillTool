using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;

namespace TestMod;

/*
public class MyModOptions : ModOptions
{
    /// The base ModOptions class takes a string name as an argument
    public MyModOptions() : base("Drill Tool")
    {
        ModSliderOption energyCostSlider = ModSliderOption.Create("DrillToolEnergyCost", "Drill Energy Cost", 0, 100, 10,
            tooltip: "Battery drain per resource deposit hit on the drill tool");
        
        //energyCostSlider.
        AddItem();
        
        
    }
}
*/

[Menu("Drill Tool")]
public class MyConfig : ConfigFile
{
    [Slider("My slider", 0, 100)]
    public static int SliderValue = 10;
}