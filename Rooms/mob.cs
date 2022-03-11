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
    public abstract class Mob
    {
        public int Type { get; protected set; }

        public double Speed { get; protected set; }

        public double X { get; protected set; }
        public double Y { get; protected set; }

        public List<Texture2D> Textures { get; protected set; }
       
        /// <summary>
        /// Number of currently used texture from Textures list
        /// </summary>
        public int TextureNumber { get; protected set; }

        /// <summary>
        /// Used to increase the texture number if reload==false and reload the whole Textures list if it's true
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="reload"></param>
        protected virtual void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TextureNumber = 0;

                while(File.Exists(@"Content\mob_"+Type.ToString()+"_"+TextureNumber.ToString()+".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>("mob_" + Type.ToString() + "_" + TextureNumber.ToString()));

                    TextureNumber++;
                }

                TextureNumber = 0;
            }
            else
            {
                TextureNumber++;

                if(TextureNumber>=Textures.Count)
                {
                    TextureNumber = 0;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[TextureNumber], new Vector2(x, y - Textures[TextureNumber].Height), Color.White);
        }

        public virtual void Update(ContentManager contentManager, GameWorld gameWorld)
        {

        }

        /// <summary>
        /// Used to automatically fill some fields (like MaxHP, Speed etc.) with given file info if any. 
        /// !USE ONLY AFTER ASSIGNING TYPE!
        /// 
        /// Currently can't be used
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        protected bool AutomaticInitialize(string path)
        {
            //TODO

            return false;
        }

        public void ChangeCoords(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}