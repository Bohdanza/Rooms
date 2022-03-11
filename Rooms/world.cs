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
    public class GameWorld
    {
        public int timeSinceLastKeyPress { get; private set; } = 0;

        public const int BlockSize=20; 
        public string Name { get; protected set; }
        public Room currentRoom { get; protected set; }

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

                currentRoom = new Room(contentManager, Int32.Parse(posRead[0]), Int32.Parse(posRead[0]), this, newHero);
            }
            catch
            {
                Hero newHero = new Hero(contentManager, Room.roomSize / 2, Room.roomSize / 2, 0, this);

                currentRoom = new Room(contentManager, 0, 0, this, newHero);
            }

            currentRoom.Save();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            currentRoom.Draw(spriteBatch, 0, 0);
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            currentRoom.Draw(spriteBatch, x, y);
        }

        public void Update(ContentManager contentManager)
        {
            timeSinceLastKeyPress++;

            currentRoom.Update(contentManager, this);

            if (currentRoom.heroReference.X < 0)
            {
                currentRoom.heroReference.ChangeCoords(Room.roomSize - 1, currentRoom.heroReference.Y);

                currentRoom.Save();

                currentRoom = new Room(contentManager, currentRoom.X - 1, currentRoom.Y, this, currentRoom.heroReference);

                currentRoom.Save();
            }

            if (currentRoom.heroReference.X > Room.roomSize)
            {
                currentRoom.heroReference.ChangeCoords(0, currentRoom.heroReference.Y);

                currentRoom.Save();

                currentRoom = new Room(contentManager, currentRoom.X + 1, currentRoom.Y, this, currentRoom.heroReference);

                currentRoom.Save();
            }

            if (currentRoom.heroReference.Y < 0)
            {
                currentRoom.heroReference.ChangeCoords(currentRoom.heroReference.X, Room.roomSize - 1);

                currentRoom.Save();

                currentRoom = new Room(contentManager, currentRoom.X, currentRoom.Y - 1, this, currentRoom.heroReference);

                currentRoom.Save();
            }

            if (currentRoom.heroReference.Y > Room.roomSize)
            {
                currentRoom.heroReference.ChangeCoords(currentRoom.heroReference.X, 0);

                currentRoom.Save();

                currentRoom = new Room(contentManager, currentRoom.X, currentRoom.Y + 1, this, currentRoom.heroReference);

                currentRoom.Save();
            }
        }

        public void Save()
        {
            currentRoom.Save();
        }
    }
}