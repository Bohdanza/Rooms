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
    public class Hero:Mob
    {
        public int WeightToCarry { get; protected set; }

        public Hero(ContentManager contentManager, double x, double y, int type, GameWorld gameWorld)
        {
            Type = type;

            ChangeCoords(x, y);

            Speed = 0.1;

            Radius = 0.25;

            updateTexture(contentManager, true);
        }

        public Hero(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);

            Speed = double.Parse(input[currentStr + 5]);

            updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            var ks = Keyboard.GetState();

            if(ks.IsKeyDown(Keys.W))
            {
                Move(Speed, Math.PI * 1.5, gameWorld);
            }

            if (ks.IsKeyDown(Keys.A))
            {
                Move(Speed, Math.PI, gameWorld);
            }

            if (ks.IsKeyDown(Keys.S))
            {
                Move(Speed, Math.PI * 0.5, gameWorld);
            }

            if (ks.IsKeyDown(Keys.D))
            {
                Move(Speed, 0, gameWorld);
            }

            base.Update(contentManager, gameWorld);
        }

        public override string SaveList()
        {
            string result = "Hero\n";

            result += base.SaveList();

            result += Speed.ToString();
            result += "\n";

            return result;
        }
    }
}