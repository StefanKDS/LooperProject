using Android.Media;
using System;
using System.IO;

/// <summary>
/// 
/// </summary>
namespace LooperApp
{
    /// <summary>
    /// 
    /// </summary>
    public class Channel
    {
        public Channel()
        {
            InitChannel();
        }

        private void InitChannel()
        {
            theChannel = new MediaPlayer();
            theChannel.Looping = true;
        }

        public void playMedia()
        {
            if (theChannel != null && !theChannel.IsPlaying)
            {
                theChannel.Start();
            }
        }

        public void stopMedia()
        {
            if (theChannel != null && theChannel.IsPlaying)
            {
                theChannel.Pause();
                theChannel.SeekTo(0);
            }
        }

        public void pauseMedia()
        {
            if (theChannel != null && theChannel.IsPlaying)
            {
                theChannel.Pause();
                resumePosition = theChannel.CurrentPosition;
                pause = true;
            }
        }

        public void resumeMedia()
        {
            if (theChannel != null && !theChannel.IsPlaying && pause)
            {
                pause = false;
                theChannel.SeekTo(resumePosition);
                theChannel.Start();
            }
        }

        public bool isPause()
        {
            return pause;
        }

        public void setVolume(float volume)
        {
            if (theChannel != null)
            {
                theChannel.SetVolume(volume, volume);
            }
        }

        public int getDuration()
        {
            int duration = 0;
            if (theChannel != null)
                duration = theChannel.Duration;

            return duration;
        }

        public int getCurPos()
        {
            int duration = 0;
            if (theChannel != null && theChannel.IsPlaying)
                duration = theChannel.CurrentPosition;

            return duration;
        }

        public int getCurPosPercent()
        {
            float percent = 0;

            if (theChannel != null && theChannel.IsPlaying)
                percent = (100 / (float)theChannel.Duration) * theChannel.CurrentPosition;

            return (int)percent;
        }

        public bool isPlaying()
        {
            if (theChannel != null)
                return theChannel.IsPlaying;

            return false;
        }

        public void loadTrack(String file)
        {
            stopMedia();
            theChannel.Reset();
            theChannel.Looping = true;
            try
            {
                theChannel.SetDataSource(file);
            }
            catch (IOException)
            {

            }
            try
            {
                theChannel.Prepare();
                m_trackLoaded = true;
            }
            catch (IOException)
            {
                m_trackLoaded = false;
            }
        }

        public bool trackLoaded()
        {
            return m_trackLoaded;
        }

        /// Variablen ///
        private MediaPlayer theChannel;
        private int resumePosition;
        bool pause = false;
        private bool m_trackLoaded = false;

    }
}
