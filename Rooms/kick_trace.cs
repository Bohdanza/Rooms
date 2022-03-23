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
    public class KickTrace:Mob
    {
        public double Direction { get; protected set; }
        private int timeLived { get; set; }
        public int Lifetime { get; protected set; }

        public KickTrace(ContentManager contentManager, double x, double y, int type, double speed, double direction, int lifetime, GameWorld gameWorld)
        {
            Direction = direction;
            Speed = speed;

            timeLived = 0;
            Lifetime = lifetime;

            ChangeCoords(x, y);

            Type = type;

            updateTexture(contentManager, true);
        }

        public override void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(Textures[TextureNumber], new Vector2(x, y),
                null, Color.White, (float)Direction, new Vector2(0, Textures[TextureNumber].Height/2), 1f, SpriteEffects.None, 0);
        }

        public override void Update(ContentManager contentManager, GameWorld gameWorld)
        {
            timeLived++;

            if(timeLived>=Lifetime)
            {
                gameWorld.currentRoom.MarkMobAsDeleted(this);
            }

            Move(Speed, Direction, gameWorld);

            TimeSinceLastTextureUpdate++;

            if (TimeSinceLastTextureUpdate > GameWorld.TextureUpdateSpeed/3)
            {
                updateTexture(contentManager, false);

                TimeSinceLastTextureUpdate = 0;
            }
        }
    }
}