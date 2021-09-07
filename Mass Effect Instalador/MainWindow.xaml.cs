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
        private const int LE1_TLK_COUNT_INSTALLER = 1678;
        private const int LE1_TLK_COUNT = 1006;
        private const int LE2_TLK_COUNT = 60;

        private bool isBusy;
        private readonly string tlkPathLE1 = Directory.GetCurrentDirectory() + @"\Files\ME1\";
        private readonly string tlkPathLE2 = Directory.GetCurrentDirectory() + @"\Files\ME2\";
        private readonly string installPathLE1 = App.directoryGame + @"\Game\ME1\BioGame\CookedPCConsole\";
        private readonly string installPathLE2 = App.directoryGame + @"\Game\ME2\BioGame\";
        private readonly string[] packagesPathLE1 = Properties.Resources.ListPackageLE1.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        private readonly string[] talksPathLE2 = Properties.Resources.ListTalkLE2.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        private bool isInstalled;
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
            //Preparação
            if(!CheckFilesToInstall()) return;
            if(!CheckFilesToReplace()) return;
            if(!MakeBackup()) return;

            //Instalação da tradução do Mass Effect 1
            for(int i = 0; i < packagesPathLE1.Length; i++)
            {
                isBusy = true;
                using IMEPackage package = MEPackageHandler.OpenLE1Package(installPathLE1 + packagesPathLE1[i]);
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
            isInstalled = true;
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
            if(isInstalled)
            {
                if(countFiles == LE1_TLK_COUNT_INSTALLER)
                    MessageBox.Show(this, "Instalação foi concluida com êxito, aproveite!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(this, "Alguns arquivos não foram encontrados, por isso a tradução pode está incompleta.\n" +
                        "Por favor reinstale a tradução.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
                MessageBox.Show("Não foi possivel concluir a instalação, arquivos não encontrados!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
        private bool CheckFilesToInstall()
        {
            if(NumberTlkToInstall(tlkPathLE1) != LE1_TLK_COUNT || NumberTlkToInstall(tlkPathLE2) != LE2_TLK_COUNT)
                return false;

            return true;
        }
        private static int NumberTlkToInstall(string path)
        {
            return Directory.Exists(path) ? Directory.GetFiles(path, "*.xml").Length : 0;
        }
        private bool CheckFilesToReplace()
        {
            if(!PackagesExist() || !TalksExist())
                return false;

            return true;
        }
        private bool PackagesExist()
        {
            if(!Directory.Exists(installPathLE1))
                return false;

            foreach(string pccLE1 in packagesPathLE1)
            {
                if (!File.Exists(installPathLE1 + pccLE1))
                    return false;
            }

            return true;
        }
        private bool TalksExist()
        {
            if(!Directory.Exists(installPathLE2))
                return false;

            foreach(string tlkLE2 in talksPathLE2)
            {
                if(Directory.GetFiles(installPathLE2, tlkLE2, SearchOption.AllDirectories).Length == 0)
                    return false;
            }

            return true;
        }
        private bool MakeBackup()
        {
            try
            {
                DirectoryInfo backupDirLE1 = Directory.CreateDirectory(App.directoryGame + @"\_Backup\ME1\");
                foreach(string pccLE1 in packagesPathLE1)
                {
                    if(!File.Exists(backupDirLE1.FullName + pccLE1))
                        File.Copy(installPathLE1 + pccLE1, backupDirLE1.FullName + pccLE1);
                    foreach (ME1TalkFile tlkFile in package.LocalTalkFiles)
                }

                DirectoryInfo backupDirLE2 = Directory.CreateDirectory(App.directoryGame + @"\_Backup\ME2\");
                foreach (string tlkLE2 in talksPathLE2)
                {
                    string[] tlkFind = Directory.GetFiles(installPathLE2, tlkLE2, SearchOption.AllDirectories);
                    if (!File.Exists(backupDirLE2.FullName + tlkLE2))
                        File.Copy(tlkFind[0], backupDirLE2.FullName + tlkLE2);
                }
                return true;
            }
            catch
            {
                MessageBox.Show("Não foi possivel fazer o backup, a instalação não pode ser concluida!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}