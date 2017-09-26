using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiAndMic1
{
    class MidiValue
    {
        public string[] value = new string[128];
        public MidiValue()
        {
            for(int i = 0;i < 128;i++)
            {
                value[i] = i.ToString() + ".";
            }
            value[35] = "35.Bass Drum 2";
            value[36] = "36.Bass Drum 1";
            value[37] = "37.Side Stick";
            value[38] = "38.Snare Drum 1";
            value[39] = "39.Hand Clap";
            value[40] = "40.Snare Drum 2";
            value[41] = "41.Low Tom 2";
            value[42] = "42.Closed Hi-hat";
            value[43] = "43.Low Tom 1";
            value[44] = "44.Pedal Hi-hat";
            value[45] = "45.Mid Tom 2";
            value[46] = "46.Open Hi-hat";
            value[47] = "47.Mid Tom 1";
            value[48] = "48.High Tom 2";
            value[49] = "49.Crash Cymbal 1";
            value[50] = "50.High Tom 1";
            value[51] = "51.Ride Cymbal 1";
            value[52] = "52.Chinese Cymbal";
            value[53] = "53.Ride Bell";
            value[54] = "54.Tambourine";
            value[55] = "55.Splash Cymbal";
            value[56] = "56.Cowbell";
            value[57] = "57.Crash Cymbal 2";
            value[58] = "58.Vibra Slap";
            value[59] = "59.Ride Cymbal 2";
            value[60] = "60.High Bongo";
            value[61] = "61.Low Bongo";
            value[62] = "62.Mute High Conga";
            value[63] = "63.Open High Conga";
            value[64] = "64.Low Conga";
            value[65] = "65.High Timbale";
            value[66] = "66.Low Timbale";
            value[67] = "67.High Agogo";
            value[68] = "68.Low Agogo";
            value[69] = "69.Cabasa";
            value[70] = "70.Maracas";
            value[71] = "71.Short Whistle";
            value[72] = "72.Long Whistle";
            value[73] = "73.Short Guiro";
            value[74] = "74.Long Guiro";
            value[75] = "75.Claves";
            value[76] = "76.High Wood Block";
            value[77] = "77.Low Wood Block";
            value[78] = "78.Mute Cuica ";
            value[79] = "79.Open Cuica ";
            value[80] = "80.Mute Triangle";
            value[81] = "81.Open Triangle";
        }
    }
}
