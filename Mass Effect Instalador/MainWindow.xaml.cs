using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.TLK.ME1;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MassEffectInstalador
{
    public partial class MainWindow : Window
    {
        private const int LE1_TLK_COUNT = 1678;
        private const int LE1_PCC_COUNT = 406;

        private bool isBusy;
        private bool packagesExistLE1;
        private int countFiles;

        public MainWindow()
        {
            InitializeComponent();
            MEPackageHandler.Initialize();
            PackageSaver.Initialize();
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync(10000);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if(isBusy)
            {
                if(MessageBox.Show(this, "A instalação ainda não está concluida, se fechar agora alguns arquivos podem ser corrompidos, tem certeza que deseja sair?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string tlkPathLE1 = Directory.GetCurrentDirectory() + "\\Files\\";
            string installPath = App.directoryGame + "\\Game\\ME1\\BioGame\\CookedPCConsole\\";
            string[] packagesPath = Properties.Resources.ListPackageLE1.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            if(NumberTlkToInstall(tlkPathLE1) != LE1_TLK_COUNT) return;
            if(!PackagesExist(installPath, packagesPath)) return;
            if(!MakeBackup(installPath, packagesPath)) return;

            for(int i = 0; i < packagesPath.Length; i++)
            {
                isBusy = true;
                using IMEPackage package = MEPackageHandler.OpenLE1Package(installPath + packagesPath[i]);
                for(int j = 0; j < package.LocalTalkFiles.Count; j++)
                {
                    ME1TalkFile tlkFile = package.LocalTalkFiles[j];
                    if(tlkFile.Name is "tlk_M" or "GlobalTlk_tlk_M" or "tlk" or "GlobalTlk_tlk")
                    {
                        string tlkPathFull = tlkPathLE1 + tlkFile.BioTlkSetName + "." + tlkFile.Name + ".xml";
                        HuffmanCompression compressor = new();
                        compressor.LoadInputData(tlkPathFull);
                        compressor.serializeTalkfileToExport(package.GetUExport(tlkFile.UIndex), true);
                        countFiles++;
                    }
                }
                (sender as BackgroundWorker).ReportProgress(i);
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dir.Value = e.ProgressPercentage;
            string s = (int)(e.ProgressPercentage / Dir.Maximum * 100) + "%";
            Title = "Extração: " + s;
            textTitulo.Text = "Extraindo... " + s;
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isBusy = false;
            if(packagesExistLE1)
            {
                if(countFiles == LE1_TLK_COUNT)
                    MessageBox.Show(this, "Instalação foi concluida com êxito, aproveite!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(this, "Alguns arquivos não foram encontrados, por isso a tradução pode está incompleta.\n" +
                        "Por favor reinstale a tradução.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
                MessageBox.Show(this, "Não foi possivel concluir a instalação, arquivos não encontrados!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
        private static int NumberTlkToInstall(string path)
        {
            return Directory.Exists(path) ? Directory.GetFiles(path, "*.xml").Length : 0;
        }
        private bool PackagesExist(string path, string[] files)
        {
            packagesExistLE1 = true;

            if (!Directory.Exists(path) || files.Length != LE1_PCC_COUNT)
                packagesExistLE1 = false;

            foreach(string file in files)
            {
                if (!File.Exists(path + file))
                    packagesExistLE1 = false;
            }

            return packagesExistLE1;
        }
        private static bool MakeBackup(string path, string[] files)
        {
            try
            {
                DirectoryInfo backupDirectory = Directory.CreateDirectory(App.directoryGame + "\\_Backup");
                foreach(string file in files)
                {
                    if(!File.Exists(backupDirectory.FullName + "\\" + file))
                        File.Copy(path + file, backupDirectory.FullName + "\\" + file);
                }
                return true;
            }
            catch(IOException e)
            {
                MessageBox.Show("Não foi possivel fazer o backup, a instalação não pode ser concluida!\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}