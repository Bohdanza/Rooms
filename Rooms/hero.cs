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
        private bool putOn = false;
        private int FlyEnergy = 0;
        private bool prevC = false;

        public Hero(ContentManager contentManager, double x, double y, double z, int type, GameWorld gameWorld)
        {
            Type = type;

            ChangeCoords(x, y, z);

            Speed = 0.12;

            base.Radius = 0.3;

            HP = 1;

            Inventory.Add(null);

            updateTexture(contentManager, true);
        }
        
        public Hero(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]), double.Parse(input[currentStr + 4]));

            Type = Int32.Parse(input[currentStr + 5]);

            Speed = double.Parse(input[currentStr + 6]);
            HP = Int32.Parse(input[currentStr + 7]);

            int xc = Int32.Parse(input[currentStr + 8]);

            int cstr = currentStr + 9;

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

            base.Radius = 0.3;

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
                    if (Action != "di")
                    {
                        TextureNumber = 0;
                    }
                    else
                    {
                        TextureNumber--;
                    }

                    if (Action == "put" || Action == "dm" || Action == "at" || Action == "dtc" || Action == "at1")
                    {
                        Action = "id";

                        updateTexture(contentManager, true);
                    }
                }
            }
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            string pact = Action;

            double realSpeed = (double)Speed / (WeightToCarry*0.5 + 1);
            var ks = Keyboard.GetState();

            if (Action != "di" && Action != "fly")
            {
                if (Action != "at1" && Action != "put" && Action != "di")
                    Action = "id";

                if (Action != "at1" && ks.IsKeyDown(Keys.Space))
                {
                    var arg1 = new List<Mob>();
                    var arg2 = new List<string>();

                    arg1.Add(this);
                    arg2.Add("Resource");

                    var closestItem = gameWorld.currentRoom.GetClosestMob(X, Y, arg1, arg2);

                    if (closestItem != null && GameWorld.GetDist(X, Y, closestItem.X, closestItem.Y) <= pickUpRadius)
                    {
                        Inventory.Add((Item)closestItem);
                        WeightToCarry += ((Item)closestItem).Weight;

                        gameWorld.currentRoom.MarkMobAsDeleted(closestItem);
                    }
                }

                //jump
                if (Action != "at1" && ks.IsKeyDown(Keys.Z))
                {
                    if ((int)Math.Round(Z) <= Room.roomSizeZ && (int)Math.Round(Z) > 0 &&
                        gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Z) - 1].Rigid)
                        ZVector = 0.75;
                }

                var ms = Mouse.GetState();
                var mousePosition = gameWorld.currentRoom.GetMouseCordinates(gameWorld);

                timeSinceLastItemPick++;

                if (timeSinceLastItemPick >= 10 && ms.LeftButton == ButtonState.Pressed)
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
                        if (mousePosition.Item1 >= 0 && mousePosition.Item1 < Room.roomSize && mousePosition.Item2 >= 0 &&
                            mousePosition.Item2 < Room.roomSize
                            && !gameWorld.currentRoom.blocks[(int)mousePosition.Item1, (int)mousePosition.Item2, 0].Rigid)
                        {
                            selectedItem.ChangeCoords(mousePosition.Item1, mousePosition.Item2, Z);

                            gameWorld.currentRoom.AddMob(selectedItem);

                            selectedItem = null;
                        }
                    }
                }

                AttackEnergy++;

                if (Action != "at1" && ks.IsKeyDown(Keys.X) && Action != "put")
                {
                    if (Inventory[0] != null)
                    {
                        putOn = false;
                        Action = "put";
                    }
                    else
                    {
                        Shell closestShell = (Shell)gameWorld.currentRoom.GetClosestMob(X, Y, this, "Shell");

                        if (closestShell != null && GameWorld.GetDist(X, Y, closestShell.X, closestShell.Y) < Radius + closestShell.Radius + 0.5)
                        {
                            putOn = true;
                            Action = "put";

                            Inventory[0] = closestShell;

                            WeightToCarry += closestShell.Weight;

                            gameWorld.currentRoom.MarkMobAsDeleted(closestShell);
                        }
                    }
                }

                if (Action != "at1" && ms.RightButton == ButtonState.Pressed && AttackEnergy >= 50)
                {
                    Action = "at1";

                    AttackEnergy = 0;

                    double directionToMouse = Math.Atan2(Y - mousePosition.Item2, X - mousePosition.Item1);

                    Move(0.075, directionToMouse + Math.PI, gameWorld);

                    Mob kck = new KickTrace(contentManager, X, Y + 4.5, Z, 6, 0, directionToMouse + Math.PI, 21, gameWorld);
                    kck.Move(0.2, directionToMouse + Math.PI, gameWorld);
                    
                    gameWorld.currentRoom.AddMob(kck);
                    var npcs = gameWorld.currentRoom.MobsByTypes["NPC"];

                    foreach (var currentMob in npcs)
                    {
                        double dist = GameWorld.GetDist(X, Y, currentMob.X, currentMob.Y);

                        if (dist <= Radius + currentMob.Radius + 2.5)
                        {
                            double directionToMob = Math.Atan2(Y - currentMob.Y, X - currentMob.X);

                            if (Math.Abs(directionToMouse - directionToMob) <= 2.7)
                            {
                                ((NPC)currentMob).Damage(contentManager, gameWorld, 1);
                            }
                        }
                    }
                }

                if (Action != "at1" && ms.MiddleButton == ButtonState.Pressed && AttackEnergy >= 30)
                {
                    AttackEnergy = 0;

                    double directionToMouse = Math.Atan2(Y - mousePosition.Item2, X - mousePosition.Item1);

                    Move(0.1, directionToMouse + Math.PI, gameWorld);

                    Mob kck = new KickTrace(contentManager, X, Y + 4.5, Z, 7, 0, directionToMouse + Math.PI, 20, gameWorld);
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

                if (Action != "at1" && ms.LeftButton == ButtonState.Pressed && selectedItem == null)
                {
                    Action = "wa";

                    Direction = Math.Atan2(mousePosition.Item2 - Y, mousePosition.Item1 - X);

                    Move(realSpeed, Direction, gameWorld);
                }

                if (Action != "at1" && ks.IsKeyDown(Keys.C))
                {
                    Action = "focus";

                    FlyEnergy++;
                }
                else if (Action != "at1" && prevC)
                {
                    if (FlyEnergy >= 120)
                    {
                        Action = "fly";
                    }
                    else
                        FlyEnergy = 0;
                }
            }

            prevC = ks.IsKeyDown(Keys.C);

            if(Action=="fly")
            {
                bool mtz = !Move(Speed * 2, Direction, gameWorld);

                if (X > 0 && Y > 0 && X < Room.roomSize && Y < Room.roomSize && mtz)
                {
                    Action = "id";
                }
            }
            else
                UpdateGravitation(gameWorld);

            TimeSinceLastTextureUpdate++;

            if (Action != "wa")
            {
                if (TimeSinceLastTextureUpdate > GameWorld.TextureUpdateSpeed)
                {
                    if (Action == "put" && !putOn && TextureNumber == Textures.Count - 1 && Inventory[0] != null) 
                    {
                        Inventory[0].ChangeCoords(X, Y + 0.001, Z);

                        gameWorld.currentRoom.AddMob(Inventory[0]);

                        WeightToCarry -= Inventory[0].Weight;

                        Inventory[0] = null;
                    }

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

            if (Action == "put")
            {
                if (TextureNumber != 3)
                    ySub = Textures[TextureNumber].Height - 5;
                else
                    ySub = 15;
            }

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

            if (Inventory[0] != null)
            {
                Inventory[0].DrawInterface(spriteBatch);
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
                    result += "null\n";
                }
            }

            return result;
        }

        public void Damage(ContentManager contentManager, GameWorld gameWorld, int power)
        {
            if (Inventory[0] != null)
            {
                ((Shell)Inventory[0]).HP -= power;

                if (((Shell)Inventory[0]).HP <= 0)
                {
                    Inventory[0] = null;
                }
            }
            else
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
}   