using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Utility;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace HearthAndHavoc_GoblinLegacy
{
    public class GoblinGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game Config
        private readonly int gameTickMs = 200;
        private readonly int chunkMarginTiles = 32;

        // Game Settings
        private readonly float[] gameSpeeds = [1f, 2f, 5f];
        private int gameSpeedIndex = 0;
        private bool isPaused = false;

        // Dynamic Variables
        private double timeSinceLastTickMs = 0;
        private KeyboardState currKeyBoardState = new();
        private KeyboardState prevKeyBoardState = new();
        private MouseState currMouseState = new();
        private MouseState prevMouseState = new();

        // Game Objects
        public static World world;
        private readonly Camera camera = new();

        // ActionProcessor
        public static ProcessorThread Processor { get; private set; } = null!;

        public GoblinGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            _graphics.ApplyChanges();

            // Create the static worker thread once
            Processor = new ProcessorThread("AI Processor");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Initialize(Content);
            DefRegistry.Load();
            world = Initializer.CreateTestWorld(5);
        }

        protected override void Update(GameTime gameTime)
        {
            prevKeyBoardState = currKeyBoardState;
            currKeyBoardState = Keyboard.GetState();

            if (currKeyBoardState.IsKeyDown(Keys.Escape))
                Exit();
            if (currKeyBoardState.IsKeyDown(Keys.RightControl) && prevKeyBoardState.IsKeyUp(Keys.RightControl) && gameSpeedIndex < gameSpeeds.Length - 1)
            {
                gameSpeedIndex++;
                Debug.WriteLine($"Game Speed set to X{gameSpeeds[gameSpeedIndex]}");
            }
            if (currKeyBoardState.IsKeyDown(Keys.LeftControl) && prevKeyBoardState.IsKeyUp(Keys.LeftControl) && gameSpeedIndex > 0)
            {
                gameSpeedIndex--;
                Debug.WriteLine($"Game Speed set to X{gameSpeeds[gameSpeedIndex]}");
            }
            if (currKeyBoardState.IsKeyDown(Keys.P) && prevKeyBoardState.IsKeyUp(Keys.P))
            {
                isPaused = !isPaused;
                Debug.WriteLine(isPaused ? "Game Paused" : "Game Unpaused");
            }
            if (currKeyBoardState.IsKeyDown(Keys.Space) && prevKeyBoardState.IsKeyUp(Keys.Space))
                _graphics.ToggleFullScreen();

            // Camera runs every frame, before the tick check, so it stays smooth and works while paused
            prevMouseState = currMouseState;
            currMouseState = Mouse.GetState();
            if (IsActive)
                camera.Update(currMouseState, prevMouseState);

            Rectangle chunkArea = camera.GetVisibleTiles(GraphicsDevice.Viewport);
            chunkArea.Inflate(chunkMarginTiles, chunkMarginTiles);
            world.GetCurrentLocale()?.LocaleMap.EnsureChunks(chunkArea);

            if (!isPaused)
                timeSinceLastTickMs += gameTime.ElapsedGameTime.TotalMilliseconds * gameSpeeds[gameSpeedIndex];
            if (timeSinceLastTickMs < gameTickMs) return;
            else 
            {
                Debug.WriteLine($"Reached world update in {timeSinceLastTickMs} ms");
                timeSinceLastTickMs -= gameTickMs;
            }
            
            world.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            world.Draw(_spriteBatch, camera.GetVisibleTiles(GraphicsDevice.Viewport));
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
