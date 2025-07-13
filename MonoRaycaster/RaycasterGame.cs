using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace MonoRaycaster;

public class RaycasterGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int ScreenWidth = 2048;//1024; 
    private const int ScreenHeight = 1536;//768;

    private readonly static Vector2 _halfScreenSize = new(ScreenWidth / 2, ScreenHeight / 2);

    // inverted, the raycaster is rendering data rotated 90 degrees
    private const int FrameBufferWidth = ScreenHeight;
    private const int FrameBufferHeight = ScreenWidth;
    private readonly static Vector2 _halfFrameBufferSize = new(FrameBufferWidth / 2, FrameBufferHeight / 2);

    private int[][] _map = [
      [ 8,8,8,8,8,8,8,8,8,8,8,4,4,6,4,4,6,4,6,4,4,4,6,4],
      [ 8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,0,0,0,0,0,0,4],
      [ 8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,6],
      [ 8,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6],
      [ 8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,4],
      [ 8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,6,6,6,0,6,4,6],
      [ 8,8,8,8,0,8,8,8,8,8,8,4,4,4,4,4,4,6,0,0,0,0,0,6],
      [ 7,7,7,7,0,7,7,7,7,0,8,0,8,0,8,0,8,4,0,4,0,6,0,6],
      [ 7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,0,0,0,0,0,6],
      [ 7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,0,0,0,0,4],
      [ 7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,6,0,6,0,6],
      [ 7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,4,6,0,6,6,6],
      [ 7,7,7,7,0,7,7,7,7,8,8,4,0,6,8,4,8,3,3,3,0,3,3,3],
      [ 2,2,2,2,0,2,2,2,2,4,6,4,0,0,6,0,6,3,0,0,0,0,0,3],
      [ 2,2,0,0,0,0,0,2,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3],
      [ 2,0,0,0,0,0,0,0,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3],
      [ 1,0,0,0,0,0,0,0,1,4,4,4,4,4,6,0,6,3,3,0,0,0,3,3],
      [ 2,0,0,0,0,0,0,0,2,2,2,1,2,2,2,6,6,0,0,5,0,5,0,5],
      [ 2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5],
      [ 2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5],
      [ 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5],
      [ 2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5],
      [ 2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5],
      [ 2,2,2,2,1,2,2,2,2,2,2,1,2,2,2,5,5,5,5,5,5,5,5,5]
    ];

    private readonly FrameCounter _frameCounter = new();
    private Camera _camera;

    private Texture2D _frameTexture;
    private Raycaster _raycaster;

    private SpriteFont _font;

    public RaycasterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _frameTexture = new Texture2D(GraphicsDevice, FrameBufferWidth, FrameBufferHeight);

        _camera = new(_map);

        var mainTexture = Content.Load<Texture2D>("wolftextures");
        var textures = mainTexture.Split(64, 64).Select(t => t.Rotate90(RotationDirection.CounterClockwise)).ToArray();
        _raycaster = new TexturedRaycaster(_map, FrameBufferWidth, FrameBufferHeight, textures);

        _font = Content.Load<SpriteFont>("Font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _camera.Update(gameTime);

        _raycaster.Update(_camera);
        _frameTexture.SetData(_raycaster.FrameBuffer);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(
            _frameTexture,
            position: _halfScreenSize, 
            sourceRectangle: null,
            color: Color.White, 
            rotation: MathHelper.PiOver2, 
            origin: _halfFrameBufferSize,
            scale: 1f,
            effects: SpriteEffects.None,
            layerDepth: 0);
        _spriteBatch.End();

        var text = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, text, Vector2.Zero, Color.White,
                               0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
