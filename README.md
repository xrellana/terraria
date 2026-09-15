# Omni Tools

A quality-of-life / cheat mod for [tModLoader](https://github.com/tModLoader/tModLoader) (Terraria 1.4.4+). It adds two cheap craftable items that remove grind: the **Omni Scepter** for buffs, stats, reforging and endgame gear, and the **Omni Globe** for on-demand personal weather.

## Items

### Omni Scepter

Crafted from 10 Wood at a Work Bench. Uses the Rainbow Rod sprite until custom art is added.

**Left click:**

- Applies a curated list of 30 beneficial buffs for two hours of real time each (movement-altering buffs like Gravitation and Featherfall are deliberately excluded).
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
OmniTools/
├── Common/         OmniItemBase, the plumbing both items share
├── Scepter/        OmniScepterItem
├── Globe/          OmniGlobeItem, the five climate buffs, and the
│                   ModPlayer / GlobalNPC hooks the climates need
├── Localization/   English and Simplified Chinese strings
└── build.txt       Mod metadata
```

## Building

Place the `OmniTools` folder in your tModLoader `ModSources` directory and build it from the in-game Workshop → Develop Mods menu, or compile with the tModLoader `.targets` via the included `.csproj`. The folder name is the mod's internal name and has to stay `OmniTools` to match `<AssemblyName>`.

## CI and releases

`.github/workflows/build.yml` runs when `OmniTools/**` or the workflow itself
changes on `main` — a direct push, or a merged pull request, which lands on
`main` as a push either way. Feature branches and README-only commits do not
trigger it; `workflow_dispatch` builds any branch on demand.

CI does not need Terraria, Steam or any game assets. It downloads
`tModLoader.zip` from the loader's latest GitHub release, writes the
`tModLoader.targets` shim that `OmniTools.csproj` imports, and runs a plain
`dotnet build` — `tMLMod.targets` inside that zip does the `.tmod` packing.
Every run uploads the `.tmod` as an artifact.

**`version` in `OmniTools/build.txt` is what publishes a release.** Each run
reads it and checks whether `v<version>` already exists:

- Not released yet → the run tags the commit and publishes a GitHub Release
  with the `.tmod` attached and generated release notes.
- Already released → the run builds and uploads the artifact, nothing else.

So bumping `version` in the commit or PR you merge is the release switch. There
is no separate tagging step, and the tag can never disagree with the version
inside the `.tmod` — which matters because tModLoader reads its update version
from `build.txt`, so a mismatch would misreport updates to players.

Players install a release by dropping the `.tmod` into their tModLoader `Mods`
folder.

## Localization

Ships with English (`en-US`) and Simplified Chinese (`zh-Hans`).
