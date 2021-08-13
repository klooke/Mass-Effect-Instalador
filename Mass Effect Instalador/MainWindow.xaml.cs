using System;
using System.ComponentModel;
using System.Windows;

namespace Mass_Effect_Instalador
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;

            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(10000);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        { 

        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
