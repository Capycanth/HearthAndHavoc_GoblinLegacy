using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class Locale
    {
        public string Id { get; private set; }
        public List<Kremlit> Kremlits { get; private set; }
        public MeterTile[,] LocaleMap { get; private set; }

        public Locale(string id, List<Kremlit> kremlits, MeterTile[,] localeMap) 
        {
            Id = id;
            Kremlits = kremlits;
            LocaleMap = localeMap;
        }

        public void Update(string id)
        {
            if (id == Id) FullUpdate();
            else PartialUpdate();
            
        }

        private void FullUpdate()
        {
            //foreach (Kremlit kremlit in Kremlits)
            //{
            //    LocaleMap[kremlit.Position.Y][kremlit.Position.X].Impassible = true;
            //}
            foreach (Kremlit kremlit in Kremlits)
            {
                kremlit.Update();
            }
        }

        private void PartialUpdate()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (MeterTile tile in LocaleMap)
            {
                tile.Draw(spriteBatch);
            }

            foreach (Kremlit kremlit in Kremlits)
            {
                kremlit.Draw(spriteBatch);
            }
        }
    }
}
