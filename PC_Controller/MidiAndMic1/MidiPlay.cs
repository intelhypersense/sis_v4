using highPerformanceTimer;
using MidiMsgspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidiAndMic1
{
    class MidiPlay
    {
        public delegate void changeButtonText(string str);
        public changeButtonText changeText;
        public delegate void StopMp3();
        public StopMp3 stopMp3;
        public MidiOut midiOut;
        public MidiMsg midiMsg;
        public bool playOutput = true;
        
        public struct msgStruct
        {
            public Queue<MidiMsg.MidiOutMessage> queue;
            public high_performance_timer HPT;
        };
        public int delayTimer = 0;
        public List<msgStruct> list = new List<msgStruct>();
        public msgStruct mst = new msgStruct();
        public List<Thread> threadList = new List<Thread>();
        public int disconnectCount = 0;
        Thread timerThread;
        public MidiPlay()
        {
            midiOut = new MidiOut();
            midiMsg = new MidiMsg();
        }
        public void midiClear()
        {
            list.Clear();
            threadList.Clear();
            Monitor.Enter(midiMsg.queue);
            midiMsg.queue.Clear();
            Monitor.Exit(midiMsg.queue);

            Monitor.Enter(midiMsg.queueList);
            midiMsg.queueList.Clear();
            Monitor.Exit(midiMsg.queueList);

            Monitor.Enter(midiMsg.queueTCP);
            midiMsg.queueTCP.Clear();
            Monitor.Exit(midiMsg.queueTCP);

            Monitor.Enter(midiMsg.queuePicture);
            midiMsg.queuePicture.Clear();
            Monitor.Exit(midiMsg.queuePicture);
        }
        public void midiStart() {
            disconnectCount = 0;
            mst.queue = midiMsg.queue;
            mst.HPT = new high_performance_timer();
            
            mst.HPT.enable();mst.HPT.reset();
            timerThread = new Thread(new ParameterizedThreadStart(timerTask));
            timerThread.IsBackground = true;
            timerThread.Start(mst);
        }
        public void midiGo()
        {
            timerThread.Start(mst);
            mst.HPT.setDelay(delayTimer);
        }
        public void midiReady()
        {
            disconnectCount = 0;
            mst.queue = midiMsg.queue;
            mst.HPT = new high_performance_timer();
           
            mst.HPT.enable(); mst.HPT.reset();
            timerThread = new Thread(new ParameterizedThreadStart(timerTask));
            timerThread.IsBackground = true;
            
        }
        void timerTask(object objectMsg)
        {
            Queue<MidiMsg.MidiOutMessage> queue = ((MidiPlay.msgStruct)objectMsg).queue;
            high_performance_timer HPT = ((MidiPlay.msgStruct)objectMsg).HPT;
            bool prePlayFlag = playOutput;
            HPT.reset();
            while (true)
            {
                
                if (queue.Count > 0)
                {
                    MidiMsg.MidiOutMessage mom = queue.Dequeue();
                    if (mom.timer > 0)
                    {
                        HPT.get_ugap(mom.timer);
                    }
                    if(mom.msg != 0x00) {
                        if (playOutput)
                        {
                            if ((mom.msg & 0x0f) == midiMsg.filer[1] || (mom.msg & 0x0f) == midiMsg.filer[2])
                            {
                                if (((mom.msg >> 24) & 0xff) > midiMsg.minStrength || (mom.msg & 0xf0) != 0x90)
                                {
                                    midiOut.ShortPlay(mom.msg);

                                }
                            }else
                            {
                                midiOut.ShortPlay(mom.msg); 
                            }                
                        } else if(prePlayFlag == true)
                        {
                            midiOut.stop();
                        }
                        midiMsg.storeTCPmsg(mom.msg);
                    }
                    prePlayFlag = playOutput;
                }
                else {
                    if (!midiMsg.readFileFlag)
                    {
                        if (queue.Count == 0)
                        {
                            changeText("Play");
                            Thread.CurrentThread.Abort();
                        }
                    }
                    HPT.reset();
                }
            }
        }
        public void midiStop() {
            timerThread.Abort();
            midiOut.stop();
        }
        public void midiContinue() {
            midiMsg.picture1 = midiMsg.picture2 = 0;
            timerThread = new Thread(new ParameterizedThreadStart(timerTask));
            mst.HPT.enable();
            timerThread.Start(mst);
        }
    }
}
