using HeartAndHavoc_GoblinLegacy.Utility;
using HearthAndHavocGoblinLegacy.GameModel.Map;
using HearthAndHavocGoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HearthAndHavoc_GoblinLegacy
{
    public class GoblinGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game Config
        private readonly int gameTickMs = 200;

        // Game Settings
        private float gameSpeed = 1f;

        // Dynamic Variables
        private int timeSinceLastTickMs = 0;
        private KeyboardState currKeyBoardState = new();
        private KeyboardState prevKeyBoardState = new();

        // Game Objects
        public static World world;

        public GoblinGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 2560;
            _graphics.PreferredBackBufferHeight = 1440;

            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Initialize(Content);
            world = Initializer.CreateTestWorld(100);
        }

        protected override void Update(GameTime gameTime)
        {
            prevKeyBoardState = currKeyBoardState;
            currKeyBoardState = Keyboard.GetState();

            if (currKeyBoardState.IsKeyDown(Keys.Escape))
                Exit();
            if (currKeyBoardState.IsKeyDown(Keys.RightControl) && prevKeyBoardState.IsKeyUp(Keys.RightControl) && gameSpeed < 5)
                gameSpeed += 0.25f;
            if (currKeyBoardState.IsKeyDown(Keys.LeftControl) && prevKeyBoardState.IsKeyUp(Keys.LeftControl) && gameSpeed != 0)
                gameSpeed -= 0.25f;
            if (currKeyBoardState.IsKeyDown(Keys.Space) && prevKeyBoardState.IsKeyUp(Keys.Space))
                _graphics.ToggleFullScreen();

            timeSinceLastTickMs += gameTime.ElapsedGameTime.Milliseconds;
            if ((timeSinceLastTickMs * gameSpeed) < gameTickMs) return;
            else timeSinceLastTickMs = 0;

            world.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            world.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
