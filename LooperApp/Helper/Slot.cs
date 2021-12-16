using Android.Widget;

namespace LooperApp.Helper
{
    public class Slot
    {
        private int m_channelNbr;
        private ChannelNew m_channel;
        private ImageView m_iv_button = null;
        private ImageView m_iv_loop_button = null;
        private TextView m_tv_Title = null;
        private SeekBar m_volume = null;

        public Slot(int channelNbr, bool sampler = false)
        {
            m_channelNbr = channelNbr;
            m_channel = new ChannelNew(m_channelNbr, sampler);
        }

        public ImageView IV_Button
        {
            get => m_iv_button;
            set
            {
                m_iv_button = value;
            }
        }

        public ImageView IV_Loop_Button
        {
            get => m_iv_loop_button;
            set
            {
                m_iv_loop_button = value;
            }
        }

        public TextView TV_Title
        {
            get => m_tv_Title;
            set
            {
                m_tv_Title = value;
            }
        }

        public SeekBar VolumeSeekbar
        {
            get => m_volume;
            set
            {
                m_volume = value;
                m_volume.Max = 100;
                m_volume.Progress = 50;
                m_channel.setVolume(0.5f);
            }
        }

        public ChannelNew Channel
        {
            get => m_channel;
            set
            {
                m_channel = value;
            }
        }

        public string ChannelName
        {
            get
            {
                return "Channel " + m_channelNbr;
            }
        }

        public bool LoopSlot { get; set; }

        public int RessourceID { get; set; }

        public int LoopRessourceID { get; set; }

        public int VolumeRessourceID { get; set; }

        public int TitleRessourceID { get; set; }

        public int ChannelNbr { get => m_channelNbr; }
    }
}