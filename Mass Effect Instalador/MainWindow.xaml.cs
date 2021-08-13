using System;
using System.ComponentModel;
using System.Windows;

namespace Mass_Effect_Instalador
{
    public partial class MainWindow : Window
    {
        private bool isBusy = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (isBusy)
            {
                if (MessageBox.Show(this, "A instalação ainda não está concluida, se fechar agora todos os arquivos podem ser corrompidos, tem certeza que deseja sair?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
