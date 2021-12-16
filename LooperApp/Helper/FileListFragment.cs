

namespace LooperApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Android.OS;
    using Android.Support.V4.App;
    using Android.Util;
    using Android.Views;
    using Android.Widget;

    /// <summary>
    ///   A ListFragment that will show the files and subdirectories of a given directory.
    /// </summary>
    /// <remarks>
    ///   <para> This was placed into a ListFragment to make this easier to share this functionality with with tablets. </para>
    ///   <para> Note that this is a incomplete example. It lacks things such as the ability to go back up the directory tree, or any special handling of a file when it is selected. </para>
    /// </remarks>
    public class FileListFragment : ListFragment
    {
        private FileListAdapter _adapter;
        private DirectoryInfo _directory;
        private IList<string> extList;
        public string CurrentLocation { get; set; }
        public string LastLocation { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _adapter = new FileListAdapter(Activity, new FileSystemInfo[0]);
            ListAdapter = _adapter;
            LastLocation = string.Empty;
            CurrentLocation = string.Empty;
        }

        public string DefaultInitialDirectory { get; set; }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var fileSystemInfo = _adapter.GetItem(position);

            if (fileSystemInfo.Attributes != FileAttributes.Directory)
            {
                ((FilePickerActivity)this.Activity).SetData(fileSystemInfo.FullName);
            }
            else
            {
                // Dig into this directory, and display it's contents
                RefreshFilesList(fileSystemInfo.FullName);
            }

            base.OnListItemClick(l, v, position, id);
        }

        public override void OnResume()
        {
            base.OnResume();
            RefreshFilesList(DefaultInitialDirectory);
        }

        public void RefreshFilesList(string directory)
        {
            LastLocation = CurrentLocation;
            CurrentLocation = directory;
            IList<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
            var dir = new DirectoryInfo(directory);

            extList = ((FilePickerActivity)this.Activity).GetExtension();

            try
            {
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.Attributes != FileAttributes.Hidden))
                {
                    if ((extList != null && extList.Contains(item.Extension)) || item.Attributes == FileAttributes.Directory)
                        visibleThings.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error("FileListFragment", "Couldn't access the directory " + _directory.FullName + "; " + ex);
                Toast.MakeText(Activity, "Problem retrieving contents of " + directory, ToastLength.Long).Show();
                return;
            }

            _directory = dir;

            _adapter.AddDirectoryContents(visibleThings);

            // If we don't do this, then the ListView will not update itself when then data set 
            // in the adapter changes. It will appear to the user that nothing has happened.
            ListView.RefreshDrawableState();

            Log.Verbose("FileListFragment", "Displaying the contents of directory {0}.", directory);
        }
    }
}