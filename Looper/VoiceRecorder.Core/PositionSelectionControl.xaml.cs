using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VoiceRecorder.Core
{
    /// <summary>
    /// Interaction logic for RangeSelectionControl.xaml
    /// </summary>
    public partial class PositionSelectionControl : UserControl
    {
        private double pos;

        public PositionSelectionControl()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(PositionSelectionControl_SizeChanged);
        }

        void PositionSelectionControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            highlightRect.Height = e.NewSize.Height;
        }

        public double Pos
        {
            get
            {
                return pos;
            }
            set
            {
                if(value != pos)
                {
                    Pos = pos;
                    highlightRect.SetValue(Canvas.LeftProperty, value);
                }
            }
        }

    }
}