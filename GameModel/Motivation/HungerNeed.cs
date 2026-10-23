using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Nutrition;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Motivation
{
    // How hungry a creature is: whichever is emptier, its stomach or its fat reserve. An animal with a full stomach
    // but no fat is still hungry, since that food has yet to be digested into the reserve.
    public class HungerNeed : Need
    {
        private readonly Creature owner;

        public HungerNeed(Creature owner)
            : base(NeedType.HUNGER, BodyUrgencyExponent)
        {
            this.owner = owner;
        }

        protected override float CalculateLevel()
        {
            Metabolism metabolism = owner.Metabolism;
            float stomachEmpty = 1f - metabolism.StomachKg / metabolism.StomachCapacityKg;
            float fatEmpty = 1f - metabolism.FatKg / metabolism.MaxFatKg;
            return MathF.Max(stomachEmpty, fatEmpty);
        }
    }
}
