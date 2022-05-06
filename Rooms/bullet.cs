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
    public class Bullet:Mob
    {
        public int Damage { get; protected set; }
        public Mob Owner { get; set; } = null;
        public double Direction { get; protected set; }
        protected string Action { get; set; }

        public Bullet(ContentManager contentManager, double direction, double x, double y, double z, 
            int type, int damage, Mob owner, double radius, double speed)
        {
            Speed = speed;
            Radius = radius;

            Direction = direction;

            ChangeCoords(x, y, z);

            Damage = damage;
            Radius = radius;

            Owner = owner;

            Type = type;

            Action = "id";

            updateTexture(contentManager, true);
        }
       
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
                }
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            string pact = Action;

            if (Action == "id")
            {
                bool wasMoved = Move(Speed, Direction, gameWorld);

                if (!wasMoved)
                {
                    Action = "di";
                }

                List<Mob> meAndOwner = new List<Mob>();
                List<string> Types = new List<string>();

                meAndOwner.Add(this);
                meAndOwner.Add(Owner);

                Types.Add("Monster");
                Types.Add("Hero");

                Mob clst = gameWorld.currentRoom.GetClosestMob(X, Y, meAndOwner, Types);

                if (GameWorld.GetDist(X, Y, clst.X, clst.Y) <= Radius + clst.Radius)
                {
                    Action = "di";
                }
            }
            else if (Action == "di" && TextureNumber == Textures.Count - 1)
            {
                gameWorld.currentRoom.MarkMobAsDeleted(this);
            }

            TimeSinceLastTextureUpdate++;

            if (pact != Action)
                updateTexture(contentManager, true);
            else if (TimeSinceLastTextureUpdate > GameWorld.TextureUpdateSpeed)
                updateTexture(contentManager, false);
        }

        public override string SaveList()
        {
            return "";
        }
    }
}
