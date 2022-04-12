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
    public class ControlCenter : Mob
    {
        public List<Mob> mobsControled = new List<Mob>();
        public double MinDist { get; protected set; }
        public double MaxDist { get; protected set; }
        private string Action { get; set; }
        private float Direction { get; set; } = 0;

        public ControlCenter(double x, double y, double minDist, double maxDist, double speed)
        {
            ChangeCoords(x, y);

            MinDist = minDist;
            MaxDist = maxDist;

            Speed = speed;

            Action = "id";
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            var rnd = new Random();

            if (rnd.Next(0, 1000) < 20)
            {
                float addToDirection = (float)rnd.NextDouble();

                if (rnd.Next(0, 2) == 0)
                {
                    addToDirection *= -1;
                }

                Direction += addToDirection;

                Direction %= (float)(Math.PI * 2);
            }

            int prob = rnd.Next(0, 1000);

            if (Action == "id" && prob < 15)
            {
                Action = "wa";
            }

            if (Action == "wa")
            {
                bool moved = Move(Speed, Direction, gameWorld);
                prob = rnd.Next(0, 1000);

                if (prob < 15)
                {
                    Action = "id";
                }

                if (!moved)
                {
                    Direction += (float)Math.PI;
                }
            }

            TimeSinceLastTextureUpdate++;
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        { }

        public void AddMob(Mob mob)
        {
            mobsControled.Add(mob);

            mob.controlCenter = this;
        }
    }
}