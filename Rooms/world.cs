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
    public class GameWorld
    {
        public const int ItemTextureSize = 55;

        public int DrawX = BlockSizeX / 2, DrawY = (1080 - BlockSizeY * Room.roomSize) / 2;
        public const int TextureUpdateSpeed = 9;

        public static Texture2D SelectionCursorTexture { get; protected set; }
        public static SpriteFont MainFont { get; protected set; }

        public const int BlockSizeX = 30;
        public const int BlockSizeY = 23;
        public string Name { get; protected set; }
        public Room currentRoom { get; protected set; }
        //   public Room leftRoom { get; protected set; }
        //   public Room rightRoom { get; protected set; }

        private Texture2D background;

        public GameWorld(ContentManager contentManager, string name)
        {
            Name = name;

            try
            {
                List<string> posRead;

                using (StreamReader sr = new StreamReader(@"info\" + Name + @"\current_pos"))
                {
                    posRead = sr.ReadToEnd().Split('\n').ToList();
                }

                Hero newHero = new Hero(contentManager, Room.roomSize / 2, Room.roomSize / 2, 0, this);

                currentRoom = new Room(contentManager, Int32.Parse(posRead[0]), Int32.Parse(posRead[1]), this, newHero);
            }
            catch
            {
                Hero newHero = new Hero(contentManager, Room.roomSize / 2, Room.roomSize / 2, 0, this);

                currentRoom = new Room(contentManager, 0, 0, this, newHero);
            }

            //loading all static things, most of them used for drawing
            SelectionCursorTexture = contentManager.Load<Texture2D>("selection_cursor");
            background = contentManager.Load<Texture2D>("background");

            MainFont = contentManager.Load<SpriteFont>("main_font_28s");

            currentRoom.Save();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);

            currentRoom.Draw(spriteBatch, DrawX, DrawY);
        }
        
        public void Update(ContentManager contentManager)
        {
            currentRoom.Update(contentManager, this);

            int roomX = currentRoom.X;
            int roomY = currentRoom.Y;

            if (currentRoom.heroReference.X <= 0)
            {
                currentRoom.heroReference.ChangeCoords(Room.roomSize - 1.00001, currentRoom.heroReference.Y);

                var newHero = currentRoom.heroReference;

                currentRoom.MarkMobAsDeleted(currentRoom.heroReference);
                currentRoom.DeleteMarked();

                Save();

                currentRoom = new Room(contentManager, currentRoom.X - 1, currentRoom.Y, this, newHero);

                Save();
            }

            if (currentRoom.heroReference.X >= Room.roomSize - 1)
            {
                currentRoom.heroReference.ChangeCoords(0, currentRoom.heroReference.Y);

                var newHero = currentRoom.heroReference;

                currentRoom.MarkMobAsDeleted(currentRoom.heroReference);
                currentRoom.DeleteMarked();

                Save();

                currentRoom = new Room(contentManager, currentRoom.X + 1, currentRoom.Y, this, newHero);

                Save();
            }

            if (currentRoom.heroReference.Y <= 0)
            {
                currentRoom.heroReference.ChangeCoords(currentRoom.heroReference.X, Room.roomSize - 1.00001);
                
                var newHero = currentRoom.heroReference;

                currentRoom.MarkMobAsDeleted(currentRoom.heroReference);
                currentRoom.DeleteMarked();

                Save();

                currentRoom = new Room(contentManager, currentRoom.X, currentRoom.Y - 1, this, newHero);

                Save();
            }

            if (currentRoom.heroReference.Y >= Room.roomSize - 1)
            {
                currentRoom.heroReference.ChangeCoords(currentRoom.heroReference.X, 0);

                var newHero = currentRoom.heroReference;

                currentRoom.MarkMobAsDeleted(currentRoom.heroReference);
                currentRoom.DeleteMarked();

                Save();

                currentRoom = new Room(contentManager, currentRoom.X, currentRoom.Y + 1, this, newHero);

                Save();
            }

            int heroDrawY = 540 - (int)(currentRoom.heroReference.Y * BlockSizeY + DrawY);
            int heroDrawX = 960 - (int)(currentRoom.heroReference.X * BlockSizeX + DrawX);

            //23.2379000772=sqrt(540), 540=1080/2
            if (heroDrawY >= 0) 
                DrawY += (int)(heroDrawY / 23.2379000772 * heroDrawY / 23.2379000772);

            if (heroDrawY <= 0)
                DrawY -= (int)(heroDrawY / 23.2379000772 * heroDrawY / 23.2379000772);

            //30.9838667697=sqrt(960)
            if (heroDrawX <= 0)
                DrawX -= (int)(heroDrawX / 30.9838667697 * heroDrawX / 30.9838667697);

            if (heroDrawX >= 0)
                DrawX += (int)(heroDrawX / 30.9838667697 * heroDrawX / 30.9838667697);
        }

        public void Save()
        {
            string str = currentRoom.X.ToString() + '\n' + currentRoom.Y.ToString();

            if (!Directory.Exists(@"info\" + Name + @"\"))
            {
                Directory.CreateDirectory(@"info\" + Name + @"\");
            }

            if(!File.Exists(@"info\" + Name + @"\current_pos"))
            {
                var nf=File.Create(@"info\" + Name + @"\current_pos");
                nf.Close();
            }

            using (StreamWriter sw = new StreamWriter(@"info\" + Name + @"\current_pos"))
            {
                sw.Write(str);
            }

            currentRoom.Save();
         //   rightRoom.Save();
         //   leftRoom.Save();
        }

        public static double GetDist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public static double GetDirection(double x1, double y1, double x2, double y2)
        {
            return Math.Atan2(y1 - y2, x1 - x2);
        }
    }
}