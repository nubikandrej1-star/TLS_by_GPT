using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLS.GameObjects
{
    public sealed class StationRoom
    {
        private readonly List<RoomObject> _objects = new();
        private readonly Texture2D _pixel;

        public string Name { get; }
        public IReadOnlyList<RoomObject> Objects => _objects;
        public bool PowerRestored { get; private set; }

        public StationRoom(string name, Texture2D pixel)
        {
            Name = name;
            _pixel = pixel;
            Build();
        }

        public void SetPower(bool restored) => PowerRestored = restored;

        public bool Intersects(Rectangle rectangle)
        {
            foreach (RoomObject obj in _objects)
                if (obj.BlocksMovement && obj.Bounds.Intersects(rectangle))
                    return true;
            return false;
        }

        public RoomObject? GetInteractable(Rectangle playerBounds)
        {
            Rectangle area = playerBounds;
            area.Inflate(18, 18);
            foreach (RoomObject obj in _objects)
            {
                if (!area.Intersects(obj.Bounds)) continue;
                if (obj.Type == RoomObjectType.Generator && !PowerRestored) return obj;
                if (obj.Type == RoomObjectType.Radio) return obj;
                if (obj.Type == RoomObjectType.Door && (!obj.RequiresPower || PowerRestored)) return obj;
            }
            return null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (RoomObject obj in _objects)
            {
                Color color = obj.Color;
                if (obj.Type == RoomObjectType.Door && obj.RequiresPower && !PowerRestored) color = Color.DarkRed;
                else if (obj.Type == RoomObjectType.Generator && PowerRestored) color = Color.LightGreen;
                spriteBatch.Draw(_pixel, obj.Bounds, color);
            }
        }

        private void Build()
        {
            _objects.Clear();
            _objects.Add(new RoomObject(RoomObjectType.Floor, new Rectangle(0, 0, 640, 360), new Color(28, 31, 38)));
            const int wall = 12;
            _objects.Add(new RoomObject(RoomObjectType.Wall, new Rectangle(0, 0, 640, wall), new Color(55, 58, 66), true));
            _objects.Add(new RoomObject(RoomObjectType.Wall, new Rectangle(0, 348, 640, wall), new Color(55, 58, 66), true));
            _objects.Add(new RoomObject(RoomObjectType.Wall, new Rectangle(0, 0, wall, 360), new Color(55, 58, 66), true));
            _objects.Add(new RoomObject(RoomObjectType.Wall, new Rectangle(628, 0, wall, 360), new Color(55, 58, 66), true));

            if (Name == "Station Entrance") BuildEntrance();
            else if (Name == "Main Corridor") BuildCorridor();
            else if (Name == "Generator Room") BuildGenerator();
            else if (Name == "Control Room") BuildControl();
            else if (Name == "Radio Room") BuildRadio();
        }

        private void BuildEntrance()
        {
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(80, 75, 130, 22), new Color(70, 72, 78)));
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(80, 250, 150, 20), new Color(65, 67, 72)));
            AddDoor(new Rectangle(600, 145, 28, 70), 1, new Vector2(35, 180), false);
        }

        private void BuildCorridor()
        {
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(80, 80, 480, 18), new Color(48, 51, 58)));
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(80, 260, 480, 18), new Color(48, 51, 58)));
            AddDoor(new Rectangle(0, 145, 28, 70), 0, new Vector2(585, 180), false);
            AddDoor(new Rectangle(300, 12, 70, 28), 2, new Vector2(335, 315), false);
            AddDoor(new Rectangle(470, 332, 70, 28), 3, new Vector2(505, 45), true);
        }

        private void BuildGenerator()
        {
            _objects.Add(new RoomObject(RoomObjectType.Generator, new Rectangle(250, 130, 140, 110), new Color(65, 70, 76), true, false, -1, default, "Press E to start the generator"));
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(100, 80, 100, 30), new Color(50, 53, 58)));
            AddDoor(new Rectangle(300, 332, 70, 28), 1, new Vector2(335, 45), false);
        }

        private void BuildControl()
        {
            _objects.Add(new RoomObject(RoomObjectType.Terminal, new Rectangle(250, 95, 120, 35), new Color(40, 55, 60), true, true));
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(100, 210, 160, 30), new Color(55, 58, 63), true));
            AddDoor(new Rectangle(470, 145, 28, 70), 4, new Vector2(45, 180), true);
            AddDoor(new Rectangle(470, 0, 70, 28), 1, new Vector2(505, 310), false);
        }

        private void BuildRadio()
        {
            _objects.Add(new RoomObject(RoomObjectType.Radio, new Rectangle(285, 120, 70, 45), new Color(75, 82, 86), true, true, -1, default, "Press E to listen to the radio"));
            _objects.Add(new RoomObject(RoomObjectType.Decoration, new Rectangle(170, 240, 300, 25), new Color(55, 58, 63), true));
            AddDoor(new Rectangle(0, 145, 28, 70), 3, new Vector2(450, 180), true);
        }

        private void AddDoor(Rectangle bounds, int targetRoom, Vector2 targetPosition, bool requiresPower)
        {
            _objects.Add(new RoomObject(RoomObjectType.Door, bounds, new Color(78, 78, 84), true, requiresPower, targetRoom, targetPosition, "Press E to open the door"));
        }
    }
}
