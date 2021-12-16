
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LooperApp
{
    public class VerticalSeekbar : SeekBar
    {

        public VerticalSeekbar(Context context)
            : base(context)
        {
        }

        public VerticalSeekbar(Context context, IAttributeSet attrs)
            : base(context,attrs)
        {
        }

        public VerticalSeekbar(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
        }

        public VerticalSeekbar(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(MeasuredHeight, MeasuredWidth);
        }

        protected override void OnDraw(Canvas c)
        {
            c.Rotate(-90);
            c.Translate(-Height, 0);

            base.OnDraw(c);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!Enabled)
            {
                return false;
            }

            switch (e.Action) 
            {
                case MotionEventActions.Down:
                case MotionEventActions.Move:
                case MotionEventActions.Up:
                    {
                        int i = 0;
                        i = Max - (int)(Max * e.GetY() / Height);
                        Progress =i;
                        OnSizeChanged(Width, Height, 0, 0);
                    }
                    break;
                case MotionEventActions.Cancel:
                    break;
            }

            return true;
        }

    }
}