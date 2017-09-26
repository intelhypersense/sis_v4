using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiAndMic1
{
    public class jsondata
    {   
        public class Track
        {
            public int Channel { get; set; }
            public bool UseInstrument { get; set; }
            public string Instrument { get; set; }
            public string Threshold { get; set; }

        }
        public string Title { get; set; }
        public string MidiFile { get; set; }
        public string Mp3File { get; set; }
        public string Mp3Offset { get; set; }
        public bool PlayMidi { get; set; }
        public Track Track1 = new Track();
        public Track Track2 = new Track();
        public Track Track3 = new Track();
        public Track Track4 = new Track();
    }
    public static class jsonDataParse
    {
        public static List<jsondata> musicList = null;
        public static List<jsondata> getData(string path)
        {
            List<jsondata> jd = new List<jsondata>();
            StreamReader sr = new StreamReader(path, Encoding.Default);
            string st = "";
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                st += line;
            }
            JsonReader reader = new JsonTextReader(new StringReader(st));
            jsondata temp = new jsondata();
            while (reader.Read())
            {
                Console.WriteLine(reader.TokenType + "\t\t" + reader.ValueType + "\t\t" + reader.Value);
                if (reader.Value != null)
                {
                    if (reader.Value.ToString() == "Title")
                    {
                        reader.Read();
                        if (reader.Value == null)
                            temp.Title = null;
                        else
                            temp.Title = reader.Value.ToString();
                    }
                    else if (reader.Value.ToString() == "MidiFile")
                    {
                        reader.Read();
                        if (reader.Value == null)
                            temp.MidiFile = null;
                        else
                            temp.MidiFile = reader.Value.ToString();
                    }
                    else if (reader.Value.ToString() == "Mp3File")
                    {
                        reader.Read();
                        if (reader.Value == null)
                            temp.Mp3File = null;
                        else
                            temp.Mp3File = reader.Value.ToString();
                    }
                    else if (reader.Value.ToString() == "Mp3Offset")
                    {
                        reader.Read();
                        if (reader.Value == null)
                            temp.Mp3Offset = null;
                        else
                            temp.Mp3Offset = reader.Value.ToString();
                    }
                    else if (reader.Value.ToString() == "PlayMidi")
                    {
                        reader.Read();
                        if (reader.Value.ToString() == "False")
                        {
                            temp.PlayMidi = false;
                        }
                        else if (reader.Value.ToString() == "True")
                        {
                            temp.PlayMidi = true;
                        }
                    }
                    else if (reader.Value.ToString() == "Track1")
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType.ToString() == "EndObject")
                            {
                                break;
                            }
                            else if (reader.Value != null)
                            {
                                if (reader.Value.ToString() == "Channel")
                                {
                                    reader.Read();
                                    temp.Track1.Channel = int.Parse(reader.Value.ToString());
                                }
                                else if (reader.Value.ToString() == "UseInstrument")
                                {
                                    if (reader.Value.ToString() == "False")
                                    {
                                        temp.Track1.UseInstrument = false;
                                    }
                                    else if (reader.Value.ToString() == "True")
                                    {
                                        temp.Track1.UseInstrument = true;
                                    }
                                }
                                else if (reader.Value.ToString() == "Instrument")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track1.Instrument = null;
                                    else
                                        temp.Track1.Instrument = reader.Value.ToString();
                                }
                                else if (reader.Value.ToString() == "Threshold")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track1.Threshold = null;
                                    else
                                        temp.Track1.Threshold = reader.Value.ToString();
                                }
                            }
                        }
                    }
                    else if (reader.Value.ToString() == "Track2")
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType.ToString() == "EndObject")
                            {
                                break;
                            }
                            else if (reader.Value != null)
                            {
                                if (reader.Value.ToString() == "Channel")
                                {
                                    reader.Read();
                                    temp.Track2.Channel = int.Parse(reader.Value.ToString());
                                }
                                else if (reader.Value.ToString() == "UseInstrument")
                                {
                                    if (reader.Value.ToString() == "False")
                                    {
                                        temp.Track2.UseInstrument = false;
                                    }
                                    else if (reader.Value.ToString() == "True")
                                    {
                                        temp.Track2.UseInstrument = true;
                                    }
                                }
                                else if (reader.Value.ToString() == "Instrument")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track2.Instrument = null;
                                    else
                                        temp.Track2.Instrument = reader.Value.ToString();
                                }
                                else if (reader.Value.ToString() == "Threshold")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track2.Threshold = null;
                                    else
                                        temp.Track2.Threshold = reader.Value.ToString();
                                }
                            }
                        }
                    }

                    else if (reader.Value.ToString() == "Track3")
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType.ToString() == "EndObject")
                            {
                                break;
                            }
                            else if (reader.Value != null)
                            {
                                if (reader.Value.ToString() == "Channel")
                                {
                                    reader.Read();
                                    temp.Track3.Channel = int.Parse(reader.Value.ToString());
                                }
                                else if (reader.Value.ToString() == "UseInstrument")
                                {
                                    if (reader.Value.ToString() == "False")
                                    {
                                        temp.Track3.UseInstrument = false;
                                    }
                                    else if (reader.Value.ToString() == "True")
                                    {
                                        temp.Track3.UseInstrument = true;
                                    }
                                }
                                else if (reader.Value.ToString() == "Instrument")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track3.Instrument = null;
                                    else
                                        temp.Track3.Instrument = reader.Value.ToString();
                                }
                                else if (reader.Value.ToString() == "Threshold")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track3.Threshold = null;
                                    else
                                        temp.Track3.Threshold = reader.Value.ToString();
                                }
                            }
                        }
                    }
                    else if (reader.Value.ToString() == "Track4")
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType.ToString() == "EndObject")
                            {
                                break;
                            }
                            else if (reader.Value != null)
                            {
                                if (reader.Value.ToString() == "Channel")
                                {
                                    reader.Read();
                                    temp.Track4.Channel = int.Parse(reader.Value.ToString());
                                }
                                else if (reader.Value.ToString() == "UseInstrument")
                                {
                                    if (reader.Value.ToString() == "False")
                                    {
                                        temp.Track4.UseInstrument = false;
                                    }
                                    else if (reader.Value.ToString() == "True")
                                    {
                                        temp.Track4.UseInstrument = true;
                                    }
                                }
                                else if (reader.Value.ToString() == "Instrument")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track4.Instrument = null;
                                    else
                                        temp.Track4.Instrument = reader.Value.ToString();
                                }
                                else if (reader.Value.ToString() == "Threshold")
                                {
                                    reader.Read();
                                    if (reader.Value == null)
                                        temp.Track4.Threshold = null;
                                    else
                                        temp.Track4.Threshold = reader.Value.ToString();
                                }
                            }
                        }
                    }

                }
                else if (reader.TokenType.ToString() == "EndObject")
                {
                    if (temp.Title != null)
                    {
                        jd.Add(temp);
                        
                    }temp = new jsondata();
                }
            }
            return jd;
        }
    }
}
