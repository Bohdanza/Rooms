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
        private int currentStage = 0;

        public List<Tuple<DialogueVariant, List<int>>> dialogueVariants { get; protected set; }
        
        public Speaker(ContentManager contentManager, GameWorld gameWorld, double x, double y, int type, double speed)
        {
            Speed = speed;

            ChangeCoords(x, y);

            Type = type;

            Action = "id";
            
            dialogueVariants = new List<Tuple<DialogueVariant, List<int>>>();
            List<string> input = new List<string>();

            using (StreamReader sr = new StreamReader(@"info\#global\dialogues\" + Type.ToString() + ".diag"))
            {
                input = sr.ReadToEnd().Split('\n').ToList();
            }

            SpriteFont mainFont = contentManager.Load<SpriteFont>("dialogue_font");

            for (int i = 0; i < input.Count; i += 2)
            {
                DialogueVariant dialogueVariant = new DialogueVariant(mainFont, Int32.Parse(input[i]));

                List<int> secondPart = new List<int>();
                List<string> outputs = input[i+1].Split(' ').ToList();

                try
                {
                    for (int j = 0; j < outputs.Count; j++)
                    {
                        secondPart.Add(Int32.Parse(outputs[j]));
                    }
                }
                catch
                {}

                dialogueVariants.Add(new Tuple<DialogueVariant, List<int>>(dialogueVariant, secondPart));
            }

            updateTexture(contentManager, true);
        }

        public Speaker(ContentManager contentManager, List<string> input, int currentStr)
        {
            currentStage = Int32.Parse(input[currentStr + 1]);

            Speed = double.Parse(input[currentStr + 2]);

            Name = input[currentStr + 3];

            ChangeCoords(double.Parse(input[currentStr + 4]), double.Parse(input[currentStr + 5]));

            Type = Int32.Parse(input[currentStr + 6]);

            Action = "id";

            dialogueVariants = new List<Tuple<DialogueVariant, List<int>>>();
            List<string> inp = new List<string>();

            using (StreamReader sr = new StreamReader(@"info\#global\dialogues\" + Type.ToString() + ".diag"))
            {
                inp = sr.ReadToEnd().Split('\n').ToList();
            }

            SpriteFont mainFont = contentManager.Load<SpriteFont>("dialogue_font");

            for (int i = 0; i < inp.Count; i += 2)
            {
                DialogueVariant dialogueVariant = new DialogueVariant(mainFont, Int32.Parse(inp[i]));

                List<int> secondPart = new List<int>();
                List<string> outputs = inp[i + 1].Split(' ').ToList();

                try
                {
                    for (int j = 0; j < outputs.Count; j++)
                    {
                        secondPart.Add(Int32.Parse(outputs[j]));
                    }
                }
                catch
                { }

                dialogueVariants.Add(new Tuple<DialogueVariant, List<int>>(dialogueVariant, secondPart));
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
            else if (AddedToList && GameWorld.GetDist(gameWorld.currentRoom.heroReference.X, gameWorld.currentRoom.heroReference.Y,
                X, Y) > gameWorld.currentRoom.heroReference.Radius + Radius + 1)
            {
                gameWorld.currentRoom.mobsWithInterface.Remove(this);

                AddedToList = false;
            }

            int selected=dialogueVariants[currentStage].Item1.Update();

            if (selected >= 0 && selected < dialogueVariants[currentStage].Item2.Count)
            {
                currentStage = dialogueVariants[currentStage].Item2[selected];
            }

            base.Update(contentManager, gameWorld);
        }
        
        public override void DrawInterface(SpriteBatch spriteBatch)
        {
            dialogueVariants[currentStage].Item1.Draw(spriteBatch);
        }

        public override string SaveList()
        {
            return "Speaker\n" + currentStage.ToString() + "\n" + Speed.ToString() + "\n" + base.SaveList();
        }
    }
}