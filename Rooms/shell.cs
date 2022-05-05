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
    public class Shell:Item
    {
        private Texture2D hpText, emptyHP;

        protected int MaxHP { get; set; }
        public int HP { get; set; }

        public Shell(ContentManager contentManager, double x, double y, double z, int type, int weight, int HP, int maxHP)
        {
            ChangeCoords(x, y, z);

            Type = type;

            Weight = weight;

            hpText = contentManager.Load<Texture2D>("shell_hp");
            emptyHP = contentManager.Load<Texture2D>("empty_hp");

            this.HP = HP;

            MaxHP = maxHP;

            updateTexture(contentManager, true);
        }

        public Shell(ContentManager contentManager, List<string> input, int currentStr)
        {
            Weight = Int32.Parse(input[currentStr + 1]);

            Name = input[currentStr + 2];

            ChangeCoords(double.Parse(input[currentStr + 3]), double.Parse(input[currentStr + 4]), double.Parse(input[currentStr + 5]));

            Type = Int32.Parse(input[currentStr + 6]);

            HP = Int32.Parse(input[currentStr + 7]);
            MaxHP = Int32.Parse(input[currentStr + 8]);

            base.Radius = 0.25;

            hpText = contentManager.Load<Texture2D>("shell_hp");
            emptyHP = contentManager.Load<Texture2D>("empty_hp");

            updateTexture(contentManager, true);
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            for (int i = 1; i <= MaxHP; i++)
            {
                if (i <= HP)
                    spriteBatch.Draw(hpText, new Vector2((int)(100 + (i - 1) * hpText.Width * 1.1), 40), Color.White);
                else
                    spriteBatch.Draw(emptyHP, new Vector2((int)(100 + (i - 1) * hpText.Width * 1.1), 40), Color.White);
            }
        }

        public override string SaveList()
        {
            return "Shell\n" + base.SaveList() + HP.ToString() + "\n" + MaxHP.ToString() + "\n";
        }
    }
}