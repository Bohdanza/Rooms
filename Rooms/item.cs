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
    public class Item : Mob
    {
        public int Weight { get; protected set; }
        protected List<Texture2D> bigTextures { get; set; }
        protected int bigTexturesNumber = 0;

        public Item(ContentManager contentManager, double x, double y, int type, int weight)
        {
            ChangeCoords(x, y);

            Type = type;

            Weight = weight;

            updateTexture(contentManager, true);
        }

        public Item(ContentManager contentManager, List<string> input, int currentStr)
        {
            Weight = Int32.Parse(input[currentStr + 1]);

            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);
            
            base.Radius = 0.25;

            updateTexture(contentManager, true);
        }

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

        public void DrawIcon(SpriteBatch spriteBatch, int x, int y, float layer)
        {
            spriteBatch.Draw(bigTextures[bigTexturesNumber], new Vector2(x, y),
                null,
                Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, layer);
        }

        public override string SaveList()
        {
            string output = "Item\n" + Weight.ToString() + "\n" + base.SaveList();

            return output;
        }
    }
}
