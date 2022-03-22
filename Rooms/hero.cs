﻿using Microsoft.VisualBasic;
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
        private Item selectedItem = null;
        private string Action = "id";

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

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            string pact = Action;

            double realSpeed = (double)Speed / (WeightToCarry*0.5 + 1);
            var ks = Keyboard.GetState();

            Action = "id";

            if(ks.IsKeyDown(Keys.W))
            {
                Move(realSpeed, Math.PI * 1.5, gameWorld);

                Action = "wa";
            }

            if (ks.IsKeyDown(Keys.A))
            {
                Move(realSpeed, Math.PI, gameWorld);

                Action = "wa";
            }

            if (ks.IsKeyDown(Keys.S))
            {
                Move(realSpeed, Math.PI * 0.5, gameWorld);

                Action = "wa";
            }

            if (ks.IsKeyDown(Keys.D))
            {
                Move(realSpeed, 0, gameWorld);

                Action = "wa";
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

            if (timeSinceLastItemPick>=10&&ms.LeftButton == ButtonState.Pressed) 
            {
                timeSinceLastItemPick = 0;
                int pickedItemIndex = (ms.X - 20) / 60;

                if (ms.Y >= 1000 && ms.Y <= 1055 && ms.X >= 20 && ms.X < 80 + Inventory.Count * 60)
                {
                    if (selectedItem == null && pickedItemIndex < Inventory.Count)
                    {
                        WeightToCarry -= Inventory[pickedItemIndex].Weight;

                        selectedItem = Inventory[pickedItemIndex];

                        Inventory.RemoveAt(pickedItemIndex);
                    }
                    else if (selectedItem != null)
                    {
                        WeightToCarry += selectedItem.Weight;

                        if (pickedItemIndex >= Inventory.Count)
                        {
                            Inventory.Add(selectedItem);

                            selectedItem = null;
                        }
                        else
                        {
                            Item prevSelected = selectedItem;

                            WeightToCarry -= Inventory[pickedItemIndex].Weight;

                            selectedItem = Inventory[pickedItemIndex];

                            Inventory[pickedItemIndex] = prevSelected;
                        }
                    }
                }
                else if(selectedItem!=null)
                {
                    var coords=gameWorld.currentRoom.GetMouseCordinates(gameWorld);
                
                    if(gameWorld.currentRoom.blocks[(int)coords.Item1, (int)coords.Item2].Passable)
                    {
                        selectedItem.ChangeCoords(coords.Item1, coords.Item2);

                        gameWorld.currentRoom.AddMob(selectedItem);

                        selectedItem = null;
                    }
                }
            }

            base.Update(contentManager, gameWorld);

            if (pact != Action)
            {
                updateTexture(contentManager, true);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            base.Draw(spriteBatch, x, y);

          //  DrawInterface(spriteBatch);
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                Inventory[i].DrawIcon(spriteBatch, 20 + i * 60, 1000);
            }

            var ms = Mouse.GetState();

            if (selectedItem != null)
            {
                selectedItem.DrawIcon(spriteBatch, ms.X - 27, ms.Y - 27);
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