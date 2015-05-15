using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class BackupEdit : Page, IPageShowData, IPageDragDrop{
        public BackupLocation EditLocation { get; private set; }
        public BackupLocation TargetLocation { get; private set; }

        private string LastWarningDirectory;
        private string TempDragDirectory;

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
            using(System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()){
                dialog.Description = Sys.Settings.Default.Language["BackupEdit.Button.DirectorySelect.DialogTitle"];
                dialog.ShowDialog();

                string path = dialog.SelectedPath;
                if (path != null)TextBoxDirectory.Text = EditLocation.Directory = path;
            }
        }

        private void ClickSave(object sender, RoutedEventArgs e){
            if (EditLocation.Name.Length == 0){
                VisualStateManager.GoToState(TextBoxName,"Invalid",true);
                MessageBox.Show(App.Window,Sys.Settings.Default.Language["BackupEdit.SaveWarning.NameEmpty"],Sys.Settings.Default.Language["BackupEdit.SaveWarning.Title"],MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            if (!EditLocation.Directory.Equals(LastWarningDirectory)){
                BackupLocation.DirectoryStatus status = EditLocation.GetDirectoryStatus();
                string warning = "";

                if (status == BackupLocation.DirectoryStatus.Empty)warning = "BackupEdit.SaveWarning.DirectoryEmpty";
                else if (status == BackupLocation.DirectoryStatus.Invalid)warning = "BackupEdit.SaveWarning.DirectoryInvalid";
                else if (status == BackupLocation.DirectoryStatus.NotAbsolute)warning = "BackupEdit.SaveWarning.DirectoryNotAbsolute";
                else if (status == BackupLocation.DirectoryStatus.NotExists)warning = "BackupEdit.SaveWarning.DirectoryNotExists";

                if (warning.Length != 0){
                    VisualStateManager.GoToState(TextBoxDirectory,"Invalid",true);
                    LastWarningDirectory = EditLocation.Directory;
                    MessageBox.Show(App.Window,Sys.Settings.Default.Language[warning],Sys.Settings.Default.Language["BackupEdit.SaveWarning.Title"],MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
            }

            TargetLocation.Set(EditLocation);
            DataStorage.BackupLocationListTracker.Changed = true;
            DataStorage.Save();
            ExplorerIntegration.Refresh();
            MainWindow.Instance.ShowPage(typeof(Backup));
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            MainWindow.Instance.ShowPage(typeof(Backup));
        }

        object IPageDragDrop.DragEnter(DragEventArgs e){
            if (e.Data.GetDataPresent(DataFormats.FileDrop)){
                string[] data = e.Data.GetData(DataFormats.FileDrop) as string[];
                TempDragDirectory = TextBoxDirectory.Text;
                TextBoxDirectory.Text = data[0];
                return data;
            }
            else return null;
        }

        void IPageDragDrop.DragExit(DragEventArgs e){
            TextBoxDirectory.Text = TempDragDirectory;
        }

        void IPageDragDrop.DragDrop(DragEventArgs e, object data){
            TempDragDirectory = null;
            TextBoxDirectory.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
