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
    public class Room
    {
        public const int roomSize = 52;
        public const int roomSizeZ = 7;

        public int biome { get; protected set; }

        public int X { get; protected set; }
        public int Y { get; protected set; }
        public Block[,,] blocks;

        private GameWorld worldReference { get; set; }
        public Hero heroReference { get; protected set; }
        public List<Mob> mobs { get; protected set; }

        private List<int> markedMobs { get; set; } = new List<int>();
        public List<Mob> mobsWithInterface { get; protected set; }
        public Dictionary<string, List<Mob>> MobsByTypes { get; protected set; } = new Dictionary<string, List<Mob>>();

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
                    List<string> lst = input[k * roomSize + i].Split("|").ToList();

                    for (int j = 0; j < roomSize; j++)
                    {
                        blocks[i, j, k] = new Block(contentManager, Int32.Parse(lst[j]));
                    }
                }

            int mobsCount = Int32.Parse(input[roomSize * roomSizeZ]);
            int currentString = roomSize * roomSizeZ + 1, mobsAdded = 0;

            for (mobsAdded = 0; mobsAdded < mobsCount; mobsAdded++)
            {
                Mob newMob = Mob.Loader(contentManager, currentString, input);

                if (newMob != null)
                {
                    string str = newMob.SaveList();

                    currentString += newMob.SaveList().Count(f => (f == '\n'));

                    AddMob(newMob);

                    if (str.StartsWith("Hero"))
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
            //0-organic village, 1-garbage, 3-wild
            int biome = 0;

            var rnd = new Random();
            int prob = rnd.Next(0, 100);

            if (prob <= 50)
            {
                biome = 3;
            }
            else if (prob <= 100)
            {
                biome = 0;
            }
            else
            {
                biome = 2;
            }

            int minIslandRad = 12, maxIslandRad = 17;

            int IslandRad = rnd.Next(minIslandRad, maxIslandRad);

            //speedup
            List<Tuple<int, int, int>> groundBlocks = new List<Tuple<int, int, int>>();

            //filling with 0-blocks
            for (int k = 0; k < roomSizeZ; k++)
                for (int i = 0; i < roomSize; i++)
                    for (int j = 0; j < roomSize; j++)
                    {
                        blocks[i, j, k] = new Block(contentManager, 1);
                    }

            int pr = (IslandRad + (rnd.Next(0, 9) - 4));

            int px = roomSize / 2 + (int)(Math.Cos(0) * pr);
            int py = roomSize / 2 + (int)(Math.Sin(0) * pr);

            int bx = px;
            int by = py;

            List<int> radiuses = new List<int>();

            radiuses.Add(pr);

            //generating island and it's points
            for (int i = 1; i < 24; i++)
            {
                int lrad = Math.Min(IslandRad + 4, Math.Max(IslandRad - 4, radiuses[i - 1] + rnd.Next(-2, 3)));

                int nx = roomSize / 2 + (int)(Math.Cos(i * Math.PI * 2 / 24) * lrad);
                int ny = roomSize / 2 + (int)(Math.Sin(i * Math.PI * 2 / 24) * lrad);

                groundBlocks.AddRange(PlaceLine(contentManager, px, py, nx, ny, 0, 0));

                radiuses.Add(lrad);

                px = nx;
                py = ny;
            }

            groundBlocks.AddRange(
                PlaceLine(contentManager, px, py, bx, by, 0, 0));

            Fill(contentManager, roomSize / 2, roomSize / 2, 0, 0);

            if (biome == 3)
            {
                //mountains
                var newBlocks = new List<Tuple<int, int, int>>();

                int mountainCount = rnd.Next(3, 10);

                for (int i = 0; i < mountainCount; i++)
                {
                    int ci = rnd.Next(0, groundBlocks.Count);

                    newBlocks.AddRange(PlaceMountain(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2,
                        6, rnd.Next(8, 10), 1, 0));
                }

                groundBlocks.AddRange(newBlocks);

                int mCount = rnd.Next(2, 7);
                int current = 0;

                while(current<mCount)
                {
                    int xmb = rnd.Next(0, roomSize);
                    int ymb = rnd.Next(0, roomSize);
                    
                    if(blocks[xmb, ymb, 0].Rigid)
                    {
                        current++;

                        int zmb = 1;

                        while(blocks[xmb, ymb, zmb].Rigid)
                        {
                            zmb++;
                        }
                        
                        AddMob(new NPC(contentManager, gameWorld, xmb, ymb, zmb, 2, 0.1, 1, 1));
                    }
                }

                mCount = rnd.Next(0, 4);
                current = 0;

                while (current < mCount)
                {
                    int xmb = rnd.Next(0, roomSize);
                    int ymb = rnd.Next(0, roomSize);

                    if (blocks[xmb, ymb, 0].Rigid)
                    {
                        current++;

                        int zmb = 1;

                        while (blocks[xmb, ymb, zmb].Rigid)
                        {
                            zmb++;
                        }

                        AddMob(new NPC(contentManager, gameWorld, xmb, ymb, zmb, 11, 0.085, 1, 1));
                    }
                }

                mCount = rnd.Next(6, 20);
                current = 0;

                while (current < mCount)
                {
                    int xmb = rnd.Next(0, roomSize);
                    int ymb = rnd.Next(0, roomSize);

                    if (blocks[xmb, ymb, 0].Rigid)
                    {
                        current++;

                        int zmb = 1;

                        while (blocks[xmb, ymb, zmb].Rigid)
                        {
                            zmb++;
                        }

                        int tmptype = 0;
                        prob = rnd.Next(0, 100);

                        if (prob < 15)
                            tmptype = 10;
                        else if (prob < 30)
                            tmptype = 8;
                        else
                            tmptype = rnd.Next(12, 17);

                        AddMob(new Decoration(contentManager, xmb, ymb, zmb, tmptype));
                    }
                }
            }

            if (biome == 0)
            {
                //huts
                var newBlocks = new List<Tuple<int, int, int>>();

                int hutCount = rnd.Next(4, 11);

                for (int i = 0; i < hutCount; i++)
                {
                    int ci = rnd.Next(0, groundBlocks.Count);
                    prob = rnd.Next(0, 100);

                    if (prob < 33)
                    {
                        var mplace = PlaceMountain(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2,
                                1, 4, 0, 0);

                        newBlocks.AddRange(mplace);

                        int rootCount = rnd.Next(4, 11);

                        for (int j = 0; j < rootCount; j++)
                        {
                            int ct = rnd.Next(0, mplace.Count);

                            AddMob(new Decoration(contentManager, mplace[ct].Item1, mplace[ct].Item2,
                                1, 10));

                            PlaceMountain(contentManager, mplace[ct].Item1, mplace[ct].Item2, 7, 1, 0, 18);

                            mplace.RemoveAt(ct);
                        }
                    }
                    else if (prob < 66)
                    {
                        PlaceMountain(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2, 3, 4, 0, 18);

                        newBlocks.AddRange(PlaceMountain(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2,
                                1, 4, 0, 0));

                        AddMob(new Decoration(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2+3,
                            1, 8));
                    }
                    else
                    {
                        newBlocks.AddRange(PlaceMountain(contentManager, groundBlocks[ci].Item1, groundBlocks[ci].Item2,
                            1, 4, 0, 0));
                        
                    }
                }

                groundBlocks.AddRange(newBlocks);
                
                int mCount = rnd.Next(4, 15);
                int current = 0;

                while (current < mCount)
                {
                    int xmb = rnd.Next(0, roomSize);
                    int ymb = rnd.Next(0, roomSize);

                    if (blocks[xmb, ymb, 0].Rigid)
                    {
                        current++;

                        int zmb = 1;

                        //while (zmb < roomSizeZ && blocks[xmb, ymb, zmb].Rigid)
                        {
                            zmb++;
                        }

                        AddMob(new NPC(contentManager, gameWorld, xmb, ymb, zmb, 22, 0.091, 1, 1));
                    }
                }
            }

            //collision map
            foreach (var currentTuple in groundBlocks)
            {
                int i = currentTuple.Item1;
                int j = currentTuple.Item2;
                int k = currentTuple.Item3;

                if (blocks[i, j, k].Type == 0)
                {
                    int newType = 11;

                    if (i>0&&blocks[i - 1, j, k].Rigid)
                    {
                        newType = 14;
                    }

                    if (j > 0 && blocks[i, j - 1, k].Rigid)
                    {
                        if (newType == 11)
                            newType = 13;

                        if (newType == 14)
                            newType = 4;
                    }

                    if (i<roomSize-1&& blocks[i + 1, j, k].Rigid)
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

                    if (j < roomSize - 1 && blocks[i, j + 1, k].Rigid)
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
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            int currentMob = 0, j = 0;

            while (j < roomSize || currentMob < mobs.Count)
            {
                if (currentMob < mobs.Count && mobs[currentMob].Y < j - 1)
                {
                    int YDelay = 0;

                    if (!mobs[currentMob].Flying
                        && (int)Math.Round(mobs[currentMob].X) < roomSize && (int)Math.Round(mobs[currentMob].X) > 0)
                    {
                        YDelay = -(int)(mobs[currentMob].Z * GameWorld.BlockSizeZ);
                    }

                    mobs[currentMob].Draw(spriteBatch, x + (int)(mobs[currentMob].X * GameWorld.BlockSizeX),
                        y + (int)(mobs[currentMob].Y * GameWorld.BlockSizeY) + YDelay);

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

            for (currentMob = 0; currentMob < mobs.Count; currentMob++)
            {
                int YDelay = 0;

                if (!mobs[currentMob].Flying
                    && (int)Math.Round(mobs[currentMob].X) < roomSize && (int)Math.Round(mobs[currentMob].X) > 0)
                {
                    YDelay = -(int)(mobs[currentMob].Z * GameWorld.BlockSizeZ);
                }

                mobs[currentMob].DrawShadow(spriteBatch, x + (int)(mobs[currentMob].X * GameWorld.BlockSizeX),
                    y + (int)(mobs[currentMob].Y * GameWorld.BlockSizeY) + YDelay);
            }

            foreach (var currentMobInterface in mobsWithInterface)
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
                    int pcount = mobs.Count;

                    mobs[i].Update(contentManager, gameWorld);

                    if (mobs.Count < pcount)
                    {
                        i += mobs.Count - pcount;
                    }

                    if (mobs[i]!=null&&mobs[i].Z < -100)
                    {
                        MarkMobAsDeleted(mobs[i]);
                    }
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

            DeleteMarked();

            mobs.Sort((a, b) => a.Y.CompareTo(b.Y));

            string key = mob.SaveList().Split('\n')[0];

            if (!MobsByTypes.ContainsKey(key))
                MobsByTypes.Add(key, new List<Mob>());

            MobsByTypes[key].Add(mob);
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

            for(int q=0; q<allowedTypes.Count; q++)
                if(MobsByTypes.ContainsKey(allowedTypes[q]))
                    for (int i = 0; i < MobsByTypes[allowedTypes[q]].Count; i++)
                    {
                        if (MobsByTypes[allowedTypes[q]][i] != null)
                        {
                            double dst = GameWorld.GetDist(x, y, MobsByTypes[allowedTypes[q]][i].X,
                                MobsByTypes[allowedTypes[q]][i].Y);

                            if (cdist > dst
                                && !ignoredMobs.Contains(MobsByTypes[allowedTypes[q]][i])
                                && allowedTypes.Any(s => MobsByTypes[allowedTypes[q]][i].SaveList().StartsWith(s)))
                            {
                                cdist = dst;

                                closestMob = MobsByTypes[allowedTypes[q]][i];
                            }
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

        public Mob GetClosestMob(double x, double y, Mob ignoredMob, List<string> allowedTypes)
        {
            double cdist = 1e9;
            Mob closestMob = null;

            for (int i = 0; i < mobs.Count; i++)
            {
                double dst = GameWorld.GetDist(x, y, mobs[i].X, mobs[i].Y);

                if (cdist > dst
                    && ignoredMob != mobs[i]
                    && allowedTypes.Any(s => mobs[i].SaveList().StartsWith(s)))
                {
                    cdist = dst;

                    closestMob = mobs[i];
                }
            }

            return closestMob;
        }

        public Mob GetClosestMob(double x, double y, Mob ignoredMob, string allowedType)
        {
            double cdist = 1e9;
            Mob closestMob = null;

            for (int i = 0; i < mobs.Count; i++)
            {
                double dst = GameWorld.GetDist(x, y, mobs[i].X, mobs[i].Y);

                if (cdist > dst
                    && ignoredMob != mobs[i]
                    && mobs[i].SaveList().StartsWith(allowedType))
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
                for (int i = Math.Max(x - radius + k * step, 0); i < Math.Min(roomSize, x + radius - k * step); i++)
                    for (int j = Math.Max(y - radius + k * step, 0); j < Math.Min(roomSize, y + radius - k * step); j++)
                        if ((double)GameWorld.GetDist(x, y, i, j) < radius - k * step/2)
                        {
                            blocks[i, j, k] = new Block(contentManager, blockType);

                            toReturn.Add(new Tuple<int, int, int>(i, j, k));
                        }

                radius -= step;
            }

            return toReturn;
        }

        public bool LineIsClear(int x1, int y1, int x2, int y2, int z)
        {
            if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0 || x1 >= roomSize || y1 >= roomSize || x2 >= roomSize || y2 >= roomSize)
                return false;

            double xstep = x2 - x1, ystep = y2 - y1;

            if (Math.Abs(xstep) > Math.Abs(ystep))
            {
                ystep /= Math.Abs(xstep);
                xstep /= Math.Abs(xstep);
            }
            else
            {
                xstep /= Math.Abs(ystep);
                ystep /= Math.Abs(ystep);
            }

            double x = x1, y = y1;

            while ((int)Math.Round(x) != x2 || (int)Math.Round(y) != y2)
            {
                if (z < roomSizeZ - 1 && blocks[(int)Math.Round(x), (int)Math.Round(y), z + 1].Rigid)
                    return false;

                x += xstep;
                y += ystep;
            }

            return true;
        }

        public List<Tuple<int, int, int>> PlaceLine(ContentManager contentManager, int x1, int y1, int x2, int y2, int z, int blockType)
        {
            double xstep = x2 - x1, ystep = y2 - y1;

            if (Math.Abs(xstep) > Math.Abs(ystep))
            {
                ystep /= Math.Abs(xstep);
                xstep /= Math.Abs(xstep);
            }
            else
            {
                xstep /= Math.Abs(ystep);
                ystep /= Math.Abs(ystep);
            }

            double x = x1, y = y1;

            var ans = new List<Tuple<int, int, int>>();

            while ((int)Math.Round(x) != x2 || (int)Math.Round(y) != y2)
            {
                ans.Add(new Tuple<int, int, int>((int)x, (int)y, z));

                blocks[(int)x, (int)y, z] = new Block(contentManager, blockType);

                x += xstep;
                y += ystep;
            }

            return ans;
        }

        public List<Tuple<int, int, int>> Fill(ContentManager contentManager, int x, int y, int z, int fillType)
        {
            int typeToFill = blocks[x, y, z].Type;
            List<Tuple<int, int, int>> ans = new List<Tuple<int, int, int>>();
            List<Tuple<int, int>> current = new List<Tuple<int, int>>();
            List<Tuple<int, int>> discovered;

            current.Add(new Tuple<int, int>(x, y));

            while (current.Count > 0)
            {
                discovered = new List<Tuple<int, int>>();

                foreach (var currentTuple in current)
                {
                    blocks[currentTuple.Item1, currentTuple.Item2, z] = new Block(contentManager, fillType);

                    ans.Add(new Tuple<int, int, int>(currentTuple.Item1, currentTuple.Item2, z));

                    if (currentTuple.Item1 > 0 && blocks[currentTuple.Item1 - 1, currentTuple.Item2, z].Type == typeToFill)
                        if (!discovered.Contains(new Tuple<int, int>(currentTuple.Item1 - 1, currentTuple.Item2)))
                            discovered.Add(new Tuple<int, int>(currentTuple.Item1 - 1, currentTuple.Item2));

                    if (currentTuple.Item2 > 0 && blocks[currentTuple.Item1, currentTuple.Item2 - 1, z].Type == typeToFill)
                        if (!discovered.Contains(new Tuple<int, int>(currentTuple.Item1, currentTuple.Item2 - 1)))
                            discovered.Add(new Tuple<int, int>(currentTuple.Item1, currentTuple.Item2 - 1));

                    if (currentTuple.Item1 < roomSize - 1 && blocks[currentTuple.Item1 + 1, currentTuple.Item2, z].Type == typeToFill)
                        if (!discovered.Contains(new Tuple<int, int>(currentTuple.Item1 + 1, currentTuple.Item2)))
                            discovered.Add(new Tuple<int, int>(currentTuple.Item1 + 1, currentTuple.Item2));

                    if (currentTuple.Item2 < roomSize - 1 && blocks[currentTuple.Item1, currentTuple.Item2 + 1, z].Type == typeToFill)
                        if (!discovered.Contains(new Tuple<int, int>(currentTuple.Item1, currentTuple.Item2 + 1)))
                            discovered.Add(new Tuple<int, int>(currentTuple.Item1, currentTuple.Item2 + 1));
                }

                current = discovered;
            }

            return ans;
        }
    }
}