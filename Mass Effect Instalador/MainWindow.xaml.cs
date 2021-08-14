using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Mass_Effect_Instalador
{
    public partial class MainWindow : Window
    {
        const int LE1_FILES_TLK = 1678;

        private bool isBusy;
        private bool checkFiles;
        private int countFiles;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(Object sender, EventArgs e)
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
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(10000);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string directoryGame = App.cmd;
            string directoryInstall = directoryGame + "\\Game\\ME1\\BioGame\\CookedPCConsole\\";
            //MessageBox.Show(directoryGame);
            //MessageBox.Show(directoryInstall);
            if (File.Exists(Directory.GetCurrentDirectory() + "\\list.txt"))
            {
                checkFiles = true;
                countFiles = 0;
                string[] filesList = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\list.txt");

                // Verificar se todos os arquivos existem e fazer backup
                for (int i = 0; i < filesList.Length; i++)
                {
                    if (!File.Exists(directoryInstall + filesList[i]))
                    {
                        checkFiles = false;
                        break;
                    }
                    else
                    {
                        DirectoryInfo infoNewDir = Directory.CreateDirectory(directoryGame + "\\_Backup");
                        File.Copy(directoryInstall + filesList[i], infoNewDir.FullName + "\\" + filesList[i], true);
                    }
                }

                // Verificar os arquivos da tradução
                for (int i = 0; i < filesList.Length; i++)
                {
                    if (Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Files\\").Length != 1006)
                    {
                        checkFiles = false;
                    }
                }
            }
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dir.Value = e.ProgressPercentage;
            string s = (int)(e.ProgressPercentage / Dir.Maximum * 100) + "%";
            Title = "Extração: " + s;
            textTitulo.Text = "Extraindo... " + s;
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isBusy = false;
            if (checkFiles)
            {
                if (countFiles == LE1_FILES_TLK)
                {
                    MessageBox.Show(this, "Instalação foi concluida com êxito, aproveite!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, "Alguns arquivos não foram encontrados por isso a tradução pode está incompleta. Por favor desinstale a tradução, repare o jogo e tente novamente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(this, "Não foi possivel concluir a instalação!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Application.Current.Shutdown();
        }
    }
}
