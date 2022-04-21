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
        public const int roomSize = 52;
        public const int roomSizeZ = 5;

        public int biome { get; protected set; }

        public int X { get; protected set; }
        public int Y { get; protected set; } 
        public Block[,,] blocks;

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

            blocks = new Block[roomSize, roomSize, roomSizeZ];
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

            for (int k = 0; k < roomSizeZ; k++)
                for (int i = 0; i < roomSize; i++)
                {
                    List<string> lst = input[k*roomSize+i].Split("|").ToList();

                    for (int j = 0; j < roomSize; j++)
                    {
                        blocks[i, j, k] = new Block(contentManager, Int32.Parse(lst[j]));
                    }
                }

            int mobsCount = Int32.Parse(input[roomSize * roomSizeZ]);
            int currentString=roomSize*roomSizeZ+1, mobsAdded=0;

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

            for (int k = 0; k < roomSizeZ; k++)
                for (int i = 0; i < roomSize; i++)
                {
                    for (int j = 0; j < roomSize; j++)
                    {
                        output += blocks[i, j, k].Type.ToString();

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

            //0-golden forest, 1-village, 2-cryzualis, 4-clouds, 5-Femuhiblu desert
            int biome=0;

            var rnd = new Random();
            int prob = rnd.Next(0, 100);

            if (prob <= 33)
            {
                biome = 0;
            }
            else if (prob <= 33 + 10)
            {
                biome = 1;
            }
            else if (prob <= 43 + 20)
            {
                biome = 5;
            }

            int minIslandRad = 5, maxIslandRad = 26;

            int IslandRad = rnd.Next(minIslandRad, maxIslandRad);

            //speedup
            List<Tuple<int, int, int>> groundBlocks = new List<Tuple<int, int, int>>();

            //filling with 0-blocks
            for (int k = 0; k < roomSizeZ; k++)
                for (int i = 0; i < roomSize; i++)
                    for (int j = 0; j < roomSize; j++)
                    {
                        blocks[i, j, k] = new Block(contentManager, 1);

                        if (k == 0 && GameWorld.GetDist(roomSize / 2, roomSize / 2, i, j) <= IslandRad - rnd.Next(0, 5))
                        {
                            blocks[i, j, k] = new Block(contentManager, 0);

                            groundBlocks.Add(new Tuple<int, int, int>(i, j, k));

                            if (biome == 0 && rnd.Next(0, 1000) < 5)
                            {
                                AddMob(new NPC(contentManager, gameWorld, i, j, 1, 0.075, 10, 10));
                            }
                            else if (rnd.Next(0, 1000) < 5)
                            {
                                //coins
                                AddMob(new Item(contentManager, i + rnd.NextDouble() * 0.75 - 0.375, j + rnd.NextDouble() * 0.75 - 0.375, 3, 1));
                            }
                        }

                        int blockChangeProb = rnd.Next(0, 100);
                    }

            int mountRad = rnd.Next(5, 10);

            groundBlocks.AddRange(PlaceMountain(contentManager, roomSize / 2 + rnd.Next(0, 10) - 5, 
                roomSize / 2 - rnd.Next(0, 5), 5, mountRad, mountRad / 5, 0));

            //collision map
            foreach (var currentTuple in groundBlocks)
            {
                int i = currentTuple.Item1;
                int j = currentTuple.Item2;
                int k = currentTuple.Item3;

                if (blocks[i, j, k].Type == 0)
                {
                    int newType = 11;

                    if (blocks[i - 1, j, k].Passable)
                    {
                        newType = 14;
                    }

                    if (blocks[i, j - 1, k].Passable)
                    {
                        if (newType == 11)
                            newType = 13;

                        if (newType == 14)
                            newType = 4;
                    }

                    if (blocks[i + 1, j, k].Passable)
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

                    if (blocks[i, j + 1, k].Passable)
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

                    blocks[i, j, k] = new Block(contentManager, newType);
                }
            }

            //forest
            if (biome == 0)
            {
                int xcent = rnd.Next(roomSize / 2 - IslandRad / 2, roomSize / 2 + IslandRad / 2);
                int ycent = rnd.Next(roomSize / 2 - IslandRad / 2, roomSize / 2 + IslandRad / 2);
                int maxLayer = rnd.Next(2, 7);

                for (int layer = 0; layer < maxLayer; layer++)
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

                            double Xplace = Math.Cos(angle) * dist + xcent;
                            double Yplace = Math.Sin(angle) * dist + ycent;

                            currentlyPlaced++;

                            int height = 0;

                            while (height < roomSizeZ - 1 && blocks[(int)Xplace, (int)Yplace, height + 1].Type != 1)
                            {
                                height++;
                            }

                            if (blocks[(int)Xplace, (int)Yplace, height].Type == 0)
                                blocks[(int)Xplace, (int)Yplace, height] = new Block(contentManager, 18);
                        }
                    }
                }
            }
            
            //village-
            if (biome==1)
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
                    int YDelay = 0;

                    if (!mobs[currentMob].Flying&& (int)Math.Round(mobs[currentMob].X) < roomSize && (int)Math.Round(mobs[currentMob].X) > 0)
                    {
                        YDelay = -blocks[(int)Math.Round(mobs[currentMob].X), (int)(j - 0.5), 0].Textures[0].Height
                            - (int)(mobs[currentMob].Z * GameWorld.BlockSizeZ);
                    }

                    mobs[currentMob].Draw(spriteBatch, x + (int)(mobs[currentMob].X * GameWorld.BlockSizeX),
                        y + GameWorld.BlockSizeY + (int)(mobs[currentMob].Y * GameWorld.BlockSizeY)+YDelay);

                    currentMob++;
                }
                else
                {
                    if (j < Room.roomSize)
                    {
                        for (int k = 0; k < roomSizeZ; k++)
                        {
                            for (int i = 0; i < roomSize; i++)
                            {
                                blocks[i, j, k].Draw(spriteBatch, x + i * GameWorld.BlockSizeX - GameWorld.BlockSizeX / 2,
                                    y + j * GameWorld.BlockSizeY - GameWorld.BlockSizeY / 2 - k * GameWorld.BlockSizeZ);
                            }
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

            for (int k = 0; k < roomSizeZ; k++)
                for (int i = 0; i < roomSize; i++)
                    for (int j = 0; j < roomSize; j++)
                    {
                        blocks[i, j, k].Update(contentManager);
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
        protected void placeRectagle(ContentManager contentManager, int x1, int y1, int x2, int y2, int z, int blockType)
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
                    blocks[i, j, z] = new Block(contentManager, blockType);
                }
            }
        }
        
        protected void placeRectagle(ContentManager contentManager, int x1, int y1, int x2, int y2, int z1, int z2, int blockType)
        {
            x1 = Math.Max(x1, 0);
            y1 = Math.Max(y1, 0);
            x2 = Math.Max(x2, 0);
            y2 = Math.Max(y2, 0);
            z1 = Math.Max(x2, 0);
            z2 = Math.Max(y2, 0);

            x1 = Math.Min(x1, roomSize);
            y1 = Math.Min(y1, roomSize);
            x2 = Math.Min(x2, roomSize);
            y2 = Math.Min(y2, roomSize);
            z1 = Math.Min(x2, roomSize);
            z2 = Math.Min(y2, roomSize);

            for (int k = z1; k < z2; k++)
                for (int i = x1; i < x2; i++)
                    for (int j = y1; j < y2; j++)
                        blocks[i, j, k] = new Block(contentManager, blockType);
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <param name="step"></param>
        /// <param name="blockType"></param>
        /// <returns>List of placed blocks coordinates</returns>
        protected List<Tuple<int, int, int>> PlaceMountain(ContentManager contentManager, int x, int y, int height, int radius, int step, int blockType)
        {
            List<Tuple<int, int, int>> toReturn = new List<Tuple<int, int, int>>();
            var rnd = new Random();

            for (int k = 0; k < height; k++)
            {
                for (int i = Math.Max(x - radius, 0); i < Math.Min(roomSize, x + radius); i++)
                    for (int j = Math.Max(y - radius, 0); j < Math.Min(roomSize, y + radius); j++)
                        if (GameWorld.GetDist(x, y, i, j) <= radius - rnd.Next(0, 2) && (k == 0 || blocks[i, j, k - 1].Type == blockType))
                        {
                            blocks[i, j, k] = new Block(contentManager, blockType);

                            toReturn.Add(new Tuple<int, int, int>(i, j, k));
                        }

                radius -= step;
            }

            return toReturn;
        }
    }
}