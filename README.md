# Omni Scepter

A quality-of-life / cheat mod for [tModLoader](https://github.com/tModLoader/tModLoader) (Terraria 1.4.4+). It adds two cheap craftable items that remove grind: the **Omni Scepter** for buffs, stats, reforging and endgame gear, and the **Omni Globe** for on-demand personal weather.

## Items

### Omni Scepter

Crafted from 10 Wood at a Work Bench. Uses the Rainbow Rod sprite until custom art is added.

**Left click:**

- Applies a curated list of 30 beneficial buffs for two in-game hours each (movement-altering buffs like Gravitation and Featherfall are deliberately excluded).
- Maxes out life and mana (500 HP / 200 MP), as if you had consumed every Life Crystal, Life Fruit and Mana Crystal, and refills both.
- Reforges everything you carry to its best prefix:
  - Ranged weapons → Unreal
  - Magic weapons → Mythical
  - Summon staffs → Ruthless (minions can't crit, so raw damage wins)
  - Whips and melee weapons → Legendary
  - Accessories → Menacing
  - Items that can't take the preferred prefix fall back through Godly / Demonic, and favorited status is preserved.

**Right click:**

- Grants a full endgame kit covering all four damage classes: Zenith, S.D.M.G. (with Luminite Bullets), Last Prism, Terraprisma, Kaleidoscope, Solar Flare and Stardust armor sets, Celestial Starboard, Ankh Shield, Terraspark Boots, Celestial Shell, Master Ninja Gear and Destroyer Emblem.
- Also hands out summoning items for every boss that has one, from King Slime through the Celestial Sigil.
- Skips anything you already own or have equipped, so it never floods your inventory with duplicates.

### Omni Globe

Crafted from 10 Wood at a Work Bench. Uses the Snow Globe sprite until custom art is added.

- **Left click** shakes the globe and rolls a random personal climate around you: Snow, Sandstorm, Blood Moon, Glowing Mushroom or Hallow. Each shake is guaranteed to change the weather.
- **Right click** calms everything back to normal.

## Project layout

```
OmniScepter/
├── Items/          OmniScepterItem, OmniGlobeItem
├── Buffs/          The five personal climate buffs
├── Common/         ModPlayer / GlobalNPC glue for the climates
├── Localization/   English and Simplified Chinese strings
└── build.txt       Mod metadata
```

## Building

Place the `OmniScepter` folder in your tModLoader `ModSources` directory and build it from the in-game Workshop → Develop Mods menu, or compile with the tModLoader `.targets` via the included `.csproj`.

## Localization

Ships with English (`en-US`) and Simplified Chinese (`zh-Hans`).
