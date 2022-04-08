using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Rooms
{
    public class Block
    {
        public List<Texture2D> Textures { get; protected set; }
        public int TextureNumber { get; protected set; }
        protected int TimeSinceLastTextureUpdate = 0;

        public int Type { get; protected set; }
        public bool Passable { get; protected set; }

        public Block(ContentManager contentManager, int type)
        {
            Type = type;

            if (type != 0)
            {
                Passable = false;
            }
            else
            {
                Passable = true;
            }

            updateTexture(contentManager, true);
        }

        public void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TextureNumber = 0;

                while (File.Exists(@"Content\block" + Type.ToString() + "_" + TextureNumber.ToString() + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>("block" + Type.ToString() + "_" + TextureNumber.ToString()));

                    TextureNumber++;
                }

                TextureNumber = 0;
            }
            else
            {
                TextureNumber++;

                if (TextureNumber >= Textures.Count)
                {
                    TextureNumber = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[TextureNumber], new Vector2(x - (int)Textures[TextureNumber].Width / 2 + GameWorld.BlockSizeX / 2,
                y - Textures[TextureNumber].Height + GameWorld.BlockSizeY), Color.White);
        }

        public void Update(ContentManager contentManager)
        {
            //Just in case...
            TimeSinceLastTextureUpdate++;

            if (TimeSinceLastTextureUpdate >= GameWorld.TextureUpdateSpeed)
            {
                updateTexture(contentManager, false);

                TimeSinceLastTextureUpdate = 0;
            }
        }
    }
}
