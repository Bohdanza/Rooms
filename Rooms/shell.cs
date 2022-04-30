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
    public class Shell:Item
    {
        public Shell(ContentManager contentManager, double x, double y, int type, int weight)
        {
            ChangeCoords(x, y);

            Type = type;

            Weight = weight;

            updateTexture(contentManager, true);
        }

        public Shell(ContentManager contentManager, List<string> input, int currentStr)
        {
            Weight = Int32.Parse(input[currentStr + 1]);

            Name = input[currentStr + 2];

            ChangeCoords(double.Parse(input[currentStr + 3]), double.Parse(input[currentStr + 4]));

            Type = Int32.Parse(input[currentStr + 5]);

            base.Radius = 0.25;

            updateTexture(contentManager, true);
        }

        public override string SaveList()
        {
            return "Shell\n" + base.SaveList();
        }
    }
}