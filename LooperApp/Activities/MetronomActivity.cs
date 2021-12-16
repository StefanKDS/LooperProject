using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Interop;

namespace LooperApp
{
    [Activity(Label = "metronom", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MetronomActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.metronom);

            NumberPicker np = FindViewById<NumberPicker>(Resource.Id.beat);
            np.MaxValue = 300;
        }

        [Export("StartPlay")]
        public void StartPlay(View view)
        {
            NumberPicker np = FindViewById<NumberPicker>(Resource.Id.beat);
            int beats = np.Value;


        }
    }
}