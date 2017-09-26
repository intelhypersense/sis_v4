using MidiAndMic1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidiMsgspace
{
    public class MidiMsg
    {
        public Queue<byte[]> queueTCP = new Queue<byte[]>();
        public Queue<byte[]> queuePicture = new Queue<byte[]>();
        private MidiMessage mmTmp;
        public bool readFileFlag = false;
        public int[] filer = new int[5] { -1, -1, -1, -1,-1 };
        public int picture1 = 0;
        public int picture2 = 0;
        public int threshold3 = 64;
        public int threshold4 = 64;
        public int frethreshold3 = 128;
        public int frethreshold4 = 128;
        public const int CHANNEL_NUM = 16;
        public bool[,] valueFlag = new bool[128,16];
        public int minStrength = 0;
        public struct MidiMessage
        {
            public int midiTrick;
            public int midiEvent;
            public int midiData;
            public int midiDataLength;
        };
        public struct MidiOutMessage
        {
            public double timer;
            public uint msg;
            public int channel;
        };
        public struct MidiOutFakeMessage
        {
            public int timer;
            public uint msg;
            public int channel;
        };
        private double intervalTimer = 0;
        private const int PropertyLength = 2;
        private int format = 1;
        private int trackCount = 0;
        private int division = 0;
        public List<List<MidiOutFakeMessage>> queueList = new List<List<MidiOutFakeMessage>>();
        public Queue<MidiOutMessage> queue = new Queue<MidiOutMessage>();
        public int Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
            }
        }
        public int TrackCount
        {
            get
            {
                return trackCount;
            }
            set
            {
                trackCount = value;
            }
        }
        public int Division
        {
            get
            {
                return division;
            }
            set
            {
                division = value;
            }
        }
        public void Read(Stream strm)
        {
            readFileFlag = true;
            bool getIntervalflag = false;
            format = trackCount = division = 0;

            FindHeader(strm);
            Format = (int)ReadProperty(strm);
            TrackCount = (int)ReadProperty(strm);
            Division = (int)ReadProperty(strm);
            Console.WriteLine("Format=" + Format.ToString());
            Console.WriteLine("TrackCount=" + TrackCount.ToString());
            Console.WriteLine("Division=" + Division.ToString());
            for(int i = 0; i < trackCount; i++) {
                int truckBytes = FindTrunkHeader(strm);
                List<MidiOutFakeMessage> list = new List<MidiOutFakeMessage>();
                while(truckBytes > 0) {
                    
                    if(!getIntervalflag) {
                        MidiMessage mmTmp = getMessage(strm, ref truckBytes);
                        if(mmTmp.midiEvent == 0xFF5103) {
                            getIntervalflag = true;
                            intervalTimer = ((double)mmTmp.midiData / (double)division) * 0.001;
                        }
                            
                    } else {
                        MidiMessage mmTmp = getMessage(strm, ref truckBytes);
                        MidiOutFakeMessage mom;
                        mom.msg = 0;
                        if((mmTmp.midiEvent & 0xF0) == 0x90 || (mmTmp.midiEvent & 0xF0) == 0x80) {//
                            
                            mom.timer = mmTmp.midiTrick;
                            mom.msg = (uint)(((mmTmp.midiData & 0xFF) << 16) | (mmTmp.midiData & 0xFF00) | mmTmp.midiEvent);
                            mom.channel = mmTmp.midiEvent & 0x0F;
                            list.Add(mom);
                        } else if((mmTmp.midiEvent & 0xF0) == 0xc0 || (mmTmp.midiEvent & 0xF0) == 0xd0) {
                            mom.timer = mmTmp.midiTrick;
                            mom.msg = (uint)((mmTmp.midiData << 8) | mmTmp.midiEvent);
                            mom.channel = mmTmp.midiEvent & 0x0F;
                            list.Add(mom);
                        } else if((mmTmp.midiEvent & 0xF0) == 0xb0
                            || (mmTmp.midiEvent & 0xF0) == 0xa0 || (mmTmp.midiEvent & 0xF0) == 0xe0) {
                            mom.timer = mmTmp.midiTrick;
                            mom.msg = (uint)(((mmTmp.midiData & 0xFF) << 16) | (mmTmp.midiData & 0xFF00) | mmTmp.midiEvent);
                            mom.channel = mmTmp.midiEvent & 0x0F;
                            list.Add(mom);
                        } else {
                            mom.timer = mmTmp.midiTrick;
                            mom.msg = 0x00;
                            mom.channel = mmTmp.midiEvent & 0x0F;
                            list.Add(mom);
                        }
                    }
                }
                queueList.Add(list);
            }
            readFileFlag = false;
        }

        public void ReadValue(Stream strm)
        {
            for(int i = 0; i < valueFlag.GetLength(1); i++)
            {
                for(int j = 0;j < valueFlag.GetLength(0); j++)
                {
                    valueFlag[j, i] = false;
                }
            }
            bool getIntervalflag = false;
            FindHeader(strm);
            ReadProperty(strm);
            int trackcount = (int)ReadProperty(strm);
            ReadProperty(strm);
            for (int i = 0; i < trackcount; i++)
            {
                int truckBytes = FindTrunkHeader(strm);
                while (truckBytes > 0)
                {

                    if (!getIntervalflag)
                    {
                        MidiMessage mmTmp = getMessage(strm, ref truckBytes);
                        if (mmTmp.midiEvent == 0xFF5103)
                        {
                            getIntervalflag = true;
                        }
                    }
                    else {
                        MidiMessage mmTmp = getMessage(strm, ref truckBytes);
                        if ((mmTmp.midiEvent & 0xF0) == 0x90 || (mmTmp.midiEvent & 0xF0) == 0x80)
                        {
                            valueFlag[(mmTmp.midiData >> 8)&0xFF,mmTmp.midiEvent & 0x0F] = true;
                        }
                
                    }
                }
            }
        }
        MidiMessage getMessage(Stream stream, ref int truckBytes)
        {

            mmTmp.midiData = mmTmp.midiDataLength = mmTmp.midiTrick = 0;
            int trick = 0x00;
            bool trickFlag = true;
            while(trickFlag) {
                int trickTmp = stream.ReadByte();
                truckBytes--;
                if((trickTmp & 0x80) == 0x80)
                    trickFlag = true;
                else
                    trickFlag = false;
                trickTmp &= ~0x80;
                trick <<= 7;
                trick |= trickTmp;
            }
            mmTmp.midiTrick = trick;
            int lastdata = 0;
            int formatTmp = stream.ReadByte();
            truckBytes--;

            if(formatTmp < 0x7F) {
                lastdata = formatTmp;
                formatTmp = mmTmp.midiEvent;
                if((formatTmp >= 0x80 && formatTmp < 0xC0) || (formatTmp >= 0xE0 && formatTmp < 0xF0)) {
                    mmTmp.midiEvent = formatTmp;
                    int data = lastdata;
                    mmTmp.midiData = (data << 8) | stream.ReadByte();
                    truckBytes--;
                } else if(formatTmp >= 0xC0 && formatTmp < 0xE0) {
                    mmTmp.midiEvent = formatTmp;
                    mmTmp.midiData = lastdata;
                    truckBytes--;
                }
            } else if((formatTmp >= 0x80 && formatTmp < 0xC0) || (formatTmp >= 0xE0 && formatTmp < 0xF0)) {
                mmTmp.midiEvent = formatTmp;
                int data = stream.ReadByte();
                truckBytes--;
                mmTmp.midiData = (data << 8) | stream.ReadByte();
                truckBytes--;
            } else if(formatTmp >= 0xC0 && formatTmp < 0xE0) {
                mmTmp.midiEvent = formatTmp;
                mmTmp.midiData = stream.ReadByte();
                truckBytes--;
            } else if(formatTmp == 0xFF) {
                int formatOther = stream.ReadByte();
                truckBytes--;

                bool lengthFlag = true;
                int length = 0;
                while(lengthFlag) {
                    int lengthTmp = stream.ReadByte();
                    truckBytes--;
                    if((lengthTmp & 0x80) == 0x80)
                        lengthFlag = true;
                    else
                        lengthFlag = false;
                    lengthTmp &= ~0x80;
                    length <<= 7;
                    length |= lengthTmp;
                }
                mmTmp.midiEvent = (formatTmp << 16) | (formatOther << 8) | length;
                int data = 0;
                while(length > 0) {
                    data = (data << 8) | stream.ReadByte();
                    truckBytes--;
                    length--;
                }
                mmTmp.midiData = data;

            } else if(formatTmp == 0xF0) {
                bool lengthFlag = true;
                int length = 0;
                while(lengthFlag) {
                    int lengthTmp = stream.ReadByte();
                    truckBytes--;
                    if((lengthTmp & 0x80) == 0x80)
                        lengthFlag = true;
                    else
                        lengthFlag = false;
                    lengthTmp &= ~0x80;
                    length <<= 7;
                    length |= lengthTmp;
                }
                mmTmp.midiEvent = (formatTmp << 8) | length;
                while(length > 0) {
                    stream.ReadByte();
                    truckBytes--;
                    length--;
                }
            }
            return mmTmp;
        }
        private ushort ReadProperty(Stream strm)
        {
            byte[] data = new byte[PropertyLength];

            int result = strm.Read(data, 0, data.Length);

            if(result != data.Length) {
                throw new MidiFileException("End of MIDI file unexpectedly reached.");
            }

            if(BitConverter.IsLittleEndian) {
                Array.Reverse(data);
            }

            return BitConverter.ToUInt16(data, 0);
        }
        public int FindTrunkHeader(Stream stream)
        {
            bool found = false;
            int result;

            while(!found) {
                result = stream.ReadByte();

                if(result == 'M') {
                    result = stream.ReadByte();

                    if(result == 'T') {
                        result = stream.ReadByte();

                        if(result == 'r') {
                            result = stream.ReadByte();

                            if(result == 'k') {
                                found = true;
                            }
                        }
                    }
                }

                if(result < 0) {
                    throw new MidiFileException("Unable to find MIDI file header.");
                }
            }
            int trunkBytes = 0;
            // Eat the header length.
            for(int i = 0; i < 4; i++) {
                trunkBytes = (trunkBytes << 8) | stream.ReadByte();
            }
            return trunkBytes;
        }
        public void FindHeader(Stream stream)
        {
            bool found = false;
            int result;

            while(!found) {
                result = stream.ReadByte();

                if(result == 'M') {
                    result = stream.ReadByte();

                    if(result == 'T') {
                        result = stream.ReadByte();

                        if(result == 'h') {
                            result = stream.ReadByte();

                            if(result == 'd') {
                                found = true;
                            }
                        }
                    }
                }

                if(result < 0) {
                    throw new MidiFileException("Unable to find MIDI file header.");
                }
            }

            // Eat the header length.
            for(int i = 0; i < 4; i++) {
                if(stream.ReadByte() < 0) {
                    throw new MidiFileException("Unable to find MIDI file header.");
                }
            }
        }
        public byte[] ReadFileToBytes(string fullFile)
        {
            byte[] bytes = null;
            if(File.Exists(fullFile)) {
                try {
                    using(FileStream fs = new FileStream(fullFile, FileMode.Open, FileAccess.Read)) {
                        Read(fs);//
                        reSortList();
                    }
                } catch(Exception ex) {
                    throw ex;
                }
            }
            return bytes;
        }
        public byte[] ReadFileGetvalue(string fullFile)
        {
            byte[] bytes = null;
            if (File.Exists(fullFile))
            {
                try
                {
                    using (FileStream fs = new FileStream(fullFile, FileMode.Open, FileAccess.Read))
                    {
                        ReadValue(fs);//
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return bytes;
        }
        public void reSortList()
        {
            List<MidiOutFakeMessage> list = new List<MidiOutFakeMessage>();
            for(int i = 0;i < queueList.Count;i++) {
                int time = 0;
                for(int j = 0;j < queueList[i].Count;j++) {
                    MidiOutFakeMessage msgTemp = queueList[i][j];
                    msgTemp.timer += time;
                    time = msgTemp.timer;
                    list.Add(msgTemp);
                }
            }
            List<MidiOutFakeMessage> list1 = list.OrderBy(a => a.timer).ToList();
            int timerTemp = 0;
            for(int i = 0;i < list1.Count;i++) {
                MidiOutMessage mom;
                mom.channel = list1[i].channel;
                mom.timer = ((double)(list1[i].timer-timerTemp))*intervalTimer;
                mom.msg = list1[i].msg;
                Monitor.Enter(queue);
                queue.Enqueue(mom);
                Monitor.Exit(queue);
                timerTemp = list1[i].timer;
            }
        }
       
        public void storeTCPmsg(uint msg)
        {
            msg = msg & 0xFFFFFF;
            uint Event = 0;
            uint l_dw1 = 0;
            uint h_dw2 = 0;
            uint l_dw2 = 0;
            Event = msg & 0xFF;
            l_dw1 = (msg >> 8) & 0xFF;
            h_dw2 = (msg >> 16) & 0xFF;
            l_dw2 = (msg >> 24) & 0xFF;
            uint temp = (Event << 24) | (l_dw1 << 16) | (h_dw2 << 8) | 0x00;
            
            
            if ((Event & 0xF0) == 0x80 || (Event & 0xF0) == 0x90)
            {
                if ((Event & 0x0F) == filer[1])
                {
                    byte[] bpara = System.BitConverter.GetBytes(temp);
                    byte[] newByte = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        newByte[i] = bpara[3 - i];
                    }
                    if (newByte[2] > minStrength || (Event & 0xF0) != 0x90)
                    {
                        newByte[3] = 1;
                        Monitor.Enter(queueTCP);
                        queueTCP.Enqueue(newByte);
                        Monitor.Exit(queueTCP);
                        Monitor.Enter(queuePicture);
                        queuePicture.Enqueue(newByte);
                        Monitor.Exit(queuePicture);
                    }
                }
                if ((Event & 0x0F) == filer[2])
                {
                    byte[] bpara = System.BitConverter.GetBytes(temp);
                    byte[] newByte = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        newByte[i] = bpara[3 - i];
                    }
                   
                    if (newByte[2] > minStrength || (Event & 0xF0) != 0x90)
                    {
                        newByte[3] = 2;
                        Monitor.Enter(queueTCP);
                        queueTCP.Enqueue(newByte);
                        Monitor.Exit(queueTCP);
                        Monitor.Enter(queuePicture);
                        queuePicture.Enqueue(newByte);
                        Monitor.Exit(queuePicture);
                    }
                }
                if ((Event & 0x0F) == filer[3])
                {
                    
                    byte[] bpara = System.BitConverter.GetBytes(temp);
                    byte[] newByte = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        newByte[i] = bpara[3 - i];
                    }
                    if (frethreshold3 == 128 || frethreshold3 == newByte[1])
                    {
                        newByte[0] = 0xA0;
                        // newByte[2] = newByte[1];
                        if ((Event & 0xF0) == 0x80)
                        {
                            newByte[1] = 0;
                            newByte[2] = 0;
                        }
                        else
                        {
                            if (newByte[2] >= threshold3)
                            {
                                newByte[1] = 1;
                            }
                            else
                            {
                                newByte[1] = 0;
                                newByte[2] = 0;
                            }
                        }
                        newByte[3] = 3;
                        Monitor.Enter(queueTCP);
                        queueTCP.Enqueue(newByte);
                        Monitor.Exit(queueTCP);
                        Monitor.Enter(queuePicture);
                        queuePicture.Enqueue(newByte);
                        Monitor.Exit(queuePicture);
                    }
                }
                if ((Event & 0x0F) == filer[4])
                {
                    byte[] bpara = System.BitConverter.GetBytes(temp);
                    byte[] newByte = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        newByte[i] = bpara[3 - i];
                    }
                    if (frethreshold4 == 128 || frethreshold4 == newByte[1])
                    {
                        newByte[0] = 0xA0;
                        if ((Event & 0xF0) == 0x80)
                        {
                            newByte[1] = 0;
                            newByte[2] = 0;
                        }
                        else
                        {
                            if (newByte[2] >= threshold4)
                            {
                                newByte[1] = 1;
                            }
                            else
                            {
                                newByte[1] = 0;
                                newByte[2] = 0;
                            }
                        }
                        newByte[3] = 4;
                        Monitor.Enter(queueTCP);
                        queueTCP.Enqueue(newByte);
                        Monitor.Exit(queueTCP);
                        Monitor.Enter(queuePicture);
                        queuePicture.Enqueue(newByte);
                        Monitor.Exit(queuePicture);
                    }
                }

            }
        }

        public void drawValue(byte[] data,ref int channel,ref int picture)
        {
            if (data[3] == 1)
            {
                if ((data[0] & 0xF0) == 0x90)
                {
                    picture1 |= 1 << (data[1] % 12);
                }
                else
                {
                    picture1 &= ~(1 << (data[1] % 12));
                }
                channel = 1;
                picture = picture1;
            }
            else if (data[3] == 2)
            {
                if ((data[0] & 0xF0) == 0x90)
                {
                    picture2 |= 1 << (data[1] % 12);
                }
                else
                {
                    picture2 &= ~(1 << (data[1] % 12));
                }
                channel = 2;
                picture = picture2;
            }
        }
        public void drawValue(byte[] data, ref int channel,ref int picture,ref bool flag)
        {
            if (data[3] == 3)
            {
                channel = 3;
                picture = data[2];
                flag = (data[1] == 1);
            }
            else if (data[3] == 4)
            {
                channel = 4;
                picture = data[2];
                flag = (data[1] == 1);
            }
        }
    }

    public class MidiFileException : ApplicationException
    {
        public MidiFileException(string message) : base(message)
        {
        }
    }
}
