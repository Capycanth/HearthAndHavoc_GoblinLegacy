# Phase 1 Delivery Plan — A Living World Without Kremlits

## Goal

Run a simulation of a world containing only flora and fauna. Plants grow, get eaten, regrow and spread. Animals eat, drink, sleep, flee, hunt, breed and die. There are no Kremlits in this phase.

## How this plan is worked

- One milestone at a time, in order. The open questions under a milestone are settled before any code for it is written.
- Each milestone ships as one or more small PRs, following the rules in `CLAUDE.md`.
- Answers to the open questions are recorded in this file as they are decided, so the plan stays the source of truth.

## Starting point (repo as of `d88b407`)

- Only Kremlits can act. `Locale` holds only a Kremlit list, and `BaseAction` / `KremlitSnapshot` take a `Kremlit` directly.
- `BiotaDef` and `ItemDef` are empty classes. `DefRegistry` rejects unknown JSON fields, so every new def attribute must exist as a C# property.
- Biota defs are categories (`flora_trees`, `mammals_deer`), not species.
- There is no game clock. A tick is 200 ms of real time and maps to no game time.
- The test map is 1000×1000 grass with 10% random blockers and no water.
- Pathfinding: an unreachable destination makes A* search up to the whole map, and `GoTo` pops from an empty stack when no path is found.

## Test species (proposed)

- **Flora:** grass, berry bush, apple tree, mushroom patch.
- **Water:** ponds.
- **Fauna:** rabbit (fast breeder, flees), deer (large grazer), wolf (predator). Optionally one omnivore, such as a boar, to exercise the mixed-diet path.

---

## Milestone 1 — Sim clock

Map ticks to game minutes, hours and days, so every rate in the defs has a unit.

**Open questions**

1. What is one tick in game time? Suggestion: 1 tick = 1 game minute, so a game day takes about 4.8 real minutes at 1× speed.
2. Should the clock track day of year and seasons now, or only time of day and day count for Phase 1?
3. Where does the clock live: on `World`, on `GoblinGame`, or as its own class?
4. Should the existing 0.5×–10× speed control stay as it is, or also offer pause and fixed speeds?

**Decisions**

1. 1 tick = 1 game minute. Time is compressed: biology (metabolism, growth) uses game time, while movement speeds are tuned to look natural on screen rather than being realistic per minute.
2. The clock tracks seasons now, since flora and fauna life cycles will use them. 28 days per season, 4 seasons per year (112-day year). A new world starts at Spring, day 1, 06:00, year 1.
3. The clock is its own class, `SimClock`, owned by `World`. It stores one value, total ticks, and derives minute, hour, day, season and year from it. `World.Update()` advances it once per tick, before any locale updates. Real-time pacing (speed and the tick accumulator) stays in `GoblinGame`.
4. Speed uses fixed steps of 1×, 2× and 5×. Right Ctrl steps up and Left Ctrl steps down. P toggles pause. 5× is the top speed for now; faster speeds (several ticks per frame) are left for Milestone 9.
5. Pacing bugs in the tick loop (lost leftover time, speed applied to the accumulated time, truncated frame milliseconds) were fixed in PR #5.

---

## Milestone 2 — Map

Replace the all-grass map with random walls with real terrain: ground types, water, and natural obstacles that shape where plants grow and how animals move.

**Open questions**

1. Which terrain types does Phase 1 need? For example grass, dirt, sand, rock, shallow water, deep water.
2. How is terrain defined: a hard-coded enum, or a `TerrainDef` loaded from JSON like items and biota, holding passability, movement cost, texture and whether plants can grow there?
3. How is the map made: procedural generation (for example noise for elevation and moisture), a hand-authored test map loaded from a file, or both?
4. If procedural, should generation use a seed, so the same world can be recreated for testing and balancing?
5. Where does water live: a terrain type, a tile flag, or an entity? Is it drinkable from adjacent tiles, and is shallow water walkable?
6. Should terrain affect movement speed (for example slower in shallow water), with A* using those costs, or is it only passable vs. impassable?
7. Should terrain feed later milestones, for example fertility or moisture values that decide where each plant can grow?
8. What map size should Phase 1 use? The current 1000×1000 is one `MeterTile` object per tile. Is a smaller test locale (for example 200×200) better, and should tiles become lighter data rather than full `GameObject`s?
9. How are the textures handled: one texture per terrain type, variations to break up repetition, or smooth transitions between neighboring types?
10. Should the empty-path crash in `GoTo` and the unbounded A* search be fixed as part of this milestone, as their own small PR?

**Decisions**

