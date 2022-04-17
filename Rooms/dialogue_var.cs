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
    public class DialogueVariant
    {
        public string Phrase { get; protected set; }
        public List<string> Answers { get; protected set; }
      
        public SpriteFont font { get; protected set; }
        public int X=0, Y=0;
        
        public int Type { get; protected set; }
        private MouseState pstate;
       
        public DialogueVariant(SpriteFont spriteFont, int type)
        {
            Type = type;

            font = spriteFont;

            List<string> input=null;

            using(StreamReader sr=new StreamReader(@"info\#global\phrases\"+Type.ToString()+".phrase"))
            {
                input = sr.ReadToEnd().Split('\n').ToList();
            }

            Phrase = input[0];

            input.RemoveAt(0);

            Answers = input;

            pstate = Mouse.GetState();
        }
        
        public int Update()
        {
            int selectedPhrase = -1;
            MouseState mstate = Mouse.GetState();

            int XNonAbsolute = mstate.X - X;
            int YNonAbsolute = mstate.Y - Y;

            if (mstate.LeftButton == ButtonState.Released && pstate.LeftButton == ButtonState.Pressed)
            {
                selectedPhrase = YNonAbsolute / font.LineSpacing;

                selectedPhrase--;
            }

            pstate = mstate;

            return selectedPhrase;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            MouseState mstate = Mouse.GetState();

            int YNonAbsolute = mstate.Y - Y;
            int selectedPhrase = YNonAbsolute / font.LineSpacing -1;

            spriteBatch.DrawString(font, Phrase, new Vector2(X, Y), Color.White);

            for (int i = 0; i < Answers.Count; i++)
            {
                if (selectedPhrase == i)
                {
                    spriteBatch.DrawString(font, Answers[i], new Vector2(X, Y + (i + 1) * font.LineSpacing), Color.Lime);
                }
                else
                {
                    spriteBatch.DrawString(font, Answers[i], new Vector2(X, Y + (i + 1) * font.LineSpacing), Color.White);
                }
            }
        }
    }
}