using BackupEssentials.Backup;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BackupEssentials.Pages{
    public partial class BackupLocationsEdit : Page, IPageShowData{
        public BackupLocation EditLocation;
        public BackupLocation TargetLocation;

        public BackupLocationsEdit(){
            InitializeComponent();
        }

        void IPageShowData.OnShow(object data){
            TargetLocation = (BackupLocation)data;
            EditLocation = TargetLocation.Clone();
            TextBoxName.DataContext = EditLocation;
            TextBoxDirectory.DataContext = EditLocation;
        }

        private void ClickSelectDirectory(object sender, RoutedEventArgs e){
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the destination folder for this backup location.";
            dialog.ShowDialog();

            string path = dialog.SelectedPath;
            if (path != null)TextBoxDirectory.Text = EditLocation.Directory = path;
        }

        private void ClickSave(object sender, RoutedEventArgs e){
            TargetLocation.Set(EditLocation);
            ExplorerIntegration.Refresh();
            MainWindow.Instance.ShowPage(typeof(BackupLocations));
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            MainWindow.Instance.ShowPage(typeof(BackupLocations));
        }

        private void UpdateAdvancedOptionsVisibility(object sender, RoutedEventArgs e){
            AdvancedOptionsContainer.Visibility = CheckBoxAdvancedOptions.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
