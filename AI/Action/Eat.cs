using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Nutrition;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Eats one bite per tick from a target the chain has walked next to (Milestone 7, decision 19). Each bite is
    // taken from the food source first, then put in the stomach with that food's composition. Ends with COMPLETE
    // when the stomach is full, TARGET_DEPLETED when the food runs out, or TARGET_LOST when the target is out of
    // reach (for example a plant or carcass removed while the animal walked to it).
    public class Eat : BaseAction
    {
        private const int Reach = 1;
        private const float GramsPerKg = 1000f;
        private const float MinBiteGrams = 1f;

        private readonly Target _target;

        public Eat(Target target)
        {
            _target = target;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            // An object target is read where it is now; a removed one sits at RemovedPosition and is out of reach.
            Point targetTile = _target.Object != null ? _target.Object.Position : _target.Tile;
            if (MapUtil.GetRingDistance(creature.Position, targetTile) > Reach) return ActionOutcome.TARGET_LOST;

            // Bites are whole grams, so less than one gram of room counts as a full stomach.
            Metabolism metabolism = creature.Metabolism;
            float grams = MathF.Min(metabolism.BiteGrams, metabolism.StomachRoomKg * GramsPerKg);
            if (grams < MinBiteGrams) return ActionOutcome.COMPLETE;

            (float taken, Composition food, bool depleted) = TakeBite(creature, grams);
            if (taken > 0f) metabolism.Ingest(food, taken);

            if (metabolism.StomachRoomKg * GramsPerKg < MinBiteGrams) return ActionOutcome.COMPLETE;
            if (depleted) return ActionOutcome.TARGET_DEPLETED;
            return ActionOutcome.RUNNING;
        }

        // Removes up to grams from the food source. Returns the grams taken, the food's composition, and whether
        // the source has nothing left to eat.
        private (float taken, Composition food, bool depleted) TakeBite(Creature creature, float grams)
        {
            switch (_target.Kind)
            {
                case TargetKind.COVER:
                {
                    CoverDef cover = creature.Locale.LocaleMap.GetCover(_target.Tile);
                    if (cover == null) return (0f, null, true);

                    int taken = creature.Locale.GrazeCover(_target.Tile, (int)grams);
                    bool depleted = creature.Locale.LocaleMap.GetCover(_target.Tile) == null;
                    return (taken, cover.Composition, depleted);
                }

                case TargetKind.FOLIAGE:
                {
                    Plant plant = (Plant)_target.Object;
                    float taken = MathF.Min(grams, plant.FoliageGrams);
                    plant.FoliageGrams -= taken;
                    return (taken, plant.Def.FoliageComposition, plant.FoliageGrams <= 0f);
                }

                case TargetKind.FRUIT:
                {
                    Plant plant = (Plant)_target.Object;
                    ItemDef fruit = plant.Def.FruitItem;
                    float gramsPerFruit = fruit.WeightKg * GramsPerKg;
                    float taken = MathF.Min(grams, plant.FruitCount * gramsPerFruit);
                    plant.FruitCount -= taken / gramsPerFruit;
                    return (taken, fruit.Composition, plant.FruitCount <= 0f);
                }

                case TargetKind.CARCASS:
                {
                    Carcass carcass = (Carcass)_target.Object;
                    float taken = carcass.Bite(grams);
                    return (taken, carcass.MeatItem?.Composition, carcass.MeatKg <= 0f || carcass.IsRotten);
                }

                default:
                    throw new InvalidOperationException($"Eat can't eat a {_target.Kind} target.");
            }
        }
    }
}
