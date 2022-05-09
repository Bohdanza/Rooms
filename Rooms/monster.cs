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
        public int TimeSinceLastAttack = 0;
        public List<int> AttackDelays = new List<int>();
        protected int currentDelay = 0;
        private bool LineClearedP=false;
        protected List<Bullet> bulletsShot;
        protected List<Tuple<Mob, int>> Loot;
        private bool looted = false;

        public NPC(ContentManager contentManager, GameWorld gameWorld, double x, double y, double z, int type, double speed, int HP, int maxHP)
        {
            MaxHP = maxHP;
            this.HP = HP;

            Speed = speed;
            
            ChangeCoords(x, y, z);

            Type = type;

            Action = "id";

            if (HP <= 0)
            {
                Action = "di";
            }

            bulletsShot = new List<Bullet>();

            using (StreamReader sr = new StreamReader(@"info\#global\monsters\" + Type.ToString() + ".bulletinfo"))
            {
                List<string> inp = sr.ReadToEnd().Split('\n').ToList();

                int count = Int32.Parse(inp[0]);

                int delayCount = Int32.Parse(inp[1]);

                for (int i = 0; i < delayCount; i++)
                    AttackDelays.Add(Int32.Parse(inp[i + 2]));

                for (int i = 0; i < count; i++)
                {
                    bulletsShot.Add(new Bullet(contentManager, double.Parse(inp[i * 6 + 2 + delayCount]), 0, 0, 0,
                        Int32.Parse(inp[i * 6 + 3 + delayCount]), Int32.Parse(inp[i * 6 + 4 + delayCount]),
                        this, double.Parse(inp[i * 6 + 5 + delayCount]),
                        double.Parse(inp[i * 6 + 6 + delayCount]), int.Parse(inp[i * 6 + 7 + delayCount])));
                }
            }

            Loot = new List<Tuple<Mob, int>>();

            using (StreamReader sr = new StreamReader(@"info\#global\monsters\" + Type.ToString() + ".loot"))
            {
                List<string> input = sr.ReadToEnd().Split('\n').ToList();

                int n = Int32.Parse(input[0]);
                int currentString = 1;

                while (currentString < input.Count)
                {
                    int prob = Int32.Parse(input[currentString]);
                    Mob added = Mob.Loader(contentManager, currentString + 1, input);

                    currentString += 1 + added.SaveList().Count(f => (f == '\n'));

                    Loot.Add(new Tuple<Mob, int>(added, prob));
                }

            }

            updateTexture(contentManager, true);
        }
       
        public NPC(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]), double.Parse(input[currentStr + 4]));

            Type = Int32.Parse(input[currentStr + 5]);

            HP = Int32.Parse(input[currentStr + 6]);
            MaxHP = Int32.Parse(input[currentStr + 7]);

            Speed = double.Parse(input[currentStr + 8]);
            
            looted = bool.Parse(input[currentStr + 9]);

            bulletsShot = new List<Bullet>();

            using (StreamReader sr = new StreamReader(@"info\#global\monsters\" + Type.ToString() + ".bulletinfo"))
            {
                List<string> inp = sr.ReadToEnd().Split('\n').ToList();

                int count = Int32.Parse(inp[0]);

                int delayCount = Int32.Parse(inp[1]);

                for (int i = 0; i < delayCount; i++)
                    AttackDelays.Add(Int32.Parse(inp[i + 2]));

                for (int i = 0; i < count; i++)
                {
                    bulletsShot.Add(new Bullet(contentManager, double.Parse(inp[i * 6 + 2+delayCount]), 0, 0, 0,
                        Int32.Parse(inp[i * 6 + 3 + delayCount]), Int32.Parse(inp[i * 6 + 4 + delayCount]), 
                        this, double.Parse(inp[i * 6 + 5 + delayCount]),
                        double.Parse(inp[i * 6 + 6 + delayCount]), int.Parse(inp[i * 6 + 7 + delayCount])));
                }
            }

            Loot = new List<Tuple<Mob, int>>();

            using (StreamReader sr = new StreamReader(@"info\#global\monsters\" + Type.ToString() + ".loot"))
            {
                List<string> inp = sr.ReadToEnd().Split('\n').ToList();

                int n = Int32.Parse(inp[0]);
                int currentString = 1;

                while (currentString < inp.Count)
                {
                    int prob = Int32.Parse(inp[currentString]);
                    Mob added = Mob.Loader(contentManager, currentString + 1, inp);

                    currentString += 1 + added.SaveList().Count(f => (f == '\n'));

                    Loot.Add(new Tuple<Mob, int>(added, prob));
                }

            }

            Action = "id";

            if (HP <= 0)
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

            bool lineCleared = false;

            if (Action != "di")
            {
                if (Math.Round(Z) == Math.Round(gameWorld.currentRoom.heroReference.Z))
                {
                    lineCleared = gameWorld.currentRoom.LineIsClear((int)Math.Round(X), (int)Math.Round(Y),
                    (int)Math.Round(gameWorld.currentRoom.heroReference.X), (int)Math.Round(gameWorld.currentRoom.heroReference.Y),
                    (int)Math.Round(Z));
                }

                if (lineCleared && TimeSinceLastAttack >= AttackDelays[currentDelay])
                {
                    if (!LineClearedP)
                    {
                        //dtc stands for detect
                        Action = "dtc";
                    }

                    if (Action != "dtc" && Action != "at" && Action != "di")
                    {
                        TimeSinceLastAttack = 0;

                        double dir = GameWorld.GetDirection(X, Y,
                            gameWorld.currentRoom.heroReference.X, gameWorld.currentRoom.heroReference.Y);

                        Direction = (float)dir;

                        Action = "at";

                        currentDelay++;
                        currentDelay %= AttackDelays.Count;

                        foreach (var currentBullet in bulletsShot)
                        {
                            gameWorld.currentRoom.AddMob(new Bullet(contentManager, dir + Math.PI + currentBullet.Direction,
                                X, Y, Z, currentBullet.Type, currentBullet.Damage, this, currentBullet.Radius,
                                currentBullet.Speed, ((Bullet)currentBullet).Lifetime));
                        }
                    }
                }
                else
                {
                    if (rnd.Next(0, 1000) < 20)
                    {
                        float addToDirection = 0.872f;

                        if (rnd.Next(0, 2) == 0)
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

                        if (rnd.Next(0, 1000) < 15)
                        {
                            Action = "id";
                        }

                        if (!moved)
                        {
                            //Direction += (float)Math.PI;

                            ZVector = 0.5;
                        }
                        else if (Z <= 1 && (int)Math.Round(X) < Room.roomSize && (int)Math.Round(Y) < Room.roomSize &&
                            !gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y), 
                            Math.Max(0, (int)Math.Floor(Z-1))].Rigid)
                        {
                            Direction += (float)Math.PI;

                            Move(Speed, Direction, gameWorld);
                        }
                    }
                }

                UpdateGravitation(gameWorld);

                LineClearedP = lineCleared;
            }

            TimeSinceLastTextureUpdate++;
            TimeSinceLastAttack++;

            if (Action == "di" && TextureNumber == Textures.Count - 1&&!looted)
            {
                looted = true;

                foreach(var currentItem in Loot)
                {
                    int prob = rnd.Next(0, 100);

                    if (prob < currentItem.Item2)
                    {
                        Mob mobToAdd = Mob.Loader(contentManager, 0,
                            currentItem.Item1.SaveList().Split('\n').ToList());

                        mobToAdd.ChangeCoords(X + rnd.NextDouble() - 0.5, Y + rnd.NextDouble() - 0.5, Z);

                        gameWorld.currentRoom.AddMob(mobToAdd);
                    }
                }
            }

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

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            if (Direction >= Math.PI * 2)
                Direction %= (float)(Math.PI * 2);
            else if (Direction < 0)
                Direction = (float)Math.PI * 2 + Direction;

            if (Direction > 0.5 * Math.PI && Direction < Math.PI * 1.5)
                base.Draw(spriteBatch, x, y);
            else
                base.Draw(spriteBatch, x, y, SpriteEffects.FlipHorizontally);
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

                    if(Action=="dm"||Action=="at"||Action=="dtc")
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

            result += looted.ToString();
            result += "\n";

            return result;
        }
    }
}