using System.Windows;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class App : Application{
        private void StartApp(object sender, StartupEventArgs args){
            MainWindow window = new MainWindow();
            // TODO handle args
            window.Show();
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e){
            // TODO
        }
    }
}
