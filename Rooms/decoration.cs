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
    }
}
