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

1. Phase 1 ground types: grass, dirt, sand, mud and rock (impassable). Water is not a ground type (see 5). *Changed in Milestone 3: grass is no longer a ground type; it is a cover on dirt (Milestone 3, decision 3).*
2. Ground is a `TerrainDef` loaded from JSON in `Content/Defs/Terrain/`, like items and biota. It holds `Passable`, `MoveCost`, `Fertility`, `TextureKey` and a temporary `Tint` color (`[R, G, B]` in JSON). *Changed in Milestone 3: `Fertility` moves off `TerrainDef` to a per-tile value (Milestone 3, decision 6).*
3. No procedural generation in this milestone; it becomes its own sub-milestone later. The starting area is grass with a few hard-coded shapes: a lake-water pond (deeper toward its centre, on a sand shore), a rock ridge and a dirt patch. *Changed in Milestone 3: the starting area is dirt covered in grass (Milestone 3, decision 3).*
4. Map seeding moves to the procedural sub-milestone. With hard-coded shapes there is nothing random to seed.
5. Water is an optional layer on top of the ground, described by a `WaterDef` loaded from `Content/Defs/Water/`: salt, swamp, lake and river. A `WaterDef` holds `Drinkable`, `Quality` (a `short` from 1 to 100), `MoveCost` and a temporary `Tint`. Each water tile also has a depth in tenths of a meter (a `byte`, 0 to 25.5 m). Until the heightmap exists, all ground is level and the water surface is level with the ground. A tile is passable when its ground is passable and it is dry or no deeper than a wading limit of 0.5 m; the limit becomes per creature in Milestone 7.
6. Terrain affects movement. `MoveCost` multiplies the A* step cost (5 orthogonal, 7 diagonal). On a water tile the water's `MoveCost` replaces the ground's. The cheapest cost is 1×, so the A* distance estimate stays correct.
7. `Fertility` is a `short` from 1 to 100 on `TerrainDef`. The rules for which plants grow where are decided in Milestone 5. *Changed in Milestone 3: fertility is a per-tile `byte` stored in each chunk (Milestone 3, decision 6).*
8. The map is endless and built from 512×512 chunks, created the first time the camera nears them and never unloaded in Phase 1. Each chunk stores a `TerrainDef[512, 512]` for ground, a `WaterDef[512, 512]` for water (`null` means dry) and a `byte[512, 512]` for water depth, so tiles become plain data and `MeterTile` is removed. The chunked map belongs to `Locale`, so the starting area is one Locale that grows. New chunks are plain grass until procedural generation exists. A* treats tiles in chunks that don't exist yet as impassable. Chunks are kept in a `ConcurrentDictionary`, because the AI thread reads the map while the main thread may be adding chunks. *Changed in Milestone 3: chunks gain cover, cover biomass, fertility, occupancy and plant arrays, and new chunks are dirt covered in grass (Milestone 3, decisions 3, 5 and 6).*
9. One texture per ground and water type. Until those textures exist, each draws the grass texture tinted with its `Tint` color.
10. Three pathfinding bugs are fixed in their own PR: `GetTraversablePoints` checking the tiles around (0,0) instead of around the current point, the `GoTo` crash when no path is found, and the unbounded A* search, which gets a search limit. The mix of pixel and tile units in `MapUtil.GetAStarPathQueue` is left for later. *Fixed in Milestone 3, PR 4: the path start is used as a tile position, without the `>> 4` shift.*
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

**Decisions**

1. An abstract `Creature : GameObject` is the shared base for anything that acts. It holds an `int Id` (from a counter on `World`), a `Size`, a reference to the `Locale` it lives in, `CurrentAction` and the update loop. `Kremlit` and a new `Animal` inherit from it. No generics.
2. `BaseAction` works on a `Creature`, and `Perform(World, Kremlit)` becomes `Perform(Creature)`, since the creature reaches the map through its `Locale`. `KremlitSnapshot` becomes `CreatureSnapshot`, and the `ProcessorThread.Enqueue` overload changes to match. Kremlit's `string Id` is replaced by the shared `int` id.
3. Grass stops being a ground type and becomes a cover type. `terrain_grass` is removed. New chunks and the test map default to dirt ground fully covered in grass.
4. Cover is a `CoverDef` loaded from `Content/Defs/Cover/`, holding `MaxBiomass` (a `ushort`), `TextureKey` and a temporary `Tint`. Grass and clover are the first two. A tile has at most one cover type. Spreadability and nutrition are added in later milestones.
5. Each chunk gains a `CoverDef[512, 512]` for cover (`null` means bare dirt), a `ushort[512, 512]` for cover biomass, a `byte[512, 512]` for occupancy and a `Plant[512, 512]` for plants (`null` means no plant). Cover is stored as a reference, like ground and water, for consistency; `Def.Index` counts across all def types, so it can't serve as a small cover index.
6. Fertility moves off `TerrainDef` (and out of `terrain.json`) to a `byte[512, 512]` per chunk, from 1 to 100, stored on every tile so mud keeps its value when it dries into dirt. Every tile gets a constant 50 until procedural generation exists.
7. Grass is finite. Grazing lowers cover biomass; at zero the cover is cleared to bare dirt, and neighboring cover spreads back onto it by a spreadability value designed in Milestone 5. Grass is a starvation fallback for species that can digest it. Whether it keeps an animal alive without letting it breed, or only slows starvation, is a per-species balance number set in Milestones 4 and 6.
8. Each tile draws only its top layer: water if present, otherwise cover if present, otherwise ground. This avoids drawing covered tiles twice.
9. A tile holds at most one plant entity. Placing a plant destroys the cover under it, so plants always stand on bare dirt.
10. `BiotaDef` gains `BlocksMovement`, true for trees and bushes for now, and a temporary `Tint` that colours plants and animals until they have textures. `IsPassable` checks the plant on the tile, so blocking stays a plain array read for A*.
11. Creatures share tiles up to a size capacity of 100 per tile. Sizes: rabbit 5, wolf 30, deer 50, stored as `Size` on `BiotaDef`. Occupancy is a `byte` because the capacity check keeps a tile's total at or below 100; capacity must stay at or below 255. *Changed in Milestone 5: `Size` is set per growth stage on `FaunaDef` (Milestone 5, decision 15).*
12. A* ignores occupancy and plans around static blockers only. Capacity is checked when stepping: a creature facing a full tile waits, and after 3 blocked ticks in a row it drops its path and repaths. After 3 repaths, `GoTo` gives up and ends, so the creature chooses a new action. *Changed in Milestone 5: stepping also refuses impassable tiles (Milestone 5, decision 21).*
13. All movement goes through `Locale.MoveCreature(creature, tile)`, which checks capacity, updates occupancy and grid cells, then sets `Position`.
14. Creatures are indexed in a 32×32-tile grid on `Locale`, a `Dictionary<Point, List<Creature>>` keyed by cell. Plants use the chunk plant array instead. `Locale.Draw` draws only creatures in the cells the camera sees.
15. Perception and decisions run on the main thread; only A* goes to the AI thread. This settles Milestone 7 question 8 for Phase 1.
16. Adding and removing creatures and plants goes through pending-add and pending-remove queues on `Locale`, processed after the update loop, where the grid, occupancy, plant array and cover bookkeeping happens.
17. `PartialUpdate` stays empty. There is only one locale, and the whole endless map belongs to it.
18. There are no Kremlits in Phase 1. The test world spawns 30 rabbits, 10 wolves and 20 deer pointing at the category defs, plus a few bushes and trees and a small clover patch. Animals wander with `GoTo` to a random tile within 16 tiles of where they stand. Animals draw the Kremlit texture tinted per species; plants draw the tile texture tinted. The 5 test Kremlits keep spawning until PR 5, so movement can be checked along the way.

