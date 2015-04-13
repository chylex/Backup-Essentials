using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BackupEssentials.Pages{
    public partial class BackupEdit : Page, IPageShowData{
        public BackupLocation EditLocation;
        public BackupLocation TargetLocation;

        private string LastWarningDirectory;

        public BackupEdit(){
            InitializeComponent();
        }

        void IPageShowData.OnShow(object data){
            TargetLocation = (BackupLocation)data;
            EditLocation = TargetLocation.Clone();
            TextBoxName.DataContext = EditLocation;
            TextBoxDirectory.DataContext = EditLocation;

            VisualStateManager.GoToState(TextBoxDirectory,"Unfocused",false);
            TextBoxName.Focus();

            LastWarningDirectory = null;
        }

        private void ClickSelectDirectory(object sender, RoutedEventArgs e){
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the destination folder for this backup location.";
            dialog.ShowDialog();

            string path = dialog.SelectedPath;
            if (path != null)TextBoxDirectory.Text = EditLocation.Directory = path;

            dialog.Dispose();
        }

        private void ClickSave(object sender, RoutedEventArgs e){
            if (EditLocation.Name.Length == 0){
                VisualStateManager.GoToState(TextBoxName,"Invalid",true);
                System.Windows.MessageBox.Show(App.Window,"Location name cannot be empty.","Caution!",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            if (!EditLocation.Directory.Equals(LastWarningDirectory)){
                BackupLocation.DirectoryStatus status = EditLocation.GetDirectoryStatus();
                string warning = "";

                if (status == BackupLocation.DirectoryStatus.Empty)warning = "Selected directory is empty, it will not be registered until the issue is fixed.";
                else if (status == BackupLocation.DirectoryStatus.Invalid)warning = "Selected directory is not a valid Windows path, it will not be registered until the issue is fixed.";
                else if (status == BackupLocation.DirectoryStatus.NotAbsolute)warning = "Selected directory is not an absolute path, it will not be registered until the issue is fixed.";
                else if (status == BackupLocation.DirectoryStatus.NotExists)warning = "Selected directory does not exist, it will be created when a first backup is made.";

                if (warning.Length != 0){
                    VisualStateManager.GoToState(TextBoxDirectory,"Invalid",true);
                    LastWarningDirectory = EditLocation.Directory;
                    System.Windows.MessageBox.Show(App.Window,warning+" Click Save again to confirm.","Caution!",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
            }

            TargetLocation.Set(EditLocation);
            DataStorage.BackupLocationListChanged = true;
            ExplorerIntegration.Refresh();
            MainWindow.Instance.ShowPage(typeof(Backup));
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            MainWindow.Instance.ShowPage(typeof(Backup));
        }

        private void UpdateAdvancedOptionsVisibility(object sender, RoutedEventArgs e){
            AdvancedOptionsContainer.Visibility = CheckBoxAdvancedOptions.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
