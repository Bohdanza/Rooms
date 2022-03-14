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
    public abstract class Mob
    { 
        public const int TextureUpdateSpeed = 25; 

        public int Type { get; protected set; }
        public string Name { get; protected set; } = "Noname mob";

        public double Speed { get; protected set; }

        public double Radius { get; protected set; } = 0;

        public bool IsSelected { get; protected set; }

        public double X { get; private set; }
        public double Y { get; private set; }

        public List<Texture2D> Textures { get; protected set; }

        protected int TimeSinceLastTextureUpdate = 0;

        /// <summary>
        /// Number of currently used texture from Textures list
        /// </summary>
        public int TextureNumber { get; protected set; }

        /// <summary>
        /// Used to increase the texture number if reload==false and reload the whole Textures list if it's true
        /// </summary>
        /// <param name="contentManager"></param>
        /// <param name="reload"></param>
        protected virtual void updateTexture(ContentManager contentManager, bool reload)
        {
            if (reload)
            {
                Textures = new List<Texture2D>();

                TextureNumber = 0;

                while(File.Exists(@"Content\mob_"+Type.ToString()+"_"+TextureNumber.ToString()+".xnb"))
                {
                    Textures.Add(contentManager.Load<Texture2D>("mob_" + Type.ToString() + "_" + TextureNumber.ToString()));

                    TextureNumber++;
                }

                TextureNumber = 0;
            }
            else
            {
                TextureNumber++;

                if(TextureNumber>=Textures.Count)
                {
                    TextureNumber = 0;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            if(IsSelected)
            {
                spriteBatch.Draw(GameWorld.SelectionCursorTexture,
                    new Vector2(x - GameWorld.SelectionCursorTexture.Width / 2, y - GameWorld.SelectionCursorTexture.Height/2),
                    Color.White);

                DrawInterface(spriteBatch);
            }

            spriteBatch.Draw(Textures[TextureNumber], new Vector2(x - Textures[TextureNumber].Width/2, y - Textures[TextureNumber].Height), Color.White);
        }

        public virtual void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            TimeSinceLastTextureUpdate++;
            
            if (TimeSinceLastTextureUpdate > TextureUpdateSpeed)
            {
                updateTexture(contentManager, false);

                TimeSinceLastTextureUpdate = 0;
            }

            var mouseCoord = gameWorld.currentRoom.GetMouseCordinates();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (GameWorld.GetDist(X, Y, mouseCoord.Item1, mouseCoord.Item2) < Radius)
                {
                    IsSelected = true;
                }
                else
                {
                    IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Used to automatically fill some fields (like MaxHP, Speed etc.) with given file info if any. 
        /// !USE ONLY AFTER ASSIGNING TYPE!
        /// 
        /// Currently can't be used
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        protected bool AutomaticInitialize(string path)
        {
            //TODO

            return false;
        }

        public void ChangeCoords(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Move(double speed, double direction, GameWorld gameWorld)
        {
            double x = Math.Cos(direction) * speed;
            double y = Math.Sin(direction) * speed;

            double px = X;
            double py = Y;

            X += x;

            if (X < 0)
            {
                X = 0;
            }

            //check for center
            if (X <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y)].Passable)
            {
                X = px;
            }

            //fast check for hitbox. Can be incorrect sometimes
            if (X + Radius <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X + Radius), (int)Math.Round(Y)].Passable)
            {
                X = px;
            }
            
            if (X - Radius <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X - Radius), (int)Math.Round(Y)].Passable)
            {
                X = px;
            }

            if (X <= Room.roomSize - 1 && Y + Radius <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y + Radius)].Passable)
            {
                X = px;
            }

            if (X<= Room.roomSize - 1 && Y + Radius <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X - Radius), (int)Math.Round(Y + Radius)].Passable)
            {
                X = px;
            }

            Y += y;

            if (Y < 0)
            {
                Y = 0;
            }

            if (X <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y)].Passable)
            {
                Y = py;
            }
            
            //fast check for hitbox. Can be incorrect sometimes
            if (X + Radius <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X + Radius), (int)Math.Round(Y)].Passable)
            {
                Y = py;
            }

            if (X - Radius <= Room.roomSize - 1 && Y <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X - Radius), (int)Math.Round(Y)].Passable)
            {
                Y = py;
            }

            if (X <= Room.roomSize - 1 && Y + Radius <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X), (int)Math.Round(Y + Radius)].Passable)
            {
                Y = py;
            }

            if (X <= Room.roomSize - 1 && Y + Radius <= Room.roomSize - 1 && X >= 0 && Y >= 0 &&
                !gameWorld.currentRoom.blocks[(int)Math.Round(X - Radius), (int)Math.Round(Y + Radius)].Passable)
            {
                Y = py;
            }
        }

        /// <summary>
        /// Used to draw interface. Automatically called in Draw function if mob is selected
        /// </summary>
        public virtual void DrawInterface(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(GameWorld.MainFont, Name, new Vector2(30, 30), Color.White);
        }
    }
}