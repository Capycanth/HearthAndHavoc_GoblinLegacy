using HearthAndHavoc_GoblinLegacy.GameModel.Items;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Kremlit : Creature
    {
        private const byte KremlitSize = 30;

        public List<BaseItem> Inventory { get; set; }
        public Dictionary<NeedType, float> Needs { get; set; }

        public Kremlit(int id, Locale locale, Texture2D texture) : base(id, KremlitSize, locale, texture)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Color.White);
        }

        protected override ActionChain ChooseChain()
        {
            return new IdleChain();
        }

        public override float GetMoveSpeed(CreatureActivity activity)
        {
            return 1f;
        }
    }
}
