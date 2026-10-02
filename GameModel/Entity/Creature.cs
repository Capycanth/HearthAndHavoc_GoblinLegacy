using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public abstract class Creature : GameObject
    {
        public int Id { get; }
        public byte Size { get; set; }
        public Locale Locale { get; }
        [AllowNull]
        public ActionChain CurrentChain { get; set; }
        public CreatureActivity Activity { get; set; } = CreatureActivity.RESTING;

        protected Creature(int id, byte size, Locale locale, Texture2D texture) : base(texture)
        {
            Id = id;
            Size = size;
            Locale = locale;
        }

        public override void Update()
        {
            CurrentChain ??= ChooseChain();

            ChainStatus status = CurrentChain.Perform(this);
            if (status != ChainStatus.RUNNING)
            {
                Console.WriteLine($"Creature {Id} finished {CurrentChain.GetType().Name}: {status}");
                CurrentChain = null;
            }
        }

        protected abstract ActionChain ChooseChain();

        public abstract float GetMoveSpeed(CreatureActivity activity);
    }
}
