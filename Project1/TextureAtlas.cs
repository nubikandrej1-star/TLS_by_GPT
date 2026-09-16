using System;
using System.Collections.Generic;
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

namespace TLS
{
    /// <summary>
    /// Represents a rectangular region within a texture.
    /// </summary>
    public class TextureRegion
    {
        #region fields
        /// <summary>
        /// Gets or Sets the source texture this texture region is part of.
        /// </summary>
        [JsonIgnore]
        public Texture2D Texture { get; set; } = default!;

        /// <summary>
        /// Gets or Sets the source rectangle boundary of this texture region within the source texture.
        /// </summary>
        [JsonIgnore]
        public Rectangle SourceRectangle { get; set; } = new();

        /// <summary>
        /// Gets the width, in pixels, of this texture region.
        /// </summary>
        
        public int Width { get => SourceRectangle.Width; set 
            {
                var rect = SourceRectangle;
                rect.Width = value;
                SourceRectangle = rect;
            } 
        }

        /// <summary>
        /// Gets the height, in pixels, of this texture region.
        /// </summary>
        
        public int Height {
            get => SourceRectangle.Height; set
            {
                var rect = SourceRectangle;
                rect.Height = value;
                SourceRectangle = rect;
            }
        }

        public int X { get => SourceRectangle.X; set
            {
                var rect = SourceRectangle;
                rect.X = value;
                SourceRectangle = rect;
            }
}

        public int Y {
            get => SourceRectangle.Y; set
            {
                var rect = SourceRectangle;
                rect.Y = value;
                SourceRectangle = rect;
            }
        }
        #endregion

        #region constructors
        public TextureRegion() { }
        public TextureRegion(Texture2D texture, int x, int y, int width, int height)
        {
            Texture = texture;
            SourceRectangle = new Rectangle(x, y, width, height);
        }
        public TextureRegion(Texture2D texture, Rectangle rectangle)
        {
            Texture = texture;
            SourceRectangle = rectangle;
        }
        #endregion

