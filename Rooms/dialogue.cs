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
using System.Reflection;

namespace Rooms
{
    public class Speaker : Mob
    {
        private string Action { get; set; }
        private bool AddedToList = false;

        public List<DialogueVariant> dialogueVariants { get; protected set; }

        public Speaker(ContentManager contentManager, GameWorld gameWorld, double x, double y, int type, double speed)
        {
            Speed = speed;

            ChangeCoords(x, y);

            Type = type;

            Action = "id";

            dialogueVariants = new List<DialogueVariant>();
            dialogueVariants.Add(new DialogueVariant(contentManager.Load<SpriteFont>("dialogue_font"), 0));

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
            else if (AddedToList && GameWorld.GetDist(gameWorld.currentRoom.heroReference.X, gameWorld.currentRoom.heroReference.Y,
                X, Y) > gameWorld.currentRoom.heroReference.Radius + Radius + 1)
            {
                gameWorld.currentRoom.mobsWithInterface.Remove(this);

                AddedToList = false;
            }

            base.Update(contentManager, gameWorld);
        }

        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            dialogueVariants[0].Draw(spriteBatch);
        }
    }
}