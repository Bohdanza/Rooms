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

            //writing   
            using (StreamWriter sw = new StreamWriter(@"info\" + worldReference.Name + @"\rooms\" + X.ToString() + "_" + Y.ToString() + ".rr"))
            {
                sw.Write(output);
            }
        }

        protected void Generate(ContentManager contentManager, int x, int y, GameWorld gameWorld)
        {
            //filling with 0-blocks
            for (int i = 0; i < roomSize; i++)
                for (int j = 0; j < roomSize; j++)
                {
                    blocks[i, j] = new Block(contentManager, 0);
                }

            //walls
            placeRectagle(contentManager, 0, 0, roomSize, 1, 1);
            placeRectagle(contentManager, 0, 0, 1, roomSize, 1);

            placeRectagle(contentManager, roomSize - 1, 1, roomSize, roomSize, 1);
            placeRectagle(contentManager, 1, roomSize - 1, roomSize, roomSize, 1);

            //doors
            blocks[0, roomSize / 2] = new Block(contentManager, 0);
            blocks[roomSize / 2, 0] = new Block(contentManager, 0);
            blocks[roomSize-1, roomSize / 2] = new Block(contentManager, 0);
            blocks[roomSize / 2, roomSize-1] = new Block(contentManager, 0);

            //generating main landscape
            var rnd = new Random();

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
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            int currentMob = 0, j = 0;

            while (j < roomSize || currentMob < mobs.Count)
            {
                if (currentMob < mobs.Count && mobs[currentMob].Y < j)
                {
                    mobs[currentMob].Draw(spriteBatch, x+(int)(mobs[currentMob].X * GameWorld.BlockSize), y+(int)(mobs[currentMob].Y * GameWorld.BlockSize));

                    currentMob++;
                }
                else if (j < Room.roomSize)
                {
                    for (int i = 0; i < roomSize; i++)
                    {
                        blocks[i, j].Draw(spriteBatch, x + i * GameWorld.BlockSize, y + j * GameWorld.BlockSize);
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
        }
    }
}