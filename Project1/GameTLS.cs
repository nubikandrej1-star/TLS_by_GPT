using System;
using System.IO;
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
        private SpriteFont _hudFont = default!;
        private Song _theme = default!;
        private Player _player = default!;
        private StationRoom[] _rooms = Array.Empty<StationRoom>();
        private int _currentRoom;
        private KeyboardState _previousKeyboard;
        private bool _powerRestored;
        private bool _terminalRead;
        private bool _signalHeard;
        private bool _gameCompleted;
        private string _message = "You are alone. Find out why the station went silent.";
        private float _messageTime = 8f;
        private string _savePath = string.Empty;

        public GameTLS()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 960,
                PreferredBackBufferHeight = 540,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true
            };
            Window.Title = "The Last Signal";
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _hudFont = Content.Load<SpriteFont>("Fonts/Hud");
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

            string saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TheLastSignal");
            Directory.CreateDirectory(saveDirectory);
            _savePath = Path.Combine(saveDirectory, "save.dat");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.25f;
            MediaPlayer.Play(_theme);

            _currentRoom = 0;
            _player.TeleportTo(new Vector2(520, 180));
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            if (_messageTime > 0)
                _messageTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Pressed(keyboard, Keys.F5))
                SaveGame();
            if (Pressed(keyboard, Keys.F9))
                LoadGame();

            StationRoom room = _rooms[_currentRoom];
            room.SetPower(_powerRestored);
            _player.Update(gameTime, keyboard, room);

            if (Pressed(keyboard, Keys.E))
                Interact(room);

            _previousKeyboard = keyboard;
            base.Update(gameTime);
        }

        private bool Pressed(KeyboardState state, Keys key) =>
            state.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);

        private void SetMessage(string message, float seconds = 5f)
        {
            _message = message;
            _messageTime = seconds;
        }

        private void Interact(StationRoom room)
        {
            RoomObject? target = room.GetInteractable(_player.Bounds);
            if (target == null)
            {
                SetMessage("Nothing within reach.");
                return;
            }

            switch (target.Type)
            {
                case RoomObjectType.Generator:
                    if (!_powerRestored)
                    {
                        _powerRestored = true;
                        foreach (StationRoom stationRoom in _rooms)
                            stationRoom.SetPower(true);
                        SetMessage("The generator coughs back to life. Somewhere, a relay clicks.");
                        MediaPlayer.Volume = 0.38f;
                    }
                    break;

                case RoomObjectType.Terminal:
                    if (!_powerRestored)
                    {
                        SetMessage("The terminal has no power.");
                        break;
                    }

                    if (!_terminalRead)
                    {
                        _terminalRead = true;
                        SetMessage("LOG 04: Evacuation cancelled. The radio room received a signal AFTER the last broadcast ended.", 8f);
                    }
                    else
                    {
                        SetMessage("LOG 04 remains on screen. No personnel records follow it.");
                    }
                    break;

                case RoomObjectType.Radio:
                    if (!_powerRestored)
                    {
                        SetMessage("The radio is dead. Restore power first.");
                        break;
                    }
                    if (!_terminalRead)
                    {
                        SetMessage("The receiver is locked to a channel marked in the control-room log. Check the terminal.");
                        break;
                    }
                    if (!_signalHeard)
                    {
                        _signalHeard = true;
                        SetMessage("SIGNAL FOUND: "...if anyone can hear this, do not come outside..."", 9f);
                        MediaPlayer.Volume = 0.5f;
                    }
                    else if (!_gameCompleted)
                    {
                        _gameCompleted = true;
                        SetMessage("END OF CHAPTER ONE. The signal repeats your own voice, recorded tomorrow.", 10f);
                    }
                    else
                    {
                        SetMessage("The signal keeps repeating. It never answers questions.");
                    }
                    break;

                case RoomObjectType.Door:
                    if (target.RequiresPower && !_powerRestored)
                    {
                        SetMessage("The magnetic lock has no power.");
                        break;
                    }

                    _currentRoom = target.TargetRoom;
                    _player.TeleportTo(target.TargetPosition);
                    SetMessage(_rooms[_currentRoom].Name, 2.5f);
                    break;
            }
        }

        private void SaveGame()
        {
            try
            {
                File.WriteAllLines(_savePath, new[]
                {
                    "TLS_SAVE_1",
                    _currentRoom.ToString(),
                    _player.Bounds.X.ToString(),
                    _player.Bounds.Y.ToString(),
                    _powerRestored.ToString(),
                    _terminalRead.ToString(),
                    _signalHeard.ToString(),
                    _gameCompleted.ToString()
                });
                SetMessage("Game saved. F9 loads this checkpoint.");
            }
            catch (Exception)
            {
                SetMessage("Could not save the game.");
            }
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    SetMessage("No save found. Press F5 to create one.");
                    return;
                }

                string[] data = File.ReadAllLines(_savePath);
                if (data.Length < 8 || data[0] != "TLS_SAVE_1")
                {
                    SetMessage("Save file is invalid.");
                    return;
                }

                int room = int.Parse(data[1]);
                if (room < 0 || room >= _rooms.Length)
                {
                    SetMessage("Save file contains an invalid room.");
                    return;
                }

                _currentRoom = room;
                _player.TeleportTo(new Vector2(int.Parse(data[2]), int.Parse(data[3])));
                _powerRestored = bool.Parse(data[4]);
                _terminalRead = bool.Parse(data[5]);
                _signalHeard = bool.Parse(data[6]);
                _gameCompleted = bool.Parse(data[7]);

                foreach (StationRoom stationRoom in _rooms)
                    stationRoom.SetPower(_powerRestored);

                SetMessage("Checkpoint loaded.");
            }
            catch (Exception)
            {
                SetMessage("Could not load the save file.");
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(7, 9, 13));
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _rooms[_currentRoom].Draw(_spriteBatch);
            _player.Draw(_spriteBatch);

            DrawPanel(new Rectangle(0, 0, 960, 72), new Color(7, 10, 15, 225));
            _spriteBatch.DrawString(_hudFont, "THE LAST SIGNAL", new Vector2(18, 9), new Color(180, 205, 213));
            _spriteBatch.DrawString(_hudFont, _rooms[_currentRoom].Name, new Vector2(18, 31), Color.White);

            string objective = _gameCompleted
                ? "Objective: Chapter complete"
                : !_powerRestored
                    ? "Objective: Restore power in the Generator Room"
                    : !_terminalRead
                        ? "Objective: Read the terminal in the Control Room"
                        : !_signalHeard
                            ? "Objective: Tune the radio in the Radio Room"
                            : "Objective: Listen to the signal again";
            _spriteBatch.DrawString(_hudFont, objective, new Vector2(390, 12), new Color(190, 202, 190));

            string status = "POWER " + (_powerRestored ? "ONLINE" : "OFFLINE")
                + "   LOG " + (_terminalRead ? "READ" : "UNREAD")
                + "   SIGNAL " + (_signalHeard ? "FOUND" : "UNKNOWN");
            _spriteBatch.DrawString(_hudFont, status, new Vector2(390, 34), new Color(135, 151, 161));

            if (_messageTime > 0)
            {
                DrawPanel(new Rectangle(24, 438, 912, 72), new Color(7, 10, 15, 225));
                _spriteBatch.DrawString(_hudFont, _message, new Vector2(42, 452), Color.White);
            }

            DrawPanel(new Rectangle(0, 516, 960, 24), new Color(7, 10, 15, 220));
            _spriteBatch.DrawString(_hudFont, "WASD Move   SHIFT Run   E Interact   F5 Save   F9 Load   ESC Quit",
                new Vector2(18, 519), new Color(145, 155, 165));

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawPanel(Rectangle bounds, Color color) =>
            _spriteBatch.Draw(_pixel, bounds, color);
    }
}
