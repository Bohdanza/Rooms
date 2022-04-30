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
        private Item selectedItem = null;
        private string Action = "id";
        public int AttackEnergy { get; protected set; } = 0;
        public int HP { get; protected set; }
        private double Direction=0;

        public Hero(ContentManager contentManager, double x, double y, int type, GameWorld gameWorld)
        {
            Type = type;

            ChangeCoords(x, y);

            Speed = 0.12;

            base.Radius = 0.25;

            HP = 1;

            Inventory.Add(null);

            updateTexture(contentManager, true);
        }

        public Hero(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);

            Speed = double.Parse(input[currentStr + 5]);
            HP = Int32.Parse(input[currentStr + 6]);

            int xc = Int32.Parse(input[currentStr + 7]);

            int cstr = currentStr + 8;

            for (int i = 0; i < xc; i++)
            {
                Inventory.Add((Item)Mob.Loader(contentManager, cstr, input));

                if (Inventory[Inventory.Count - 1] != null)
                {
                    WeightToCarry += Inventory[Inventory.Count - 1].Weight;
                 
                    cstr += Inventory[Inventory.Count - 1].SaveList().Count(f => (f == '\n'));
                }
                else
                    cstr++;
            }

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
            var mousePosition = gameWorld.currentRoom.GetMouseCordinates(gameWorld);
             
            timeSinceLastItemPick++;

            if (timeSinceLastItemPick>=10&&ms.LeftButton == ButtonState.Pressed) 
            {
                timeSinceLastItemPick = 0;
                int pickedItemIndex = (ms.X - 20) / 60 + 1;

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
                else if (selectedItem != null)
                {
                    if(mousePosition.Item1>=0&& mousePosition.Item1<Room.roomSize&& mousePosition.Item2 >= 0 && 
                        mousePosition.Item2 < Room.roomSize 
                        && gameWorld.currentRoom.blocks[(int)mousePosition.Item1, (int)mousePosition.Item2, 0].Passable)
                    {
                        selectedItem.ChangeCoords(mousePosition.Item1, mousePosition.Item2);

                        gameWorld.currentRoom.AddMob(selectedItem);

                        selectedItem = null;
                    }
                }
            }

            AttackEnergy++;

            if (ms.RightButton == ButtonState.Pressed && AttackEnergy >= 20)
            {
                AttackEnergy = 0;

                double directionToMouse = Math.Atan2(Y - mousePosition.Item2, X - mousePosition.Item1);

                Move(0.075, directionToMouse + Math.PI, gameWorld);

                Mob kck = new KickTrace(contentManager, X, Y+5, 6, 0, directionToMouse + Math.PI, 9, gameWorld);
                kck.Move(0.2, directionToMouse + Math.PI, gameWorld);

                gameWorld.currentRoom.AddMob(kck);

                foreach (var currentMob in gameWorld.currentRoom.mobs)
                {
                    if (currentMob.SaveList().StartsWith("NPC"))
                    {
                        double dist = GameWorld.GetDist(X, Y, currentMob.X, currentMob.Y);

                        if (dist <= Radius + currentMob.Radius + 2.5)
                        {
                            double directionToMob = Math.Atan2(Y - currentMob.Y, X - currentMob.X);

                            if (Math.Abs(directionToMouse - directionToMob) <= 2.7)
                            {
                                ((NPC)currentMob).Damage(contentManager, gameWorld, 5);
                            }
                        }
                    }
                }
            }

            if (ms.MiddleButton == ButtonState.Pressed && AttackEnergy >= 30)
            {
                AttackEnergy = 0;

                double directionToMouse = Math.Atan2(Y - mousePosition.Item2, X - mousePosition.Item1);

                Move(0.1, directionToMouse + Math.PI, gameWorld);

                Mob kck = new KickTrace(contentManager, X, Y + 5, 7, 0, directionToMouse + Math.PI, 20, gameWorld);
                kck.Move(0.2, directionToMouse + Math.PI, gameWorld);

                gameWorld.currentRoom.AddMob(kck);

                foreach (var currentMob in gameWorld.currentRoom.mobs)
                {
                    if (currentMob.SaveList().StartsWith("NPC"))
                    {
                        double dist = GameWorld.GetDist(X, Y, currentMob.X, currentMob.Y);

                        if (dist <= Radius + currentMob.Radius + 1)
                        {
                            double directionToMob = Math.Atan2(Y - currentMob.Y, X - currentMob.X);

                            if (Math.Abs(directionToMouse - directionToMob) <= 0.5)
                            {
                                ((NPC)currentMob).Damage(contentManager, gameWorld, 10);
                            }
                        }
                    }
                }
            }

            if (ms.LeftButton == ButtonState.Pressed && selectedItem == null)
            {
                Action = "wa";

                Direction = Math.Atan2(mousePosition.Item2 - Y, mousePosition.Item1 - X);

                Move(realSpeed, Direction, gameWorld);
            }

            TimeSinceLastTextureUpdate++;

            if (Action != "wa")
            {
                if (TimeSinceLastTextureUpdate > GameWorld.TextureUpdateSpeed)
                {
                    updateTexture(contentManager, false);

                    TimeSinceLastTextureUpdate = 0;
                }
            }
            else if (TimeSinceLastTextureUpdate > GameWorld.TextureUpdateSpeed * 0.5)
            {
                updateTexture(contentManager, false);

                TimeSinceLastTextureUpdate = 0;
            }
                

            if (pact != Action)
            {
                updateTexture(contentManager, true);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            if (Direction >= Math.PI * 2)
                Direction %= Math.PI * 2;
            else if (Direction < 0) 
                Direction = Math.PI * 2 + Direction;

            if (Direction > 0.5 * Math.PI && Direction < Math.PI * 1.5)
                base.Draw(spriteBatch, x, y);
            else
                base.Draw(spriteBatch, x, y, SpriteEffects.FlipHorizontally);

            int ySub = 12;

            if (Action == "id" && TextureNumber == 1)
                ySub = 10;

            if (Inventory[0] != null)
            {
                Inventory[0].Draw(spriteBatch, x, y - ySub);
            }

            //  DrawInterface(spriteBatch);
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            for (int i = 1; i < Inventory.Count; i++)
            {
                if (Inventory[i] != null)
                    Inventory[i].DrawIcon(spriteBatch, -40 + i * 60, 1000);
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

            result += HP.ToString();
            result += "\n";

            result += Inventory.Count;
            result += "\n";

            foreach (var currentItem in Inventory)
            {
                if (currentItem != null)
                {
                    result += currentItem.SaveList();

                    if (result[result.Length - 1] != '\n')
                    {
                        result += "\n";
                    }
                }
                else
                {
                    result += "null";
                }
            }

            return result;
        }

        public void Damage(ContentManager contentManager, GameWorld gameWorld, int power)
        {
            HP -= power;

            if (HP <= 0)
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
    }
}   