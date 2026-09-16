using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TLS.GameObjects
{
    public enum RoomObjectType
    {
        Wall,
        Floor,
        Decoration,
        Door,
        Generator,
        Radio,
        Terminal
    }

    public sealed class RoomObject
    {
        public RoomObjectType Type { get; }
        public Rectangle Bounds { get; }
        public Color Color { get; }
        public bool BlocksMovement { get; set; }
        public bool RequiresPower { get; }
        public int TargetRoom { get; }
        public Vector2 TargetPosition { get; }
        public string InteractionText { get; }

        public RoomObject(
            RoomObjectType type,
            Rectangle bounds,
            Color color,
            bool blocksMovement = false,
            bool requiresPower = false,
            int targetRoom = -1,
            Vector2 targetPosition = default,
            string interactionText = "")
        {
            Type = type;
            Bounds = bounds;
            Color = color;
            BlocksMovement = blocksMovement;
            RequiresPower = requiresPower;
            TargetRoom = targetRoom;
            TargetPosition = targetPosition;
            InteractionText = interactionText;
        }
    }
}
