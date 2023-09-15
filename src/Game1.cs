using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace game;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private VehicleParams vparams;
    private Engine engine;
    private Vehicle vehicle;
    private Player player;

    private SpriteFont font;
    private Texture2D cursor;
    private Texture2D vtex;
    private Texture2D prototype;

    // audio

    private FMOD.Studio.System system;
    private FMOD.System core_system;

    private FMOD.Studio.Bank master_bank;
    private FMOD.Studio.Bank strings_bank;
    private FMOD.Studio.Bank vehicles_bank;

    private FMOD.Studio.EventDescription event_description;
    private FMOD.Studio.EventInstance event_instance;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        vparams = new VehicleParams(1100.0f, 0.45f, 1.0f, 1.0f, 0.34f, 3.72f);
        engine = new Engine(84.0f, 90.0f, 9500.0f);
        vehicle = new Vehicle(vparams, engine);
        player = new Player(vehicle);

        base.Initialize();

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Fonts/Font");
        cursor = Content.Load<Texture2D>("Sprites/Cursor");
        vtex = Content.Load<Texture2D>("Sprites/Car");
        prototype = Content.Load<Texture2D>("Sprites/texture_01");
        Mouse.SetCursor(MouseCursor.FromTexture2D(cursor, cursor.Width/2, cursor.Height/2));
        // TODO: use this.Content to load your game content here

        FMOD.Studio.System.create(out system);
        system.getCoreSystem(out core_system);
        core_system.setSoftwareFormat(
            0, FMOD.SPEAKERMODE._5POINT1, 0);
        system.initialize(
            1024,
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            IntPtr.Zero);

        system.loadBankFile(
            "Content/Audio/Master.bank",
            FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
            out master_bank
        );

        system.loadBankFile(
            "Content/Audio/Master.strings.bank",
            FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
            out strings_bank
        );

        system.loadBankFile(
            "Content/Audio/Vehicles.bank",
            FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
            out vehicles_bank
        );

        system.getEvent("event:/Vehicles/Car Engine", out event_description);
        event_description.createInstance(out event_instance);
        event_instance.setParameterByName("RPM", 0.0f);
        event_instance.start();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        player.Update(gameTime);
        event_instance.setParameterByName("RPM", player.rpm);
        base.Update(gameTime);
        system.update();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        
        _spriteBatch.Begin(transformMatrix: player.CameraMatrix);

        _spriteBatch.Draw(prototype, new Vector2(0, 0), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(0, prototype.Height), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(prototype.Width, 0), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(prototype.Width, prototype.Height), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(0, -prototype.Height), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(-prototype.Width, 0), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(-prototype.Width, -prototype.Height), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(prototype.Width, -prototype.Height), Color.White);
        _spriteBatch.Draw(prototype, new Vector2(-prototype.Width, prototype.Height), Color.White);
        
        _spriteBatch.Draw(
            vtex, 
            player.position, 
            null,
            Color.White,
            player.heading,
            new Vector2(vtex.Width/2, 0),
            1.0f,
            SpriteEffects.None,
            0.0f
        );
        _spriteBatch.End();

        _spriteBatch.Begin();
        _spriteBatch.DrawString(
            font, "Speed: " + player.speed + " kmh, RPM: " + player.rpm + ", Angle: " + player.heading, new Vector2(0, 0), Color.White
        );
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