1. Phase 1 ground types: grass, dirt, sand, mud and rock (impassable). Water is not a ground type (see 5).
2. Ground is a `TerrainDef` loaded from JSON in `Content/Defs/Terrain/`, like items and biota. It holds `Passable`, `MoveCost`, `Fertility`, `TextureKey` and a temporary `Tint` color (`[R, G, B]` in JSON).
3. No procedural generation in this milestone; it becomes its own sub-milestone later. The starting area is grass with a few hard-coded shapes: a lake-water pond (deeper toward its centre, on a sand shore), a rock ridge and a dirt patch.
4. Map seeding moves to the procedural sub-milestone. With hard-coded shapes there is nothing random to seed.
5. Water is an optional layer on top of the ground, described by a `WaterDef` loaded from `Content/Defs/Water/`: salt, swamp, lake and river. A `WaterDef` holds `Drinkable`, `Quality` (a `short` from 1 to 100), `MoveCost` and a temporary `Tint`. Each water tile also has a depth in tenths of a meter (a `byte`, 0 to 25.5 m). Until the heightmap exists, all ground is level and the water surface is level with the ground. A tile is passable when its ground is passable and it is dry or no deeper than a wading limit of 0.5 m; the limit becomes per creature in Milestone 7.
6. Terrain affects movement. `MoveCost` multiplies the A* step cost (5 orthogonal, 7 diagonal). On a water tile the water's `MoveCost` replaces the ground's. The cheapest cost is 1×, so the A* distance estimate stays correct.
7. `Fertility` is a `short` from 1 to 100 on `TerrainDef`. The rules for which plants grow where are decided in Milestone 5.
8. The map is endless and built from 512×512 chunks, created the first time the camera nears them and never unloaded in Phase 1. Each chunk stores a `TerrainDef[512, 512]` for ground, a `WaterDef[512, 512]` for water (`null` means dry) and a `byte[512, 512]` for water depth, so tiles become plain data and `MeterTile` is removed. The chunked map belongs to `Locale`, so the starting area is one Locale that grows. New chunks are plain grass until procedural generation exists. A* treats tiles in chunks that don't exist yet as impassable. Chunks are kept in a `ConcurrentDictionary`, because the AI thread reads the map while the main thread may be adding chunks.
9. One texture per ground and water type. Until those textures exist, each draws the grass texture tinted with its `Tint` color.
10. Three pathfinding bugs are fixed in their own PR: `GetTraversablePoints` checking the tiles around (0,0) instead of around the current point, the `GoTo` crash when no path is found, and the unbounded A* search, which gets a search limit. The mix of pixel and tile units in `MapUtil.GetAStarPathQueue` is left for later.
11. A basic camera: drag with the left mouse button, zoom with the mouse wheel in whole steps from 1× to 4×, and no map edges. Only tiles inside the camera view are drawn. MonoGame's `SpriteBatch` does not skip off-screen sprites itself. Zoom keeps the world point under the cursor in place. Chunks are created for the visible area plus a 32-tile margin. Mouse input is ignored while the window isn't focused.
12. Height (Z) is a heightmap, one height per tile, added with the procedural sub-milestone. Walking up or down costs more and a drop bigger than one step blocks movement. How height is shown on screen is decided later.

**PRs, in order**

1. Record these decisions (this PR).
2. Pathfinding bug fixes.
3. `TerrainDef` and `WaterDef` with their JSON defs and tint colors, plus the `GetCalculatedPath` crash fix (a search ending on the start tile).
4. Chunked map replacing `MeterTile`, the hard-coded starting chunk, and terrain costs in A*.
5. Camera: drag, zoom and drawing only the visible tiles.

---

## Milestone 3 — Creature and plant foundations

A shared base for anything that acts (animals now, Kremlits later) and a plant entity. `Locale` holds and updates both, plus a spatial lookup for finding the nearest food, water or threat.

**Open questions**

1. Should the new creature base replace the Kremlit-only action types (`BaseAction`, `KremlitSnapshot`), so Kremlits reuse it later? This changes existing code.
2. Grass and ground cover: store as a biomass value on each tile (cheap across a million tiles), with bushes and trees as individual entities? Or make everything an entity?
3. Spatial lookup: a simple grid of buckets (for example 16×16-tile chunks), or scan lists for now and optimize later?
4. Can a plant or creature share a tile with others? Do trees block movement?
5. What happens to `Locale.PartialUpdate` for locales the player isn't viewing: simulate fully, simulate cheaply, or pause?

---

## Milestone 4 — Def schema

Species-level defs with every attribute the simulation needs.

**Proposed attributes**

- **FaunaDef:** body mass, basal kcal per hour, activity multipliers (sleep, walk, run), stomach capacity, fat reserve, water per day, walk and run speed, perception range, activity cycle (diurnal, nocturnal or crepuscular), sleep hours per day, lifespan, maturity age, gestation length, litter size, diet (edible tags), prey tags, flee-from tags, carcass yield (meat and hide item keys and amounts).
- **FloraDef:** growth stages with durations, edible biomass (kcal), regrowth per day, fruit yield (item key, max count, regrow days), which parts are edible (foliage vs. fruit), spread chance and radius, lifespan, whether it blocks movement.
- **ItemDef:** kcal per unit, weight, spoil time.

