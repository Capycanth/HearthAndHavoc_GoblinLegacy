using Microsoft.Xna.Framework.Graphics;

namespace HeartAndHavoc_GoblinLegacy.GameModel.Items
{
    public class SingleItem : BaseItem
    {
        public float Condition { get; set; }

        public SingleItem(Texture2D texture, string flavor) : base(texture, flavor)
        {
        }

        public override void Update()
        {
            
        }
    }
}
