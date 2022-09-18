using LegendaryExplorerCore.Packages;
using LegendaryExplorerCore.TLK.ME1;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using ME2HuffmanCompression = LegendaryExplorerCore.TLK.ME2ME3.HuffmanCompression;

namespace MassEffectInstalador
{
    public partial class MainWindow : Window
    {
        private const int LE1_TLK_COUNT_INSTALLER = 1678;
        private const int LE1_TLK_COUNT = 1006;
        private const int LE2_TLK_COUNT = 60;

        private BackgroundWorker worker;

        private int indexPercentage;
        private string subTitleUpdate;
        private readonly string tlkPathLE1 = Directory.GetCurrentDirectory() + @"\Files\ME1\";
        private readonly string tlkPathLE2 = Directory.GetCurrentDirectory() + @"\Files\ME2\";
        private readonly string installPathLE1 = App.directoryGame + @"\Game\ME1\BioGame\CookedPCConsole\";
        private readonly string installPathLE2 = App.directoryGame + @"\Game\ME2\BioGame\";
        private readonly string[] packagesPathLE1 = Properties.Resources.ListPackageLE1.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        private readonly string[] talksPathLE2 = Properties.Resources.ListTalkLE2.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        private int countFiles;

        public MainWindow()
        {
            InitializeComponent();
            MEPackageHandler.Initialize();
            PackageSaver.Initialize();
            Dir.Maximum = (packagesPathLE1.Length * 2) + (talksPathLE2.Length * 2);
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            worker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Translation;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerCompleted += Finished;
            worker.RunWorkerAsync();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (worker.IsBusy)
            {
                if (MessageBox.Show(this, "A instalação ainda não está concluida, se fechar agora alguns arquivos podem ser corrompidos, tem certeza que deseja sair?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void Translation(object sender, DoWorkEventArgs e)
        {
            CheckFilesToInstall();
            MakeBackup();

            InstallTranslationLE1();
            InstallTranslationLE2();
        }

        private void CheckFilesToInstall()
        {
            if (NumberTlkToInstall(tlkPathLE1) != LE1_TLK_COUNT || NumberTlkToInstall(tlkPathLE2) != LE2_TLK_COUNT)
                throw new InvalidOperationException("Arquivos da tradução não foram encontrados!\n" +
                    "Por favor, baixe a tradução novamente, caso o erro persista desative o ant-virus e tente novamente.");
        }

        private static int NumberTlkToInstall(string path)
        {
            return Directory.Exists(path) ? Directory.GetFiles(path, "*.xml").Length : 0;
        }

        private void MakeBackup()
        {
            try
            {
                DirectoryInfo backupDirLE1 = Directory.CreateDirectory(App.directoryGame + @"\_Backup\ME1\");
                foreach (string pccLE1 in packagesPathLE1)
                {
                    if (!File.Exists(installPathLE1 + pccLE1))
                        return;

                    UpdateProgess("Fazendo backup do Mass Effect 1: ", indexPercentage++);

                    if (!File.Exists(backupDirLE1.FullName + pccLE1))
                        File.Copy(installPathLE1 + pccLE1, backupDirLE1.FullName + pccLE1);
                }
                DirectoryInfo backupDirLE2 = Directory.CreateDirectory(App.directoryGame + @"\_Backup\ME2\");
                foreach (string tlkLE2 in talksPathLE2)
                {
                    string tlkFind = Directory.GetFiles(installPathLE2, tlkLE2, SearchOption.AllDirectories)[0];

                    UpdateProgess("Fazendo backup do Mass Effect 2: ", indexPercentage++);

                    if (!File.Exists(backupDirLE2.FullName + tlkLE2))
                        File.Copy(tlkFind, backupDirLE2.FullName + tlkLE2);
                }
            }
            catch
            {
                throw new InvalidOperationException("Não foi possivel fazer o backup dos arquivos!\n" +
                    "Por favor, execute a tradução como administrador e tente novamente, caso o erro persista repare o game.");
            }
        }

        private void InstallTranslationLE1()
        {
            try
            {
                foreach (string pccPathLE1 in packagesPathLE1)
                {
                    UpdateProgess("Instalando tradução do Mass Effect 1: ", indexPercentage++);
                    using IMEPackage package = MEPackageHandler.OpenLE1Package(installPathLE1 + pccPathLE1);
                    foreach (ME1TalkFile tlkFile in package.LocalTalkFiles)
                    {
                        if (tlkFile.Name is "tlk_M" or "GlobalTlk_tlk_M" or "tlk" or "GlobalTlk_tlk")
                        {
                            string tlkPathFull = tlkPathLE1 + tlkFile.BioTlkSetName + "." + tlkFile.Name + ".xml";
                            HuffmanCompression compressor = new();
                            compressor.LoadInputData(tlkPathFull);
                            compressor.serializeTalkfileToExport(package.GetUExport(tlkFile.UIndex), true);
                            countFiles++;
                        }
                    }
                }
            }
            catch
            {
                throw new InvalidOperationException("Um erro ocorreu ao instalar a tradução do Mass effect 1!\n" +
                    "Por favor, restaure o backup e tente novamente, caso o erro persista entre em contato klooke2018@gmail.com.");
            }
        }

        private void InstallTranslationLE2()
        {
            try
            {
                foreach (string talkPathLE2 in talksPathLE2)
                {
                    UpdateProgess("Instalando tradução do Mass Effect 2: ", indexPercentage++);
                    string tlkFind = Directory.GetFiles(installPathLE2, talkPathLE2, SearchOption.AllDirectories)[0];
                    string tlkPathFull = tlkPathLE2 + talkPathLE2.Replace(".tlk", ".xml");
                    ME2HuffmanCompression compressor = new();
                    compressor.LoadInputData(tlkPathFull);
                    compressor.SaveToFile(tlkFind);
                    countFiles++;
                }
            }
            catch
            {
                throw new InvalidOperationException("Um erro ocorreu ao instalar a tradução do Mass effect 2!\n" +
                    "Por favor, restaure o backup e tente novamente, caso o erro persista entre em contato klooke2018@gmail.com.");
            }
        }

        private void UpdateProgess(string message, int value)
        {
            subTitleUpdate = message;
            worker.ReportProgress(value);
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dir.Value = e.ProgressPercentage;
            string percentage = (int)(e.ProgressPercentage / Dir.Maximum * 100) + "%";
            textTitulo.Text = subTitleUpdate + percentage;
        }

        private void Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Instalação foi cancelada!\nÉ recomendado restaurar o backup.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (e.Error != null)
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (countFiles == (LE1_TLK_COUNT_INSTALLER + LE2_TLK_COUNT))
                    MessageBox.Show(this, "Instalação foi concluida com êxito, aproveite!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(this, "Alguns arquivos não foram instalados, a tradução está incompleta.\n" +
                        "Por favor, reinstale a tradução.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Application.Current.Shutdown();
        }
    }
}