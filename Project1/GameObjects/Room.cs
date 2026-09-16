using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    // Бажано доробити логіку кімнати так щоб вана мала список з атласув текстур і кожен об'єкт міг брати текстуру з атласу, а не з окремого файлу.
    // Це дозволить оптимізувати рендеринг та зменшити кількість завантажень текстур.
    // Також треба щоб Draw виконувався в порядку цих атласів, спочатку рендерились об'єкти з першого атласу, потім з другого і так далі.

    public class Room
    {
        private uint lastId = 0;
        private Dictionary<uint, AnimatedCollisionObject> hitboxes { get; set; } = new Dictionary<uint, AnimatedCollisionObject>();
        private ContentManager contentManager;

        public Room(ContentManager content)
        {
            contentManager = content;
        }

        public void AddHitbox(Texture2D texture, Rectangle rectangle)
        {
            hitboxes.Add(lastId, new AnimatedCollisionObject(texture, rectangle));
            lastId++;
        }
        public void AddHitbox(Sprite sprite)
        {
            hitboxes.Add(lastId, new AnimatedCollisionObject(sprite));
            lastId++;
        }
        public void AddHitbox(AnimatedSprite animatedSprite, uint maxAnamations)
        {
            AnimatedCollisionObject collisionObject = new AnimatedCollisionObject(animatedSprite);
            collisionObject.InitAnimation(maxAnamations);
            collisionObject.AddAnimation(0, animatedSprite.Animation);
            hitboxes.Add(lastId, collisionObject);
            lastId++;
        }

        public AnimatedCollisionObject GetHitbox(uint id)
        {
            return hitboxes[id];
        }
        public bool Intersects(Rectangle rectangle, HitboxType hitboxType)
        {
            foreach (var hitbox in hitboxes)
            {
                if (hitbox.Value.Hitbox.Intersects(rectangle) && hitbox.Value.HitboxType == hitboxType)
                { return true; }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(var hitbox in hitboxes)
            {
                hitbox.Value.Draw(spriteBatch);
            }
        }
    }
}