**PRs, in order**

1. Record these decisions (this PR).
2. Creature base: `Creature`, `Kremlit` moved onto it, the id counter on `World`, `BaseAction.Perform(Creature)`, `CreatureSnapshot` and the `ProcessorThread` overload. Behavior unchanged.
3. Cover and fertility: `CoverDef` with its JSON, the cover, biomass and fertility arrays, grass removed as a ground type, `Fertility` removed from `TerrainDef`, new defaults and test map (with the clover patch), and cover drawing.
4. Occupancy and spatial grid: the occupancy array and capacity check, the creature grid, `MoveCreature`, `GoTo` stepping through it with wait-then-repath, the pending queues and culled creature drawing.
5. Plants and animals: the `Plant` entity, the plant array and its blocking check, `BlocksMovement` in `BiotaDef` and `flora.json`, `Animal`, and the test spawns replacing the Kremlits.

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

**Decisions**

1. Body values use real units (kcal, kg, liters). Life-cycle durations (lifespan, maturity, gestation, regrowth) use tuned game days, since a 112-day year is too short for real ones.
2. `BiotaDef` is replaced by `FloraDef` (`Content/Defs/Flora/`) and `FaunaDef` (`Content/Defs/Fauna/`), with no shared base.
3. Species defs replace the category defs. Fauna are tagged `herbivore`, `omnivore` or `carnivore`.
4. Defs reference each other by key string in JSON. After all folders load, a resolve pass fills `[JsonIgnore]` def references and fails on unknown keys.
5. Each def type has a `Validate()` that runs after the resolve pass and throws an `InvalidDataException` naming the def on bad values.
6. All values are fixed per species; nothing varies per individual in Phase 1.
7. Slow rates (growth, regrowth) are per game day. Fast rates (calorie burn, digestion) are per game hour. Rates are `float`; counts and durations are `int`. Enums are read from JSON as strings with `JsonStringEnumConverter`.
8. **FaunaDef:** `Size` (volume, for occupancy), `BodyMassKg` (weight), `BasalKcalPerHour`, sleep, walk and run activity multipliers, `StomachCapacityKg`, `MaxFatKg`, `WaterLitersPerDay`, walk and run speed, `PerceptionRange`, `ActivityCycle` (diurnal, nocturnal or crepuscular, an enum), `SleepHoursPerDay`, `LifespanDays`, `MaturityDays`, `GestationDays`, `LitterSize`, `Diet` (a list of flora, fauna or cover keys), `CarcassYield` (item key and amount) and `Tint`. *Changed in Milestone 5: `Size`, `BodyMassKg` and `MaturityDays` are replaced by weight-based growth stages (Milestone 5, decisions 10 and 15).* *Changed in Milestone 7: the diet also takes fruit item keys, and a flora key means that plant's foliage (Milestone 7, decision 26).*
9. The diet decides what an animal will try to eat; enzymes (Milestone 4b) decide how much energy it gets from it. A carnivore ignores meat it has never learned to hunt, which is why invasive species with no natural predators thrive.
10. Predators are not written in JSON. The resolve pass gives each fauna def a generated list of the fauna whose diet includes it, which drives fleeing. An explicit list can be added later for fear that isn't about being eaten.
11. **FloraDef:** `BlocksMovement`, `Tint`, `GrowthStages` (a list of `{name, durationDays}`), `MaxFoliageGrams`, `FoliageRegrowGramsPerDay`, `FruitItem`, `FruitMaxCount`, `FruitRegrowDays`, `SpreadChance`, `SpreadRadius` and `LifespanDays`. Foliage is edible when `MaxFoliageGrams` is above 0, and fruit when `FruitItem` is set. Mushroom patches have no foliage and yield mushroom items as fruit. Per-stage textures are decided in Milestone 5. *Changed in Milestone 5: growth stages are `{name, stageMaturityWeightKg, growthKgPerDay}` and are reached by weight (Milestone 5, decision 10).* *Changed in Milestone 5: `BlocksMovement` moves onto each growth stage (Milestone 5, decision 21).*
12. **ItemDef:** `WeightKg` per unit and `SpoilDays` (0 means it never spoils). No def stores kcal directly; energy comes from compositions (Milestone 4b).
13. Phase 1 species: berry bush, apple tree, mushroom patch and dandelion (edible foliage, does not block movement), plus rabbit, deer, wolf and boar. The test spawns add dandelions and boars.
14. Real durations are scaled into game days by one rule: game days = real days × 112 / 365, so a real year becomes one game year and every species keeps its real proportions. Sleep hours are not scaled, since a game day still has 24 hours.

**PRs, in order**

1. Record the decisions for Milestones 4 and 4b (this PR).
2. Registry: the resolve pass, `Validate()` and the enum converter.
3. `ItemDef` attributes.
4. `FloraDef` with species JSON, replacing `BiotaDef` in `Plant` and the test plants. `BiotaDef` stays for animals until PR 5.
5. `FaunaDef` with species JSON, the boar's meat and hide items and the generated predator lists, replacing `BiotaDef` in `Animal` and the test animals, and removing `BiotaDef`.

---

## Milestone 4b — Digestion data

Every edible substance is made of sub-substances, and each sub-substance needs exactly one enzyme to unlock its energy. A creature absorbs the sub-substances it has enzymes for and gets nothing from the rest, so the food chain comes from the data instead of hand-written rules.

