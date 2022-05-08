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
    public class UFO:Mob
    {
        public string Action { get; protected set; }

        public UFO(ContentManager contentManager, double X, double Y, double Z, double Speed)
        {
            
        }
    }
}