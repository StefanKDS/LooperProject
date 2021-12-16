using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Looper
{
    class Channel
    {
        public Channel()
        {
            
        }

        public DirectSoundOut   audioOutput;
        public WavePlayer       Clip;
        public Thread           progressThread  = null;
        public bool             runChannel      = false;
        public bool             ChannelActive   = true;
        public CustomControls.RecButton RecButton = null;
        public CustomControls.EditBtn EditButton = null;
        public String           TempFileName;
        public CustomControls.ChannelVolumeSlider VolumeSlider = null;
        public CustomControls.ProgressCycle ProgressBar = null;
        public CustomControls.Loaded Loaded = null;
    }
}
