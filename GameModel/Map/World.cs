using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class World
    {
        public string CurrentLocaleId { get; set; }
        public Dictionary<string, Locale> LocalesById { get; private set; }

        public World() 
        { 
            LocalesById = [];
        }

        public void Update()
        {
            foreach (Locale locale in LocalesById.Values)
            {
                locale.Update(CurrentLocaleId);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            GetCurrentLocale()?.Draw(spriteBatch);
        }

        public Locale GetCurrentLocale()
        {
            if (LocalesById.TryGetValue(CurrentLocaleId, out Locale currentLocale)) return currentLocale;
            else return null;
        }
    }
}
