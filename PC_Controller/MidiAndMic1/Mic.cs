using Microsoft.DirectX.DirectSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiAndMic1
{
    public class Mic
    {
        //
        List<string> sList = new List<string>();
        public int fft_num = 2048;
        private string strRecSaveFile = string.Empty;//File path

        private FileStream fsWav = null;//save file stream
        private int iNotifyNum = 16;//notification number
        private int iBufferOffset = 0;//Start offset for the next data
        private int iSampleSize = 0;//sample size
        private int iNotifySize = 0;//notification size
        private int iBufferSize = 0;//buf size
        private BinaryWriter mWriter;
        private AutoResetEvent notifyevent = null;
        private Thread notifythread = null;
        private Thread fftthread = null;
        private Notify myNotify = null;//notifi event
        private Capture capture = null;//capture device object
        private CaptureBuffer capturebuffer = null;//capture buffer
        private WaveFormat mWavFormat;//PCM format
        public FFT fft = new FFT();
        Queue<float> queue = new Queue<float>();
        public int minVoice = 5;
        private WaveFormat SetWaveFormat()
        {
            WaveFormat format = new WaveFormat();
            format.FormatTag = WaveFormatTag.Pcm;//type of voice
            format.SamplesPerSecond = 22050;//sample rate（unit：Hz）typical：11025、22050、44100Hz
            format.BitsPerSample = 16;//sample bits
            format.Channels = 1;//Channel
            format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));//Number of bytes in the unit sampling point
            format.AverageBytesPerSecond = format.BlockAlign * format.SamplesPerSecond;
            return format;
            //Bytes of sample in one second : 22050*2=55100B
        }
        //创建WAVE文件
        private void CreateWaveFile(string strFileName)
        {
            if(File.Exists(strFileName))
            {
                File.Delete(strFileName);
            }
            fsWav = new FileStream(strFileName, FileMode.CreateNew);
            mWriter = new BinaryWriter(fsWav);
            /**************************************************************************
               Here is where the file will be created. A
               wave file is a RIFF file, which has chunks
               of data that describe what the file contains.
               A wave RIFF file is put together like this:
               The 12 byte RIFF chunk is constructed like this:
               Bytes 0 - 3 :  'R' 'I' 'F' 'F'
               Bytes 4 - 7 :  Length of file, minus the first 8 bytes of the RIFF description.
                                 (4 bytes for "WAVE" + 24 bytes for format chunk length +
                                 8 bytes for data chunk description + actual sample data size.)
                Bytes 8 - 11: 'W' 'A' 'V' 'E'
                The 24 byte FORMAT chunk is constructed like this:
                Bytes 0 - 3 : 'f' 'm' 't' ' '
                Bytes 4 - 7 : The format chunk length. This is always 16.
                Bytes 8 - 9 : File padding. Always 1.
                Bytes 10- 11: Number of channels. Either 1 for mono,  or 2 for stereo.
                Bytes 12- 15: Sample rate.
                Bytes 16- 19: Number of bytes per second.
                Bytes 20- 21: Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or
                                16 bit mono, 4 for 16 bit stereo.
                Bytes 22- 23: Number of bits per sample.
                The DATA chunk is constructed like this:
                Bytes 0 - 3 : 'd' 'a' 't' 'a'
                Bytes 4 - 7 : Length of data, in bytes.
                Bytes 8 -: Actual sample data.
              ***************************************************************************/
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };
            short shPad = 1;                // File padding
            int nFormatChunkLength = 0x10;  // Format chunk length.
            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.
            short shBytesPerSample = 0;     // Bytes per sample.
            // number of bytes in one sample
            if (8 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels)
                shBytesPerSample = 1;
            else if ((8 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels) || (16 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels))
                shBytesPerSample = 2;
            else if (16 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels)
                shBytesPerSample = 4;
            // RIFF
            mWriter.Write(ChunkRiff);
            mWriter.Write(nLength);
            mWriter.Write(ChunkType);
            // WAVE
            mWriter.Write(ChunkFmt);
            mWriter.Write(nFormatChunkLength);
            mWriter.Write(shPad);
            mWriter.Write(mWavFormat.Channels);
            mWriter.Write(mWavFormat.SamplesPerSecond);
            mWriter.Write(mWavFormat.AverageBytesPerSecond);
            mWriter.Write(shBytesPerSample);
            mWriter.Write(mWavFormat.BitsPerSample);
            //DATA
            mWriter.Write(ChunkData);
            mWriter.Write((int)0);   // The sample length will be written in later.
        }
        //Build two object
        private bool CreateCaputerDevice()
        {
            //enumerate all the working device
            CaptureDevicesCollection capturedev = new CaptureDevicesCollection();
            Guid devguid;
            if (capturedev.Count > 0)
            {
                devguid = capturedev[1].DriverGuid;
                MessageBox.Show("The device GUID is：" + devguid.ToString(), "System");
            }
            else {
                MessageBox.Show("There are currently no devices available for audio capture", "System");
                return false;
            }
            //Use the device GUID to create a capture device object
            capture = new Capture(devguid);
            return true;
        }
        private bool CreateSelectCaputerDevice(int i)
        {
            CaptureDevicesCollection capturedev = new CaptureDevicesCollection();
            Guid devguid = Guid.Empty;
            if (capturedev.Count > 0)
            {
                for (int j = 0; j < capturedev.Count; j++)
                {
                    if (sList[i] == capturedev[j].Description)
                    {
                        devguid = capturedev[j].DriverGuid;
                    }
                }
            }
            //Use the device GUID to create a capture device object
            if (devguid != Guid.Empty)
            {
                capture = new Capture(devguid);
                return true;
            } else
            {
                return false;
            }
        }
        public List<string> getMicList()
        {
            sList.Clear();
           // List<string> sL = new List<string>();
            CaptureDevicesCollection capturedev = new CaptureDevicesCollection();
            if (capturedev.Count > 0)
            {
                int j = 0;
                for (int i = 0; i < capturedev.Count; i++)
                {
                    if (capturedev[i].DriverGuid != System.Guid.Empty)
                    {
                        //sList[j++] = capturedev[i].Description;
                        sList.Add(capturedev[i].Description);
                    }
                }
            }
            return sList;
        }

        private void CreateCaptureBuffer()
        {
            //To create a capture buffer, you must have two parameters: 
            //the buffer information (describing the format in this buffer, etc.), the buffer device.

            CaptureBufferDescription bufferdescription = new CaptureBufferDescription();
            bufferdescription.Format = mWavFormat;//Sets the data format to be captured by the buffer
            iNotifySize = 1024;//Set the notification size
            iBufferSize = iNotifyNum * iNotifySize;
            bufferdescription.BufferBytes = iBufferSize;
            capturebuffer = new CaptureBuffer(bufferdescription, capture);//Create a device buffer object
        }
        //Set up notifications
        private void CreateNotification()
        {
            BufferPositionNotify[] bpn = new BufferPositionNotify[iNotifyNum];//Set the number of buffer notifications
                                                                              //Set the notification event
            notifyevent = new AutoResetEvent(false);
            notifythread = new Thread(RecoData);

            notifythread.IsBackground = true;
            notifythread.Start();
            fftthread = new Thread(fftFunction);
            fftthread.IsBackground = true;
            fftthread.Start();
            for (int i = 0; i < iNotifyNum; i++)
            {
                bpn[i].Offset = iNotifySize + i * iNotifySize - 1;//Set each specific location
                bpn[i].EventNotifyHandle = notifyevent.SafeWaitHandle.DangerousGetHandle();
            }
            myNotify = new Notify(capturebuffer);
            myNotify.SetNotificationPositions(bpn);

        }
        //Thread in the event
        private void RecoData()
        {
            while (true)
            {
                // Wait for the notification message for the buffer
                notifyevent.WaitOne(Timeout.Infinite, true);
                // Record data
                RecordCapturedData();
            }
        }
        private void fftFunction()
        {
            while (true)
            {
                if (queue.Count > fft_num)
                {
                    Monitor.Enter(queue);
                    float[] fData = new float[fft_num];
                    for (int i = 0; i < fft_num; i++)
                    {
                        fData[i] = queue.Dequeue();
                    }
                    Monitor.Exit(queue);
                    fft.calculate(fData);

                } else {
                    Thread.Sleep(10);
                }
            }
        }
        //The real transfer of data events, in fact, the data is transferred to the WAV file.

        private void RecordCapturedData()
        {
            byte[] capturedata = null;
            int readpos = 0, capturepos = 0, locksize = 0;
            capturebuffer.GetCurrentPosition(out capturepos, out readpos);
            locksize = readpos - iBufferOffset;//This size is the size we can safely read
            if (locksize == 0)
            {
                return;
            }
            if (locksize < 0)
            {
                //Because we are using the buffer for the loop, 
                //so there is a case of negative: 
                //when the text to read the pointer back to the first notification point, 
                //and Ibuffeoffset is still the last notification
                locksize += iBufferSize;
            }
            if(locksize%2 == 1)
            {
                locksize -= 1;
            }
            capturedata = (byte[])capturebuffer.Read(iBufferOffset, typeof(byte), LockFlag.FromWriteCursor, locksize);
            for (int i = 0; i < locksize; i += 2)
            {
                int temp = ((capturedata[i + 1] << 8) | (capturedata[i])) << 48 >> 48;
                if (temp < minVoice && temp > (0- minVoice))
                {
                    temp = 0;
                }
                float Ftemp = ((float)temp) / 65536;
                try {
                    Monitor.Enter(queue);
                    queue.Enqueue(Ftemp);
                    Monitor.Exit(queue);
                } catch(Exception ex)
                {
                    ;
                }
            }
            iSampleSize += capturedata.Length;
            iBufferOffset += capturedata.Length;
            iBufferOffset %= iBufferSize;//Modulo is because the buffer is looped.

        }
        /// <summary>
        /// 
        /// </summary>
        private void stoprec()
        {
            capturebuffer.Stop();//Call the buffer to stop the method. Stop collecting sound
            if (notifyevent != null)
                notifyevent.Set();//Close notification
            notifythread.Abort();//End the thread
            fftthread.Abort();
            RecordCapturedData();//Write the last part of the buffer to the file

        }
        public void MicCall(int deviceNumber,int comboboxNumber)
        {
            fft.selectNumber = deviceNumber;
            if (fft.numberEmpty())
            {
                mWavFormat = SetWaveFormat();
                CreateSelectCaputerDevice(deviceNumber);
                CreateCaptureBuffer();
                CreateNotification();
                capturebuffer.Start(true);
            }
            fft.numberAdd((byte)comboboxNumber);
        }
        public bool MicEmpty(int selectNum)
        {
            if(fft.selectNumber == -1 || fft.selectNumber == selectNum)
            {
                return true;
            }
            return false;
        }
        public bool MicEmpty()
        {
            if (fft.selectNumber == -1 )
            {
                return true;
            }
            return false;
        }
        public void MicAddNumber(int comboboxNumber)
        {
            fft.numberAdd((byte)comboboxNumber);
        }
        public void MicStop()
        {
            stoprec();
            fft.numberClear();
        }
    }
}
