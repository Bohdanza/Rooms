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
        public const double pickUpRadius = 0.8;
        public int WeightToCarry { get; protected set; } = 0;
        public List<Item> Inventory { get; protected set; } = new List<Item>();
        private int timeSinceLastItemPick = 0;

        public Hero(ContentManager contentManager, double x, double y, int type, GameWorld gameWorld)
        {
            Type = type;

            ChangeCoords(x, y);

            Speed = 0.1;

            base.Radius = 0.25;

            updateTexture(contentManager, true);
        }

        public Hero(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);

            Speed = double.Parse(input[currentStr + 5]);

            base.Radius = 0.25;

            updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            double realSpeed = (double)Speed / (WeightToCarry*0.5 + 1);
            var ks = Keyboard.GetState();

            if(ks.IsKeyDown(Keys.W))
            {
                Move(realSpeed, Math.PI * 1.5, gameWorld);
            }

            if (ks.IsKeyDown(Keys.A))
            {
                Move(realSpeed, Math.PI, gameWorld);
            }

            if (ks.IsKeyDown(Keys.S))
            {
                Move(realSpeed, Math.PI * 0.5, gameWorld);
            }

            if (ks.IsKeyDown(Keys.D))
            {
                Move(realSpeed, 0, gameWorld);
            }

            if(ks.IsKeyDown(Keys.Space))
            {
                var arg1 = new List<Mob>();
                var arg2 = new List<string>();

                arg1.Add(this);
                arg2.Add("Item");

                var closestItem = gameWorld.currentRoom.GetClosestMob(X, Y, arg1, arg2);

                if (closestItem != null && GameWorld.GetDist(X, Y, closestItem.X, closestItem.Y) <= pickUpRadius)
                {
                    Inventory.Add((Item)closestItem);
                    WeightToCarry += ((Item)closestItem).Weight;

                    gameWorld.currentRoom.MarkMobAsDeleted(closestItem);
                }
            }

            var ms = Mouse.GetState();

            timeSinceLastItemPick++;

            if (timeSinceLastItemPick>=10&&ms.LeftButton == ButtonState.Pressed && ms.Y >= 1000 && ms.Y <= 1055 && ms.X >= 20 && ms.X < 20 + Inventory.Count * 60) 
            {
                timeSinceLastItemPick = 0;

                int pickedItemIndex = (ms.Y - 20) / 60;

                WeightToCarry -= Inventory[pickedItemIndex].Weight;

                Inventory.RemoveAt(pickedItemIndex);
            }

            base.Update(contentManager, gameWorld);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            base.Draw(spriteBatch, x, y);

        //    DrawInterface(spriteBatch);
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                Inventory[i].DrawIcon(spriteBatch, 20 + i * 60, 1000, 0f);
            }

            //base.DrawInterface(spriteBatch);
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