**Decisions**

1. `Enzyme` and `Substance` are enums, each with all 7 values: amylase (simple carbs), cellulase (cellulose), chitinase (chitin), galactosidase (complex sugars), protease (protein), lipase (fat) and keratinase (keratin). A static table maps each substance to its enzyme and kcal per gram: 9 for fat and 4 for everything else.
2. A `Composition` lists grams per 100 g: water as its own field and the substances in a map. The total may be at most 100 g; the remainder is inert (minerals, ash). Validation rejects totals over 100. Water is explicit because nutrition will take it into account.
3. Compositions live on `CoverDef` (grazing), `FloraDef` (foliage) and `ItemDef` (fruit, mushrooms, meat, hide). Fauna are eaten through their carcass items.
4. Cover biomass is measured in grams per tile.
5. `FaunaDef` gains an `Enzymes` list. Rabbit and deer: amylase, cellulase, protease. Boar: amylase, protease, lipase, chitinase, galactosidase. Wolf: protease, lipase. Nothing has keratinase in Phase 1.
6. This milestone adds data only. How much energy a creature extracts is calculated in Milestone 6.

**PRs, in order**

1. The enums and the substance table.
2. `Composition` on `CoverDef`, `FloraDef` and `ItemDef`, with JSON values.
3. Enzymes on `FaunaDef`.

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

**Decisions**

