[Nexus Page](https://www.nexusmods.com/subnautica/mods/2541)
# Drill Tool
This Subnautica mod adds a new tool that can harvest large resource deposits.  

https://github.com/user-attachments/assets/36136930-0e46-415b-b322-4e0e720d6781  

[old1]: <> (https://github.com/user-attachments/assets/45a5aaf1-6897-4d81-9717-0794fdee641d)

### Details
- The tool can also drill & bash outcrops, and hit creatures.
- Blueprint fragments are found at: 
  - Crash Zone sand
  - Mushroom Forest wreck interior
  - Sparse Reef wreck interior
  - Bulb Zone wreck interior
- It is crafted with:
  - 3 Diamonds
  - 2 Titanium
  - 1 Wiring kit
  - 1 Battery

### Configurable Values
- Hit interval: By default, it mines drillables as quickly as the Prawn drill arm.
- Battery consumption rate: By default, one battery is enough for about three deposits.
- Creature Damage: Damage dealt to creatures, flora, and habitat.
- Auto Collect: Mined resources will automatically enter the player's inventory.

### Supports all 39 languages!
[Translation Sheet](https://docs.google.com/spreadsheets/d/1DaJR9qEDWyGWqvxv5k4h_7KObrdWGRYjMBmU2DjlGjw/edit?usp=sharing)  
Thanks for translation help! Credits are at the [Nexus Page](https://www.nexusmods.com/subnautica/mods/2541)  

### Deathrun Support
Supports [Deathrun Remade](https://www.nexusmods.com/subnautica/mods/1495)!
- The crafting recipe is adjusted depending on difficulty level (Configured in Deathrun's "Tool and Building Costs")
- The required fragment count is adjusted depending on difficulty level (Configured in Deathrun's "Required Fragment Scans")
- Both the recipes and fragment count for each difficulty level can be changed in the mod's config folder.
- The recipes are viewable in this file: [ConfigFileLoader.cs](DrillTool/Deathrun/ConfigFileLoader.cs)
- When Deathrun is installed, then a battery is not required to craft because a battery isn't included in a crafted tool.

### Useful Debug Commands
- New command `RestoreDrillable` reverts the nearest drillable to its full state.
- `item drilltool`
- `ency drilltool`
- `spawn drilltool`
- `spawn drilltoolfragment`
- `spawn drilltoolfragmentcrate`

# Install
- (Requires Nautilus)
- Go to [Releases](https://github.com/Cammin/DrillTool/releases)
- Download the mod
- Extract it to your Bepinex/plugins like most other Subnautica mods

### Ideas
- single-use item that attaches to a drillable and destroys it instantly. single use system like flares. recipe could be a crashfish powder and diamond for 2 copies, like how crafting flares gives multiple
- Upgrade DrillTool on the workbench to make it instantly destroy nodes instead
- Support for deathrun remade

### Todo
- n/a

## Stretch Goals
- IK for aiming the tool to the spot on the drillable that is currently being mined
- First-use animation (maybe like the stasis rifle)
- PDA voice describing it upon first craft
- More highly customized animations
- Make the tool use the power cell instead of the battery (would handle backwards compatibility)
- Custom Model
- Custom Fragment Model
- Custom SFX
- Custom VFX

## Premise
My goal was to make a tool that can mine large resources as an addition to the Prawn drill arm.  

I always felt like there ought to be an alternate way to mine from these because there are so many large resources scattered around the map, and restricting to only prawn suit usage feels like there's an opportunity to put something new in that place, especially because there's so much of those resources everywhere.  

Another goal was to make it feel faithful, as if the original developers had made it themselves.
