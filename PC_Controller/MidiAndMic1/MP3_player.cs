using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MidiAndMic1
{
    class MP3_player
    {            
        public static uint SND_ASYNC = 0x0001;
        public static uint SND_FILENAME = 0x00020000;
        public string song = "";
        [DllImport("winmm.dll")]
        public static extern uint mciSendString(string lpstrCommand,
        string lpstrReturnString, uint uReturnLength, uint hWndCallback);
        public void getSong(string song)
        {
            mciSendString(@"close temp_alias", null, 0, 0);
            this.song = @"open " + '\"' + song + '\"' + " alias temp_alias";
        }
        public void Play()
        {
            mciSendString(song, null, 0, 0);
            mciSendString("play temp_alias", null, 0, 0);//repeat
        }
        public void Play(string name)
        {
            mciSendString(@"close temp_alias", null, 0, 0);
            string a = @"open " + '\"' + name + '\"' + " alias temp_alias";
            mciSendString(a, null, 0, 0);
            mciSendString("play temp_alias", null, 0, 0);//repeat
        }
        public void stop()
        {
            mciSendString(@"pause temp_alias", null, 0, 0);
        }
        public void continue_play() {
            mciSendString("resume temp_alias", null, 0, 0);
        }
        public void end()
        {
            mciSendString(@"close temp_alias", null, 0, 0);
        }
    }

    
}
