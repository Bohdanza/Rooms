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
        public Hero(ContentManager contentManager, double x, double y, int type, GameWorld gameWorld)
        {
            Type = type;

            X = x;
            Y = y;

            Speed = 0.1;

            updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            var ks = Keyboard.GetState();

            if(ks.IsKeyDown(Keys.W))
            {
                Y -= Speed;
            }

            if (ks.IsKeyDown(Keys.A))
            {
                X -= Speed;
            }

            if (ks.IsKeyDown(Keys.S))
            {
                Y += Speed;
            }

            if (ks.IsKeyDown(Keys.D))
            {
                X += Speed;
            }

            base.Update(contentManager, gameWorld);
        }
    }
}