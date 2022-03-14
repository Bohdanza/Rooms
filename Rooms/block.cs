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
        public Texture2D Texture { get; protected set; }
        public int Type { get; protected set; }
        public bool Passable { get; protected set; }

        public Block(ContentManager contentManager, int type)
        {
            Type = type;

            if (type == 1 || type == 2)
            {
                Passable = false;
            }
            else
            {
                Passable = true;
            }

            updateTexture(contentManager);
        }

        public void updateTexture(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>("block" + Type.ToString());
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Texture, new Vector2(x, y-Texture.Height+GameWorld.BlockSizeY), Color.White);
        }

        public void Update()
        {
            //Just in case...
        }
    }
}
