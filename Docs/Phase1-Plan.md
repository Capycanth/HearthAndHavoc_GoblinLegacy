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

---

## Milestone 2 — Creature and plant foundations

A shared base for anything that acts (animals now, Kremlits later) and a plant entity. `Locale` holds and updates both, plus a spatial lookup for finding the nearest food, water or threat.

**Open questions**

1. Should the new creature base replace the Kremlit-only action types (`BaseAction`, `KremlitSnapshot`), so Kremlits reuse it later? This changes existing code.
2. Grass and ground cover: store as a biomass value on each tile (cheap across a million tiles), with bushes and trees as individual entities? Or make everything an entity?
3. Spatial lookup: a simple grid of buckets (for example 16×16-tile chunks), or scan lists for now and optimize later?
4. Can a plant or creature share a tile with others? Do trees block movement?
5. Where does water live: a tile type, a tile flag, or an entity?
6. Should Phase 1 use a smaller test locale with water (for example 200×200)?
7. Should the empty-path crash in `GoTo` and the unbounded A* search be fixed first, as their own small PR?
8. What happens to `Locale.PartialUpdate` for locales the player isn't viewing: simulate fully, simulate cheaply, or pause?

---

## Milestone 3 — Def schema

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

## Milestone 4 — Flora simulation

Plants grow, get grazed down, regrow, fruit, spread and die.

**Open questions**

1. How often does flora update: every tick, once per game hour, or staggered across ticks?
2. How does spreading work: seeds landing on nearby free tiles by chance, or a density target per area?
3. What limits plant growth: nothing, crowding, water nearby, or later soil and season?
4. When a plant is grazed to zero, does it die or regrow from its roots?
5. Does uneaten fruit drop and rot, stay on the plant, or disappear?
6. What does a plant look like at each growth stage? Does each stage need its own texture?

---

## Milestone 5 — Nutrition and metabolism

Calories go in by eating and out by basal plus activity burn. Surplus is stored as fat; a deficit draws fat down, then health drops and the animal starves. Thirst and sleep work the same way.

**Open questions**

1. Is digestion instant, or does food sit in the stomach and convert to energy over time?
2. How do hunger, thirst and tiredness map to behavior: raw values, or thresholds (satisfied, hungry, starving)?
3. What damages health (starvation, dehydration, exhaustion, injury), how fast, and can it recover?
4. Should animals share the existing `NeedType` enum with Kremlits, or get their own set?
5. Are there nutrients beyond calories (protein, etc.), or only calories for Phase 1?
6. Does eating take time (ticks spent eating), or happen in one tick?

---

## Milestone 6 — Animal AI

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

## Milestone 7 — Life cycle

Aging, mating, gestation, births, natural death, and carcasses that decay.

**Open questions**

1. Do animals have sex (male/female), or is reproduction simplified?
2. What triggers mating: maturity plus being well fed, a breeding season, or a cooldown?
3. Are young animals different (smaller, slower, dependent on a parent)?
4. How long do carcasses last, and can scavengers eat them?
5. Is there a population cap per locale to prevent runaway growth, or should balance come only from predation and food?

---

## Milestone 8 — Observation

A population and stats readout, so we can tell whether the ecosystem holds steady and tune the numbers.

**Open questions**

1. On screen (text overlay), console logs, a CSV export, or a mix?
2. Which numbers matter: population per species, births and deaths by cause, average hunger, total plant biomass?
3. Should clicking an animal or plant show its current state and action?
4. Is a camera (pan and zoom) needed to watch a larger map, since the current view is fixed at 4× scale?
5. What does "stable" mean for Phase 1, for example all species surviving N game days?
