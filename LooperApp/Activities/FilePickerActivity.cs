using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using System.Collections.Generic;

namespace LooperApp
{
    [Activity(Label = "file_picker")]
    public class FilePickerActivity : FragmentActivity
    {
        private IList<string> extList;
        private string filePath;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.file_picker);
            var bundle = Intent.GetBundleExtra("ExtList");
            if (bundle != null)
                extList = bundle.GetStringArrayList("ExtList");
            bundle = Intent.GetBundleExtra("FilePath");
            if (bundle != null)
                filePath = bundle.GetString("FilePath");

            FileListFragment fragment =  (FileListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.file_list_fragment);
            fragment.DefaultInitialDirectory = filePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<string> GetExtension()
        {
            return extList;
        }

        public override void OnBackPressed()
        {
            FileListFragment fragment = (FileListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.file_list_fragment);
            if (fragment.CurrentLocation == fragment.DefaultInitialDirectory)
            {
                base.OnBackPressed();
                return;
            }
            else
            {
                fragment.RefreshFilesList(fragment.LastLocation);
                return;
            }
        }

        public void SetData(string path)
        {
            Intent intent = new Intent();
            intent.PutExtra("GetPath", path);

            SetResult(Result.Ok, intent);
            Finish();
        }
    }
}