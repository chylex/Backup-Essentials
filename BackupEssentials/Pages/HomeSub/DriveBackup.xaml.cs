using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages.HomeSub{
    public partial class DriveBackup : Page{
        public DriveBackup(){
            InitializeComponent();
        }

        private void OpenMyComputer(object sender, RoutedEventArgs e){
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }
    }
}
