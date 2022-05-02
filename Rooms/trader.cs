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
    public class Trader:Mob
    {
        public List<Deal> deals { get; protected set; }
        private bool AddedToList = false;

        public Trader(ContentManager contentManager, double x, double y, double z, int type, GameWorld gameWorld)
        {
            deals = new List<Deal>();

            ChangeCoords(x, y, z);

            Type = type;

            for (int i = 0; i < 4; i++)
            {
                deals.Add(new Deal(contentManager));
            }

            Radius = 1;

            updateTexture(contentManager, true);
        }

        public Trader(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]), double.Parse(input[currentStr + 4]));

            Type = Int32.Parse(input[currentStr + 5]);

            Radius = 1;

            deals = new List<Deal>();

            for (int i = 0; i < 4; i++)
            {
                deals.Add(new Deal(contentManager));
            }

            updateTexture(contentManager, true);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            if (!AddedToList && GameWorld.GetDist(gameWorld.currentRoom.heroReference.X, gameWorld.currentRoom.heroReference.Y,
                X, Y) <= gameWorld.currentRoom.heroReference.Radius + Radius + 1)
            {
                gameWorld.currentRoom.mobsWithInterface.Add(this);

                AddedToList = true;
            }
            else if(AddedToList && GameWorld.GetDist(gameWorld.currentRoom.heroReference.X, gameWorld.currentRoom.heroReference.Y,
                X, Y) > gameWorld.currentRoom.heroReference.Radius + Radius + 1)
            {
                gameWorld.currentRoom.mobsWithInterface.Remove(this);

                AddedToList = false;
            }

            base.Update(contentManager, gameWorld);
        }

        public override string SaveList()
        {
            string ans = "Trader\n"+base.SaveList();

            return ans;
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < deals.Count; i++)
            {
                deals[i].Draw(spriteBatch, 960, i * (GameWorld.ItemTextureSize + 5));
            }
        }
    }

    public class Deal
    {
        public List<Item> pay { get; protected set; }
        public List<Item> recieve { get; protected set; }
        private Texture2D arrowTexture;

        public Deal(ContentManager contentManager)
        {
            arrowTexture = contentManager.Load<Texture2D>("deal_arrow");

            pay = new List<Item>();
            recieve = new List<Item>();

          //  pay.Add(new Item(contentManager, 0, 0, 3, 1));

          //  recieve.Add(new Item(contentManager, 0, 0, 4, 1));
        }

        /// <summary>
        /// Used to draw all deal items and "arrow" in given place. ATTENTION: given place marks upper left corner of arrow, not the whole deal
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(arrowTexture, new Vector2(x, y), Color.White);

            for (int i = 0; i < pay.Count; i++)
            {
                pay[i].DrawIcon(spriteBatch, x - (pay.Count-i) * GameWorld.ItemTextureSize, y);
            }
        }
    }
}