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
    public class Room
    {
        public const int roomSize = 128;

        public int biome { get; protected set; }

        public int X { get; protected set; }
        public int Y { get; protected set; } 
        public Block[,] blocks;

        private GameWorld worldReference { get;  set; }
        public Hero heroReference { get; protected set; }
        public List<Mob> mobs { get; protected set; }

        private List<int> markedMobs { get; set; } = new List<int>();
        public List<Mob> mobsWithInterface { get; protected set; }

        public Room(ContentManager contentManager, int x, int y, GameWorld gameWorld, Hero heroMoved)
        {
            X = x;
            Y = y;

            worldReference = gameWorld;

            blocks = new Block[roomSize, roomSize];
            mobs = new List<Mob>();

            heroReference = null;

            try
            { 
                Load(contentManager, x, y, gameWorld);
            }
            catch
            {
                Generate(contentManager, x, y, gameWorld);
            }

            if (heroReference == null)
            {
                heroReference = heroMoved;

                AddMob(heroMoved);
            }

            mobsWithInterface = new List<Mob>();

            mobsWithInterface.Add(heroReference);
        }

        protected void Load(ContentManager contentManager, int x, int y, GameWorld gameWorld)
        {
            List<string> input;

            using (StreamReader sr = new StreamReader(@"info\" + gameWorld.Name + @"\rooms\" + x.ToString() + "_" + y.ToString() + ".rr"))
            {
                input = sr.ReadToEnd().Split('\n').ToList();
            }

            for (int i = 0; i < roomSize; i++)
            {
                List<string> lst = input[i].Split("|").ToList();

                for (int j = 0; j < roomSize; j++)
                {
                    blocks[i, j] = new Block(contentManager, Int32.Parse(lst[j]));
                }
            }

            int mobsCount = Int32.Parse(input[roomSize]);
            int currentString=roomSize+1, mobsAdded=0;

            for (mobsAdded=0; mobsAdded < mobsCount; mobsAdded++)
            {
                Mob newMob = Mob.Loader(contentManager, currentString, input);

                if (newMob != null)
                {
                    string str = newMob.SaveList();

                    currentString += newMob.SaveList().Count(f => (f == '\n'));

                    AddMob(newMob);
                    
                    if(str.StartsWith("Hero"))
                    {
                        heroReference = (Hero)newMob;
                    }
                }
            }
        }

        public void Save()
        {
            //getting path ready
            if (!Directory.Exists(@"info\" + worldReference.Name + @"\rooms"))
                Directory.CreateDirectory(@"info\" + worldReference.Name + @"\rooms");

            //forming string
            string output = "";

            for (int i = 0; i < roomSize; i++)
            {
                for (int j = 0; j < roomSize; j++)
                {
                    output += blocks[i, j].Type.ToString();

                    output += "|";
                }

                output += '\n';
            }

            string tmpout = "";
            int realMobs = 0;

            for (int i = 0; i < mobs.Count; i++)
            {
                string csave = mobs[i].SaveList();

                if (csave.Length > 0)
                {
                    realMobs++;

                    if (csave[csave.Length - 1] != '\n')
                    {
                        csave += "\n";
                    }
                }

                tmpout += csave;
            }

            output += realMobs.ToString();
            output += "\n";

            output += tmpout;

            //writing   
            using (StreamWriter sw = new StreamWriter(@"info\" + worldReference.Name + @"\rooms\" + X.ToString() + "_" + Y.ToString() + ".rr"))
            {
                sw.Write(output);
            }
        }

        protected void Generate(ContentManager contentManager, int x, int y, GameWorld gameWorld)
        {
            bool IsFirstTemple = x == 0 && y == 0;
            bool IsVillage = false;

            var rnd = new Random();

            if (rnd.Next(0, 100) < 20&&!IsFirstTemple)
            {
                IsVillage = true;
            }

            int IslandRad = rnd.Next(5, roomSize / 4);

            //filling with 0-blocks and walls
            for (int i = 0; i < roomSize; i++)
                for (int j = 0; j < roomSize; j++)
                {
                    blocks[i, j] = new Block(contentManager, 1);

                    if (GameWorld.GetDist(roomSize / 2, roomSize / 2, i, j) <= IslandRad + rnd.Next(0, 5))
                    {
                        blocks[i, j] = new Block(contentManager, 0);

                        if (!IsVillage && rnd.Next(0, 1000) < 5)
                        {
                            AddMob(new NPC(contentManager, gameWorld, i, j, 1, 0.075, 10, 10));
                        }
                        else if (!IsVillage && rnd.Next(0, 1000) < 5)
                        {
                            //coins
                            AddMob(new Item(contentManager, i + rnd.NextDouble() * 0.75 - 0.375, j + rnd.NextDouble() * 0.75 - 0.375, 3, 1));
                        }
                    }

                    int blockChangeProb = rnd.Next(0, 100);
                }

            //normal borders
            for (int i = 1; i < roomSize-1; i++)
                for (int j = 1; j < roomSize-1; j++)
                    if (blocks[i, j].Type == 0)
                    {
                        int newType = 11;

                        if (blocks[i - 1, j].Passable)
                        {
                            newType = 14;
                        }

                        if (blocks[i, j - 1].Passable)
                        {
                            if (newType == 11)
                                newType = 13;

                            if(newType==14)
                                newType = 4;
                        }

                        if (blocks[i + 1, j].Passable)
                        {
                            if (newType == 11)
                                newType = 15;

                            if (newType == 13)
                                newType = 3;

                            if (newType == 14)
                                newType = 16;

                            if (newType == 4)
                                newType = 10;
                        }

                        if (blocks[i, j + 1].Passable)
                        {
                            if (newType == 11)
                                newType = 12;

                            if (newType == 14)
                                newType = 6;

                            if (newType == 13)
                                newType = 17;

                            if (newType == 4)
                                newType = 7;

                            if (newType == 15)
                                newType = 5;

                            if (newType == 3)
                                newType = 8;

                            if (newType == 16)
                                newType = 9;
                            
                            if (newType == 10)
                                newType = 0;
                        }

                        blocks[i, j] = new Block(contentManager, newType);
                    }

            int placeBeholder = rnd.Next(0, 100);
            
            if (placeBeholder < -1)
            {
                int xb = rnd.Next(0, roomSize);
                int yb = rnd.Next(0, roomSize);
                
                if(blocks[xb, yb].Passable)
                {
                    AddMob(new NPC(contentManager, gameWorld, xb + 0.5, yb + 0.5, 5, 0.1, 20, 20));
                }
            }

            if (!IsVillage)
            {
                //Adding sharokz group
                if (rnd.Next(0, 100) < 70)
                {
                    double xGroup = rnd.NextDouble() * Room.roomSize;
                    double yGroup = rnd.NextDouble() * Room.roomSize;

                    ControlCenter comCenter = new ControlCenter(xGroup, yGroup, 0, 6, 0.11);

                    int groupSize = rnd.Next(3, 13);

                    for (int i = 0; i < groupSize; i++)
                    {
                        double xUnit = xGroup - (rnd.NextDouble() - 0.5) * 6;
                        double yUnit = yGroup - (rnd.NextDouble() - 0.5) * 6;

                        if ((int)xUnit >= 0 && (int)xUnit < Room.roomSize &&
                            (int)yUnit >= 0 && (int)yUnit < Room.roomSize &&
                            blocks[(int)xUnit, (int)yUnit].Passable)
                        {
                            var mbadd = new NPC(contentManager, gameWorld, xUnit, yUnit, 22, 0.12, 50, 50);

                            comCenter.AddMob(mbadd);

                            AddMob(mbadd);
                        }

                        AddMob(comCenter);
                    }
                }
            }
            
            //village-
            if (IsVillage)
            {
                for (int layer = 0; layer < 4; layer++)
                {
                    int dist = layer * 3 + 6;
                    int count = rnd.Next(3 + layer * 2, 6 + (int)(layer * 1.5));
                    int currentlyPlaced = 0;
                    List<Vector2> forbiddenPositions = new List<Vector2>();

                    while (currentlyPlaced < count)
                    {
                        float angle = (float)(rnd.NextDouble() * Math.PI * 2);
                        bool canBeUsed = true;

                        for (int i = 0; i < forbiddenPositions.Count && canBeUsed; i++)
                        {
                            if (angle < forbiddenPositions[i].Y && angle > forbiddenPositions[i].X)
                            {
                                canBeUsed = false;
                            }
                        }

                        if (canBeUsed)
                        {
                            forbiddenPositions.Add(new Vector2(angle - ((float)Math.PI / count),
                                angle + ((float)Math.PI / count)));

                            double Xplace = Math.Cos(angle) * dist + roomSize / 2;
                            double Yplace = Math.Sin(angle) * dist + roomSize / 2;

                            currentlyPlaced++;

                            Mob hutToAdd;

                            if (layer == 0 && rnd.Next(0, 100) < 33)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 10);
                            }
                            else if (layer == 0 && rnd.Next(0, 100) < 10)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 13);
                            }
                            else if ((layer == 1 || layer == 2) && rnd.Next(0, 100) < 25)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 13);
                            }
                            else if (rnd.Next(0, 100) < layer * 15)
                            {
                                hutToAdd = new Decoration(contentManager, (int)Math.Round(Xplace), (int)Math.Round(Yplace), 14);

                                //blocks[(int)Math.Round(Xplace), (int)Math.Round(Yplace)] = new Block(contentManager, 5);
                            }
                            else if (rnd.Next(0, 100) <= -5 * (layer - 2) * (layer - 2) + 20)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 16);
                            }
                            else if (rnd.Next(0, 100) <= layer * layer * 2.5)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 15);
                            }
                            else if (rnd.Next(0, 100) <= -4 * (layer - 2) * (layer - 2) + 15)
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 17);
                            }
                            else
                            {
                                hutToAdd = new Decoration(contentManager, Xplace, Yplace, 8);

                                if (rnd.Next(0, 100) < 33)
                                {
                                    AddMob(new NPC(contentManager, gameWorld, Xplace, Yplace, 11, 0.05, 30, 30));
                                }
                                else if(rnd.Next(0, 100)<20)
                                {
                                    AddMob(new Trader(contentManager, Xplace+(rnd.NextDouble()-0.5)*2, Yplace + 1, 9, gameWorld));
                                }
                            }

                            AddMob(hutToAdd);
                        }
                    }
                }
            }

            //the beginning
            if(IsFirstTemple)
            {
                AddMob(new Decoration(contentManager, roomSize / 2, roomSize / 2, 20));

                AddMob(new Speaker(contentManager, gameWorld, roomSize / 2, roomSize / 2 + 1, 21, 0));
            }
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            int currentMob = 0, j = 0;

            while (j < roomSize || currentMob < mobs.Count)
            {
                if (currentMob < mobs.Count && mobs[currentMob].Y < j - 0.5)
                {
                    mobs[currentMob].Draw(spriteBatch, x + (int)(mobs[currentMob].X * GameWorld.BlockSizeX), y + (int)(mobs[currentMob].Y * GameWorld.BlockSizeY));

                    currentMob++;
                }
                else
                {
                    if (j < Room.roomSize)
                    {
                        for (int i = 0; i < roomSize; i++)
                        {
                            blocks[i, j].Draw(spriteBatch, x + i * GameWorld.BlockSizeX - GameWorld.BlockSizeX / 2, y + j * GameWorld.BlockSizeY - GameWorld.BlockSizeY / 2);
                        }
                    }

                    j++;
                }
            }

            foreach(var currentMobInterface in mobsWithInterface)
            {
                currentMobInterface.DrawInterface(spriteBatch);
            }

           // heroReference.DrawInterface(spriteBatch);
        }

        public void Update(ContentManager contentManager, GameWorld gameWorld)
        { 
            for (int i = 0; i < mobs.Count; i++)
            {
                if (mobs[i] != null)
                {
                    mobs[i].Update(contentManager, gameWorld);
                }
            }

            for (int i = 0; i < roomSize; i++)
                for (int j = 0; j < roomSize; j++)
                {
                    blocks[i, j].Update(contentManager);
                }

            DeleteMarked();

            mobs.Sort((a, b) => a.Y.CompareTo(b.Y));
        }

        //used to get mouse cordinates in room's coord system. Can work incorrectly if Draw is called with push
        public Tuple<double, double> GetMouseCordinates(GameWorld gameWorld)
        {
            var ms = Mouse.GetState();

            return new Tuple<double, double>((double)(ms.X - gameWorld.DrawX) / GameWorld.BlockSizeX, (double)(ms.Y - gameWorld.DrawY) / GameWorld.BlockSizeY);
        }

        //methods below are used to edit room "landscape"

        /// <summary>
        /// Used to replace all blocks in area ([x1, x2); [y1, y2)) with block of given type. 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="blockType"></param>
        protected void placeRectagle(ContentManager contentManager, int x1, int y1, int x2, int y2, int blockType)
        {
            x1 = Math.Max(x1, 0);
            y1 = Math.Max(y1, 0);
            x2 = Math.Max(x2, 0);
            y2 = Math.Max(y2, 0);

            x1 = Math.Min(x1, roomSize);
            y1 = Math.Min(y1, roomSize);
            x2 = Math.Min(x2, roomSize);
            y2 = Math.Min(y2, roomSize);

            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    blocks[i, j] = new Block(contentManager, blockType);
                }
            }
        }

        //methods below are used to edit mob list indirectly (useful when editing from other classes)
        public void AddMob(Mob mob)
        {
            mobs.Add(mob);

            mobs.Sort((a, b) => a.Y.CompareTo(b.Y));
        }

        /// <summary>
        /// Used to mark mob and later delete it with DeleteMarked 
        /// </summary>
        /// <param name="index"></param>
        public void MarkMobIndexAsDeleted(int index)
        {
            mobs[index] = null;

            markedMobs.Add(index);
        }
        
        /// <summary>
        /// Used to mark mob and later delete it with DeleteMarked 
        /// </summary>
        /// <param name="index"></param>
        public void MarkMobAsDeleted(Mob mob)
        { 
            int ind = mobs.IndexOf(mob);

            MarkMobIndexAsDeleted(ind);
        }

        public void DeleteMarked()
        {
            markedMobs.Sort();

            for (int i = 0; i < markedMobs.Count; i++)
            {
                mobs.RemoveAt(markedMobs[i] - i);
            }

            markedMobs = new List<int>();
        }

        public Mob GetClosestMob(double x, double y, List<Mob> ignoredMobs, List<string> allowedTypes)
        {
            double cdist = 1e9;
            Mob closestMob = null;

            for (int i = 0; i < mobs.Count; i++)
            {
                double dst = GameWorld.GetDist(x, y, mobs[i].X, mobs[i].Y);

                if (cdist > dst 
                    && !ignoredMobs.Contains(mobs[i]) 
                    && allowedTypes.Any(s=>mobs[i].SaveList().StartsWith(s)))
                {
                    cdist = dst;

                    closestMob = mobs[i];
                }
            }

            return closestMob;
        }

        public Mob GetClosestMob(double x, double y, List<Mob> ignoredMobs, List<Mob> allowedMobs)
        {
            double cdist = 1e9;
            Mob closestMob = null;

            for (int i = 0; i < mobs.Count; i++)
            {
                double dst = GameWorld.GetDist(x, y, mobs[i].X, mobs[i].Y);

                if (cdist > dst
                    && !ignoredMobs.Contains(mobs[i])
                    && allowedMobs.Contains(mobs[i]))
                {
                    cdist = dst;

                    closestMob = mobs[i];
                }
            }

            return closestMob;
        }
    }
}