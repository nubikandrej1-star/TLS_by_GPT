using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TLS.GameObjects
{
    public class Player
    {
        private static Player? _elum;
        private readonly AnimatedCollisionObject _collisionSprite;

        private Player(Texture2D texture2D)
        {
            _collisionSprite = new AnimatedCollisionObject(texture2D, new Rectangle(1, 0, 22, 64));
        }

        public static Player CreatePlayer(Texture2D texture2D)
        {
            if (_elum == null)
            {
                _elum = new Player(texture2D);
                _elum._collisionSprite.Position = new Vector2(102, 275);
            }
            return _elum;
        }

        public static Player? GetPlayer() => _elum;

        public Rectangle Bounds => _collisionSprite.Hitbox;

        public void Draw(SpriteBatch spriteBatch) => _collisionSprite.Draw(spriteBatch);

        public void Update(GameTime gameTime, KeyboardState state, StationRoom room)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float speed = state.IsKeyDown(Keys.LeftShift) ? 250f : 120f;
            Vector2 direction = Vector2.Zero;

            if (state.IsKeyDown(Keys.D))
            {
                direction.X++;
                _collisionSprite.Effects = SpriteEffects.None;
            }
            if (state.IsKeyDown(Keys.A))
            {
                direction.X--;
                _collisionSprite.Effects = SpriteEffects.FlipHorizontally;
            }
            if (state.IsKeyDown(Keys.S)) direction.Y++;
            if (state.IsKeyDown(Keys.W)) direction.Y--;

            if (direction == Vector2.Zero)
                return;

            direction.Normalize();
            float moveX = direction.X * speed * deltaTime;
            float moveY = direction.Y * speed * deltaTime;

            Rectangle nextX = _collisionSprite.Hitbox;
            nextX.Offset((int)moveX, 0);
            if (!room.Intersects(nextX))
                _collisionSprite.Position += new Vector2(moveX, 0);

            Rectangle nextY = _collisionSprite.Hitbox;
            nextY.Offset(0, (int)moveY);
            if (!room.Intersects(nextY))
                _collisionSprite.Position += new Vector2(0, moveY);
        }

        public void TeleportTo(Vector2 position)
        {
            _collisionSprite.Position = position;
        }
    }
}
