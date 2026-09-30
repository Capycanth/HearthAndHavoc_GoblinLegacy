using HearthAndHavoc_GoblinLegacy.AI.Action;
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
        public BaseAction CurrentAction { get; set; }
        public Activity Activity { get; set; } = Activity.RESTING;

        protected Creature(int id, byte size, Locale locale, Texture2D texture) : base(texture)
        {
            Id = id;
            Size = size;
            Locale = locale;
        }

        public override void Update()
        {
            CurrentAction ??= ChooseAction();

            if (CurrentAction.Perform(this))
            {
                Console.WriteLine($"Creature {Id} finished {CurrentAction.GetType().Name}");
                CurrentAction = null;
            }
        }

        protected abstract BaseAction ChooseAction();

        public abstract float GetMoveSpeed(Activity activity);
    }
}
