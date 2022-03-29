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
    public class Decoration:Mob
    {
        public Decoration(ContentManager contentManager, double x, double y, int type)
        {
            Type = type;

            ChangeCoords(x, y);

            updateTexture(contentManager, true);
        }

        public Decoration(ContentManager contentManager, List<string> input, int currentStr)
        {
            Name = input[currentStr + 1];

            ChangeCoords(double.Parse(input[currentStr + 2]), double.Parse(input[currentStr + 3]));

            Type = Int32.Parse(input[currentStr + 4]);

            Radius = 1;

            updateTexture(contentManager, true);
        }

        public override string SaveList()
        {
            return "Decoration\n" +base.SaveList();
        }
    }
}
