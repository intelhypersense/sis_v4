using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MidiAndMic1
{
    class MidiOut
    {
        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutOpen(out UInt32 lphMidiOut, uint uDeviceID, UInt32 dwCallback, UInt32 dwInstance, UInt32 dwFlags);

        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutClose(UInt32 hMidiOut);

        [DllImport("winmm.dll")]
        private static extern UInt32 midiOutShortMsg(UInt32 hMidiOut, UInt32 dwMsg);

        private bool _isOpened;
        private uint _deviceHandle;

        public bool IsOpened
        {
            get
            {
                return _isOpened;
            }
        }

        /// <summary>
        /// Device handle can be made public
        /// </summary>
        public uint DeviceHandle
        {
            get
            {
                return _deviceHandle;
            }
        }

        public uint ShortPlay(uint msg)
        {
            if (_isOpened)
                return midiOutShortMsg(_deviceHandle, msg);
            else
                return 621;
        }

        /// <summary>
        /// Play the sound
        /// </summary>
        /// <param name="key">Pitch</param>
        /// <param name="volume">volume</param>
        /// <param name="chenel">aisle</param>
        /// <returns></returns>
        public uint ShortPlay(uint key, uint volume, uint chenel)
        {
            return ShortPlay(144 + key * 256 + volume * 65536 + chenel);
        }

        public UInt32 Open()
        {
            if (_isOpened)
                return _deviceHandle;

            uint h_Device;
            uint h_r = midiOutOpen(out h_Device, 0, 0, 0, 0);
            _isOpened = (h_r == 0);

            if (_isOpened)
            {
                _deviceHandle = h_Device;
                return h_Device;
            }
            else
            {
                _deviceHandle = 0;
                return 0;
            }
        }
        public void stop()
        {
            for (uint i = 0; i < 16; i++)
            {
                uint stop = (0x0078 << 8) | (0xB0 | i);
                ShortPlay(stop);
                stop = (0x007b << 8) | (0xB0 | i);
                ShortPlay(stop);
            }
        }
        public uint Close()
        {
            if (_isOpened)
            {
                _isOpened = false;
                _deviceHandle = 0;
                return midiOutClose(_deviceHandle);
            }
            else
                return 5;
        }
    }
}