        #region functions
        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            Draw(spriteBatch, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
        /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
        /// <param name="scale">The scale factor to apply when drawing this texture region on screen.</param>
        /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
        /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Draw(
                spriteBatch,
                position,
                color,
                rotation,
                origin,
                new Vector2(scale, scale),
                effects,
                layerDepth
            );
        }

        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
        /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
        /// <param name="scale">The amount of scaling to apply to the x- and y-axes when drawing this texture region on screen.</param>
        /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
        /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            spriteBatch.Draw(
                Texture,
                position,
                SourceRectangle,
                color,
                rotation,
                origin,
                scale,
                effects,
                layerDepth
            );
        }
        #endregion

    }


    public class TextureAtlas
    {
        #region fields
        private Dictionary<string, TextureRegion> _regions;

        // Stores animations added to this atlas.
        private Dictionary<string, Animation> _animations;

        /// <summary>
        /// Gets or Sets the source texture represented by this texture atlas.
        /// </summary>
        [JsonIgnore]
        public Texture2D? Texture { get; set; } = null;
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new texture atlas.
        /// </summary>
        public TextureAtlas()
        {
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Creates a new texture atlas instance using the given texture.
        /// </summary>
        /// <param name="texture">The source texture represented by the texture atlas.</param>
        public TextureAtlas(Texture2D texture)
        {
            Texture = texture;
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        #endregion

        #region functions

        #region TextureRegion
        /// <summary>
        /// Creates a new region and adds it to this texture atlas.
        /// </summary>
        /// <param name="name">The name to give the texture region.</param>
        /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
        /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
        /// <param name="width">The width, in pixels, of the region.</param>
        /// <param name="height">The height, in pixels, of the region.</param>
        public void AddRegion(string name, int x, int y, int width, int height)
        {
            if (Texture == null) return;

            TextureRegion region = new TextureRegion(Texture, x, y, width, height);
            _regions.Add(name, region);
        }
        public void AddRegion(string name, Rectangle rectangle)
        {
            if (Texture == null) return;

            TextureRegion region = new TextureRegion(Texture, rectangle);
            _regions.Add(name, region);
        }

        public void AddRegion(KeyValuePair<string, TextureRegion> keyValue)
        {
            if (Texture == null) return;

            TextureRegion region = new TextureRegion(Texture, keyValue.Value.SourceRectangle);
            _regions.Add(keyValue.Key, region);
        }

        /// <summary>
        /// Gets the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="name">The name of the region to retrieve.</param>
        /// <returns>The TextureRegion with the specified name.</returns>
        public TextureRegion GetRegion(string name)
        {
            return _regions[name];
        }

        /// <summary>
        /// Removes the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="name">The name of the region to remove.</param>
        /// <returns></returns>
        public bool RemoveRegion(string name)
        {
            return _regions.Remove(name);
        }

        /// <summary>
        /// Removes all regions from this texture atlas.
        /// </summary>
        public void Clear()
        {
            _regions.Clear();
        }
        #endregion

        #region another
        /// <summary>
        /// Creates a new texture atlas based on a texture atlas xml configuration file.
        /// </summary>
        /// <param name="content">The content manager used to load the texture for the atlas.</param>
        /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
        /// <returns>The texture atlas created by this method.</returns>
        /// <summary>
        /// Creates a new texture atlas based a texture atlas xml configuration file.
        /// </summary>
        /// <param name="content">The content manager used to load the texture for the atlas.</param>
        /// <param name="fileName">The path to the xml file, relative to the content root directory..</param>
        /// <returns>The texture atlas created by this method.</returns>

        /* Useless FromFile
        public static TextureAtlas FromFile(ContentManager content, string fileName)
        {
            TextureAtlas atlas = new TextureAtlas();

            string filePath = Path.Combine(content.RootDirectory, fileName);

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(reader);
                    XElement root = doc.Root;

                    // The <Texture> element contains the content path for the Texture2D to load.
                    // So we will retrieve that value then use the content manager to load the texture.
                    string texturePath = root.Element("Texture").Value;
                    atlas.Texture = content.Load<Texture2D>(texturePath);

                    // The <Regions> element contains individual <Region> elements, each one describing
                    // a different texture region within the atlas.  
                    //
                    // Example:
                    // <Regions>
                    //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
                    //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
                    // </Regions>
                    //
                    // So we retrieve all of the <Region> elements then loop through each one
                    // and generate a new TextureRegion instance from it and add it to this atlas.
                    var regions = root.Element("Regions")?.Elements("Region");

                    if (regions != null)
                    {
                        foreach (var region in regions)
                        {
                            string name = region.Attribute("name")?.Value;
                            int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                            int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                            int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                            int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                            if (!string.IsNullOrEmpty(name))
                            {
                                atlas.AddRegion(name, x, y, width, height);
                            }
                        }
                    }

                    // The <Animations> element contains individual <Animation> elements, each one describing
                    // a different animation within the atlas.
                    //
                    // Example:
                    // <Animations>
                    //      <Animation name="animation" delay="100">
                    //          <Frame region="spriteOne" />
                    //          <Frame region="spriteTwo" />
                    //      </Animation>
                    // </Animations>
                    //
                    // So we retrieve all of the <Animation> elements then loop through each one
                    // and generate a new Animation instance from it and add it to this atlas.
                    var animationElements = root.Element("Animations").Elements("Animation");

                    if (animationElements != null)
                    {
                        foreach (var animationElement in animationElements)
                        {
                            string name = animationElement.Attribute("name")?.Value;
                            float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
                            TimeSpan delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

                            List<TextureRegion> frames = new List<TextureRegion>();

                            var frameElements = animationElement.Elements("Frame");

                            if (frameElements != null)
                            {
                                foreach (var frameElement in frameElements)
                                {
                                    string regionName = frameElement.Attribute("region").Value;
                                    TextureRegion region = atlas.GetRegion(regionName);
                                    frames.Add(region);
                                }
                            }

                            Animation animation = new Animation(frames.ToArray(), delay);
                            atlas.AddAnimation(name, animation);
                        }
                    }

                    return atlas;
                }
            }
        }
        */

        private class _SerializeAtlas
        {
            public Dictionary<string, TextureRegion> Regions { get; set; }
            public Dictionary<string, Animation> Animations { get; set; }
            public _SerializeAtlas(Dictionary<string, TextureRegion> regions, Dictionary<string, Animation> animations)
            {
                Regions = regions;
                Animations = animations;
            }
        }
        public void LoadFromFile(string fileName)
        {
            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException("Файл не знайдено.", fileName);
            }
            if (Texture==null)
            {
                throw new Exception("Перед тим як імпортувати атлас з файлу спочатку проініціалізуй текстуру");
            }

            string json;

            using (StreamReader sr = new StreamReader(fileName))
            {
                json = sr.ReadToEnd();
            }
            _SerializeAtlas? atlas = JsonSerializer.Deserialize<_SerializeAtlas>(json);

            if (atlas == null)
            {
                throw new InvalidDataException("Не вдалося десеріалізувати TextureAtlas.");
            }

            if (atlas.Regions != null)
            {
                foreach (var region in atlas.Regions)
                {
                    AddRegion(region);
                }
            }
            else
            {
                _regions = new Dictionary<string, TextureRegion>();
            }
            if (atlas.Animations != null)
            {
                foreach (var animation in atlas.Animations)
                {
                    for (int i = 0; i < animation.Value.Frames.Length; i++)
                    {
                        animation.Value.Frames[i].Texture=Texture;
                    }
                }
                _animations = atlas.Animations;
            }
            else
            {
                _animations = new Dictionary<string, Animation>();
            }
        }

        public void LoadFromFile(string fileName, Texture2D texture)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Файл не знайдено.", fileName);
            }
            Texture = texture;

            string json;

            using (StreamReader sr = new StreamReader(fileName))
            {
                json = sr.ReadToEnd();
            }
            _SerializeAtlas? atlas = JsonSerializer.Deserialize<_SerializeAtlas>(json);

            if (atlas == null)
            {
                throw new InvalidDataException("Не вдалося десеріалізувати TextureAtlas.");
            }

            if (atlas.Regions != null)
            {
                foreach (var region in atlas.Regions)
                {
                    AddRegion(region);
                }
            }
            else
            {
                _regions = new Dictionary<string, TextureRegion>();
            }
            if (atlas.Animations != null)
            {
                foreach (var animation in atlas.Animations)
                {
                    for (int i = 0; i < animation.Value.Frames.Length; i++)
                    {
                        animation.Value.Frames[i].Texture = Texture;
                    }
                }
                _animations = atlas.Animations;
            }
            else
            {
                _animations = new Dictionary<string, Animation>();
            }
        }

        public void ToFile(string fileName)
        {
            _SerializeAtlas serializeAtlas = new _SerializeAtlas(_regions, _animations);

            string json = JsonSerializer.Serialize<_SerializeAtlas>(serializeAtlas);
            File.WriteAllText(fileName, json);
        }
        #endregion

        #region sprite
        /// <summary>
        /// Creates a new sprite using the region from this texture atlas with the specified name.
        /// </summary>
        /// <param name="regionName">The name of the region to create the sprite with.</param>
        /// <returns>A new Sprite using the texture region with the specified name.</returns>
        public Sprite CreateSprite(string regionName)
        {
            TextureRegion region = GetRegion(regionName);
            return new Sprite(region);
        }
        #endregion

        #region animation
        public void AddAnimation(string animationName, Animation animation)
        {
            _animations.Add(animationName, animation);
        }

        public Animation GetAnimation(string animationName)
        {
            return _animations[animationName];
        }

        public bool RemoveAnimation(string animationName)
        {
            return _animations.Remove(animationName);
        }

        public AnimatedSprite CreateAnimatedSprite(string animationName)
        {
            Animation animation = GetAnimation(animationName);
            return new AnimatedSprite(animation);
        }
        #endregion

        #endregion
    }



    public class Sprite
    {
        #region fields
        /// <summary>
        /// Gets or Sets the source texture region represented by this sprite.
        /// </summary>
        public TextureRegion Region { get; set; }

        /// <summary>
        /// Gets or Sets the color mask to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Color.White
        /// </remarks>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or Sets the amount of rotation, in radians, to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is 0.0f
        /// </remarks>
        public float Rotation { get; set; } = 0.0f;

        /// <summary>
        /// Gets or Sets the scale factor to apply to the x- and y-axes when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Vector2.One
        /// </remarks>
        public Vector2 Scale { get; set; } = Vector2.One;

        /// <summary>
        /// Gets or Sets the xy-coordinate origin point, relative to the top-left corner, of this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Vector2.Zero
        /// </remarks>
        public Vector2 Origin { get; set; } = Vector2.Zero;

        /// <summary>
        /// Gets or Sets the sprite effects to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is SpriteEffects.None
        /// </remarks>
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        /// <summary>
        /// Gets or Sets the layer depth to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is 0.0f
        /// </remarks>
        public float LayerDepth { get; set; } = 0.0f;

        /// <summary>
        /// Gets the width, in pixels, of this sprite. 
        /// </summary>
        /// <remarks>
        /// Width is calculated by multiplying the width of the source texture region by the x-axis scale factor.
        /// </remarks>
        public float Width => Region.Width * Scale.X;

        /// <summary>
        /// Gets the height, in pixels, of this sprite.
        /// </summary>
        /// <remarks>
        /// Height is calculated by multiplying the height of the source texture region by the y-axis scale factor.
        /// </remarks>
        public float Height => Region.Height * Scale.Y;
        #endregion

        #region constructors
        public Sprite()
        {
            Region = new TextureRegion();
        }
        public Sprite(TextureRegion region)
        {
            Region = region;
        }

        public Sprite(Texture2D texture, Rectangle rectangle)
        {
            Region = new TextureRegion(texture, rectangle);
        }
        #endregion

        #region functions
        public void CenterOrigin()
        {
            Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Region.Draw(spriteBatch, position, Color, Rotation, Origin, Scale, Effects, LayerDepth);
        }
        #endregion
    }



    public class Animation
    {
        #region fields
        public TextureRegion[] Frames { get; set; }


        public TimeSpan Delay { get; set; }
        #endregion

        #region constructors

        public Animation(int frameCount)
        {
            Frames = new TextureRegion[frameCount];
            Delay = TimeSpan.FromMilliseconds(100);
        }

        [JsonConstructor]
        public Animation(TextureRegion[] frames, TimeSpan delay)
        {
            Frames = frames;
            Delay = delay;
        }

        public Animation(TextureRegion[] frames, int delay)
        {
            Frames = frames;
            Delay = TimeSpan.FromMilliseconds(delay);
        }
        #endregion
    }




    public class AnimatedSprite : Sprite
    {
        #region fields
        private int _currentFrame = 0;
        private TimeSpan _elapsed = new TimeSpan();
        private Animation _animation = default!;
        #endregion

        #region constructors
        /// <summary>
        /// Gets or Sets the animation for this animated sprite.
        /// </summary>
        public Animation Animation
        {
            get => _animation;
            set
            {
                _animation = value;
                Region = _animation.Frames[0];
            }
        }

        public AnimatedSprite() { }

        public AnimatedSprite(Animation animation)
        {
            Animation = animation;
        }
        #endregion

        #region functions
        /// <summary>
        /// Updates this animated sprite.
        /// </summary>
        /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
        public void Update(GameTime gameTime)
        {
            _elapsed += gameTime.ElapsedGameTime;

            if (_elapsed >= _animation.Delay)
            {
                _elapsed -= _animation.Delay;
                _currentFrame++;

                if (_currentFrame >= _animation.Frames.Length)
                {
                    _currentFrame = 0;
                }

                Region = _animation.Frames[_currentFrame];
            }
        }
        #endregion
    }
}
