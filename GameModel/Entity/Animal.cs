using HearthAndHavoc_GoblinLegacy.AI;
using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.AI.Scan;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    // A creature of one fauna species. The body (weight, growth, metabolism, death) lives on Creature; Def is the
    // same object as Body, typed as FaunaDef for the animal-only data such as diet and perception.
    public class Animal : Creature
    {
        // Finds the nearest predator this animal notices on a detection check. Built once, since its predators
        // and condition never change.
        private readonly CreatureScanner _predatorScanner;

        public FaunaDef Def { get; }

        public override WeaponDef Weapon => Def.NaturalWeapon;

        public Animal(int id, FaunaDef def, Locale locale, float weightKg)
            : base(id, def, locale, weightKg)
        {
            Def = def;
            _predatorScanner = new CreatureScanner(def.Predators, IsNewlyNoticed);
        }

        protected override ActionChain ChooseChain()
        {
            return new IdleChain();
        }

        // Checks for predators every Detection.CheckIntervalTicks (Milestone 7, decision 21). A noticed predator
        // joins the Flee already running, or forces a new Flee if the current chain can be interrupted.
        protected override void Perceive()
        {
            if (Def.Predators.Count == 0) return;
            if ((Locale.Clock.TotalTicks + Id) % Detection.CheckIntervalTicks != 0) return;

            FleeChain flee = CurrentChain as FleeChain;
            if (flee == null && CurrentChain != null && !CurrentChain.IsInterruptible) return;

            Target noticed = _predatorScanner.FindNearest(this, Def.PerceptionRange);
            if (noticed == null) return;

            Creature predator = (Creature)noticed.Object;
            if (flee != null)
            {
                flee.AddThreat(predator);
            }
            else
            {
                Console.WriteLine($"Creature {Id} noticed creature {predator.Id} and flees");
                CurrentChain = new FleeChain(predator, Def.PerceptionRange);
            }
        }

        // The scanner's condition: a predator the animal is already fleeing from needs no roll; any other is
        // noticed with its notice chance.
        private bool IsNewlyNoticed(Creature predator)
        {
            if (CurrentChain is FleeChain flee && flee.IsKnownThreat(predator)) return false;
            return SimRandom.Instance.NextDouble() < Detection.GetNoticeChance(predator, this);
        }
    }
}
