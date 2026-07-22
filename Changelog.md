# 1.2.0
- Added support for Deathrun Remade: Harder recipes/fragments for the Drill Tool!
  - Drill Tool crafting recipes for the four difficulties levels
  - Drill Tool fragment scan amounts for the four difficulty levels
  - Recipes/Scans are configurable in the mod's Config folder
- Added a configurable slider in the mod menu to adjust damage dealt to creatures
- Tweaked the standard recipe (Less diamond and less titanium needed)
- The fragment's model now looks like the drill instead of the terraformer
- Fixed important bug where many fragment crates would not spawn, and instead appear at the world origin (actually fixed now)
- Fixed SpriteAtlas-related warning reported in the console
- Updated the localization files

# 1.1.1
- Fixed bug where many fragment crates would spawn at the world origin
- Slightly reduced the chance of spawning in the trench regions of the crash zone

# 1.1.0
- Adjusted fragment spawns
  - Added a new spawn chance in the Crash Zone sand
  - Increased chance at wrecks, but they will now only spawn inside wrecks
  - NOTE: If you have already entered the proximity of a spawn location, then that spot will not retroactively spawn new fragments in your save file
- Tool now has a short startup delay before it can begin drilling (0.45s)
- Tool now drains battery when damaging a creature, outcrop or others
- Tweaked crafting recipe (Wiring Kit instead of Advanced Wiring Kit)
- Tweaked configurable values
  - Reduced battery drain (0.09 to 0.07)
  - Auto Collect is now enabled by default
  - NOTE: You will need to manually adjust to the new defaults

# 1.0.1
- Fixed a battery that would unexpectedly render above the tool
- Updated the localization files

# 1.0.0
- Added support for all 39 possible languages
- Added scannable fragments to unlock the drill tool instead of via the exosuit drill arm. Fragments are found at wrecks in Mushroom Forest, Sparse Reef, and Bulb Zone
- Added PDA databank entry for the drill tool, revealed upon the first fragment scan. Complete with an image and in-universe description.
- Added item icon for the drill tool
- Added a configurable toggle to auto-collect the mined resources like the exosuit does (off by default)