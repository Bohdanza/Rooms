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
using System.Reflection;

namespace Rooms
{
    public class NPC:Mob
    {
        public int HP { get; protected set; }
        public int MaxHP { get; protected set; }
        private string Action { get; set; }

        public NPC(ContentManager contentManager, GameWorld gameWorld, double x, double y, int type, double speed, int HP, int maxHP)
        {
            MaxHP = maxHP;
            this.HP = HP;

            Speed = speed;
            
            ChangeCoords(x, y);

            Type = type;

            Action = "wa";

            updateTexture(contentManager, true);
        }
       
        public NPC(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);

            HP = Int32.Parse(input[currentStr + 5]);
            MaxHP = Int32.Parse(input[currentStr + 6]);

            Speed = double.Parse(input[currentStr + 7]);

            Action = "wa";

            updateTexture(contentManager, true);
        }
        
        /// <summary>
        /// Used to increase the texture number if reload==false and reload the whole Textures list if it's true
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="reload"></param>
        protected override void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TextureNumber = 0;

                while (File.Exists(@"Content\mob_" + Type.ToString() + "_" + TextureNumber.ToString() + "_" + Action + ".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>("mob_" + Type.ToString() + "_" + TextureNumber.ToString() + "_" + Action));

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

        public override string SaveList()
        {
            string result = "NPC\n";

            result += base.SaveList();

            result += HP.ToString();
            result += "\n";

            result += MaxHP.ToString();
            result += "\n";

            result += Speed.ToString();
            result += "\n";

            return result;
        }
    }
}