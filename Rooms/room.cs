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
        public const int roomSize = 30;

        public int biome { get; protected set; }

        public int X { get; protected set; }
        public int Y { get; protected set; } 
        public Block[,] blocks;

        private GameWorld worldReference { get;  set; }
        public Hero heroReference { get; protected set; }
        public List<Mob> mobs { get; protected set; }

        public Room(ContentManager contentManager, int x, int y, GameWorld gameWorld, Hero heroMoved)
        {
            X = x;
            Y = y;

            worldReference = gameWorld;

            blocks = new Block[roomSize, roomSize];
            mobs = new List<Mob>();

            if (Directory.Exists(@"info\" + gameWorld.Name + @"\rooms") && 
                File.Exists(@"info\" + gameWorld.Name + @"\rooms\" + x.ToString() + "_" + y.ToString() + ".rr"))
            {
                try
                {
                    Load(contentManager, x, y, gameWorld);
                }
                catch
                {
                    Generate(contentManager, x, y, gameWorld);
                }
            }
            else
            {
                Generate(contentManager, x, y, gameWorld);
            }

            AddMob(heroMoved);

            heroReference = heroMoved;
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

            output += mobs.Count.ToString();
            output += "\n";

            for (int i = 0; i < mobs.Count; i++)
            {
                string csave = mobs[i].SaveList();

                if (!csave.StartsWith("Hero"))
                {
                    if (csave[csave.Length - 1] != '\n')
                    {
                        csave += "\n";
                    }

                    output += csave;
                }
            }

            //writing   
            using (StreamWriter sw = new StreamWriter(@"info\" + worldReference.Name + @"\rooms\" + X.ToString() + "_" + Y.ToString() + ".rr"))
            {
                sw.Write(output);
            }
        }

        protected void Generate(ContentManager contentManager, int x, int y, GameWorld gameWorld)
        {
            var rnd = new Random();

            //filling with 0-blocks and walls
            for (int i = 0; i < roomSize; i++)
                for (int j = 0; j < roomSize; j++)
                {
                    if (GameWorld.GetDist(roomSize / 2 - 0.5, roomSize / 2 - 0.5, i, j) < roomSize / 2 - 3 + rnd.Next(0, 3))
                    {
                        blocks[i, j] = new Block(contentManager, 0);

                        if(rnd.Next(0, 100)<1)
                        {
                            AddMob(new NPC(contentManager, gameWorld, i, j, 1, 0.075, 10, 10));
                        }
                    }
                    else
                    {
                        blocks[i, j] = new Block(contentManager, 1);
                    }
                }

            //generating main landscape
            //points which are used to generate "rocks" around them
            int pointCount = rnd.Next(5, 11);

            //counter of already added
            int c = 0;

            //placing in random places
            while (c < pointCount)
            {
                int tmpx = rnd.Next(1, roomSize - 1);
                int tmpy = rnd.Next(1, roomSize - 1);

                if (blocks[tmpx, tmpy].Type != 1)
                {
                    blocks[tmpx, tmpy] = new Block(contentManager, 1);

                    c++;
                }
            }

            //doors
            placeRectagle(contentManager, roomSize / 2, 0, roomSize / 2 + 1, roomSize, 0);
            placeRectagle(contentManager, 0, roomSize / 2, roomSize, roomSize / 2 + 1, 0);

            //door lights
            blocks[1, roomSize / 2 - 1] = new Block(contentManager, 2);
            blocks[1, roomSize / 2 + 1] = new Block(contentManager, 2); 

            blocks[roomSize / 2 - 1, 1] = new Block(contentManager, 2);
            blocks[roomSize / 2 + 1, 1] = new Block(contentManager, 2);
            
            blocks[roomSize - 2, roomSize / 2 - 1] = new Block(contentManager, 2);
            blocks[roomSize - 2, roomSize / 2 + 1] = new Block(contentManager, 2);

            blocks[roomSize / 2 - 1, roomSize - 2] = new Block(contentManager, 2);
            blocks[roomSize / 2 + 1, roomSize - 2] = new Block(contentManager, 2);
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            int currentMob = 0, j = 0;

            while (j < roomSize || currentMob < mobs.Count)
            {
                if (currentMob < mobs.Count && mobs[currentMob].Y < j-0.5)
                {
                    mobs[currentMob].Draw(spriteBatch, x + (int)(mobs[currentMob].X * GameWorld.BlockSizeX), y + (int)(mobs[currentMob].Y * GameWorld.BlockSizeY));

                    currentMob++;
                }
                else if (j < Room.roomSize)
                {
                    for (int i = 0; i < roomSize; i++)
                    {
                        blocks[i, j].Draw(spriteBatch, x + i * GameWorld.BlockSizeX - GameWorld.BlockSizeX / 2, y + j * GameWorld.BlockSizeY - GameWorld.BlockSizeY / 2);
                    }

                    j++;
                }
            }
        }

        public void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            for (int i = 0; i < mobs.Count; i++)
            {
                mobs[i].Update(contentManager, gameWorld);
            }

            for (int i = 0; i < roomSize; i++)
                for (int j = 0; j < roomSize; j++)
                {
                    blocks[i, j].Update(contentManager);
                }
        }

        //used to get mouse cordinates in room's coord system. Can work incorrectly if Draw is called with push
        public Tuple<double, double> GetMouseCordinates(GameWorld gameWorld)
        {
            var ms = Mouse.GetState();

            return new Tuple<double, double>(ms.X - gameWorld.DrawX / GameWorld.BlockSizeX, ms.Y - gameWorld.DrawY / GameWorld.BlockSizeY);
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
    }
}