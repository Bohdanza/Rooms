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
    public class Boss:Mob
    {
        public int HP { get; protected set; }
        public int MaxHP { get; protected set; }

        public Boss(int type, double x, double y, double z)
        {
            Type = type;

            ChangeCoords(x, y, z);

            if (type == 25)
            {
                Speed = 0.5;

                MaxHP = 350;
                HP = 350;
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            base.Update(contentManager, gameWorld);
        }
    }
}