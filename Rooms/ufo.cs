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
    public class UFO:Item
    {
        public string Action { get; protected set; }
        public double FlyHight { get; protected set; }
        protected double ZMove { get; set; }

        public UFO(ContentManager contentManager, double x, double y, double z, int type, double speed)
        {
            ZMove = 0.12;

            Speed = speed;

            ChangeCoords(x, y, z);

            Type = type;

            Speed = speed;

            Action = "id";

            updateTexture(contentManager, true);
        }

       
    }
}