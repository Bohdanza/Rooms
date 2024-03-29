﻿using Microsoft.VisualBasic;
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
    public abstract class Item : Mob
    {
        public int Weight { get; protected set; }
        protected List<Texture2D> bigTextures { get; set; }
        protected int bigTexturesNumber = 0;

        protected override void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                bigTextures = new List<Texture2D>();

                bigTexturesNumber = 0;

                while (File.Exists(@"Content\mob_big_" + Type.ToString() + "_" + bigTexturesNumber.ToString() + ".xnb"))
                {
                    bigTextures.Add(contentManager.Load<Texture2D>("mob_big_" + Type.ToString() + "_" + bigTexturesNumber.ToString()));

                    bigTexturesNumber++;
                }

                bigTexturesNumber = 0;
            }
            else
            {
                bigTexturesNumber++;

                if (bigTexturesNumber >= bigTextures.Count)
                {
                    bigTexturesNumber = 0;
                }
            }

            base.updateTexture(contentManager, reload);
        }

        public virtual void DrawIcon(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(bigTextures[bigTexturesNumber], new Vector2(x, y), Color.White);
        }

        public override string SaveList()
        {
            string output = Weight.ToString() + "\n" + base.SaveList();

            return output;
        }
    }
}
