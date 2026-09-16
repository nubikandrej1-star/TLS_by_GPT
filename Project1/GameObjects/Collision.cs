using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TLS.GameObjects
{
    public enum HitboxType
    {
        None,
        Damage,
        StopWalking
    }
    public class CollisionObject : Sprite
    {
        public Rectangle Hitbox = Rectangle.Empty;
        public Vector2 Position
        {
            get => new Vector2(Hitbox.X, Hitbox.Y);
            set => Hitbox.Location = value.ToPoint();
        }

        public HitboxType HitboxType { get; set; } = HitboxType.None;

        #region constructors
        public CollisionObject()
        {
            Region = new TextureRegion();
        }
        public CollisionObject(TextureRegion region)
        {
            Region = region;
            Hitbox = region.SourceRectangle;
        }
        public CollisionObject(Texture2D texture, Rectangle rectangle)
        {
            Region = new TextureRegion(texture, rectangle);
            Hitbox = rectangle;
        }
        public CollisionObject(Sprite sprite)
        {
            Region = sprite.Region;
            Color = sprite.Color;
            Rotation = sprite.Rotation;
            Scale = sprite.Scale;
            Origin = sprite.Origin;
            Effects = sprite.Effects;
            LayerDepth = sprite.LayerDepth;

            Hitbox = Region.SourceRectangle;
        }
        #endregion

        public void ChangeHitbox(Rectangle rectangle)
        {
            Hitbox = rectangle;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch, Position);
        }
        public void CorrectHytboxByScale()
        {
            Hitbox.Width = (int)(Region.Width * Scale.X);
            Hitbox.Height = (int)(Region.Height * Scale.Y);
        }
    }

    public class AnimatedCollisionObject : CollisionObject
    {
        #region fields
        private int _currentAnimation = -1;

        private uint _currentFrame = 0;
        private Animation[]? _animations = null;
        private TimeSpan _elapsed = TimeSpan.Zero;

        private readonly TextureRegion _defaultFrame;
        #endregion

        #region constructors
        public AnimatedCollisionObject(TextureRegion region) : base(region)
        {
            _defaultFrame = region;
        }

        public AnimatedCollisionObject(Texture2D texture, Rectangle rectangle) : base(texture, rectangle)
        {
            _defaultFrame = Region;
        }

        public AnimatedCollisionObject(Sprite sprite) : base(sprite)
        {
            _defaultFrame = Region;
        }
        #endregion

        #region functions
        public void InitAnimation(uint countAnimations)
        {
            _animations = new Animation[countAnimations];
        }
        public void AddAnimation(int index, Animation animation)
        {
            if (_animations == null)
            {
                _animations = new Animation[index + 1];
            }
            _animations![index] = animation!;
        }
        public void SetAnimation(int index)
        {
            if (_animations == null)
            {
                throw new InvalidOperationException("Field _animations in AnimatedCollisionObject not initialized" +
                    "\n_animations == null. Operation SetAnimation can't execute. ");
            }
            if (index<0 || index>=_animations.Length)
            {
                throw new InvalidOperationException("Error in class 'AnimatedCollisionObject'" +
                    "Parameter index cannot be less than zero or greater than or equal to _animations.Length" +
                    "\nindex < 0 || index >= _animations.Length. Operation SetAnimation can't execute. ");
            }

            _currentAnimation = index;
            _currentFrame = 0;
            _elapsed = TimeSpan.Zero;
        }

        public void Update()
        {
            _elapsed = TimeSpan.Zero;
            Region = _defaultFrame;
        }

        public void Update(GameTime gameTime)
        {
            if (_animations == null || _currentAnimation<0)
            {
                _elapsed = TimeSpan.Zero;
                Region = _defaultFrame;
                return;
            }

            PlayAnimation(gameTime);
        }

        private void PlayAnimation(GameTime gameTime)
        {
            _elapsed += gameTime.ElapsedGameTime;
            int animIndex = _currentAnimation;
            Animation animation = _animations![animIndex];

            if (_elapsed >= animation.Delay)
            {
                _elapsed -= animation.Delay;

                Region = animation.Frames[_currentFrame];

                _currentFrame++;

                if (_currentFrame >= animation.Frames.Length)
                {
                    _currentFrame = 0;
                }
            }
        }
        #endregion
    }
}
