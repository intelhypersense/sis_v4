using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace console_midi
{

    public class InputPort
    {
        public int pictureValue1 = 0;
        public int pictureValue2 = 0;
        public int pictureValue3 = 0;
        public int pictureValue4 = 0;
        public byte[] number = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };
        public int selectNumber = -1;
        public Queue<byte[]> queue = new Queue<byte[]>();
        public Queue<byte[]> queuePicture = new Queue<byte[]>();
        public Queue<int> queue1 = new Queue<int>();
        public Queue<int> queue2 = new Queue<int>();
        public Queue<int> queue3 = new Queue<int>();
        public Queue<int> queue4 = new Queue<int>();
        public Queue<uint> queueOut = new Queue<uint>();
        private NativeMethods.MidiInProc midiInProc;
        private IntPtr handle;
        public int threshold3 = 64;
        public int threshold4 = 64;
        public int prethreshold3 = 128;
        public int prethreshold4 = 128;
        public int filter1 = -1;
        public int filter2 = -1;
        public int filter3 = -1;
        public int filter4 = -1;
        public byte[] filter_min = new byte[11] {0,12,24,36,48,60,72,84,96,108,120 };
        public byte[] filter_max = new byte[11] { 11,23,35,47,59,71,83,95,107,119,127};
        //{ new filter_range({ 0, 11 }), { 12,23}, { 24, 35 }, { 34, 47 }, { 48, 59 }, { 60, 71 }, { 72, 83 }, { 84, 95 }, { 96, 107 }, { 108, 119 }, { 120,127} };
        public void pictureValueReset()
        {
            pictureValue1 = pictureValue2 = pictureValue3 = pictureValue4 = 0;
        }
        public InputPort()
        {
            midiInProc = new NativeMethods.MidiInProc(MidiProc);
            handle = IntPtr.Zero;
        }

        public static int InputCount
        {
            get { return NativeMethods.midiInGetNumDevs(); }
        }

        public bool Close()
        {
            selectNumber = -1;
            number[0] = 0x00;
            number[1] = 0x00;
            number[2] = 0x00;
            number[3] = 0x00;
            number[4] = 0x00;
            pictureValueReset();
            bool result = NativeMethods.midiInClose(handle)
                == NativeMethods.MMSYSERR_NOERROR;
            handle = IntPtr.Zero;
            return result;
        }

        public bool Open(int id,int num)
        {
            selectNumber = id;
            
            if (number[1] == 0x00 && number[2] == 0x00 && number[3] == 0x00 && number[4] == 0x00)
            {
                return NativeMethods.midiInOpen(
                out handle,
                id,
                midiInProc,
                IntPtr.Zero,
                NativeMethods.CALLBACK_FUNCTION)
                    == NativeMethods.MMSYSERR_NOERROR;
            }
            else
            {
                return true;
            }
           
        }

        public bool Start()
        {
            return NativeMethods.midiInStart(handle)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool Stop()
        {
            return NativeMethods.midiInStop(handle)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public IntPtr hmidi = IntPtr.Zero;
        public uint midi_wMsg = 0;
        public IntPtr midi_dwIns = IntPtr.Zero;
        public UInt32 midi_dw1 = 0;
        public UInt32 mide_dw2 = 0;

        public void MidiProc(IntPtr hMidiIn,
          uint wMsg,
          IntPtr dwInstance,
          UInt32 dwParam1,
          UInt32 dwParam2)
        {
            // Receive messages here
            hmidi = hMidiIn;

            if (wMsg == 963)
            {
                queueOut.Enqueue(dwParam1);
                dwParam1 = dwParam1 & 0xFFFFFF;
                uint Event = 0;
                uint l_dw1 = 0;
                uint h_dw2 = 0;
                uint l_dw2 = 0;
                Event = dwParam1 & 0xFF;
                l_dw1 = (dwParam1 >> 8) & 0xFF;
                h_dw2 = (dwParam1 >> 16) & 0xFF;
                l_dw2 = (dwParam1 >> 24) & 0xFF;

                if ((Event & 0xF0) == 0x80 || (Event & 0xF0) == 0x90)
                {

                    if (number[1] == 1)
                    {
                        uint temp = (Event << 24) | (l_dw1 << 16) | (h_dw2 << 8) | 0x00;
                        byte[] bpara = System.BitConverter.GetBytes(temp);
                        byte[] newByte = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            newByte[i] = bpara[3 - i];
                        }
                        if (filter1 == -1)
                        {
                            newByte[3] = 1;
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                        else if (newByte[1] >= filter_min[filter1] && newByte[1] <= filter_max[filter1])
                        {
                            newByte[3] = 1;
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                    }
                    if (number[2] == 2)
                    {
                        uint temp = (Event << 24) | (l_dw1 << 16) | (h_dw2 << 8) | 0x00;
                        byte[] bpara = System.BitConverter.GetBytes(temp);
                        byte[] newByte = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            newByte[i] = bpara[3 - i];
                        }
                        if (filter2 == -1)
                        {
                            newByte[3] = 2;
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                        else if (newByte[1] >= filter_min[filter2] && newByte[1] <= filter_max[filter2])
                        {
                            newByte[3] = 2;
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                    }
                    if (number[3] == 3)
                    {
                        uint temp = (Event << 24) | (l_dw1 << 16) | (h_dw2 << 8) | 0x00;
                        byte[] bpara = System.BitConverter.GetBytes(temp);
                        byte[] newByte = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            newByte[i] = bpara[3 - i];
                        }
                        newByte[0] = 0xA0;
                        if (newByte[1] == prethreshold3 || prethreshold3 == 128)
                        {
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
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                    }
                    if (number[4] == 4)
                    {
                        uint temp = (Event << 24) | (l_dw1 << 16) | (h_dw2 << 8) | 0x00;
                        byte[] bpara = System.BitConverter.GetBytes(temp);
                        byte[] newByte = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            newByte[i] = bpara[3 - i];
                        }
                        newByte[0] = 0xA0;
                        if (newByte[1] == prethreshold4 || prethreshold4 == 128)
                        {
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
                            Monitor.Enter(queue);
                            queue.Enqueue(newByte);
                            Monitor.Exit(queue);
                            Monitor.Enter(queuePicture);
                            queuePicture.Enqueue(newByte);
                            Monitor.Exit(queuePicture);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(Convert.ToString(wMsg, 16));
                Console.WriteLine(Convert.ToString(dwParam1, 16));
                Console.WriteLine(Convert.ToString(dwParam2, 16));
                Console.WriteLine("-------------------------------");
            }

        }
        public bool MidiEmpty(int selectNum)
        {
            if (selectNumber == -1 || selectNumber == selectNum)
            {
                return true;
            }
            return false;
        }
        public bool MidiEmpty()
        {
            if (selectNumber == -1)
            {
                return true;
            }
            return false;
        }
        public void MidiAddNumber(int num)
        {
            number[num] = (byte)num;
        }

        public void drawValue(byte[] data, ref int channel, ref int picture)
        {
            if (data[3] == 1)
            {
                if ((data[0] & 0xF0) == 0x90)
                {
                    pictureValue1 |= 1 << (data[1] % 12);
                }
                else
                {
                    pictureValue1 &= ~(1 << (data[1] % 12));
                }
                channel = 1;
                picture = pictureValue1;
            }
            else if (data[3] == 2)
            {
                if ((data[0] & 0xF0) == 0x90)
                {
                    pictureValue2 |= 1 << (data[1] % 12);
                }
                else
                {
                    pictureValue2 &= ~(1 << (data[1] % 12));
                }
                channel = 2;
                picture = pictureValue2;
            }
        }
        public void drawValue(byte[] data, ref int channel, ref int picture, ref bool flag)
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

    internal static class NativeMethods
    {
        internal const int MMSYSERR_NOERROR = 0;
        internal const int CALLBACK_FUNCTION = 0x00030000;

        internal delegate void MidiInProc(
            IntPtr hMidiIn,
            uint wMsg,
            IntPtr dwInstance,
            UInt32 dwParam1,
            UInt32 dwParam2);

        [DllImport("winmm.dll")]
        internal static extern int midiInGetNumDevs();

        [DllImport("winmm.dll")]
        internal static extern int midiInClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInOpen(
            out IntPtr lphMidiIn,
            int uDeviceID,
            MidiInProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);

        [DllImport("winmm.dll")]
        internal static extern int midiInStart(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInStop(
            IntPtr hMidiIn);
    }
}


