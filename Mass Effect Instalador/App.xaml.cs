using System.Windows;

namespace MassEffectInstalador
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string directoryGame = string.Empty;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new();
            directoryGame = e.Args[0];
            wnd.Show();
        }
    }
}
