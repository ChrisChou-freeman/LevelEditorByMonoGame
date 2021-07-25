using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ActionGameExample
{
    // game music use The Chain 2:49
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static Vector2 originalScreenSize = new Vector2(800, 480);
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Vector2 _setScreenSize;
        private Matrix _globalTransformation;
        private int _backbufferWidth;
        private int _backbufferHeight;
        private FrameCounter _frameCounter;
        private LevelEditor _levelEditor;


        public Game()
        {
            this._graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = false;
            this._setScreenSize = new Vector2(1280, 720);
            this._levelEditor = new LevelEditor(this.Services);
        }

        private void InitialScreenSize()
        {
            this._graphics.IsFullScreen = false;
            this._graphics.PreferredBackBufferWidth = (int)this._setScreenSize.X;
            this._graphics.PreferredBackBufferHeight = (int)this._setScreenSize.Y;
            this._graphics.ApplyChanges();
            this._levelEditor.Initialize(this._graphics.GraphicsDevice);
        }

        protected override void Initialize()
        {
            this.InitialScreenSize();
            base.Initialize();
        }

        public void ScalePresentationArea()
        {
            //Work out how much we need to scale our graphics to fill the screen
            this._backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            this._backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = this._backbufferWidth / originalScreenSize.X;
            float verScaling = this._backbufferHeight / originalScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            this._globalTransformation = Matrix.CreateScale(screenScalingFactor);
        }

        protected override void LoadContent()
        {
            this._spriteBatch = new SpriteBatch(GraphicsDevice);
            var uiFount = this.Content.Load<SpriteFont>("Font/UI");
            this._frameCounter = new FrameCounter(uiFount);
            this._levelEditor.LoadContent();
            this.ScalePresentationArea();
        }

        protected override void Update(GameTime gameTime)
        {
            // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //     Exit();
            this._frameCounter.Update(gameTime);
            this._levelEditor.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            this._spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null,null, this._globalTransformation);
            this._levelEditor.Draw(this._spriteBatch);
            this._frameCounter.Draw(this._spriteBatch);
            this._spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
