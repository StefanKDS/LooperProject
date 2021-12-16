using Android.Content.Res;
using Android.Media;
using Android.OS;
using System;
using System.IO;

namespace LooperApp
{
    public class ChannelNew : Java.Lang.Object, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnCompletionListener
    {
        private int m_channelIndex = 0;
        private bool m_trackLoaded = false;
        private PlaybackParams mCurrentPlaybackParams = null;
        private MediaPlayer mCurrentPlayer = null;
        private string mFileName;
        private bool mLoopChannel;
        private MediaPlayer mNextPlayer = null;
        private float mPitch = 1.0f;
        private bool mplaybackParamsActiv = false;
        private float mSpeed = 1.0f;
        private float mVolume = 100;
        private bool pause = false;
        private int resumePosition;

        public ChannelNew(int channelIndex, bool sampler = false)
        {
            m_channelIndex = channelIndex;
            mLoopChannel = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && !sampler)
            {
                mplaybackParamsActiv = true;
            }

            InitChannel();
        }

        public delegate void CallbackEventHandler(int channelIndex);

        public event CallbackEventHandler ChannelStoped;

        public event CallbackEventHandler NewLoopStarts;

        public float Pitch
        {
            get { return mPitch; }
            set
            {
                if (mPitch != value)
                {
                    mPitch = value;
                    if (mPitch <= 0)
                        mPitch = 0.1f;
                    if (mplaybackParamsActiv && mCurrentPlayer?.PlaybackParams != null)
                    {
                        mCurrentPlayer.PlaybackParams = mCurrentPlayer.PlaybackParams.SetPitch(mPitch);
                    }
                }
            }
        }

        public float Speed
        {
            get { return mSpeed; }
            set
            {
                if (mSpeed != value)
                {
                    mSpeed = value;
                    if (mplaybackParamsActiv && mCurrentPlayer?.PlaybackParams != null)
                    {
                        mCurrentPlayer.PlaybackParams = mCurrentPlayer.PlaybackParams.SetSpeed(mSpeed);
                    }
                }
            }
        }

        public int getCurPos()
        {
            int duration = 0;
            if (mCurrentPlayer != null && mCurrentPlayer.IsPlaying)
                duration = mCurrentPlayer.CurrentPosition;

            return duration;
        }

        public int getCurPosPercent()
        {
            float percent = 0;

            if (mCurrentPlayer != null && mCurrentPlayer.IsPlaying)
                percent = (100 / (float)mCurrentPlayer.Duration) * mCurrentPlayer.CurrentPosition;

            return (int)percent;
        }

        public int getDuration()
        {
            int duration = 0;
            if (mCurrentPlayer != null)
                duration = mCurrentPlayer.Duration;

            return duration;
        }

        public string getLoadedTrack()
        {
            return mFileName;
        }

        public float getPitch()
        {
            return mPitch;
        }

        public float getSpeed()
        {
            return mSpeed;
        }

        public float getVolume()
        {
            return mVolume;
        }

        public void goToStart()
        {
            if (mCurrentPlayer != null)
            {
                mCurrentPlayer.SeekTo(0);
            }
        }

        public bool isPause()
        {
            return pause;
        }

        public bool isPlaying()
        {
            if (mCurrentPlayer != null)
                return mCurrentPlayer.IsPlaying;

            return false;
        }

        public void loadTrack(AssetFileDescriptor afd)
        {
            stopMedia();

            if (mCurrentPlayer == null)
                return;

            mCurrentPlayer.Reset();

            
            try
            {
                mCurrentPlayer.SetDataSource(afd);
            }
            catch (IOException)
            {
            }

            try
            {
                mCurrentPlayer.Prepare();
                m_trackLoaded = true;
            }
            catch (IOException)
            {
                m_trackLoaded = false;
            }

            mCurrentPlayer.SetOnCompletionListener(this);
        }

        public void loadTrack(String file)
        {
            if (string.IsNullOrEmpty(file))
                return;

            stopMedia();

            if (mCurrentPlayer == null)
                return;

            mCurrentPlayer.Reset();

            mFileName = file;
            try
            {
                mCurrentPlayer.SetDataSource(file);
            }
            catch (IOException)
            {
            }

            try
            {
                mCurrentPlayer.Prepare();
                m_trackLoaded = true;
            }
            catch (IOException)
            {
                m_trackLoaded = false;
            }

            mCurrentPlayer.SetOnCompletionListener(this);
        }

