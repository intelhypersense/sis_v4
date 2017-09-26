using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using console_midi;

using Microsoft.DirectX.DirectSound;
using Microsoft.DirectX;
using System.IO;
using System.Drawing.Drawing2D;
using MidiMsgspace;
using highPerformanceTimer;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MidiAndMic1
{
   
    public partial class Form1 : Form
    {
        private static int THRESHOLD_DEFAULT = 64;
        private static int FRE_MIN = 1;
        private static int FRE_MIN_DEFAULT = 50;
        private static int FRE_MAX = 11264;
        private static int FRE_MAX_DEFAULT = 11264;
        private int combobox3_threshold = THRESHOLD_DEFAULT;
        private int combobox4_threshold = THRESHOLD_DEFAULT;
        private int combobox1_min = FRE_MIN_DEFAULT;
        private int combobox1_max = FRE_MAX_DEFAULT;
        private int combobox2_min = FRE_MIN_DEFAULT;
        private int combobox2_max = FRE_MAX_DEFAULT;
        public int MicNumber = 0;
        public int MidiNumber = 0;
        public int Combobox1Select = 0;
        public int Combobox2Select = 0;
        public int Combobox3Select = 0;
        public int Combobox4Select = 0;
        public Mic mic = new Mic();
        public Mic mic1 = new Mic();
        public Mic mic2 = new Mic();
        public Mic mic3 = new Mic();
        public InputPort midi = new InputPort();
        public InputPort midi1 = new InputPort();
        public InputPort midi2 = new InputPort();
        public InputPort midi3 = new InputPort();
        public Thread thread = null;
        public Thread loopThread = null;
        public Thread Threadmic4value = null;
        public Thread Threadmic3value = null;
        public Thread Threadmic2value = null;
        public Thread Threadmic1value = null;
        public Thread Threadmidi1value = null;
        public Thread Threadmidi2value = null;
        public Thread Threadmidi3value = null;
        public Thread Threadmidi4value = null;
        public Thread ThreadmidiMsgPic = null;
        public Thread ThreadUDP = null;
        //Thread timerThread = null;
        ToolTip tipCombobox1Fre = new ToolTip();
        ToolTip tipCombobox2Fre = new ToolTip();
        //MidiOut _hoverTreeMidi=new MidiOut();
        //public MidiMsg midiMsg = new MidiMsg();
        MidiPlay midiPlay = new MidiPlay();
        MP3_player mp3play = new MP3_player();
        List<jsondata> musicList = null;
        // high_performance_timer HPT = new high_performance_timer();
        public void buttonText(string text)
        {
            if (button_play.InvokeRequired)
            {
                Action<string> actionDelegate = (x) =>
                {
                    this.button_play.Text = x;
                    mp3play.stop();
                    mp3play.end();
                };
                this.button_play.Invoke(actionDelegate, text);
            }
            else {
                this.button_play.Text = text;
            }
        }
        public void stopMp()
        {
            mp3play.stop();
            mp3play.end();
        }
        void InitMusicList(List<jsondata> musicList)
        {
            for(int i = 0;i < musicList.Count; i++)
            {
                comboBoxMusicList.Items.Add(musicList[i].Title);
            }
            comboBoxMusicList.Items.Add("NULL");
            comboBoxMusicList.SelectedIndex = comboBoxMusicList.Items.Count - 1;
        }
        public Form1()
        {
            ThreadUDP = new Thread(AsynchronousSocketListener.UDPRecvThread);
            ThreadUDP.IsBackground = true;
            ThreadUDP.Start();
            InitializeComponent();
            jsonDataParse.musicList = jsonDataParse.getData("sis.config.json");
            
            updateCombox();
            InitMusicList(jsonDataParse.musicList);
            Color setBlue;
            setBlue = Color.FromArgb(57, 102, 140);
            this.Show();
            setTip();
            this.Paint += new PaintEventHandler(backgroudSet);
            Thread midiout = new Thread(midiOutput);
            midiout.IsBackground = true;
            midiout.Start();
            midiPlay.midiOut.Open();
            midiPlay.changeText += buttonText;
            midiPlay.stopMp3 = new MidiPlay.StopMp3(stopMp);
        }
        
        void midiOutput()
        {

            while (true)
            {
              //  uint temp = null;
                if (midi.queueOut.Count > 0)
                {
                    Monitor.Enter(midi.queueOut);
                    midiPlay.midiOut.ShortPlay(midi.queueOut.Dequeue());
                    Monitor.Exit(midi.queueOut);
                }
                else if (midi1.queueOut.Count > 0)
                {
                    Monitor.Enter(midi1.queueOut);
                    midiPlay.midiOut.ShortPlay(midi1.queueOut.Dequeue());
                    Monitor.Exit(midi1.queueOut);
                }
                else if (midi3.queueOut.Count > 0)
                {
                    Monitor.Enter(midi3.queueOut);
                    midiPlay.midiOut.ShortPlay(midi2.queueOut.Dequeue());
                    Monitor.Exit(midi3.queueOut);
                }
                else if (midi2.queueOut.Count > 0)
                {
                    Monitor.Enter(midi2.queueOut);
                    midiPlay.midiOut.ShortPlay(midi3.queueOut.Dequeue());
                    Monitor.Exit(midi2.queueOut);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        void backgroudSet(object sender, PaintEventArgs e)
        {
            Color setBlue;
            setBlue = Color.FromArgb(57, 102, 140);
            Pen bluePen = new Pen(setBlue, 1);
            Graphics g = panel5.CreateGraphics();
            Rectangle myR = new Rectangle(0+3, 0+3, panel5.Width-6, panel5.Width-6);
            g.DrawEllipse(bluePen, myR);
            g.FillEllipse(new SolidBrush(setBlue), myR);
            label15.BackColor = setBlue;

            g = panel6.CreateGraphics();
            myR = new Rectangle(0 + 3, 0 + 3, panel6.Width - 6, panel6.Width - 6);
            g.DrawEllipse(bluePen, myR);
            g.FillEllipse(new SolidBrush(setBlue), myR);
            label16.BackColor = setBlue;

            g = panel7.CreateGraphics();
            myR = new Rectangle(0 + 3, 0 + 3, panel7.Width - 6, panel8.Width - 6);
            g.DrawEllipse(bluePen, myR);
            g.FillEllipse(new SolidBrush(setBlue), myR);
            label17.BackColor = setBlue;

            g = panel8.CreateGraphics();
            myR = new Rectangle(0 + 3, 0 + 3, panel7.Width - 6, panel8.Width - 6);
            g.DrawEllipse(bluePen, myR);
            g.FillEllipse(new SolidBrush(setBlue), myR);
            label18.BackColor = setBlue;
        }
        private void setTip()
        {
            tipCombobox1Fre.InitialDelay = 200;
            tipCombobox1Fre.AutoPopDelay = 10 * 1000;
            tipCombobox1Fre.ReshowDelay = 200;
            tipCombobox1Fre.ShowAlways = true;
            tipCombobox1Fre.IsBalloon = true;

            tipCombobox2Fre.InitialDelay = 200;
            tipCombobox2Fre.AutoPopDelay = 10 * 1000;
            tipCombobox2Fre.ReshowDelay = 200;
            tipCombobox2Fre.ShowAlways = true;
            tipCombobox2Fre.IsBalloon = true;
        }
        private void func()
        {
            AsynchronousSocketListener.StartListening(AsynchronousSocketListener.GetLocalIp(), "8887");
        }
        private void updateIplist(string str,int function)
        {
            int ipCount = 0;
            if(comboBox_client.InvokeRequired) {
                if(function == 1) {
                    Action<string> actionDelegate = (x) => {
                        this.comboBox_client.Items.Add(x);
                        ipCount = this.comboBox_client.Items.Count;
                    };
                    this.comboBox_client.Invoke(actionDelegate, str);
                } else {
                    Action<string> actionDelegate = (x) => {
                        this.comboBox_client.Items.Remove(x);
                        ipCount = this.comboBox_client.Items.Count;
                    };
                    this.comboBox_client.Invoke(actionDelegate, str);
                }
            } else {
                if(function == 1) {
                    this.comboBox_client.Items.Add(str);
                    ipCount = this.comboBox_client.Items.Count;
                } else {
                    this.comboBox_client.Items.Remove(str);
                    ipCount = this.comboBox_client.Items.Count;
                }
            }
            if(label_IPcount.InvokeRequired) {
                Action<string> actionDelegate = (x) => {
                    this.label_IPcount.Text = x;
                };
                this.label_IPcount.Invoke(actionDelegate, ipCount.ToString());
            } else {
                this.label_IPcount.Text = ipCount.ToString();
            }
        }
        private void loopFunc()
        {
            int j = 0;
            while (true)
            {
                if(AsynchronousSocketListener.newConnectIpLst.Count != 0)
                {
                    string str = AsynchronousSocketListener.newConnectIpLst.Dequeue();
                    updateIplist(str, 1);
                }
                byte[] msgbyte = { };
                if (midi.queue.Count != 0)
                {
                    Monitor.Enter(midi.queue);
                    msgbyte = msgbyte.Concat(midi.queue.Dequeue()).ToArray();
                    Monitor.Exit(midi.queue);
                }
                if (midi1.queue.Count != 0)
                {
                    Monitor.Enter(midi1.queue);
                    msgbyte = msgbyte.Concat(midi1.queue.Dequeue()).ToArray();
                    Monitor.Exit(midi1.queue);
                }
                if (midi2.queue.Count != 0)
                {
                    Monitor.Enter(midi2.queue);
                    msgbyte = msgbyte.Concat(midi2.queue.Dequeue()).ToArray();
                    Monitor.Exit(midi2.queue);
                }
                if (midi3.queue.Count != 0)
                {
                    Monitor.Enter(midi3.queue);
                    msgbyte = msgbyte.Concat(midi3.queue.Dequeue()).ToArray();
                    Monitor.Exit(midi3.queue);
                }
                if (mic.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic.fft.queue);
                    msgbyte = msgbyte.Concat(mic.fft.queue.Dequeue()).ToArray();
                    Monitor.Exit(mic.fft.queue);
                }
                if (mic1.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic1.fft.queue);
                    msgbyte = msgbyte = msgbyte.Concat(mic1.fft.queue.Dequeue()).ToArray();
                    Monitor.Exit(mic1.fft.queue);
                }
                if (mic2.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic2.fft.queue);
                    msgbyte = msgbyte.Concat(mic2.fft.queue.Dequeue()).ToArray();
                    Monitor.Exit(mic2.fft.queue);
                }
                if (mic3.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic3.fft.queue);
                    msgbyte = msgbyte.Concat(mic3.fft.queue.Dequeue()).ToArray();
                    Monitor.Exit(mic3.fft.queue);
                }
                if (midiPlay.midiMsg.queueTCP.Count != 0)
                {
                    Monitor.Enter(midiPlay.midiMsg.queueTCP);
                    msgbyte = msgbyte.Concat(midiPlay.midiMsg.queueTCP.Dequeue()).ToArray();
                    Monitor.Exit(midiPlay.midiMsg.queueTCP);
                }
                /*               if (AsynchronousSocketListener.newConnectIp != "")
                               {
                                   updateIplist(AsynchronousSocketListener.newConnectIp, 1);
                                   AsynchronousSocketListener.newConnectIp = "";
                               }*/
                /*byte[] midiData = null;
                if(midi.queue.Count != 0) {
                   Monitor.Enter(midi.queue);
                   midiData = midi.queue.Dequeue();
                   Monitor.Exit(midi.queue);
                }
                byte[] midi1Data = null;
                if (midi1.queue.Count != 0)
                {
                    Monitor.Enter(midi1.queue);
                    midi1Data = midi1.queue.Dequeue();
                    Monitor.Exit(midi1.queue);
                }
                byte[] midi2Data = null;
                if (midi2.queue.Count != 0)
                {
                    Monitor.Enter(midi2.queue);
                    midi2Data = midi2.queue.Dequeue();
                    Monitor.Exit(midi2.queue);
                }
                byte[] midi3Data = null;
                if (midi3.queue.Count != 0)
                {
                    Monitor.Enter(midi3.queue);
                    midi3Data = midi3.queue.Dequeue();
                    Monitor.Exit(midi3.queue);
                }
                byte[] micData = null;
                if(mic.fft.queue.Count != 0) {
                    Monitor.Enter(mic.fft.queue);
                    micData = mic.fft.queue.Dequeue();
                    Monitor.Exit(mic.fft.queue);
                }
                byte[] mic1Data = null;
                if (mic1.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic1.fft.queue);
                    mic1Data = mic1.fft.queue.Dequeue();
                    Monitor.Exit(mic1.fft.queue);
                }
                byte[] mic2Data = null;
                if (mic2.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic2.fft.queue);
                    mic2Data = mic2.fft.queue.Dequeue();
                    Monitor.Exit(mic2.fft.queue);
                }
                byte[] mic3Data = null;
                if (mic3.fft.queue.Count != 0)
                {
                    Monitor.Enter(mic2.fft.queue);
                    mic3Data = mic3.fft.queue.Dequeue();
                    Monitor.Exit(mic3.fft.queue);
                }
                byte[] midiMsgData = null;
                if(midiPlay.midiMsg.queueTCP.Count != 0) {
                    Monitor.Enter(midiPlay.midiMsg.queueTCP);
                    midiMsgData = midiPlay.midiMsg.queueTCP.Dequeue();
                    Monitor.Exit(midiPlay.midiMsg.queueTCP);
                }*/

                if (AsynchronousSocketListener.handleList.Count != 0)
                {
                    for(int i = 0; i < AsynchronousSocketListener.handleList.Count; i++) {
                        try {
                            if(AsynchronousSocketListener.handleList[i].Connected == true) {
 //                               if (midiData == null && midi1Data == null && midi2Data == null && midi3Data == null
 //                                   && mic1Data == null && micData == null && mic2Data == null && mic3Data == null)
                                if(msgbyte.Length == 0)
                                {
                                    Thread.Sleep(10);
                                }
                                if(msgbyte.Length > 0)
                                     AsynchronousSocketListener.handleList[i].Send(msgbyte);

                                /*if(midiData != null) {
                                    AsynchronousSocketListener.handleList[i].Send(midiData);
                                }
                                if(midi1Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(midi1Data);
                                }
                                if(midi2Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(midi2Data);
                                }
                                if(midi3Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(midi3Data);
                                }
                                if(mic1Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(mic1Data);
                                }
                                if(micData != null) {
                                    AsynchronousSocketListener.handleList[i].Send(micData);
                                }
                                if(mic2Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(mic2Data);
                                }
                                if(mic3Data != null) {
                                    AsynchronousSocketListener.handleList[i].Send(mic3Data);
                                }
                                if(midiMsgData != null) {
                                    AsynchronousSocketListener.handleList[i].Send(midiMsgData);
                                }*/
                                string comboboxfile = "";
                                if(comboBox_file.InvokeRequired) {
                                    Action<string> actionDelegate = (x) => {
                                        x = this.comboBox_file.SelectedItem.ToString();
                                    };
                                    this.label_IPcount.Invoke(actionDelegate, comboboxfile);
                                } else {
                                    comboboxfile = this.comboBox_file.SelectedItem.ToString();
                                }
                                if(comboboxfile == "NULL") {
                                    if(Combobox1Select == (MicNumber + MidiNumber) && Combobox2Select == (MicNumber + MidiNumber)
                                        && Combobox3Select == MicNumber && Combobox4Select == MicNumber) {
                                        byte[] heart = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
                                        AsynchronousSocketListener.handleList[i].Send(heart);
                                        Thread.Sleep(100);
                                    }
                                }
                            } else {
                                updateIplist(AsynchronousSocketListener.handleList[i].RemoteEndPoint.ToString(), 0);
                                AsynchronousSocketListener.handleList.Remove(AsynchronousSocketListener.handleList[i]);
                            }
                        } catch(Exception ex) {
                            ;
                        }
                    }
                } else {
                    Thread.Sleep(10);
                }
            }
        }
        void cleanComboxItems()
        {
            comboBox_1.Items.Clear();
            comboBox_2.Items.Clear();
            comboBox_3.Items.Clear();
            comboBox_4.Items.Clear();
        }
        string findFile(string name, DirectoryInfo dir)
        {
            foreach (FileSystemInfo fsi in dir.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    FileInfo fi = (FileInfo)fsi;
                    if(fi.Name == name)
                        return fi.FullName;
                }
                else
                {
                    DirectoryInfo di = (DirectoryInfo)fsi;
                    return findFile(name,di);
                }
            }
            return null;
        }
        void updateChannelCombox(jsondata jdata)
        {
            if (jdata.MidiFile != null)
            {
                string s = findFile(jdata.MidiFile, new DirectoryInfo(@".."));
                comboBox_file.Items.Add(s);
                comboBox_file.SelectedIndex = comboBox_file.Items.Count - 1;
            } else
            {
                comboBox_file.SelectedIndex = 0;
            }
            if (jdata.Mp3File != null)
            {
                string sMp3 = findFile(jdata.Mp3File, new DirectoryInfo(@".."));
                comboBox_mp3.Items.Add(sMp3);
                comboBox_mp3.SelectedIndex = comboBox_mp3.Items.Count - 1;
            } else
            {
                comboBox_mp3.SelectedIndex = 0;
            }
            comboBox_1.SelectedIndex = jdata.Track1.Channel - 1;
            comboBox_2.SelectedIndex = jdata.Track2.Channel - 1;
            comboBox_3.SelectedIndex = jdata.Track3.Channel - 1;
            comboBox_4.SelectedIndex = jdata.Track4.Channel - 1;
            if (jdata.Track3.Threshold != null) {
                int temp = int.Parse(jdata.Track3.Threshold);
                if (temp >= 0 && temp < 128)
                {
                    trackBar_mic3_threshold.Value = int.Parse(jdata.Track3.Threshold);
                    midi1.threshold3 = midi2.threshold3 = midi.threshold3 = midi3.threshold3 = trackBar_mic3_threshold.Value;
                    midiPlay.midiMsg.threshold3 = trackBar_mic3_threshold.Value;
                    combobox3_threshold = trackBar_mic3_threshold.Value;
                    label20.Location = new Point(trackBar_mic3_threshold.Location.X + (int)(((double)trackBar_mic3_threshold.Value / trackBar_mic3_threshold.Maximum) * trackBar_mic3_threshold.Size.Width) - label20.Size.Width / 2, label20.Location.Y);
                    label20.Text = trackBar_mic3_threshold.Value.ToString();
                }

            }
            if(jdata.Track3.Instrument != null)
            {
                comboBox_midi3_select.Items.Add(jdata.Track3.Instrument);
                comboBox_midi3_select.SelectedIndex = comboBox_midi3_select.Items.Count - 1;
                midiPlay.midiMsg.frethreshold3 = int.Parse(jdata.Track3.Instrument);
            }
            if (jdata.Track4.Instrument != null)
            {
                comboBox_midi4_select.Items.Add(jdata.Track4.Instrument);
                comboBox_midi4_select.SelectedIndex = comboBox_midi4_select.Items.Count - 1;
                midiPlay.midiMsg.frethreshold4 = int.Parse(jdata.Track4.Instrument);
            }
            if (jdata.Track4.Threshold != null)
            {
                int temp = int.Parse(jdata.Track4.Threshold);
                if (temp >= 0 && temp < 128)
                {
                    trackBar_mic4_threshold.Value = int.Parse(jdata.Track4.Threshold);
                    midi1.threshold4 = midi2.threshold4 = midi.threshold4 = midi3.threshold4 = trackBar_mic4_threshold.Value;
                    midiPlay.midiMsg.threshold4 = trackBar_mic4_threshold.Value;
                    combobox4_threshold = trackBar_mic4_threshold.Value;
                    
                    label19.Location = new Point(trackBar_mic4_threshold.Location.X + (int)(((double)trackBar_mic4_threshold.Value / trackBar_mic4_threshold.Maximum) * trackBar_mic4_threshold.Size.Width) - label19.Size.Width / 2, label19.Location.Y);
                    label19.Text = trackBar_mic4_threshold.Value.ToString();
                }
            }
            midiPlay.playOutput = jdata.PlayMidi;
            checkBox_output.Checked = jdata.PlayMidi;
            try
            {
                int value = int.Parse(jdata.Mp3Offset);
                if (value > 999)
                {
                    value = 999;
                    textBox_delay.Text = "999";
                }
                else if (value < -999)
                {
                    value = -999;
                    textBox_delay.Text = "-999";
                } else
                {
                    textBox_delay.Text = value.ToString();
                }

                if (midiPlay.mst.HPT != null)
                {
                    midiPlay.mst.HPT.setDelay(value);
                } else
                {
                    midiPlay.delayTimer = value;
                }

            }
            catch (Exception ex)
            {
                if (textBox_delay.Text != "" && textBox_delay.Text != "-")
                {
                    if (midiPlay.mst.HPT != null)
                    {
                        midiPlay.mst.HPT.setDelay(0);
                        textBox_delay.Text = "0";
                    }
                    else
                    {
                        midiPlay.delayTimer = 0;
                        textBox_delay.Text = "0";
                    }
                  //  midiPlay.mst.HPT.setDelay(0);
                    textBox_delay.Text = "invalid";
                }
            }
        }
        void updateDeviceCombox()
        {

            cleanComboxItems();
            comboBox_file.SelectedIndex = 0;
            comboBox_mp3.SelectedIndex = 0;
            List<string> sList = new List<string>();
            sList = mic.getMicList();
            mic1.getMicList();
            mic2.getMicList();
            mic3.getMicList();
            MicNumber = sList.Count<string>();
            if (MicNumber > 0)
            {
                for (int i = 0; i < MicNumber; i++)
                {
                    comboBox_1.Items.Add(sList[i]);
                    comboBox_2.Items.Add(sList[i]);
                    comboBox_3.Items.Add(sList[i]);
                    comboBox_4.Items.Add(sList[i]);
                }
            }
            MidiNumber = InputPort.InputCount;
            if (MidiNumber > 0)
            {
                for (int i = 0; i < MidiNumber; i++)
                {
                    string nameMidi = "Midi" + i.ToString();
                    comboBox_1.Items.Add(nameMidi);
                    comboBox_2.Items.Add(nameMidi);
                    comboBox_3.Items.Add(nameMidi);
                    comboBox_4.Items.Add(nameMidi);
                }
            }
            string emptyName = "Null";
            comboBox_1.Items.Add(emptyName);
            comboBox_1.SelectedIndex = MicNumber + MidiNumber;
            comboBox_2.Items.Add(emptyName);
            comboBox_2.SelectedIndex = MicNumber + MidiNumber;
            comboBox_3.Items.Add(emptyName);
            comboBox_3.SelectedIndex = MicNumber + MidiNumber;
            comboBox_4.Items.Add(emptyName);
            comboBox_4.SelectedIndex = MicNumber + MidiNumber;
        }
        void updateChannelCombox()
        {
            cleanComboxItems();
            for (int i = 1; i <= 9; i++)
            {
                comboBox_1.Items.Add("Channel  " + i.ToString());
                comboBox_2.Items.Add("Channel  " + i.ToString());
                comboBox_3.Items.Add("Channel  " + i.ToString());
                comboBox_4.Items.Add("Channel  " + i.ToString());
            }
            for (int i = 10;i <= MidiMsg.CHANNEL_NUM;i++)
            {
                comboBox_1.Items.Add("Channel " + i.ToString());
                comboBox_2.Items.Add("Channel " + i.ToString());
                comboBox_3.Items.Add("Channel " + i.ToString());
                comboBox_4.Items.Add("Channel " + i.ToString());
            }
            comboBox_1.Items.Add("Null");
            comboBox_2.Items.Add("Null");
            comboBox_3.Items.Add("Null");
            comboBox_4.Items.Add("Null");
            comboBox_4.SelectedIndex = comboBox_4.Items.Count - 1;
            comboBox_3.SelectedIndex = comboBox_3.Items.Count - 1;
            comboBox_2.SelectedIndex = comboBox_2.Items.Count - 1;
            comboBox_1.SelectedIndex = comboBox_1.Items.Count - 1;
        }
        void updateCombox()
        {
            comboBox_mp3.Items.Add("NULL");
            comboBox_mp3.SelectedIndex = comboBox_mp3.Items.Count - 1;
            comboBox_file.Items.Add("NULL");
            comboBox_file.SelectedIndex = comboBox_file.Items.Count - 1;
            updateDeviceCombox();
        }
        void selectDevice()
        {
            if (comboBox_1.SelectedIndex < MicNumber)
            {

                if (mic.MicEmpty(comboBox_1.SelectedIndex))
                {
                    mic.MicCall(comboBox_1.SelectedIndex, 1);
                    mic.fft.update_valueList1(combobox1_min, combobox1_max);
                    string tip = "";
                    tip = "";
                    for (int i = 0; i < mic.fft.valueList1.Length; i++)
                    {
                        tip += (mic.fft.valueList1[i] * mic.fft.minInteral1).ToString() + " ";
                    }
                    tipCombobox1Fre.SetToolTip(comboBox_1, tip);
                }
                else if (mic1.MicEmpty(comboBox_1.SelectedIndex))
                {
                    mic1.MicCall(comboBox_1.SelectedIndex, 1);
                    mic1.fft.update_valueList1(combobox1_min, combobox1_max);
                    string tip = "";
                    tip = "";
                    for (int i = 0; i < mic1.fft.valueList1.Length; i++)
                    {
                        tip += (mic1.fft.valueList1[i] * mic1.fft.minInteral1).ToString() + " ";
                    }
                    tipCombobox1Fre.SetToolTip(comboBox_1, tip);
                }
                else if (mic2.MicEmpty(comboBox_1.SelectedIndex))
                {
                    mic2.MicCall(comboBox_1.SelectedIndex, 1);
                    mic2.fft.update_valueList1(combobox1_min, combobox1_max);
                }
                else if (mic3.MicEmpty(comboBox_1.SelectedIndex))
                {
                    mic3.MicCall(comboBox_1.SelectedIndex, 1);
                    mic3.fft.update_valueList1(combobox1_min, combobox1_max);
                }
                if (combobox1_min.ToString() != textBox_Mic1_min.Text)
                {
                    textBox_Mic1_min.Text = combobox1_min.ToString();

                }
                if (combobox1_max.ToString() != textBox_Mic1_max.Text)
                {
                    textBox_Mic1_max.Text = combobox1_max.ToString();
                }
            }
            else if (comboBox_1.SelectedIndex < MidiNumber + MicNumber)
            {
                tipCombobox1Fre.RemoveAll();
                bool isOpen = false;
                bool isStart = false;
                if (midi.MidiEmpty(comboBox_1.SelectedIndex - MicNumber))
                {
                    isOpen = midi.Open(comboBox_1.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi1.Checked)
                        {
                            midi.filter1 = comboBox_midi1.SelectedIndex;
                        }
                        else
                        {
                            midi.filter1 = -1;
                        }
                        midi.MidiAddNumber(1);
                        isStart = midi.Start();
                    }

                }
                else if (midi1.MidiEmpty(comboBox_1.SelectedIndex - MicNumber))
                {
                    isOpen = midi1.Open(comboBox_1.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi1.Checked)
                        {
                            midi1.filter1 = comboBox_midi1.SelectedIndex;
                        }
                        else
                        {
                            midi1.filter1 = -1;
                        }
                        midi1.MidiAddNumber(1);
                        isStart = midi1.Start();
                    }
                }
            }
            if (comboBox_2.SelectedIndex < MicNumber)
            {
                if (mic.MicEmpty(comboBox_2.SelectedIndex))
                {
                    mic.MicCall(comboBox_2.SelectedIndex, 2);
                    mic.fft.update_valueList2(combobox2_min, combobox2_max);
                    string tip = "";
                    tip = "";
                    for (int i = 0; i < mic.fft.valueList1.Length; i++)
                    {
                        tip += (mic.fft.valueList2[i] * mic.fft.minInteral2).ToString() + " ";
                    }
                    tipCombobox2Fre.SetToolTip(comboBox_2, tip);
                }
                else if (mic1.MicEmpty(comboBox_2.SelectedIndex))
                {
                    mic1.MicCall(comboBox_2.SelectedIndex, 2);
                    mic1.fft.update_valueList2(combobox2_min, combobox2_max);
                    string tip = "";
                    tip = "";
                    for (int i = 0; i < mic1.fft.valueList1.Length; i++)
                    {
                        tip += (mic1.fft.valueList2[i] * mic1.fft.minInteral2).ToString() + " ";
                    }
                    tipCombobox2Fre.SetToolTip(comboBox_2, tip);
                }
                else if (mic2.MicEmpty(comboBox_2.SelectedIndex))
                {
                    mic2.MicCall(comboBox_2.SelectedIndex, 2);
                    mic2.fft.update_valueList2(combobox2_min, combobox2_max);
                }
                else if (mic3.MicEmpty(comboBox_2.SelectedIndex))
                {
                    mic3.MicCall(comboBox_2.SelectedIndex, 2);
                    mic3.fft.update_valueList2(combobox2_min, combobox2_max);
                }
                if (combobox2_min.ToString() != textBox_Mic2_min.Text)
                {
                    textBox_Mic2_min.Text = combobox2_min.ToString();
                }
                if (combobox2_max.ToString() != textBox_Mic2_max.Text)
                {
                    textBox_Mic2_max.Text = combobox2_max.ToString();
                }
            }
            else if (comboBox_2.SelectedIndex < MidiNumber + MicNumber)
            {
                tipCombobox2Fre.RemoveAll();
                bool isOpen = false;
                bool isStart = false;
                if (midi.MidiEmpty(comboBox_2.SelectedIndex - MicNumber))
                {
                    isOpen = midi.Open(comboBox_2.SelectedIndex - MicNumber, 2);
                    if (isOpen == true)
                    {
                        if (checkBox_midi2.Checked)
                        {
                            midi.filter2 = comboBox_midi2.SelectedIndex;
                        }
                        else
                        {
                            midi.filter2 = -1;
                        }
                        midi.MidiAddNumber(2);
                        isStart = midi.Start();
                    }
                }
                else if (midi1.MidiEmpty(comboBox_2.SelectedIndex - MicNumber))
                {
                    isOpen = midi1.Open(comboBox_2.SelectedIndex - MicNumber, 2);
                    if (isOpen == true)
                    {
                        if (checkBox_midi2.Checked)
                        {
                            midi1.filter1 = comboBox_midi2.SelectedIndex;
                        }
                        else
                        {
                            midi1.filter1 = -1;
                        }
                        midi1.MidiAddNumber(2);
                        isStart = midi1.Start();
                    }
                }
            }
            if (comboBox_3.SelectedIndex < MicNumber)
            {

                if (mic.MicEmpty(comboBox_3.SelectedIndex))
                {
                    mic.MicCall(comboBox_3.SelectedIndex, 3);
                    mic.fft.threshold3 = combobox3_threshold;
                }
                else if (mic1.MicEmpty(comboBox_3.SelectedIndex))
                {
                    mic1.MicCall(comboBox_3.SelectedIndex, 3);
                    mic1.fft.threshold3 = combobox3_threshold;
                }
                else if (mic2.MicEmpty(comboBox_3.SelectedIndex))
                {
                    mic2.MicCall(comboBox_3.SelectedIndex, 3);
                    mic2.fft.threshold3 = combobox3_threshold;
                }
                else if (mic3.MicEmpty(comboBox_3.SelectedIndex))
                {
                    mic3.MicCall(comboBox_3.SelectedIndex, 3);
                    mic3.fft.threshold3 = combobox3_threshold;
                }
            }
            else if (comboBox_3.SelectedIndex < MidiNumber + MicNumber)
            {
                bool isOpen = false;
                bool isStart = false;
                if (midi.MidiEmpty(comboBox_3.SelectedIndex - MicNumber))
                {
                    isOpen = midi.Open(comboBox_3.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi.filter3 = comboBox_midi3.SelectedIndex;
                        }
                        else
                        {
                            midi.filter3 = -1;
                        }
                        midi.MidiAddNumber(3);
                        isStart = midi.Start();
                    }

                }
                else if (midi1.MidiEmpty(comboBox_3.SelectedIndex - MicNumber))
                {
                    isOpen = midi1.Open(comboBox_3.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi1.filter3 = comboBox_midi3.SelectedIndex;
                        }
                        else
                        {
                            midi1.filter3 = -1;
                        }
                        midi1.MidiAddNumber(3);
                        isStart = midi1.Start();
                    }
                }
                else if (midi2.MidiEmpty(comboBox_3.SelectedIndex - MicNumber))
                {
                    isOpen = midi2.Open(comboBox_3.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi2.filter3 = comboBox_midi3.SelectedIndex;
                        }
                        else
                        {
                            midi2.filter3 = -1;
                        }
                        midi2.MidiAddNumber(3);
                        isStart = midi2.Start();
                    }
                }
                else if (midi3.MidiEmpty(comboBox_3.SelectedIndex - MicNumber))
                {
                    isOpen = midi3.Open(comboBox_3.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi3.filter3 = comboBox_midi3.SelectedIndex;
                        }
                        else
                        {
                            midi3.filter3 = -1;
                        }
                        midi3.MidiAddNumber(3);
                        isStart = midi3.Start();
                    }
                }
            }
            if (comboBox_4.SelectedIndex < MicNumber)
            {
                if (mic.MicEmpty(comboBox_4.SelectedIndex))
                {
                    mic.MicCall(comboBox_4.SelectedIndex, 4);
                    mic.fft.threshold4 = combobox4_threshold;
                }
                else if (mic1.MicEmpty(comboBox_4.SelectedIndex))
                {
                    mic1.MicCall(comboBox_4.SelectedIndex, 4);
                    mic.fft.threshold4 = combobox4_threshold;
                }
                else if (mic2.MicEmpty(comboBox_4.SelectedIndex))
                {
                    mic2.MicCall(comboBox_4.SelectedIndex, 4);
                    mic.fft.threshold4 = combobox4_threshold;
                }
                else if (mic3.MicEmpty(comboBox_4.SelectedIndex))
                {
                    mic3.MicCall(comboBox_4.SelectedIndex, 4);
                    mic.fft.threshold4 = combobox4_threshold;
                }
            }
            else if (comboBox_4.SelectedIndex < MidiNumber + MicNumber)
            {
                bool isOpen = false;
                bool isStart = false;
                if (midi.MidiEmpty(comboBox_4.SelectedIndex - MicNumber))
                {
                    isOpen = midi.Open(comboBox_4.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi.filter4 = comboBox_midi4.SelectedIndex;
                        }
                        else
                        {
                            midi.filter4 = -1;
                        }
                        midi.MidiAddNumber(4);
                        isStart = midi.Start();
                    }

                }
                else if (midi1.MidiEmpty(comboBox_4.SelectedIndex - MicNumber))
                {
                    isOpen = midi1.Open(comboBox_4.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi1.filter4 = comboBox_midi4.SelectedIndex;
                        }
                        else
                        {
                            midi1.filter4 = -1;
                        }
                        midi1.MidiAddNumber(4);
                        isStart = midi1.Start();
                    }
                }
                else if (midi2.MidiEmpty(comboBox_4.SelectedIndex - MicNumber))
                {
                    isOpen = midi2.Open(comboBox_4.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi2.filter4 = comboBox_midi4.SelectedIndex;
                        }
                        else
                        {
                            midi2.filter4 = -1;
                        }
                        midi2.MidiAddNumber(4);
                        isStart = midi2.Start();
                    }
                }
                else if (midi3.MidiEmpty(comboBox_4.SelectedIndex - MicNumber))
                {
                    isOpen = midi3.Open(comboBox_4.SelectedIndex - MicNumber, 1);
                    if (isOpen == true)
                    {
                        if (checkBox_midi3.Checked)
                        {
                            midi3.filter4 = comboBox_midi4.SelectedIndex;
                        }
                        else
                        {
                            midi3.filter4 = -1;
                        }
                        midi3.MidiAddNumber(4);
                        isStart = midi3.Start();
                    }
                }
            }
        }
        void getMic4value()
        {
            double uintLength = (double)panel1.Width / 127;

            while(true)
            {
                bool flag = true;
                if(mic.fft.number[4] == 4)
                {
                    if(mic.fft.queue4.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic.fft.queue4);
                        int mic4value = 0;
                            mic4value = mic.fft.queue4.Dequeue();
                        Monitor.Exit(mic.fft.queue4);
                        DrawPictureMic34(4,mic4value,uintLength,mic.fft.uintThreshold4,mic4value > mic.fft.threshold4);
                    }
                }
                if (mic1.fft.number[4] == 4)
                {
                    if (mic1.fft.queue4.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic1.fft.queue4);
                        int mic4value = mic1.fft.queue4.Dequeue();
                        Monitor.Exit(mic1.fft.queue4);
                        DrawPictureMic34(4, mic4value, uintLength, mic1.fft.uintThreshold4, mic4value > mic1.fft.threshold4);
                    }
                }
                if (mic2.fft.number[4] == 4)
                {
                    if (mic2.fft.queue4.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic2.fft.queue4);
                        int mic4value = mic2.fft.queue4.Dequeue();
                        Monitor.Exit(mic2.fft.queue4);
                        DrawPictureMic34(4, mic4value, uintLength, mic2.fft.uintThreshold4, mic4value > mic2.fft.threshold4);
                    }
                }
                if (mic3.fft.number[4] == 4)
                {
                    if (mic3.fft.queue4.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic3.fft.queue4);
                        int mic4value = mic3.fft.queue4.Dequeue();
                        Monitor.Exit(mic3.fft.queue4);
                        DrawPictureMic34(4, mic4value, uintLength, mic3.fft.uintThreshold4, mic4value > mic3.fft.threshold4);
                    }
                }
                if (flag)
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMic3value()
        {
            double uintLength = (double)panel2.Width / 127;
            while (true)
            {
                bool flag = true;
                if (mic.fft.number[3] == 3)
                {
                    if (mic.fft.queue3.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic.fft.queue3);
                        int mic3value = mic.fft.queue3.Dequeue();
                        Monitor.Exit(mic.fft.queue3);
                        DrawPictureMic34(3, mic3value, uintLength, mic.fft.uintThreshold3, mic3value > mic.fft.threshold3);
                    }
                }
                if (mic1.fft.number[3] == 3)
                {
                    if (mic1.fft.queue3.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic1.fft.queue3);
                        int mic3value = mic1.fft.queue3.Dequeue();
                        Monitor.Exit(mic1.fft.queue3);
                        DrawPictureMic34(3, mic3value, uintLength, mic1.fft.uintThreshold3, mic3value > mic1.fft.threshold3);
                    }
                }
                if (mic2.fft.number[3] == 3)
                {
                    if (mic2.fft.queue3.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic2.fft.queue3);
                        int mic3value = mic2.fft.queue3.Dequeue();
                        Monitor.Exit(mic2.fft.queue3);
                        DrawPictureMic34(3, mic3value, uintLength, mic2.fft.uintThreshold3, mic3value > mic2.fft.threshold3);
                    }
                }
                if (mic3.fft.number[3] == 3)
                {
                    if (mic3.fft.queue3.Count > 0)
                    {
                        flag = false;
                        Monitor.Enter(mic3.fft.queue3);
                        int mic3value = mic3.fft.queue3.Dequeue();
                        Monitor.Exit(mic3.fft.queue3);
                        DrawPictureMic34(3, mic3value, uintLength, mic3.fft.uintThreshold3, mic3value > mic3.fft.threshold3);
                    }
                }
                if (flag)
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMidi1value()
        { 
            while (true)
            {
                if (midi.queuePicture.Count > 0)
                {
                    Monitor.Enter(midi.queuePicture);
                    byte[] data = midi.queuePicture.Dequeue();
                    Monitor.Exit(midi.queuePicture);
                    if (data != null)
                    {
                        if (data[3] == 1 || data[3] == 2)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            midi.drawValue(data, ref channel, ref pictureValue);
                            Midi12drawCircle(channel, pictureValue);
                        }
                        else if (data[3] == 3 || data[3] == 4)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            bool flag = false;
                            midi.drawValue(data, ref channel, ref pictureValue, ref flag);
                            DrawPicture34(channel, pictureValue, flag);
                        }
                    }

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMidi2value()
        {
            while (true)
            {
                if (midi1.queuePicture.Count > 0)
                {
                    Monitor.Enter(midi1.queuePicture);
                    byte[] data = midi1.queuePicture.Dequeue();
                    Monitor.Exit(midi1.queuePicture);
                    if (data != null)
                    {
                        if (data[3] == 1 || data[3] == 2)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            midi1.drawValue(data, ref channel, ref pictureValue);
                            Midi12drawCircle(channel, pictureValue);
                        }
                        else if (data[3] == 3 || data[3] == 4)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            bool flag = false;
                            midi1.drawValue(data, ref channel, ref pictureValue, ref flag);
                            DrawPicture34(channel, pictureValue, flag);
                        }
                    }

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMidi3value()
        {
            while (true)
            {
                if (midi2.queuePicture.Count > 0)
                {
                    Monitor.Enter(midi2.queuePicture);
                    byte[] data = midi2.queuePicture.Dequeue();
                    Monitor.Exit(midi2.queuePicture);
                    if (data != null)
                    {
                        if (data[3] == 1 || data[3] == 2)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            midi2.drawValue(data, ref channel, ref pictureValue);
                            Midi12drawCircle(channel, pictureValue);
                        }
                        else if (data[3] == 3 || data[3] == 4)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            bool flag = false;
                            midi2.drawValue(data, ref channel, ref pictureValue, ref flag);
                            DrawPicture34(channel, pictureValue, flag);
                        }
                    }

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMidi4value()
        {
            while (true)
            {
                if (midi3.queuePicture.Count > 0)
                {
                    Monitor.Enter(midi3.queuePicture);
                    byte[] data = midi3.queuePicture.Dequeue();
                    Monitor.Exit(midi3.queuePicture);
                    if (data != null)
                    {
                        if (data[3] == 1 || data[3] == 2)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            midi3.drawValue(data, ref channel, ref pictureValue);
                            Midi12drawCircle(channel, pictureValue);
                        }
                        else if (data[3] == 3 || data[3] == 4)
                        {
                            int channel = 0;
                            int pictureValue = 0;
                            bool flag = false;
                            midi3.drawValue(data, ref channel, ref pictureValue, ref flag);
                            DrawPicture34(channel, pictureValue, flag);
                        }
                    }

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMidiMsgValue()
        {
            while (true)
            {
                byte[] data = null;
                Monitor.Enter(midiPlay.midiMsg.queuePicture);
                if (midiPlay.midiMsg.queuePicture.Count > 0)
                {
                    data = midiPlay.midiMsg.queuePicture.Dequeue();
                }
                Monitor.Exit(midiPlay.midiMsg.queuePicture);
                if (data != null)
                {
                    if (data[3] == 1 || data[3] == 2)
                    {
                        int channel = 0;
                        int pictureValue = 0;
                        midiPlay.midiMsg.drawValue(data, ref channel, ref pictureValue);
                        Midi12drawCircle(channel, pictureValue);
                    }
                    else if (data[3] == 3 || data[3] == 4)
                    {
                        int channel = 0;
                        int pictureValue = 0;
                        bool flag = false;
                        midiPlay.midiMsg.drawValue(data, ref channel, ref pictureValue, ref flag);
                        DrawPicture34(channel, pictureValue, flag);
                    }

                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        public void DrawPicture34(int channel, int volume, bool flag)
        {
            try
            {
                if (channel == 4)
                {
                    Graphics dcMic4 = this.panel1.CreateGraphics();
                    dcMic4.Clear(panel1.BackColor);
                    Pen bluePen = new Pen(Color.LightBlue, 1);
                    Rectangle myRectangle = new Rectangle(0, 0, (int)(((double)(panel1.Width)) / 127) * volume, panel1.Height);
                    dcMic4.DrawRectangle(bluePen, myRectangle);
                    dcMic4.FillRectangle(new SolidBrush(Color.Blue), myRectangle);
                    Graphics dcVol4 = panel14.CreateGraphics();
                    Color cl;
                    if (flag)
                        cl = Color.Red;
                    else
                        cl = Color.WhiteSmoke;
                    Pen redPen = new Pen(cl, 1);
                    Rectangle myR = new Rectangle(0, 0, panel14.Width, panel14.Height);
                    dcVol4.DrawEllipse(redPen, myR);
                    dcVol4.FillEllipse(new SolidBrush(cl), myR);
                    //LinearGradientBrush myLGB = new LinearGradientBrush(myRectangle, Color.Blue, Color.Green, LinearGradientMode.Horizontal);
                    //dc.FillRectangle(myLGB, myRectangle);
                    //for (int i = 0; i < volume; i++)
                    //{
                    //    Pen BluePen = new Pen(Color.Blue, 1); // 创建蓝色画笔
                    //    int x = 3*i;
                    //    int y = 0;
                    //    dc.DrawRectangle(BluePen, x,y, 1,10);
                    //}
                }
                else if (channel == 3)
                {
                    Graphics dcMic3 = this.panel2.CreateGraphics();
                    dcMic3.Clear(panel2.BackColor);
                    Pen bluePen = new Pen(Color.LightBlue, 1);
                    Rectangle myRectangle = new Rectangle(0, 0, (int)(((double)(panel2.Width)) / 127) * volume, panel2.Height);
                    dcMic3.DrawRectangle(bluePen, myRectangle);
                    dcMic3.FillRectangle(new SolidBrush(Color.Blue), myRectangle);
                    Graphics dcVol3 = panel13.CreateGraphics();
                    Color cl;
                    if (flag)
                        cl = Color.Red;
                    else
                        cl = Color.WhiteSmoke;
                    Pen redPen = new Pen(cl, 1);
                    Rectangle myR = new Rectangle(0, 0, panel13.Width, panel13.Height);
                    dcVol3.DrawEllipse(redPen, myR);
                    dcVol3.FillEllipse(new SolidBrush(cl), myR);
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        
        void getMic2value()
        {
            while (true)
            {
                bool flag = true;
                if (mic.fft.number[2] == 2)
                {
                    if (mic.fft.queue2.Count > 0)
                    {
                        Monitor.Enter(mic.fft.queue2);
                        int mic2value = mic.fft.queue2.Dequeue();
                        Monitor.Exit(mic.fft.queue2);
                        Mic12drawCircle(2,mic2value);
                        flag = false;
                    }
                }
                if (mic1.fft.number[2] == 2)
                {
                    if (mic1.fft.queue2.Count > 0)
                    {
                        Monitor.Enter(mic1.fft.queue2);
                        int mic2value = mic1.fft.queue2.Dequeue();
                        Monitor.Exit(mic1.fft.queue2);
                        Mic12drawCircle(2, mic2value);
                        flag = false;
                    }
                }
                if (mic2.fft.number[2] == 2)
                {
                    if (mic2.fft.queue2.Count > 0)
                    {
                        Monitor.Enter(mic2.fft.queue2);
                        int mic2value = mic2.fft.queue2.Dequeue();
                        Monitor.Exit(mic2.fft.queue2);
                        Mic12drawCircle(2, mic2value);
                        flag = false;
                    }
                }
                if (mic3.fft.number[2] == 2)
                {
                    if (mic3.fft.queue2.Count > 0)
                    {
                        Monitor.Enter(mic3.fft.queue2);
                        int mic2value = mic3.fft.queue2.Dequeue();
                        Monitor.Exit(mic3.fft.queue2);
                        Mic12drawCircle(2, mic2value);
                        flag = false;
                    }
                }
                if(flag)
                {
                    Thread.Sleep(10);
                }
            }
        }
        void getMic1value()
        {
            while (true)
            {
                bool flag = true;
                if (mic.fft.number[1] == 1)
                {
                    if (mic.fft.queue1.Count > 0)
                    {
                        Monitor.Enter(mic.fft.queue1);
                        int mic1value = mic.fft.queue1.Dequeue();
                        Monitor.Exit(mic.fft.queue1);
                        Mic12drawCircle(1, mic1value);
                        flag = false;
                    }
                }
                if (mic1.fft.number[1] == 1)
                {
                    if (mic1.fft.queue1.Count > 0)
                    {
                        Monitor.Enter(mic1.fft.queue1);
                        int mic1value = mic1.fft.queue1.Dequeue();
                        Monitor.Exit(mic1.fft.queue1);
                        Mic12drawCircle(1, mic1value);
                        flag = false;
                    }
                }
                if (mic2.fft.number[1] == 1)
                {
                    if (mic2.fft.queue1.Count > 0)
                    {
                        Monitor.Enter(mic2.fft.queue1);
                        int mic1value = mic2.fft.queue1.Dequeue();
                        Monitor.Exit(mic2.fft.queue1);
                        Mic12drawCircle(1, mic1value);
                        flag = false;
                    }
                }
                if (mic3.fft.number[1] == 1)
                {
                    if (mic3.fft.queue1.Count > 0)
                    {
                        Monitor.Enter(mic3.fft.queue1);
                        int mic1value = mic3.fft.queue1.Dequeue();
                        Monitor.Exit(mic3.fft.queue1);
                        Mic12drawCircle(1, mic1value);
                        flag = false;
                    }
                }
                if (flag)
                {
                    Thread.Sleep(10);
                }
            }
        }
        void Midi12drawCircle(int micNum, int value)
        {
            try
            {
                if (micNum == 2)
                {
                    Graphics dcMic2 = this.panel_midi2.CreateGraphics();
                    int r = panel_midi2.Height - 2;
                    int distance = panel_midi2.Width / 12;
                    for (int i = 0; i < 12; i++)
                    {
                        Color cl;
                        if (((1<<i) & value) != 0)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * i, 0, r, r);
                        dcMic2.DrawEllipse(bluePen, myRectangle);
                        dcMic2.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
                if (micNum == 1)
                {
                    Graphics dcMic1 = this.panel_midi1.CreateGraphics();
                    int r = panel_midi1.Height - 2;
                    int distance = panel_midi1.Width / 12;
                    for (int i = 0; i < 12; i++)
                    {
                        Color cl;
                        if (((1 << i) & value) != 0)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * i, 0, r, r);
                        dcMic1.DrawEllipse(bluePen, myRectangle);
                        dcMic1.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
                if (micNum == 3)
                {
                    Graphics dcMic1 = this.panel_midi3.CreateGraphics();
                    int r = panel_midi3.Height - 2;
                    int distance = panel_midi3.Width / 12;
                    for (int i = 0; i < 12; i++)
                    {
                        Color cl;
                        if (((1 << i) & value) != 0)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * i, 0, r, r);
                        dcMic1.DrawEllipse(bluePen, myRectangle);
                        dcMic1.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
                if (micNum == 4)
                {
                    Graphics dcMic1 = this.panel_midi4.CreateGraphics();
                    int r = panel_midi4.Height - 2;
                    int distance = panel_midi4.Width / 12;
                    for (int i = 0; i < 12; i++)
                    {
                        Color cl;
                        if (((1 << i) & value) != 0)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * i, 0, r, r);
                        dcMic1.DrawEllipse(bluePen, myRectangle);
                        dcMic1.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
            }catch(Exception ex)
            {
                ;
            }
        }
        void Mic12drawCircle(int micNum, int value)
        {
            try {
                if (micNum == 2)
                {
                    Graphics dcMic2 = this.panel_midi2.CreateGraphics();
                    int r = panel_midi2.Height - 2;
                    int distance = panel_midi2.Width / 12;
                    for (int i = 1; i <= 12; i++)
                    {
                        Color cl;
                        if (i == value)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * (i - 1), 0, r, r);
                        dcMic2.DrawEllipse(bluePen, myRectangle);
                        dcMic2.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
                if (micNum == 1)
                {
                    Graphics dcMic1 = this.panel_midi1.CreateGraphics();
                    int r = panel_midi1.Height - 2;
                    int distance = panel_midi1.Width / 12;
                    for (int i = 1; i <= 12; i++)
                    {
                        Color cl;
                        if (i == value)
                            cl = Color.Red;
                        else
                            cl = Color.WhiteSmoke;
                        Pen bluePen = new Pen(cl, 1);
                        Rectangle myRectangle = new Rectangle(distance * (i - 1), 0, r, r);
                        dcMic1.DrawEllipse(bluePen, myRectangle);
                        dcMic1.FillEllipse(new SolidBrush(cl), myRectangle);
                    }
                }
            }catch(Exception ex)
            {
                ;
            }
        }
        void DrawPictureMic34(int micNum,int volume,double uintLength,int uintThreshold,bool flag)
        {
            try {
                if (micNum == 4)
                {
                    Graphics dcMic4 = this.panel1.CreateGraphics();
                    dcMic4.Clear(panel1.BackColor);
                    Pen bluePen = new Pen(Color.LightBlue, 1);
                    Rectangle myRectangle = new Rectangle(0, 0, (int)(uintLength * volume), panel1.Height);
                    dcMic4.DrawRectangle(bluePen, myRectangle);
                    dcMic4.FillRectangle(new SolidBrush(Color.Blue), myRectangle);
                    Graphics dcVol4 = panel14.CreateGraphics();
                    Color cl;
                    if (flag)
                        cl = Color.Red;
                    else
                        cl = Color.WhiteSmoke;
                    Pen redPen = new Pen(cl, 1);
                    Rectangle myR = new Rectangle(0, 0, panel14.Width, panel14.Height);
                    dcVol4.DrawEllipse(redPen, myR);
                    dcVol4.FillEllipse(new SolidBrush(cl), myR);
                    //LinearGradientBrush myLGB = new LinearGradientBrush(myRectangle, Color.Blue, Color.Green, LinearGradientMode.Horizontal);
                    //dc.FillRectangle(myLGB, myRectangle);
                    //for (int i = 0; i < volume; i++)
                    //{
                    //    Pen BluePen = new Pen(Color.Blue, 1); // 创建蓝色画笔
                    //    int x = 3*i;
                    //    int y = 0;
                    //    dc.DrawRectangle(BluePen, x,y, 1,10);
                    //}
                } else if (micNum == 3)
                {
                    Graphics dcMic3 = this.panel2.CreateGraphics();
                    dcMic3.Clear(panel2.BackColor);
                    Pen bluePen = new Pen(Color.LightBlue, 1);
                    Rectangle myRectangle = new Rectangle(0, 0, (int)(uintLength * volume), panel2.Height);
                    dcMic3.DrawRectangle(bluePen, myRectangle);
                    dcMic3.FillRectangle(new SolidBrush(Color.Blue), myRectangle);
                    Graphics dcVol3 = panel13.CreateGraphics();
                    Color cl;
                    if (flag)
                        cl = Color.Red;
                    else
                        cl = Color.WhiteSmoke;
                    Pen redPen = new Pen(cl, 1);
                    Rectangle myR = new Rectangle(0, 0, panel13.Width, panel13.Height);
                    dcVol3.DrawEllipse(redPen, myR);
                    dcVol3.FillEllipse(new SolidBrush(cl), myR);
                }
            }catch(Exception ex)
            {
                ;
            }
        }
        private void button_connect_Click(object sender, EventArgs e)
        {
            
            if (button_connect.Text != "Stop server")
            {

                Threadmic4value = new Thread(getMic4value);
                Threadmic4value.IsBackground = true;
                Threadmic4value.Start();
                Threadmic3value = new Thread(getMic3value);
                Threadmic3value.IsBackground = true;
                Threadmic3value.Start();
                Threadmic2value = new Thread(getMic2value);
                Threadmic2value.IsBackground = true;
                Threadmic2value.Start();
                Threadmic1value = new Thread(getMic1value);
                Threadmic1value.IsBackground = true;
                Threadmic1value.Start();
                Threadmidi1value = new Thread(getMidi1value);
                Threadmidi1value.IsBackground = true;
                Threadmidi1value.Start();
                Threadmidi2value = new Thread(getMidi2value);
                Threadmidi2value.IsBackground = true;
                Threadmidi2value.Start();
                Threadmidi3value = new Thread(getMidi3value);
                Threadmidi3value.IsBackground = true;
                Threadmidi3value.Start();
                Threadmidi4value = new Thread(getMidi4value);
                Threadmidi4value.IsBackground = true;
                Threadmidi4value.Start();
                ThreadmidiMsgPic = new Thread(getMidiMsgValue);
                ThreadmidiMsgPic.IsBackground = true;
                ThreadmidiMsgPic.Start();
                thread = new Thread(func);
                thread.IsBackground = true;
                thread.Start();
                loopThread = new Thread(loopFunc);
                loopThread.IsBackground = true;
                loopThread.Start();
                button_connect.Text = "Stop server";
                if (comboBox_file.SelectedItem.ToString() == "NULL")
                {
                    selectDevice();
                }
                Midi12drawCircle(1, 0);
                Midi12drawCircle(2, 0);
                Midi12drawCircle(3, 0);
                Midi12drawCircle(4, 0);
                DrawPicture34(3,0,false);
                DrawPicture34(4, 0, false);
                button_play.Enabled = true;
            } else
            {
                button_play.Enabled = false;
                tipCombobox1Fre.RemoveAll();
                tipCombobox2Fre.RemoveAll();
                loopThread.Abort();
                Threadmidi1value.Abort();
                Threadmidi1value.DisableComObjectEagerCleanup();
                Threadmidi2value.Abort();
                Threadmidi2value.DisableComObjectEagerCleanup();
                Threadmidi3value.Abort();
                Threadmidi3value.DisableComObjectEagerCleanup();
                Threadmidi4value.Abort();
                Threadmidi4value.DisableComObjectEagerCleanup();
                Threadmic1value.Abort();
                Threadmic1value.DisableComObjectEagerCleanup();
                Threadmic2value.Abort();
                Threadmic2value.DisableComObjectEagerCleanup();
                Threadmic3value.Abort();
                Threadmic3value.DisableComObjectEagerCleanup();
                Threadmic4value.Abort();
                Threadmic4value.DisableComObjectEagerCleanup();
                if (!mic.MicEmpty() || !midi.MidiEmpty() || !mic1.MicEmpty() || !mic2.MicEmpty() ||!mic3.MicEmpty() || !midi1.MidiEmpty())
                {
                    if (!mic.MicEmpty())
                    {
                        mic.MicStop();
                    }
                    if(!mic1.MicEmpty())
                    {
                        mic1.MicStop();
                    }
                    if (!mic2.MicEmpty())
                    {
                        mic2.MicStop();
                    }
                    if (!mic3.MicEmpty())
                    {
                        mic3.MicStop();
                    }
                    if (!midi.MidiEmpty())
                    {
                        midi.Close();
                    }
                    if (!midi1.MidiEmpty())
                    {
                        midi1.Close();
                    }
                    // thread.Abort();

                    Midi12drawCircle(1, 0);
                    Midi12drawCircle(2, 0);
                    Midi12drawCircle(3, 0);
                    Midi12drawCircle(4, 0);

                }
                midiPlay.midiClear();
                mp3play.end();
                button_play.Text = "Play";
                if (midiPlay.midiOut != null)
                {
                    midiPlay.midiOut.stop();
                }
                Mic12drawCircle(1,0);
                Mic12drawCircle(2, 0);
                button_connect.Text = "Start server";
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox_1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_file.SelectedItem.ToString() == "NULL")
            {
                Combobox1Select = comboBox_1.SelectedIndex;
                if (comboBox_1.SelectedIndex < MicNumber)
                {
                    comboBox_midi1.Visible = false;
                    checkBox_midi1.Visible = false;
                    label11.Visible = true;
                    label12.Visible = true;
                    textBox_Mic1_min.Visible = true;
                    textBox_Mic1_max.Visible = true;
                    panel_midi1.Visible = true;
                }
                else if (comboBox_1.SelectedIndex < MicNumber + MidiNumber)
                {
                    comboBox_midi1.Visible = true;
                    checkBox_midi1.Visible = true;
                    label11.Visible = false;
                    label12.Visible = false;
                    textBox_Mic1_min.Visible = false;
                    textBox_Mic1_max.Visible = false;
                    panel_midi1.Visible = true;
                }
                else
                {
                    comboBox_midi1.Visible = false;
                    checkBox_midi1.Visible = false;
                    label11.Visible = false;
                    label12.Visible = false;
                    textBox_Mic1_min.Visible = false;
                    textBox_Mic1_max.Visible = false;
                    panel_midi1.Visible = false;
                }
            } else
            {
                midiPlay.midiMsg.filer[1] = comboBox_1.SelectedIndex;
                midiPlay.midiMsg.picture1 = 0;
                Midi12drawCircle(1, 0);
                if (comboBox_1.SelectedIndex == comboBox_1.Items.Count -1)
                {
                    comboBox_midi1.Visible = false;
                    checkBox_midi1.Visible = false;
                    label11.Visible = false;
                    label12.Visible = false;
                    textBox_Mic1_min.Visible = false;
                    textBox_Mic1_max.Visible = false;
                    panel_midi1.Visible = false;
                } else
                {
                    comboBox_midi1.Visible = false;
                    checkBox_midi1.Visible = false;
                    label11.Visible = false;
                    label12.Visible = false;
                    textBox_Mic1_min.Visible = false;
                    textBox_Mic1_max.Visible = false;
                    panel_midi1.Visible = true;
                }

            }
        }

        private void comboBox_2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_file.SelectedItem.ToString() == "NULL")
            {
                Combobox2Select = comboBox_2.SelectedIndex;
                if (comboBox_2.SelectedIndex < MicNumber)
                {
                    comboBox_midi2.Visible = false;
                    checkBox_midi2.Visible = false;
                    label13.Visible = true;
                    label14.Visible = true;
                    textBox_Mic2_min.Visible = true;
                    textBox_Mic2_max.Visible = true;
                    panel_midi2.Visible = true;
                }
                else if (comboBox_2.SelectedIndex < MicNumber + MidiNumber)
                {
                    comboBox_midi2.Visible = true;
                    checkBox_midi2.Visible = true;
                    label13.Visible = false;
                    label14.Visible = false;
                    textBox_Mic2_min.Visible = false;
                    textBox_Mic2_max.Visible = false;
                    panel_midi2.Visible = true;
                }
                else
                {
                    comboBox_midi2.Visible = false;
                    checkBox_midi2.Visible = false;
                    label13.Visible = false;
                    label14.Visible = false;
                    textBox_Mic2_min.Visible = false;
                    textBox_Mic2_max.Visible = false;
                    panel_midi2.Visible = false;
                }
            } else
            {
                Midi12drawCircle(2, 0);
                midiPlay.midiMsg.filer[2] = comboBox_2.SelectedIndex;
                midiPlay.midiMsg.picture2 = 0;
                if (comboBox_2.SelectedIndex == comboBox_2.Items.Count - 1)
                {
                    comboBox_midi2.Visible = false;
                    checkBox_midi2.Visible = false;
                    label13.Visible = false;
                    label14.Visible = false;
                    textBox_Mic2_min.Visible = false;
                    textBox_Mic2_max.Visible = false;
                    panel_midi2.Visible = false;

                } else
                {
                    comboBox_midi2.Visible = false;
                    checkBox_midi2.Visible = false;
                    label13.Visible = false;
                    label14.Visible = false;
                    textBox_Mic2_min.Visible = false;
                    textBox_Mic2_max.Visible = false;
                    panel_midi2.Visible = true;
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            
        }

        private void comboBox_3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  if (comboBox_file.SelectedItem.ToString() == "NULL" && comboBoxMusicList.SelectedItem.ToString() == "NULL")
            try {
                if (comboBoxMusicList.SelectedItem.ToString() == "NULL")
                {
                    Combobox3Select = comboBox_3.SelectedIndex;
                    if (comboBox_3.SelectedIndex < MicNumber)
                    {
                        panel13.Visible = true;
                        comboBox_midi3.Visible = false;
                        checkBox_midi3.Visible = false;
                        label20.Visible = true;
                        trackBar_mic3_threshold.Visible = true;
                        panel2.Visible = true;
                        panel_midi3.Visible = false;
                        textBox_midi3.Visible = false;
                        comboBox_midi3_select.Visible = false;
                    }
                    else if (comboBox_3.SelectedIndex < MicNumber + MidiNumber)
                    {
                        panel13.Visible = true;
                        comboBox_midi3.Visible = false;
                        checkBox_midi3.Visible = false;
                        label20.Visible = true;
                        trackBar_mic3_threshold.Visible = true;
                        panel2.Visible = true;
                        panel_midi3.Visible = false;
                        textBox_midi3.Visible = true;
                        comboBox_midi3_select.Visible = false;
                    }
                    else
                    {
                        panel13.Visible = false;
                        comboBox_midi3.Visible = false;
                        checkBox_midi3.Visible = false;
                        label20.Visible = false;
                        trackBar_mic3_threshold.Visible = false;
                        panel2.Visible = false;
                        panel_midi3.Visible = false;
                        textBox_midi3.Visible = false;
                        comboBox_midi3_select.Visible = false;
                    }
                } else
                {
                    DrawPictureMic34(3, 0, 0, 0, false);
                    midiPlay.midiMsg.filer[3] = comboBox_3.SelectedIndex;
                    if (comboBox_3.SelectedIndex == comboBox_3.Items.Count - 1)
                    {
                        comboBox_midi3_select.Items.Clear();
                        panel13.Visible = false;
                        comboBox_midi3.Visible = false;
                        checkBox_midi3.Visible = false;
                        label20.Visible = false;
                        trackBar_mic3_threshold.Visible = false;
                        panel2.Visible = false;
                        panel_midi3.Visible = false;
                        textBox_midi3.Visible = false;
                        comboBox_midi3_select.Visible = false;
                    } else
                    {
                        comboBox_midi3_select.Items.Clear();
                        update_midi_slect(comboBox_3.SelectedIndex, 3);
                        panel13.Visible = true;
                        comboBox_midi3.Visible = false;
                        checkBox_midi3.Visible = false;
                        label20.Visible = true;
                        trackBar_mic3_threshold.Visible = true;
                        panel2.Visible = true;
                        panel_midi3.Visible = false;
                        textBox_midi3.Visible = false;
                        comboBox_midi3_select.Visible = true;
                    }
                }
            }
            catch { }
        }

        private void comboBox_4_SelectedIndexChanged(object sender, EventArgs e)
        {
            //           if (comboBox_file.SelectedItem.ToString() == "NULL")
            try
            {
                if (comboBoxMusicList.SelectedItem.ToString() == "NULL")
                {
                    Combobox4Select = comboBox_4.SelectedIndex;
                    if (comboBox_4.SelectedIndex < MicNumber)
                    {
                        panel14.Visible = true;
                        label19.Visible = true;
                        trackBar_mic4_threshold.Visible = true;
                        panel1.Visible = true;
                        comboBox_midi4.Visible = false;
                        checkBox_midi4.Visible = false;
                        panel_midi4.Visible = false;
                        textBox_midi4.Visible = false;
                        comboBox_midi4_select.Visible = false;
                    }
                    else if (comboBox_4.SelectedIndex < MicNumber + MidiNumber)
                    {
                        panel14.Visible = true;
                        label19.Visible = true;
                        trackBar_mic4_threshold.Visible = true;
                        panel1.Visible = true;
                        comboBox_midi4.Visible = false;
                        checkBox_midi4.Visible = false;
                        panel_midi4.Visible = false;
                        textBox_midi4.Visible = true;
                        comboBox_midi4_select.Visible = false;
                    }
                    else
                    {
                        panel14.Visible = false;
                        label19.Visible = false;
                        trackBar_mic4_threshold.Visible = false;
                        panel1.Visible = false;
                        comboBox_midi4.Visible = false;
                        checkBox_midi4.Visible = false;
                        panel_midi4.Visible = false;
                        textBox_midi4.Visible = false;
                        comboBox_midi4_select.Visible = false;
                    }
                }
                else {
                    midiPlay.midiMsg.filer[4] = comboBox_4.SelectedIndex;
                    DrawPictureMic34(4, 0, 0, 0, false);
                    if (comboBox_4.SelectedIndex == comboBox_4.Items.Count - 1)
                    {
                        comboBox_midi4_select.Items.Clear();
                        panel14.Visible = false;
                        label19.Visible = false;
                        trackBar_mic4_threshold.Visible = false;
                        panel1.Visible = false;
                        comboBox_midi4.Visible = false;
                        checkBox_midi4.Visible = false;
                        panel_midi4.Visible = false;
                        textBox_midi4.Visible = false;
                        comboBox_midi4_select.Visible = false;
                    } else
                    {
                        comboBox_midi4_select.Items.Clear();
                        update_midi_slect(comboBox_4.SelectedIndex, 4);
                        panel14.Visible = true;
                        label19.Visible = true;
                        trackBar_mic4_threshold.Visible = true;
                        panel1.Visible = true;
                        comboBox_midi4.Visible = false;
                        checkBox_midi4.Visible = false;
                        panel_midi4.Visible = false;
                        textBox_midi4.Visible = false;
                        comboBox_midi4_select.Visible = true;
                    }
                }
            }
            catch { }

        } 
        void update_midi_slect(int channel,int comboboxNum)
        {
            if(comboboxNum == 4)
            {
                
                for(int i = 0; i < midiPlay.midiMsg.valueFlag.GetLength(0); i++)
                {
                    if(midiPlay.midiMsg.valueFlag[i,channel])
                    {
                        if (channel != 9)
                        {
                            comboBox_midi4_select.Items.Add(i.ToString() + "...");
                        } else
                        {
                            MidiValue midiValue = new MidiValue();
                            comboBox_midi4_select.Items.Add(midiValue.value[i]);
                        }
                    }
                }

                comboBox_midi4_select.Items.Add("128..");
                comboBox_midi4_select.SelectedIndex = comboBox_midi4_select.Items.Count - 1;
            }
            if (comboboxNum == 3)
            {

                for (int i = 0; i < midiPlay.midiMsg.valueFlag.GetLength(0); i++)
                {
                    if (midiPlay.midiMsg.valueFlag[i, channel])
                    {
                        if (channel != 9)
                        {
                            comboBox_midi3_select.Items.Add(i.ToString() + "...");
                        }
                        else
                        {
                            MidiValue midiValue = new MidiValue();
                            comboBox_midi3_select.Items.Add(midiValue.value[i]);
                        }
                    }
                }

                comboBox_midi3_select.Items.Add("128..");
                comboBox_midi3_select.SelectedIndex = comboBox_midi3_select.Items.Count - 1;
            }
        }

        private void textBox_Mic1_min_TextChanged(object sender, EventArgs e)
        {
            try
            {
                combobox1_min = int.Parse(textBox_Mic1_min.Text);
                if (combobox1_min < FRE_MIN)
                {
                    combobox1_min = FRE_MIN;
                    textBox_Mic1_min.Text = FRE_MIN.ToString(); ;
                }
                else if (combobox1_min > FRE_MAX)
                {
                    combobox1_min = FRE_MAX;
                    textBox_Mic1_min.Text = FRE_MAX.ToString();
                }
            }
            catch (Exception ex)
            {
                if (textBox_Mic1_min.Text != "-")
                {
                    textBox_Mic1_min.Text = "invalid";
                }
            }
        }

        private void textBox_Mic1_max_TextChanged(object sender, EventArgs e)
        {
            try
            {
                combobox1_max = int.Parse(textBox_Mic1_max.Text);
                if (combobox1_max < FRE_MIN)
                {
                    combobox1_max = FRE_MIN;
                    textBox_Mic1_max.Text = FRE_MIN.ToString(); ;
                }
                else if (combobox1_max > FRE_MAX)
                {
                    combobox1_max = FRE_MAX;
                    textBox_Mic1_max.Text = FRE_MAX.ToString();
                }
            }
            catch (Exception ex)
            {
                if (textBox_Mic1_max.Text != "-")
                {
                    textBox_Mic1_max.Text = "invalid";
                }
            }
        }

        private void textBox_Mic2_min_TextChanged(object sender, EventArgs e)
        {
            try
            {
                combobox2_min = int.Parse(textBox_Mic2_min.Text);
                if (combobox2_min < FRE_MIN)
                {
                    combobox2_min = FRE_MIN;
                    textBox_Mic2_min.Text = FRE_MIN.ToString(); ;
                }
                else if (combobox2_min > FRE_MAX)
                {
                    combobox2_min = FRE_MAX;
                    textBox_Mic2_min.Text = FRE_MAX.ToString();
                }
            }
            catch (Exception ex)
            {
                if (textBox_Mic2_min.Text != "-")
                {
                    textBox_Mic2_min.Text = "invalid";
                }
            }
        }

        private void textBox_Mic2_max_TextChanged(object sender, EventArgs e)
        {
            try
            {
                combobox2_max = int.Parse(textBox_Mic2_max.Text);
                if (combobox2_max < FRE_MIN)
                {
                    combobox2_max = FRE_MIN;
                    textBox_Mic2_max.Text = FRE_MIN.ToString(); ;
                }
                else if (combobox2_max > FRE_MAX)
                {
                    combobox2_max = FRE_MAX;
                    textBox_Mic2_max.Text = FRE_MAX.ToString();
                }
            }
            catch (Exception ex)
            {
                if (textBox_Mic2_max.Text != "-")
                {
                    textBox_Mic2_max.Text = "invalid";
                }
            }
        }


        private void trackBar_mic4_threshold_Scroll(object sender, EventArgs e)
        {
            midi1.threshold4 = midi2.threshold4 = midi.threshold4 = midi3.threshold4 = trackBar_mic4_threshold.Value;
            midiPlay.midiMsg.threshold4 = trackBar_mic4_threshold.Value;
            combobox4_threshold = trackBar_mic4_threshold.Value;
            label19.Location = new Point(trackBar_mic4_threshold.Location.X + (int)(((double)trackBar_mic4_threshold.Value/ trackBar_mic4_threshold.Maximum)*trackBar_mic4_threshold.Size.Width)-label19.Size.Width/2, label19.Location.Y);
            label19.Text = trackBar_mic4_threshold.Value.ToString();
        }

        private void trackBar_mic3_threshold_Scroll(object sender, EventArgs e)
        {
            midi1.threshold3 = midi2.threshold3 = midi.threshold3 = midi3.threshold3 = trackBar_mic3_threshold.Value;
            midiPlay.midiMsg.threshold3 = trackBar_mic3_threshold.Value;
            combobox3_threshold = trackBar_mic3_threshold.Value;
            label20.Location = new Point(trackBar_mic3_threshold.Location.X + (int)(((double)trackBar_mic3_threshold.Value / trackBar_mic3_threshold.Maximum) * trackBar_mic3_threshold.Size.Width) - label20.Size.Width / 2, label20.Location.Y);
            label20.Text = trackBar_mic3_threshold.Value.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int value = int.Parse(textBox1.Text);
                if (value < 0)
                {
                    value = 0;
                    textBox1.Text = "0";
                }
                if (value > 127)
                {
                    value = 127;
                    textBox1.Text = "127";
                }
                if (comboBox_file.SelectedItem.ToString() == "NULL")
                {
                    mic.minVoice = value<<8;
                    mic1.minVoice = value << 8;
                    mic2.minVoice = value << 8;
                    mic3.minVoice = value << 8;
                } else
                {

                    midiPlay.midiMsg.minStrength = value;
                }
            }
            catch (Exception ex)
            {
                if (comboBox_file.SelectedItem.ToString() == "NULL")
                {
                    mic.minVoice = 0;
                    mic1.minVoice = 0;
                    mic2.minVoice = 0;
                    mic3.minVoice = 0;
                }
                else
                {
                    midiPlay.midiMsg.minStrength = 0;
                }
                if (textBox1.Text != "")
                {
                    textBox1.Text = "invalid";
                }
            }
        }
        bool flag = false;
        void startMidiPlay()
        {
            while (!flag) ;
            midiPlay.midiGo();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button_play.Text == "Play")
            {
                flag = false;
                button_play.Text = "Stop";
                string path = this.comboBox_file.SelectedItem.ToString();
                mp3play.end();
                midiPlay.midiMsg.picture1 = midiPlay.midiMsg.picture2 = 0;
                midiPlay.midiClear();
                midiPlay.midiMsg.ReadFileToBytes(path);
                if (comboBox_file.SelectedItem.ToString() != "NULL")
                {
                    if (comboBox_mp3.SelectedItem.ToString() != "NULL")
                    {
                        string s = this.comboBox_mp3.SelectedItem.ToString(); mp3play.getSong(s);
                        midiPlay.midiReady();
                        mp3play.Play();
                        midiPlay.midiGo();
                    }
                    else
                    {
                        midiPlay.midiStart();
                    }
                }
                else
                {
                    
                    button_play.Text = "Play";
                   // midiPlay.midiStart();
                }
               // midiPlay.midiStart();

            }
            else if (button_play.Text == "Stop")
            {

 
                button_play.Text = "Continue";
                midiPlay.midiStop();
                mp3play.stop();
                Mic12drawCircle(1, 0);
                Mic12drawCircle(2, 0);
            }
            else if (button_play.Text == "Continue")
            {
                button_play.Text = "Stop";
                midiPlay.midiContinue();
                mp3play.continue_play();

            }
        }



        private void comboBox_file_SelectedIndexChanged(object sender, EventArgs e)
        {
            midiPlay.midiMsg.ReadFileGetvalue(comboBox_file.SelectedItem.ToString());
            midiPlay.midiClear();
            mp3play.end();
            textBox1.Text = "0";
            //  midiMsg.queue.Clear();
            if (midiPlay.midiOut != null)
            {
                midiPlay.midiOut.stop();
            }
            button_play.Text = "Play";
            if(comboBox_file.SelectedItem.ToString() == "NULL")
            {
                updateDeviceCombox();

            } else
            {
                stopDevice();
                updateChannelCombox();
            }
        }
        void stopDevice()
        {
            if (tipCombobox1Fre != null)
            {
                tipCombobox1Fre.RemoveAll();
                tipCombobox2Fre.RemoveAll();
            }
            if (Threadmidi1value != null)
            {
                Threadmidi1value.Abort();
            }
            if (Threadmidi2value != null)
            {
                Threadmidi2value.Abort();
            }
            if (Threadmidi3value != null)
            {
                Threadmidi3value.Abort();
            }
            if (Threadmidi4value != null)
            {
                Threadmidi4value.Abort();
            }
            if (Threadmic1value != null)
            {
                Threadmic1value.Abort();
            }
            if (Threadmic2value != null)
            {
                Threadmic2value.Abort();
            }
            if (Threadmic3value != null)
            {
                Threadmic3value.Abort();
            }
            if (Threadmic4value != null)
            {
                Threadmic4value.Abort();
            }
        }

        private void comboBox_client_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox_midi4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int value = int.Parse(textBox_midi4.Text);
                if (value > 127)
                {
                    value = 127;
                    textBox_midi4.Text = "127";
                }
                midiPlay.midiMsg.frethreshold4 = value;
                midi.prethreshold4 = midi1.prethreshold4 = midi2.prethreshold4 = midi3.prethreshold4 = value;
                
            }
            catch (Exception ex)
            { 
                midiPlay.midiMsg.frethreshold4 = 128;
                midi.prethreshold4 = midi1.prethreshold4 = midi2.prethreshold4 = midi3.prethreshold4 = 128;
                if (textBox_midi4.Text != "")
                {
                    textBox_midi4.Text = "invalid";
                }
            }
        }

        private void textBox_midi3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int value = int.Parse(textBox_midi3.Text);
                if (value > 127)
                {
                    value = 127;
                    textBox_midi3.Text = "127";
                }
                midiPlay.midiMsg.frethreshold3 = value;
                midi.prethreshold3 = midi1.prethreshold3 = midi2.prethreshold3 = midi3.prethreshold3 = value;

            }
            catch (Exception ex)
            {
                midiPlay.midiMsg.frethreshold3 = 128;
                midi.prethreshold3 = midi1.prethreshold3 = midi2.prethreshold3 = midi3.prethreshold3 = 128;
                if (textBox_midi3.Text != "")
                {
                    textBox_midi3.Text = "invalid";
                }
            }
        }

        private void comboBox_midi4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button_file_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Filter = "Midi File|*.mid";
            this.openFileDialog1.ShowDialog();
            this.comboBox_file.Items.Add(this.openFileDialog1.FileName);
            this.comboBox_file.SelectedIndex = this.comboBox_file.Items.Count - 1;
        }

        private void button_mp3_Click_1(object sender, EventArgs e)
        {
            this.openFileDialog2.Filter = "MP3 File|*.mp3";
            this.openFileDialog2.ShowDialog();
            this.comboBox_mp3.Items.Add(this.openFileDialog2.FileName);
            this.comboBox_mp3.SelectedIndex = this.comboBox_mp3.Items.Count - 1;
        }

        private void comboBox_midi4_select_SelectedIndexChanged(object sender, EventArgs e)
        {
            Regex r = new Regex(@"\w+?(?=\.)");
            Match m = r.Match(comboBox_midi4_select.SelectedItem.ToString());
            if (m.Success)
            {
                try
                {
                    int value = int.Parse(m.Value);

                    midiPlay.midiMsg.frethreshold4 = value;
                    // midi.prethreshold4 = midi1.prethreshold4 = midi2.prethreshold4 = midi3.prethreshold4 = value;
                    //if (value == 128)
                    //{
                    //    textBox_midi4.Text = "";
                    //}
                    //else {
                    //    textBox_midi4.Text = m.Value;
                    //}
                } catch(Exception ex)
                {
                    ;
                }
            }
        }

        private void comboBox_midi3_select_SelectedIndexChanged(object sender, EventArgs e)
        {
            Regex r = new Regex(@"\w+?(?=\.)");
            Match m = r.Match(comboBox_midi3_select.SelectedItem.ToString());
            if (m.Success)
            {
                try
                {
                    int value = int.Parse(m.Value);
                    midiPlay.midiMsg.frethreshold3 = value;
                    // midi.prethreshold3 = midi1.prethreshold3 = midi2.prethreshold3 = midi3.prethreshold3 = value;
                    //if (value == 128)
                    //{
                    //    textBox_midi3.Text = "";
                    //}
                    //else {
                    //    textBox_midi3.Text = m.Value;
                    //}
                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }

        private void checkBox_output_CheckedChanged(object sender, EventArgs e)
        {
            midiPlay.playOutput = checkBox_output.Checked;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox_delay_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int value = int.Parse(textBox_delay.Text);
                if (value > 999)
                {
                    value = 999;
                    textBox_delay.Text = "999";
                } else if (value < -999)
                {
                    value = -999;
                    textBox_delay.Text = "-999";
                }

                if (midiPlay.mst.HPT != null)
                {
                    midiPlay.mst.HPT.setDelay(value);
                } else
                {
                    midiPlay.delayTimer = value;
                }

            }
            catch (Exception ex)
            {
                if (textBox_delay.Text != "" && textBox_delay.Text != "-")
                {
                    if (midiPlay.mst.HPT != null)
                    {
                        midiPlay.mst.HPT.setDelay(0);
                    }
                    else
                    {
                        midiPlay.delayTimer = 0;
                    }
                    textBox_delay.Text = "invalid";
                }
            }
        }

        private void textBox_port_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxMusicList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMusicList.SelectedItem.ToString() == "NULL")
            {
                updateDeviceCombox();


            }
            else
            {
                stopDevice();
                updateChannelCombox();
                try
                {
                    
                    updateChannelCombox(jsonDataParse.musicList[comboBoxMusicList.SelectedIndex]);
                }
                catch (Exception)
                {

                }
            }
        }

    }
}
