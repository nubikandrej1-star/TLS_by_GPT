using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TLS.GameObjects;

namespace The_Last_Signal
{
    public class GameTLS : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = default!;
        private Texture2D _pixel = default!;
        private Song _theme = default!;
        private Player _player = default!;
        private StationRoom[] _rooms = Array.Empty<StationRoom>();
        private int _currentRoom;
        private KeyboardState _previousKeyboard;
        private bool _powerRestored;
        private bool _signalHeard;

        public GameTLS()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 640,
                PreferredBackBufferHeight = 360,
                IsFullScreen = true
            };
            _graphics.ApplyChanges();
            Window.Title = "The Last Signal";
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _player = Player.CreatePlayer(Content.Load<Texture2D>("Textures/PlayerAtlas"));
            _theme = Content.Load<Song>("Songs/Alone");

            _rooms = new[]
            {
                new StationRoom("Station Entrance", _pixel),
                new StationRoom("Main Corridor", _pixel),
                new StationRoom("Generator Room", _pixel),
                new StationRoom("Control Room", _pixel),
                new StationRoom("Radio Room", _pixel)
            };

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.4f;
            MediaPlayer.Play(_theme);

            _currentRoom = 0;
            _player.TeleportTo(new Vector2(520, 180));
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Escape)) Exit();

            StationRoom room = _rooms[_currentRoom];
            room.SetPower(_powerRestored);
            _player.Update(gameTime, keyboard, room);

            if (Pressed(keyboard, Keys.E))
                Interact(room);

            _previousKeyboard = keyboard;
            base.Update(gameTime);
        }

        private bool Pressed(KeyboardState state, Keys key)
        {
            return state.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
        }

        private void Interact(StationRoom room)
        {
            RoomObject? target = room.GetInteractable(_player.Bounds);
            if (target == null) return;

            if (target.Type == RoomObjectType.Generator && !_powerRestored)
            {
                _powerRestored = true;
                foreach (StationRoom stationRoom in _rooms)
                    stationRoom.SetPower(true);
                Window.Title = "The Last Signal | POWER RESTORED";
                return;
            }

            if (target.Type == RoomObjectType.Radio)
            {
                _signalHeard = true;
                Window.Title = "The Last Signal | SIGNAL DETECTED";
                MediaPlayer.Volume = 0.65f;
                return;
            }

            if (target.Type == RoomObjectType.Door)
            {
                if (target.RequiresPower && !_powerRestored) return;
                _currentRoom = target.TargetRoom;
                _player.TeleportTo(target.TargetPosition);
                Window.Title = "The Last Signal | " + _rooms[_currentRoom].Name;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(8, 10, 14));
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _rooms[_currentRoom].Draw(_spriteBatch);
            _player.Draw(_spriteBatch);

            if (_signalHeard)
                _spriteBatch.Draw(_pixel, new Rectangle(280, 175, 80, 2), Color.White * 0.55f);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
