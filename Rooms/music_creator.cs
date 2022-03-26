using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Rooms
{
    public class MusicCreator
    {
        public List<List<byte>> soundPlayList { get; protected set; }
        public List<SoundEffect> soundEffects { get; protected set; }
        public int currentIndex { get; protected set; }

        public MusicCreator(ContentManager contentManager)
        {
            currentIndex = 0;

            soundEffects = new List<SoundEffect>();
            soundPlayList = new List<List<byte>>();

            for (int i = 0; i < 4; i++)
            {
                soundEffects.Add(contentManager.Load<SoundEffect>("sound" + i.ToString()));
            }

            Random rnd = new Random();

            for (int i = 0; i < 4; i++)
            {
                List<byte> tmplist = new List<byte>();
                
                for (int j = 0; j < 16; j++)
                {
                    byte rn = (byte)rnd.Next(0, 2);

                    tmplist.Add(rn);
                }

                soundPlayList.Add(tmplist);
            }
        }

        public void Update()
        {
            for(int i=0; i<soundPlayList.Count; i++)
            {
                if(soundPlayList[i][currentIndex]==1)
                {
                    var inst = soundEffects[i].CreateInstance();

                    if(i==0)
                    {
                        inst.Volume *= 0.5f;
                    }

                    inst.Play();
                }
            }

            currentIndex++;

            if (currentIndex >= 16)
            {
                currentIndex = 0;

                var rnd = new Random();

                if(rnd.Next(0, 100)<100)
                {
                    for (int selectedSound = 0; selectedSound < 4; selectedSound++)
                    {
                        int newInd = rnd.Next(0, 16);

                        soundPlayList[selectedSound][newInd] = (byte)Math.Abs(soundPlayList[selectedSound][newInd] - 1);
                    }
                }
            }
        }
    }
}