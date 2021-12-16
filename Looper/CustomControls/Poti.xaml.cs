using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für Poti.xaml
    /// </summary>
    public partial class Poti : UserControl {
        public Poti ()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomPotiClick;

        /// <summary>
        /// 
        /// </summary>
        public float PotiPosInPercent {
            get
            {
                return ((float)100/(float)270) * potiPos;
            }
            set
            {
                potiPos = (270/100 ) * value;
            }
        }

        /// <summary>
        /// Volume up.
        /// </summary>
        public void VolumeUp()
        {
            if (potiPos <= 270 && potiPos >= 0)
            {
                potiPos += 10;
            }
            else if (potiPos >= 270)
            {
                potiPos = 270;
                return;
            }
            else
            {
                potiPos = 0;
                return;
            }

            RotateTransform rotateTransform1 = new RotateTransform(potiPos);
            rotateTransform1.CenterX = Poti_Btn.Width / 2;
            rotateTransform1.CenterY = Poti_Btn.Height / 2;
            Poti_Btn.RenderTransform = rotateTransform1;

            CustomPotiClick(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Volume down.
        /// </summary>
        public void VolumeDown()
        {
            if (potiPos <= 270 && potiPos >= 0)
            {
                potiPos -= 10;
            }
            else if (potiPos >= 270)
            {
                potiPos = 270;
                return;
            }
            else
            {
                potiPos = 0;
                return;
            }

            RotateTransform rotateTransform1 = new RotateTransform(potiPos);
            rotateTransform1.CenterX = Poti_Btn.Width / 2;
            rotateTransform1.CenterY = Poti_Btn.Height / 2;
            Poti_Btn.RenderTransform = rotateTransform1;

            CustomPotiClick(this, new RoutedEventArgs());
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Poti_Btn_PreviewMouseLeftButtonUp (object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            if (potiPos <= 270 && potiPos >= 0)
            {
                if (p.X > Poti_Btn.Width / 2)
                    potiPos += 10;
                else
                {
                    if(potiPos >= 10)
                        potiPos -= 10;
                }
            } else if ( potiPos >= 270)
                potiPos = 270;
            else
                potiPos = 0;

            RotateTransform rotateTransform1 = new RotateTransform(potiPos);
            rotateTransform1.CenterX = Poti_Btn.Width / 2;
            rotateTransform1.CenterY = Poti_Btn.Height / 2;
            Poti_Btn.RenderTransform = rotateTransform1;

            CustomPotiClick (this, new RoutedEventArgs ());
        }

        // Variablen
        private float potiPos = 0;
    }
}