1. `Locale` keeps a list of its plants, since the chunk plant arrays can't be looped over cheaply. Each plant updates once per game hour. Each plant has an update slot from 0 to 59, set when it is created, and updates on the tick whose minute matches its slot, so about 1/60 of the plants update each tick instead of all at once on the hour. Each update applies 1/24 of the plant's per-day rates.
2. Plants spread by chance-based seeding. `SpreadChance` is a chance per game day. Once per game day, a mature plant (in its final growth stage) rolls `SpreadChance`. On success it picks a random tile within `SpreadRadius`. The seed takes only if that tile's chunk exists and the tile is dry, passable ground with no plant. A second roll, scaled by the tile's fertility, decides whether it sprouts. Sprouting destroys the cover under it (Milestone 3, decision 9).
3. Growth is limited by fertility, season and crowding. Fertility scales growth, foliage and fruit regrowth and the sprouting roll. Seasons use one table shared by all species, starting at full speed in spring and summer, half in autumn and none in winter (no growth, regrowth or fruiting); the values are tuned later. Crowding comes from the one-plant-per-tile rule. Distance to water waits for the heightmap and moisture. *Changed by decision 19: each flora def has its own season tables.*
4. A plant grazed to 0 g of foliage does not die; it regrows from its roots. Plants die only when they reach `LifespanDays`, leaving bare dirt that cover can spread back onto. A per-species flag for plants that die when eaten whole can be added later if needed.
5. Fruit stays on the plant as a count, up to `FruitMaxCount`. It regrows linearly at `FruitMaxCount / FruitRegrowDays` per day, so `FruitRegrowDays` is the number of days to regrow the full crop. Dropped and rotting fruit waits until items can exist on the map.
6. There are no per-stage textures yet. Each growth stage draws the plant at a larger scale, from small for the first stage to the full tile for the final stage, with the existing tint. Per-stage textures are added to the supplemental work list. *Changed by decision 28: each stage has its own sprite instead of a scale.*
7. Cover regrows and spreads without looping over every tile. Grazed tiles below `MaxBiomass` go into a regrowing set, and bare tiles next to cover go into a spreadable set. Cover regrowth and spreading only look at tiles in these sets. `CoverDef` gains `RegrowGramsPerDay` and `SpreadChance` (a chance per game day), with values in `cover.json`. *Details in decision 27.*
8. `World` owns one seeded `Random` for the simulation, so a run can be repeated while tuning balance. *Changed by decision 22: the seeded `Random` lives in a static `SimRandom` class instead of on `World`.*
9. Test plants spawn at a random age within their final growth stage, so fruiting and spreading show up right away. *Changed by decision 17: test plants spawn at their final stage weight.*
10. Growth stages are reached by weight, not age. Each `GrowthStage` holds `Name`, `StageMaturityWeightKg` (the weight at which the plant or animal leaves the stage) and `GrowthKgPerDay` (its gain per day at full rate). `DurationDays` is removed. Each plant and animal stores its current `WeightKg` and moves to the next stage when it reaches the current stage's `StageMaturityWeightKg`. The final stage's `StageMaturityWeightKg` is the adult maximum, and growth stops once it is reached. Age is still derived from the birth tick and only decides death by `LifespanDays`.
11. Plant growth per hourly update is `GrowthKgPerDay / 24 × fertility / 50 × season`. The same fertility and season multiplier scales foliage and fruit regrowth. Dividing by 50 makes the JSON rates the rates on average soil, so fertility 100 doubles them. Season values: spring 1, summer 1, autumn 0.5, winter 0. Tile fertility is read with `TileMap.GetFertility`, and the season table lives in a static `SeasonGrowth` class, kept out of `SimClock` since the clock only tells time. *Changed by decision 19: the season multipliers come from each def, and the `SeasonGrowth` class is not used.*
12. Animal growth is driven by calorie intake. Its formula is designed with metabolism in Milestone 6; this milestone adds only the data. *Designed in Milestone 6, decisions 6 and 7.*
13. Weights are in kg for flora and fauna. New plants start at 0 kg.
14. The foliage cap grows with the plant: `MaxFoliageGrams × WeightKg / final StageMaturityWeightKg`, so a seedling can't hold a full tree's leaves. Grazing removes foliage only, never weight.
15. Fauna stages use `FaunaGrowthStage : GrowthStage`, which adds `Size`, so flora never carries a field it doesn't use. `Size` moves off `FaunaDef`, and an animal's size is its current stage's size. `BodyMassKg` is replaced by the final stage's `StageMaturityWeightKg`, and `MaturityDays` by the stages. Changing an animal's size and occupancy at runtime arrives with growth in Milestone 6 (see creature pushing under supplemental work). *Changed in Milestone 6: growth waits until the tile has room for the larger size (Milestone 6, decision 9).*
16. Validation: `StageMaturityWeightKg` must be above 0 and strictly increasing, `GrowthKgPerDay` above 0, and fauna `Size` from 1 to the tile capacity of 100. A computed `FullRateMaturityDays` (days to reach the final stage at full growth rate) replaces `FinalStageStartDays` and must be below `LifespanDays`.
17. Test plants and animals spawn at their final stage weight. Test plants get a random age from `FullRateMaturityDays` up to `LifespanDays`.
18. `BasalKcalPerHour`, `StomachCapacityKg`, `MaxFatKg` and `CarcassYield` amounts are adult values, scaled by an animal's current weight divided by its adult maximum weight. Stomach, fat and carcass scale linearly. Basal kcal follows Kleiber's law: the weight ratio raised to the power 0.75, so smaller bodies burn more per kg. Each value is scaled in the PR that first uses it (Milestone 6 for metabolism, Milestone 8 for carcasses).
19. Season multipliers are per def instead of shared, so species can differ (a tree that grows a little in winter, a flower that doesn't grow at all). `FloraDef` has two maps keyed by season: `SeasonGrowth`, which scales weight growth and foliage regrowth, and `SeasonFruiting`, which scales fruit regrowth. Fertility still scales all three. `SeasonGrowth` is required on every flora def; `SeasonFruiting` is required when the def has a fruit item and rejected when it doesn't. Every season must be present, with a value from 0 to 3. `CoverDef` gets `SeasonGrowth` with cover regrowth. Phase 1 values (spring / summer / autumn / winter): apple tree growth 1 / 1 / 0.5 / 0 and fruiting 0 / 1 / 3 / 0 (one full crop a year, mostly in autumn); berry bush growth 1 / 1 / 0.5 / 0 and fruiting 0 / 1 / 0.5 / 0; dandelion growth 1.5 / 1 / 0.5 / 0; mushroom patch growth and fruiting 0.5 / 0.5 / 1.5 / 0.
20. A mature plant makes its daily spread roll on its update during hour 0, so the rolls stay spread across that hour's minutes. On success it picks a random offset of up to `SpreadRadius` in each direction (a square, like animal wander targets). The seed takes only if the tile's chunk exists and the tile is dry, passable ground with no plant. It then sprouts with a chance of `fertility / 100 × SeasonGrowth[season]`, capped at 1, so seeds don't sprout in winter and dandelions spread fastest in spring. A sprout is a new plant at 0 kg, born on the current tick, using its parent's texture, and is placed through `QueueAddPlant`.
21. `BlocksMovement` moves from `FloraDef` onto each stage, using `FloraGrowthStage : GrowthStage`, matching `FaunaGrowthStage`. `Plant.BlocksMovement` returns the current stage's value, and `TileMap.IsPassable` reads it. Apple tree: seedling no, sapling and mature yes. Berry bush: seedling no, young and mature yes. Dandelion and mushroom patch never block. Seeds always sprout as non-blocking seedlings, so a sprout can't trap a creature. A plant can start blocking when it grows into a new stage: a creature standing on it can still walk off (A* only checks the tiles it moves into), and `Locale.MoveCreature` refuses impassable tiles as well as full ones, so a creature whose path crosses a new blocker waits and repaths as usual.
22. The simulation's seeded `Random` lives in a static `SimRandom` class (`Initialize(seed)` and `Instance`), matching the codebase's other static services. `Initializer` seeds it with a fixed test seed. Plants, animals and the test spawns all draw from it. It is used from the main thread only, since `Random` isn't thread-safe. .NET's `Random` can't save its position in the sequence, so a future save and load will re-seed on load rather than replay exactly; animal movement already isn't exactly repeatable because A* results return from the AI thread at timing-dependent moments.
23. Def keys are named from the largest category to the smallest: `type_category_name`. Flora keys become `flora_tree_apple`, `flora_bush_berry`, `flora_flower_dandelion` and `flora_fungus_oyster` (the mushroom patch is renamed to match its `item_mushroom_oyster`), and fauna keys become `fauna_mammal_boar`, `fauna_mammal_deer`, `fauna_mammal_rabbit` and `fauna_mammal_wolf`. Item keys already follow this order, and cover, terrain and water keys have no category level. Flora defs gain descriptive tags: apple tree `tree` and `hardwood`, berry bush `bush`, dandelion `flower`, oyster mushroom `fungus`.
24. Defs can select other defs by tag with a query written as a list of lists: the outer list ORs its groups and each inner list ANDs its tags, so `[["tree", "hardwood"], ["bush"]]` means (tree and hardwood) or bush. A tag starting with `!` excludes defs with that tag from its group, for example `[["tree", "!conifer"]]`. A static `DefHelper.MatchTags<T>` expands a query into a set of defs of type `T` during the resolve pass, so nothing matches strings at runtime. Loading fails on an empty query or group, a tag no def has, a group with only `!` tags, or a group that matches no def of the requested type.
25. A flora def can need a host plant: `HostFloraTags` (a tag query) and `HostRadius` (tiles in each direction) on `FloraDef`, resolved into a set of host defs. A seed only takes if a mature host is within `HostRadius` of the target tile, and once a day (on the hour-0 update) a hosted plant removes itself if no mature host is within its radius, so it dies the same day its host dies. `HostRadius` must be at least 1 with host tags and 0 without. The oyster mushroom uses `[["tree", "hardwood"]]` with radius 1, so hardwood trees host it (apple, and maple from decision 26). `LifespanDays` still caps each patch. Oysters can move to dead-wood hosts when decay arrives in Milestone 8.
26. A maple tree (`flora_tree_maple`, tags `tree` and `hardwood`) is added as a non-fruiting hardwood: stages seedling to 0.5 kg at 0.015 kg per day (not blocking), sapling to 50 kg at 0.2 (blocking) and mature up to 1000 kg at 1.5 (blocking), a lifespan of 11,200 days, 40,000 g of foliage regrowing 300 g per day with the apple's leaf composition, season growth 1 / 1 / 0.5 / 0 and the apple's spread values. Deer eat maple foliage. The test world gains a forest: a circle of radius 20 centered at (40, 40) where each dry, passable, unused tile has a 10% chance of a tree, 90% maple and 10% apple, spawned mature like the other test plants. The three test mushrooms spawn beside randomly chosen forest trees instead of the lone apple trees. The oyster mushroom's spread changes to a 0.1 chance within radius 2, since with the old values (0.02 within radius 4) most seeds landed too far from a host and the mushrooms died out.
27. Cover regrowth and spreading run once a game day, at midnight, in a `CoverGrowth` class owned by `Locale`. Daily steps suit cover biomass, which is stored in whole grams, where hourly amounts would be lost to rounding.
    - `CoverDef` gains `RegrowGramsPerDay`, `SpreadChance` (per day), `SeasonGrowth` and `TerrainKeys`, the ground types it can grow on, resolved to a set of terrain defs. Grass: 40 g per day, 0.05, 1 / 1 / 0.5 / 0, dirt. Clover: 30 g per day, 0.03, 0.8 / 1 / 0.5 / 0, dirt. The season table check moves to `DefHelper.ValidateSeasonTable`, shared by flora and cover.
    - Regrowing set: tiles whose cover is below `MaxBiomass`. Each day they gain `RegrowGramsPerDay × fertility / 50 × SeasonGrowth[season]`, rounded and capped, and leave the set when full.
    - Spreadable set: bare tiles (no cover, plant or water). Each day a tile picks a random neighbor, of the 8 around it, whose cover can grow on the tile's ground, and rolls that cover's `SpreadChance × min(1, fertility / 100 × SeasonGrowth[season])`. On success it gains that cover at 10% of `MaxBiomass` and joins the regrowing set, and its bare neighbors join the spreadable set. A tile with no suitable neighbor leaves the set until a neighbor gains cover. New cover is applied after the day's rolls, so cover spreads at most one tile a day.
    - `Locale.GrazeCover(tile, grams)` lowers biomass and returns the grams eaten; at 0 g it clears the cover. Removing a plant makes its tile spreadable, and placing one takes its tile out of both sets. At startup `Locale` scans its existing chunks for bare tiles; chunks created later start fully covered.
28. Each growth stage has its own sprite instead of a draw scale. `GrowthStage` gains a required `TextureKey`, so both flora and fauna stages have one, and validation fails at load if the key isn't a texture loaded by `ContentLoader` (new `ContentLoader.HasTexture`). `Plant.Draw` and `Animal.Draw` look up the current stage's texture each frame, the same way terrain, water and cover draw, so the texture parameter comes off the `Plant` and `Animal` constructors. Until sprites exist, flora stages use `Tile_Grass` and fauna stages use `Kremlit_Male` as placeholders, and `Tint` stays on the defs so species remain distinguishable; tint is set to white or removed once real sprites arrive.

**PRs, in order**

1. Record decisions 1–9 (PR #26).
2. Plant state (age, growth stage, foliage and fruit), the `Locale` plant list, staggered hourly updates, aging and death (PR #27).
3. Record the growth redesign, decisions 10–17 (this PR).
4. Growth schema: `GrowthStage` and `FaunaGrowthStage` with validation, weight, growth and size values in the flora and fauna JSON (numbers approved before they are written), `BodyMassKg`, `MaturityDays` and fauna `Size` removed, plant weight and weight-based stages, and test spawns at final stage weight.
5. Growth and regrowth: the fertility and per-def season multipliers on weight, foliage and fruit, `TileMap.GetFertility`, the `SeasonGrowth` and `SeasonFruiting` tables on `FloraDef` with their JSON values, and the scaled foliage cap.
6. Plant spreading, per-stage `BlocksMovement`, `MoveCreature` refusing impassable tiles, and the static `SimRandom`.
7. Category-first def keys and flora tags (PR #33).
8. Mushroom hosts: `DefHelper` tag queries, host fields and validation on `FloraDef`, the daily host check, host-aware spreading, `TileMap.GetPlant`, the maple tree, and a test forest with the test mushrooms in it.
9. Cover regrowth and spreading: `CoverGrowth`, `Locale.GrazeCover`, the new `CoverDef` fields (including `SeasonGrowth` and `TerrainKeys`) with JSON values, and the shared season table check.
10. Per-stage sprites: `TextureKey` on each growth stage with load-time validation, and plants and animals drawing their current stage's texture.

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

**Decisions**

1. This milestone builds the body model and the entry points that actions will call. There are no Eat, Graze, Browse, Drink or Sleep actions and no temporary feeding behavior; those arrive with the AI in Milestone 7. Nothing eats or drinks, so every test animal starves or dies of thirst. That is expected: the milestone is checked by watching death times match the math.
2. Each `Animal` owns a `Metabolism` class that holds its body state in plain fields. Animals don't use the `NeedType` enum, which stays Kremlit-only for now. *Changed in Milestone 7: animals use `NeedType` through the shared `Needs` component, and their needs are still calculated from `Metabolism` (Milestone 7, decision 8).*
3. `Metabolism` updates once per game hour on the animal's update slot (0 to 59, set when it is created), like plants (Milestone 5, decision 1). Each update runs in order: digest, burn, balance the energy, water, sleep, health recovery, then the death check.
4. `WeightKg` is lean body weight: it alone drives growth stages, `Size` and the weight scaling of Milestone 5, decision 18. `FatKg` is a separate energy reserve and never affects stages. Starvation burns only fat, and after that health, so lean weight never goes down and stages never move backwards.
5. The stomach holds three values: kg of food, kcal still to release and liters of water still to release. Its capacity is `StomachCapacityKg` scaled by weight. Enzymes are applied when food goes in: each substance in the food's `Composition` adds `grams × kcal per gram` if the animal has its enzyme and nothing otherwise. Water in the food counts toward thirst. Each hour the stomach releases 1/6 of what it holds (kg, kcal and water), a constant tuned later. *Changed in PR 4: each hour the stomach releases 1/6 of its capacity in kg (or what is left, if less), so a full stomach always empties in 6 hours. Kcal and water leave in the same proportion as the kg.*
6. The hourly burn is `BasalKcalPerHour × (WeightKg / adult weight)^0.75 × activity multiplier`. The multiplier is 1 for resting and `SleepMultiplier`, `WalkMultiplier` or `RunMultiplier` for the other activities, read from the animal's activity at the moment of the update. This settles Milestone 5, decision 12.
7. When kcal released exceed the burn, the surplus goes first to growth, up to the current stage's `GrowthKgPerDay / 24`, at a cost of 3,000 kcal per kg grown. What is left goes to fat at 7,700 kcal per kg, up to `MaxFatKg` scaled by weight. Energy beyond both caps is lost. Both kcal-per-kg values are constants tuned later.
8. When the burn exceeds the kcal released, the deficit is paid from fat at 7,700 kcal per kg. Once fat is 0, the animal is starving and loses health (decision 11).
9. Growing into the next stage changes the animal's `Size`, and its tile's occupancy with it. If the next stage's `Size` would not fit the tile's capacity, weight stops at the current stage's `StageMaturityWeightKg` and the surplus goes to fat instead, until the tile has room. In Milestone 7 the AI can walk the animal to an open tile when its growth is blocked. `Creature.Size` becomes settable, and the size change goes through `Locale` so occupancy stays correct. *PR 4 detail: `Animal.Grow` adds weight up to the current stage's maturity weight and advances one stage at a time through `Locale.TryResizeCreature`, so `StageIndex` is no longer recalculated from weight after spawning.* *Settled in Milestone 7: the Relocate chain walks a growth-blocked animal to a tile with room (Milestone 7, decision 29).*
10. The water reserve holds one day's `WaterLitersPerDay`, scaled by weight, and drains evenly over the day. Water released from the stomach and drunk water refill it, up to its capacity. At 0 the animal is dehydrated and loses health. *Changed in PR 4: the reserve holds 3 days' water and still drains one day's worth per day.*
11. Sleep debt rises by 1 for each hour awake and falls by `(24 − SleepHoursPerDay) / SleepHoursPerDay` for each hour asleep, so a day with the def's sleep balances out. The debt does not go below 0. It causes no health damage in Phase 1. When an animal sleeps, based on `ActivityCycle`, is decided in Milestone 7. *Settled in Milestone 7: the rest window (Milestone 7, decision 10).*
12. `Health` is a `float` from 0 to 100. Starvation costs 100 over 3 game days and dehydration costs 100 over 1 game day, and the two add together. When the animal is neither starving nor dehydrated, health recovers 10 per game day. At 0 the animal dies and is removed with `QueueRemove`; carcasses arrive in Milestone 8. *Changed in Milestone 7: every death leaves a minimal carcass (Milestone 7, decision 25).*
13. `Metabolism` exposes needs as states for Milestone 7 to read, alongside the raw values. Hunger: satisfied (stomach at least 25% full), hungry (below 25%) or starving (no fat). Thirst: satisfied (reserve at least 50%), thirsty (below 50%) or dehydrated (empty). Tiredness: rested (debt below `SleepHoursPerDay / 2`), tired, or exhausted (debt at least `SleepHoursPerDay`). The threshold values are tuned later. *Changed in PR 3: starving means no fat and a stomach below 25%, so an animal with no fat but food still digesting reports hungry or satisfied. Health damage still follows decision 8. `SleepHoursPerDay` must be from 1 to 24, so the tiredness thresholds and the sleep debt formula never divide by 0.*
14. Entry points for Milestone 7's actions, which nothing calls in this milestone: `Ingest(Composition, grams)` puts food into the stomach and returns the grams accepted, limited by stomach room; `Drink(liters)` returns the liters accepted, limited by room in the water reserve; and read-only `StomachRoomKg` and need states. Every eating action (grazing, browsing, fruit, carcasses) goes through `Ingest`, so the enzyme rules live in one place. *Changed in PR 3: `Ingest` takes one bite per tick, so eating takes time. `FaunaDef` gains `EatGramsPerMinute`, an adult rate scaled by weight like the stomach, so every age fills its stomach in the same time; `Ingest` accepts at most one bite, capped by stomach room. Adult rates fill a stomach in about 30 to 45 minutes: rabbit 4, deer 90, boar 75, wolf 250 g per minute. Every gram accepted takes stomach space, including water, inert matter and substances the animal has no enzyme for; only the kcal depend on enzymes.*
15. An `Activity` enum (sleeping, resting, walking, running) is stored on `Creature`. `GoTo` takes a movement mode, walking by default or running, sets that activity while it moves and sets resting when it ends. *Changed in PR 2: `GoTo` is abstract and takes its mode as an `Activity` through a protected constructor. `WalkTo` and `RunTo` are the subclasses that set it, and later ones such as `SwimTo` can be added the same way. A creature stays resting while it waits for its A\* path, and switches to the mode when it starts stepping.*
16. Movement speed is in tiles per tick. `Creature` gets an abstract `GetMoveSpeed(Activity)`: `Animal` returns its def's `WalkSpeed` or `RunSpeed`, and `Kremlit` returns 1. `GoTo` keeps a movement budget: each tick it adds the creature's speed, and while the budget covers the next step and the path isn't empty, it tries `MoveCreature` and pays the step's cost. A step costs 1 orthogonally or 1.4 diagonally (the 5:7 ratio A* uses) multiplied by the entered tile's `TileMap.GetMoveCost`, so mud and shallow water slow creatures down as well as steering A*. Leftover budget carries over, so a speed of 2.5 takes 2 or 3 steps a tick. A blocked step resets the budget to 0 and counts toward wait-then-repath as before (Milestone 3, decision 12), so a waiting creature can't save up movement and jump several tiles.
17. Test animals spawn full: a full stomach, fat at `MaxFatKg` scaled by weight, a full water reserve, no sleep debt and 100 health. What food fills the starting stomach is proposed and approved in PR 4. *Settled in PR 4: `Metabolism.FillStomach` fills it to capacity with grass for rabbits and deer, apples for boars and deer meat for wolves.*

**PRs, in order**

1. Record these decisions (this PR).
2. Movement: `Activity` on `Creature`, `GetMoveSpeed`, and `GoTo`'s movement mode, movement budget and terrain step cost.
3. Body state: `Metabolism` with its fields, the stomach, `Ingest` and `Drink`, and the need states. No hourly update yet.
4. Hourly update: digestion, burn, growth and fat, capacity-blocked growth with runtime size changes, water, sleep debt, health and death, and full test spawns.

---

## Milestone 7 — Animal AI

A decision layer scores needs and picks an action chain. A chain runs small, interchangeable actions in an order it controls.

**Open questions**

1. AI style: utility scoring (recommended, since needs compete naturally), a simple priority state machine, or GOAP?
2. How often does an animal re-decide: every tick, on a timer, or only when its current action ends or is interrupted?
3. Which actions can interrupt others (for example, a threat interrupting grazing)?
4. How far do animals wander, and how is that kept cheap for the single AI thread?
5. How does a chase resolve: speed comparison, stamina, or a success chance?
6. Do wolves hunt in packs or alone? Do rabbits and deer form herds?
7. How do animals remember things (water locations, where a predator was seen), if at all?
8. The AI thread gets the live map rather than a copy. Is that acceptable for Phase 1, or should snapshots become real copies?

**Decisions**

*Structure*

1. The decision layer picks an `ActionChain`, never a single action. `ActionChain` is its own abstract class that mirrors `BaseAction` without inheriting from it; its `Perform(creature)` returns running, succeeded or failed. `Creature.CurrentAction` becomes `CurrentChain`, and `ChooseAction` becomes `ChooseChain`. Every behavior is a chain. Chains don't nest, and steps repeated across chains are accepted for now. This settles question 1: utility scoring (decisions 8 and 9).
2. Actions are bare and species-agnostic. A chain creates a fresh action when it enters a step and passes it plain values from its context. `Perform` returns an `ActionOutcome`: `Running`, `Arrived`, `NoPath`, `TargetFound`, `TargetNotFound`, `TargetReached`, `TargetLost`, `TargetFled`, `TargetDead`, `TargetEscaped`, `TargetDepleted` or `Complete`. The chain reads whatever the action produced and stores it in its context; actions never see chain contexts. `GoTo` returns `Arrived`, or `NoPath` when it gives up.
3. `JobHandle`, `GenerateSnapshots` and the async path callback move off `BaseAction` onto a new abstract `PathingAction`, which `GoTo` and `Follow` inherit from. `CalculateActionChain` is renamed `CalculatePath`.
4. Each chain type has its own context class. Transitions are hard-coded: step plus outcome gives the next step, Done or Fail, and they may also use hard-coded conditions, such as what kind of target Find returned. A step can only move to the steps its rules list.
5. Steps can be interrupted by default. Only Flee and Attack can't be. A non-interruptible step blocks every other chain, Flee included, which gives predators an ambush bonus.
6. Each animal re-scores on a staggered timer, and immediately when it detects a predator. Another chain replaces the current one only if its score beats the current score plus an additive commitment margin. Ties are broken by a small jitter from `SimRandom`. A failed chain goes on cooldown for that creature and scores 0 until the cooldown ends; a succeeded chain gets no cooldown. This settles questions 2 and 3.
7. `FaunaDef` gains `Chains`, the chain keys the species can use. A static `ChainRegistry` maps each key to `Score(animal)` and `Create(animal)`. A def with Hunt must have a `NaturalWeaponKey` (decision 23).

*Needs and scoring*

8. `Creature` gets a `Needs` component holding a list of `Need` objects, and `Kremlit`'s unused `Needs` dictionary is removed. `Need` is abstract, with `Type` (`NeedType`), `UrgencyExponent` and `Level` (0 to 1). The body needs read `Metabolism` live, use exponent 1.5, and are the only needs animals have in Phase 1: `HungerNeed` is `max(1 − StomachKg/StomachCapacityKg, 1 − FatKg/MaxFatKg)`; `ThirstNeed` is `1 − WaterLiters/WaterCapacityLiters`; `SleepNeed` is `SleepDebtHours / SleepHoursPerDay`, capped at 1 and multiplied by a constant during the species' rest window (decision 10), with sleep debt itself unchanged. A shared abstract `StoredNeed` keeps a value, offers `Satisfy(amount)` and rises hourly through `GetHourlyRise`; its subclasses `FunNeed`, `SocialNeed` and `MateNeed` use exponent 2 and are designed now but built later. Kremlits will use the same `Metabolism` and body needs; how their body data is defined is decided with Kremlits.
9. A chain's score is `Level^UrgencyExponent × the chain's effort weight`. A chain scores 0 when it can't run: on cooldown, Hunt while exhausted, eating with a full stomach, or drinking with a full reserve. Idle has a constant baseline score, and Relocate a fixed score while growth is blocked (decision 29). Flee isn't scored: detecting a predator forces it (decision 21). The margin, effort weights, baselines and jitter are constants tuned in Milestone 9.
10. The rest window is `SleepHoursPerDay` long, centred on midnight for diurnal animals and on noon for nocturnal ones; crepuscular animals get two halves, centred on noon and midnight. This settles when animals sleep (Milestone 6, decision 11).

*Actions*

11. Wander walks to a random point within a radius, rests for a random time, and repeats. It never ends on its own. The radius and rest range are constants.
12. One `Find` action walks to a random point within a radius, running its scanners every few steps. It ends with `TargetFound`, holding the nearest reachable match from any scanner, or `TargetNotFound` if it arrives with nothing found. The scanners are Cover, Foliage, Fruit, Carcass, Creature, Water and Tile; an Item scanner comes with Kremlits. Each scanner takes def keys or tags, plus an optional condition (a delegate such as `Func<Plant, bool>`). A candidate is skipped if its tile can't be stood on and none of its neighbours can either. Chains retry Find up to a constant number of times, then fail.
13. A `Target` holds a kind (cover, foliage, fruit, carcass, creature, water or tile), a tile, and an object when there is one; cover, water and tile targets have no object. Chains walk to the target's tile if it's passable, otherwise to the nearest passable tile next to it.
14. `TileScanner` takes a footprint (width × height) and a condition that every tile in the footprint must pass, for example `CanFit` a size, or no blockers. The target tile is the footprint's top-left corner. Animals use 1×1; larger footprints, such as a 5×3 house, are for Kremlits later.
15. `Follow` is an abstract `PathingAction` for moving targets, with `Chase` (running) and `Stalk` (crouching) subclasses. It re-targets on an interval through the async A\* and keeps moving on its old path while a new one is calculated. It ends with `TargetReached` at its reach distance (next to the target for Chase, a constant striking distance for Stalk), and with `TargetLost` when the target hasn't been in `PerceptionRange` for a few checks in a row or a repath fails.
16. When `Locale` removes a creature, plant or carcass, it clears the grid, occupancy and plant array using the real position first, then sets `Position` to `GameObject.RemovedPosition`, `(int.MinValue / 4, int.MinValue / 4)`. That puts the object out of every range check without the range math overflowing, so removed targets are lost without an alive flag.
17. `Activity.CROUCHING` has walking speed and walking burn but is harder to detect. Stalk uses it.
18. `FaunaDef` gains `MaxRunningMinutes`. Running drains stamina; walking recovers it slowly, resting faster, and sleeping at least as fast as resting. At 0 the animal is exhausted: it can't run until an exhaustion cooldown ends, and then stamina resets to full. An exhausted Chase ends with `TargetLost`; an exhausted Flee continues at walking speed. Rates and the cooldown are constants. This settles question 5.
19. Eat takes one `BiteGrams` bite per tick through `Ingest`, depending on the `Target` kind: cover through `GrazeCover` with the `CoverDef`'s composition; foliage from `FoliageGrams` with `FoliageComposition`; fruit by lowering `FruitCount` by the bite divided by the fruit's weight, with the fruit item's composition; and carcass meat with the meat item's composition. It ends with `Complete` when the stomach is full, or `TargetDepleted` when the food runs out.
20. Drink drinks `DrinkLitersPerMinute`, a new `FaunaDef` value (an adult rate scaled by weight), and ends with `Complete` when the reserve is full. Water never runs out, and `Quality` is ignored. Sleep sets `Activity.SLEEPING` on the spot and ends with `Complete` when sleep debt reaches 0.
21. On its re-score timer, a prey animal runs a `CreatureScanner` over its `Predators` within its `PerceptionRange`. Each predator in range is noticed with a chance equal to the predator's activity factor (crouching low, walking medium, running close to certain) times the prey's own state factor (lower while sleeping); both are constants. Noticing a predator forces Flee straight away, and a Stalk targeting that prey ends with `TargetFled`.
22. Flee is its own action. It runs a short-range A\* on the main thread, so a fleeing animal starts running the same tick it reacts instead of waiting for a path from the AI thread. It can't be interrupted, and it ends once no known predator has been within `PerceptionRange` for a safe period (a constant).
23. A new `WeaponDef` in `Content/Defs/Weapons/` holds `Damage` and `AttacksPerMinute`. `FaunaDef` gains `NaturalWeaponKey`, for example the wolf's teeth. Kremlit fists and crafted weapons come with Kremlits.
24. Attack needs the attacker next to the target. Each attack deals `Damage × (attacker WeightKg ÷ target WeightKg)` to the target's `Health`, through a new `Metabolism` damage entry point. There's no hit chance, and prey don't fight back. It ends with `TargetDead` at 0 health, or `TargetEscaped` when the target moves out of reach.
25. Every animal death (attack, starvation or dehydration) queues a `Carcass` on its tile. It holds the kg of the `CarcassYield` entry whose item is tagged `meat`, scaled by `WeightKg ÷ FinalWeightKg`. It takes no tile capacity, doesn't block movement, doesn't decay, and is removed when empty. Decay and hides come in Milestone 8.

*Diet and chains*

26. A diet entry is a cover key (that cover), a flora key (that plant's foliage), a fruit item key (that fruit, on any plant that grows it) or a fauna key (that animal's meat). Validation: an item key must be some flora's `FruitItem`, and a flora key needs `MaxFoliageGrams` above 0. The four species' diets are split into foliage and fruit entries, with the values approved in their PR.
27. The Phase 1 chains:

    | Chain | Steps and transitions | Who |
    |---|---|---|
    | Idle | Wander | all |
    | Feed | Find (scanners built from the diet's cover, foliage, fruit and fauna entries) → WalkTo → Eat. `TargetNotFound`: retry, then Fail. `NoPath`: Fail. `Complete` or `TargetDepleted`: Done. | every animal |
    | Hunt | Find (Creature and Carcass scanners). A creature found goes to Stalk; a carcass found goes to WalkTo → Eat. Stalk (`TargetReached` or `TargetFled`) → Chase (`TargetReached`) → Attack (`TargetDead`) → Eat. `TargetEscaped` goes back to Chase; `TargetLost` fails. Eat ends Done. | carnivores, omnivores |
    | Drink | Find (WaterScanner, drinkable) → WalkTo → Drink → Done | all |
    | Sleep | Sleep → Done | all |
    | Flee | Flee, until safe | prey |
    | Relocate | Find (1×1 TileScanner with room for the next stage's size) → WalkTo → Done | all |

28. Feed replaces separate Graze, Forage and Scavenge chains. A Scavenge chain is kept for a future pure scavenger.
29. `Animal` gets an `IsGrowthBlocked` flag, set when `Grow` reaches the stage mark with no room for the next `Size` and cleared when the animal moves up a stage. While it is set, Relocate scores a fixed constant: below urgent needs, above Idle.
30. Phase 1 scope: Wander and Find use constant radii; there are no herds, packs or memory of places; and the AI thread keeps the live map. This settles questions 4, 6, 7 and 8. Harvest (with per-flora harvest rules), EatMeal, Haul and the Item scanner come with Kremlits.

**PRs, in order**

1. Record these decisions (this PR).
2. Action and chain foundations: `ActionOutcome`, a slimmer `BaseAction`, `PathingAction` and the rename, `GoTo` outcomes, `ActionChain` with contexts, `CurrentChain`, and the Idle chain (Wander) replacing the random `WalkTo`.
3. Find and Target: `Find`, `Target`, and the Cover, Foliage, Fruit, Water and Tile scanners.
4. Follow: `RemovedPosition`, `Follow` with `Chase` and `Stalk`, `CROUCHING`, and the Creature scanner.
5. Stamina and exhaustion.
6. Carcass and Eat: `Carcass`, the Carcass scanner, and `Eat`.
7. Weapons and Attack: `WeaponDef`, natural weapons, `Attack`, and the `Metabolism` damage entry point.
8. Drink and Sleep: the `Drink` and `Sleep` actions, and `DrinkLitersPerMinute`.
9. Diet split: diet validation and the species diets (values approved before they're written).
10. Detection and Flee.
11. Needs: `Needs`, `Need`, and the three body needs, with the Kremlit dictionary removed.
12. Decision layer: `ChainRegistry`, scoring, the rest window, the margin, cooldowns, `FaunaDef.Chains`, and the Feed, Hunt, Drink, Sleep, Flee and Relocate chains with `IsGrowthBlocked`.

---

## Milestone 8 — Life cycle

Aging, mating, gestation, births, natural death, and carcasses that decay. A minimal carcass already exists from Milestone 7 (decision 25); this milestone adds decay and hides.

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
- [ ] Create a sprite for each plant and animal growth stage, load them in `ContentLoader`, point each stage's `TextureKey` at them, and remove or whiten the placeholder tints (any time after Milestone 5, PR 10). Every texture still needed is listed in `texture-todo.txt` at the repo root.
- [ ] Design creature pushing (after Milestone 6, alongside Milestone 7). When a creature grows and its tile goes over capacity, the smallest creatures are pushed to nearby tiles until the tile fits. A creature moving into a full tile that is larger than every creature on it pushes them out to nearby tiles; this would replace wait-then-repath (Milestone 3, decision 12) for that case. Open questions: what happens to a pushed creature's path and action, whether pushed creatures can push others in turn, what happens when no nearby tile has room, and how ties between equal sizes are settled.
- [ ] Design a tile signal system (after Milestone 5). When a tile's state changes (for example dirt turning to mud, fertility changing, water arriving or cover being cleared), the tile sends a signal to everything on it, so each plant or creature can update any data it keeps about the tile instead of reading it on every update. For example, a plant could keep its fertility factor and recalculate it only when signalled, instead of reading tile fertility every hour. Open questions: which changes send signals; who receives them (the plant, creatures, cover); whether signals are handled immediately or queued and processed with the pending adds and removes; which data inhabitants keep; and whether world-wide changes, such as the season changing, use the same system.