        public void loadTrackAgain()
        {
            if (string.IsNullOrEmpty(mFileName))
                return;

            InitChannel();

            loadTrack(mFileName);
        }

        public void LoopChannel(bool loop)
        {
            mLoopChannel = loop;
        }

        public void OnCompletion(MediaPlayer mp)
        {
            if (!mLoopChannel)
            {
                Reset();
                mNextPlayer = null;
                ChannelStoped?.Invoke(m_channelIndex);
                return;
            }
            NewLoopStarts?.Invoke(m_channelIndex);
            mp.Release();
            mCurrentPlayer = mNextPlayer;
            CreateNextMediaPlayer();
        }

        public void OnPrepared(MediaPlayer mp)
        {
        }

        public void pauseMedia()
        {
            if (mCurrentPlayer != null && mCurrentPlayer.IsPlaying)
            {
                mCurrentPlayer.Pause();
                resumePosition = mCurrentPlayer.CurrentPosition;
                pause = true;
            }
        }

        public void playMedia()
        {
            if (mCurrentPlayer != null && !mCurrentPlayer.IsPlaying)
            {
                if (mLoopChannel)
                {
                    CreateNextMediaPlayer();
                }

                if (mplaybackParamsActiv)
                {
                    mCurrentPlaybackParams = new PlaybackParams();
                    mCurrentPlaybackParams.SetSpeed(mSpeed);
                    mCurrentPlaybackParams.SetPitch(mPitch);
                    mCurrentPlayer.PlaybackParams = mCurrentPlaybackParams;
                }
                mCurrentPlayer.Start();
            }
        }

        public void Release()
        {
            mCurrentPlayer.Release();
        }

        public void Reset()
        {
            mCurrentPlayer.Reset();
        }

        public void resumeMedia()
        {
            if (mCurrentPlayer != null && !mCurrentPlayer.IsPlaying && pause)
            {
                pause = false;
                mCurrentPlayer.SeekTo(resumePosition);
                mCurrentPlayer.Start();
            }
        }

        public bool SaveTrackAs(String fileName)
        {
            string DefaultInitialDirectory = Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper" + Java.IO.File.Separator + "loops" + Java.IO.File.Separator;

            string destinationFile = DefaultInitialDirectory + fileName;

            if (!System.IO.File.Exists(destinationFile))
            {
                System.IO.File.Copy(mFileName, destinationFile);
                return true;
            }

            return false;
        }

        public void setVolume(float volume)
        {
            mVolume = volume;

            if (mCurrentPlayer != null)
                mCurrentPlayer.SetVolume(volume, volume);

            if (mNextPlayer != null)
                mNextPlayer.SetVolume(volume, volume);
        }

        public void stopMedia()
        {
            if (mCurrentPlayer != null && mCurrentPlayer.IsPlaying)
            {
                mCurrentPlayer.Pause();
                mCurrentPlayer.SeekTo(0);
                Reset();
                mNextPlayer = null;
                loadTrackAgain();
            }
        }

        public bool trackLoaded()
        {
            return m_trackLoaded;
        }

        private void CreateNextMediaPlayer()
        {
            mNextPlayer = new MediaPlayer();
            mNextPlayer.SetDataSource(mFileName);
            mNextPlayer.SetVolume(mVolume, mVolume);
            mNextPlayer.Prepare();
            mNextPlayer.SetOnCompletionListener(this);
            if (mCurrentPlayer != null)
                mCurrentPlayer.SetNextMediaPlayer(mNextPlayer);
        }

        public void InitChannel()
        {
            mCurrentPlayer = new MediaPlayer();
            mCurrentPlayer.SetVolume(mVolume, mVolume);
            mCurrentPlayer.Looping = false;
            mCurrentPlayer.SetOnPreparedListener(this);

            if (mNextPlayer != null)
            {
                mNextPlayer = null;
            }
        }
    }
}