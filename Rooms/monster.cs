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
        public bool Agressive { get; set; } = false;
        private float Direction { get; set; } = 0;

        public NPC(ContentManager contentManager, GameWorld gameWorld, double x, double y, int type, double speed, int HP, int maxHP)
        {
            MaxHP = maxHP;
            this.HP = HP;

            Speed = speed;
            
            ChangeCoords(x, y);

            Type = type;

            Action = "id";

            if (HP <= 0)
            {
                Action = "di";
            }

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

            Action = "id";

            if(HP<=0)
            {
                Action = "di";
            }

            updateTexture(contentManager, true);

            if (Action == "di")
            {
                TextureNumber = Textures.Count-1;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            string pact = Action;
            var rnd = new Random();

            if(rnd.Next(0, 1000)<20)
            {
                float addToDirection = 0.872f;

                if(rnd.Next(0, 2)==0)
                {
                    addToDirection *= -1;
                }

                Direction += addToDirection;
            }

            if (Action == "id" && rnd.Next(0, 1000) < 15)
            {
                Action = "wa";
            }

            if (Action == "wa")
            {
                bool moved = Move(Speed, Direction, gameWorld);

                if(rnd.Next(0, 1000) < 15)
                {
                    Action = "id";
                }

                if(!moved)
                {
                    Direction += (float)Math.PI;
                }
            }

            TimeSinceLastTextureUpdate++;

            if (Action != pact)
            {
                updateTexture(contentManager, true);
            }
            else if (TimeSinceLastTextureUpdate >= GameWorld.TextureUpdateSpeed)
            {
                TimeSinceLastTextureUpdate = 0;

                updateTexture(contentManager, false);
            }
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
                    if (Action != "di")
                    {
                        TextureNumber = 0;
                    }
                    else
                    {
                        TextureNumber--;
                    }

                    if(Action=="dm")
                    {
                        Action = "id";

                        updateTexture(contentManager, true);
                    }
                }
            }
        }

        public void Damage(ContentManager contentManager, GameWorld gameWorld, int power)
        {
            HP -= power;

            if(HP<=0)
            {
                HP = 0;

                if (Action != "di")
                {
                    Action = "di";

                    updateTexture(contentManager, true);
                }
            }
            else if (Action != "dm")
            {
                Action = "dm";

                updateTexture(contentManager, true);
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