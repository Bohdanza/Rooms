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
    public class Resource : Item
    {
        public Resource(ContentManager contentManager, double x, double y, double z, int type, int weight)
        {
            ChangeCoords(x, y, z);

            Type = type;

            Weight = weight;

            updateTexture(contentManager, true);
        }


        public Resource(ContentManager contentManager, List<string> input, int currentString)
        {
            Weight = Int32.Parse(input[currentString + 1]);

            Name = input[currentString + 2];

            ChangeCoords(double.Parse(input[currentString + 3]), double.Parse(input[currentString + 4]), 
                double.Parse(input[currentString + 5]));

            Type = Int32.Parse(input[currentString + 6]);

            updateTexture(contentManager, true);
        }

        public override string SaveList()
        {
            return "Resource\n" + base.SaveList();
        }
    }
}