**Open questions**

1. Units: real kcal, kg and liters (easy to research, so defs are believable), or abstract units?
2. Keep one `BiotaDef` class, or split it into `FloraDef` and `FaunaDef`, each with its own folder?
3. Should the existing category defs (`flora_trees`, `mammals_deer`) be replaced by species defs, or kept as tags on them?
4. How do defs reference each other (for example a berry bush yielding `fruit_berry`): by key string, resolved and validated at load time?
5. Should the registry validate values at load (no negative rates, referenced keys exist)?
6. How is diet expressed: a list of edible tags, per-food preference weights, or both?
7. Which attributes vary per individual (for example size or speed within a range), and which are fixed per species?
8. Is the omnivore (boar) in or out for Phase 1?

---

## Milestone 5 — Flora simulation

Plants grow, get grazed down, regrow, fruit, spread and die.

**Open questions**

1. How often does flora update: every tick, once per game hour, or staggered across ticks?
2. How does spreading work: seeds landing on nearby free tiles by chance, or a density target per area?
3. What limits plant growth: nothing, crowding, water nearby, or later soil and season?
4. When a plant is grazed to zero, does it die or regrow from its roots?
5. Does uneaten fruit drop and rot, stay on the plant, or disappear?
6. What does a plant look like at each growth stage? Does each stage need its own texture?

---

## Milestone 6 — Nutrition and metabolism

Calories go in by eating and out by basal plus activity burn. Surplus is stored as fat; a deficit draws fat down, then health drops and the animal starves. Thirst and sleep work the same way.

**Open questions**

1. Is digestion instant, or does food sit in the stomach and convert to energy over time?
2. How do hunger, thirst and tiredness map to behavior: raw values, or thresholds (satisfied, hungry, starving)?
3. What damages health (starvation, dehydration, exhaustion, injury), how fast, and can it recover?
4. Should animals share the existing `NeedType` enum with Kremlits, or get their own set?
5. Are there nutrients beyond calories (protein, etc.), or only calories for Phase 1?
6. Does eating take time (ticks spent eating), or happen in one tick?

---

## Milestone 7 — Animal AI

A decision layer scores needs and threats and picks an action: Wander, Graze or Browse, Drink, Sleep, Flee, Stalk or Chase, Kill, Eat Carcass.

**Open questions**

1. AI style: utility scoring (recommended, since needs compete naturally), a simple priority state machine, or GOAP?
2. How often does an animal re-decide: every tick, on a timer, or only when its current action ends or is interrupted?
3. Which actions can interrupt others (for example, a threat interrupting grazing)?
4. How far do animals wander, and how is that kept cheap for the single AI thread?
5. How does a chase resolve: speed comparison, stamina, or a success chance?
6. Do wolves hunt in packs or alone? Do rabbits and deer form herds?
7. How do animals remember things (water locations, where a predator was seen), if at all?
8. The AI thread gets the live map rather than a copy. Is that acceptable for Phase 1, or should snapshots become real copies?

---

## Milestone 8 — Life cycle

Aging, mating, gestation, births, natural death, and carcasses that decay.

**Open questions**

1. Do animals have sex (male/female), or is reproduction simplified?
2. What triggers mating: maturity plus being well fed, a breeding season, or a cooldown?
3. Are young animals different (smaller, slower, dependent on a parent)?
4. How long do carcasses last, and can scavengers eat them?
5. Is there a population cap per locale to prevent runaway growth, or should balance come only from predation and food?

---

## Milestone 9 — Observation

A population and stats readout, so we can tell whether the ecosystem holds steady and tune the numbers.

**Open questions**

1. On screen (text overlay), console logs, a CSV export, or a mix?
2. Which numbers matter: population per species, births and deaths by cause, average hunger, total plant biomass?
3. Should clicking an animal or plant show its current state and action?
4. Is a camera (pan and zoom) needed to watch a larger map, since the current view is fixed at 4× scale?
5. What does "stable" mean for Phase 1, for example all species surviving N game days?

---

## Supplemental work

Tasks that aren't tied to a milestone. They can be picked up any time after the milestone listed.

- [ ] Include a SpriteFont for clock visualization (any time after Milestone 1).
- [ ] Create a texture for each ground type (grass, dirt, sand, mud, rock) and water type (salt, swamp, lake, river) to replace the tinted grass placeholders (any time after Milestone 2, PR 3).
- [ ] Decide how tile height is shown on the map (after the heightmap lands in the procedural sub-milestone).
