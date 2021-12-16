using NAudio.Wave;
using System;
using System.Windows;

namespace Looper
{
    class Audio
    {
        public Audio ( int source )
        {
            sourceDevice = source;
        }

        /// <summary>
        /// Startet die Aufnahme
        /// </summary>
        public void StartRecord (string filename)
        {
            if (sourceDevice == -1) return;

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
          
            fileName = documentsPath + "\\" + filename;

            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, sourceDevice);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveFile = new WaveFileWriter(fileName, waveSource.WaveFormat);

            waveSource.StartRecording();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartRecordFromSoundcard (string filenamePath)
        {
            OutputSource = new WasapiLoopbackCapture ();

            OutputSource.DataAvailable += waveSource_DataAvailable;
            OutputSource.RecordingStopped += new EventHandler<StoppedEventArgs> (waveSource_RecordingSCStopped);

            waveFile = new WaveFileWriter (filenamePath, OutputSource.WaveFormat);

            OutputSource.StartRecording ();
        }

        /// <summary>
        /// Stopt die Aufnahme
        /// </summary>
        public void StopSCRecord ()
        {
            if (OutputSource != null)
                OutputSource.StopRecording ();
        }

        /// <summary>
        /// Stopt die Aufnahme
        /// </summary>
        public void StopRecord ()
        {
            if (waveSource != null)
                waveSource.StopRecording ();
        }

        /// <summary>
        /// Sind Daten vorhanden, dann werden diese in die Datei geaschrieben
        /// </summary>
        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Aufnahme gestoppt wurde
        /// </summary>
        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            if (RecordingStopped != null)
                RecordingStopped(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Aufnahme gestoppt wurde
        /// </summary>
        void waveSource_RecordingSCStopped (object sender, StoppedEventArgs e)
        {
            if (OutputSource != null)
            {
                OutputSource.Dispose ();
                OutputSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose ();
                waveFile = null;
            }

            if (RecordingStopped != null)
                RecordingStopped (this, new RoutedEventArgs ());
        }

        /// <summary>
        /// Setter und Getter für den Dateinamen
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }


        // Variablen
        public event RoutedEventHandler     RecordingStopped;
        public WaveIn                       waveSource      = null;
        public WasapiLoopbackCapture        OutputSource      = null;
        public WaveFileWriter               waveFile        = null;
        private int                         sourceDevice    =-1;
        private string                      fileName;       
    }
}
