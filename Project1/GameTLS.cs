using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TLS;
using TLS.GameObjects;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace The_Last_Signal
{
    public class GameTLS : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = default!;

        private TextureAtlas _textureAtlas = default!;
        public static Room[] rooms = new Room[2];
        public static uint currentRoom = 0;

        private Song _theme = default!;

        private Player _player = default!;

        private static Logger _log = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.File(
                "logs/app-.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        public GameTLS()
        {
            _graphics = new GraphicsDeviceManager(this);
            //640 x 360 || 320 x 180
            _graphics.PreferredBackBufferWidth = 640;
            _graphics.PreferredBackBufferHeight = 360;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            Window.Title = "The Last Signal";

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic her
            try
            {
                _textureAtlas = new TextureAtlas(Content.Load<Texture2D>("Textures/RadioAtlas"));
                _player = Player.CreatePlayer(Content.Load<Texture2D>("Textures/PlayerAtlas"));
                _theme = Content.Load<Song>("Songs/Alone");
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.4f;
            }
            catch (Exception e)
            {
                _log.Debug(e.Message + "\n");
                throw;
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                List<int> numbers = new List<int> { 1, 2, 3 };
                _spriteBatch = new SpriteBatch(GraphicsDevice);

                // TODO: use this.Content to load your game content here
                _textureAtlas.LoadFromFile("TestAtlas");
                rooms[0] = new Room(Content);
                rooms[1] = new Room(Content);
                /*
                _textureAtlas.AddRegion("radio-1", 0, 0, 32, 15);
                _textureAtlas.AddRegion("radio-2", 32, 0, 32, 15);
                _textureAtlas.AddRegion("radio-3", 0, 16, 32, 15);
                _textureAtlas.AddRegion("radio-4", 32, 16, 32, 15);

                TextureRegion[] framesArr = { 
                    _textureAtlas.GetRegion("radio-1"), _textureAtlas.GetRegion("radio-2"),
                    _textureAtlas.GetRegion("radio-3"), _textureAtlas.GetRegion("radio-4")
                };
                _textureAtlas.AddAnimation("radioAnimation", new Animation(framesArr,250));
                */
                if (File.Exists("TestAtlas"))
                {
                    rooms[currentRoom].AddHitbox(_textureAtlas.CreateAnimatedSprite("radioAnimation"), 1);
                    //_textureAtlas.ToFile("TestAtlas");
                }
                else
                {
                    using (File.Create("TestAtlas")) { }
                    rooms[currentRoom].AddHitbox(_textureAtlas.CreateAnimatedSprite("radioAnimation"), 1);
                }
                rooms[currentRoom].GetHitbox(0).SetAnimation(0);
                rooms[currentRoom].GetHitbox(0).Scale = new Vector2(3.0f);
                rooms[currentRoom].GetHitbox(0).CorrectHytboxByScale();
                rooms[currentRoom].GetHitbox(0).Position = new Vector2(Window.ClientBounds.Width,Window.ClientBounds.Height) * 0.5f;
                rooms[currentRoom].GetHitbox(0).HitboxType = HitboxType.StopWalking;

                MediaPlayer.Play(_theme);
            }
            catch(Exception e)
            {
                _log.Debug(e.Message+"\n");
                throw;
            }
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // TODO: Add your update logic here
            rooms[currentRoom].GetHitbox(0).Update(gameTime);
            _player.Update(gameTime, Keyboard.GetState());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.Clear(Color.Black);

                // TODO: Add your drawing code here
                _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack);

                rooms[currentRoom].Draw(_spriteBatch);
                _player.Draw(_spriteBatch);

                _spriteBatch.End();

                base.Draw(gameTime);
            }
            catch (Exception e)
            {
                _log.Debug(e.Message + "\n");
                throw;
            }
        }
    }
}
