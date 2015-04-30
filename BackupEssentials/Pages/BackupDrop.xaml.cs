using BackupEssentials.Backup.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class BackupDrop : Page, IPageShowData{
        private Type PrevPageType;
        private string[] FileList;
        private bool CompatMode;
        private bool Running;

        public BackupDrop(){
            InitializeComponent();

            LocationsListView.Items.Clear();
            LocationsListView.ItemsSource = DataStorage.BackupLocationList;
        }

        void IPageShowData.OnShow(object data){
            Object[] array = (Object[])data;
            FileList = (string[])array[0];
            PrevPageType = (Type)array[1];
            CompatMode = array.Length >= 3 && (bool)array[2];

            if (CompatMode)ButtonCancel.Visibility = Visibility.Collapsed;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e){
            ButtonBackup.IsEnabled = LocationsListView.SelectedItems.Count == 1;
        }

        private void ClickBackup(object sender, RoutedEventArgs e){
            if (FileList.Length == 0 || Running)return;

            if (FileList.Length == 1 && FileList[0].EndsWith(@":\",StringComparison.Ordinal)){
                if (MessageBox.Show(App.Window,Sys.Settings.Default.Language["BackupDrop.DriveBackupWarning",Path.GetFileName(DataStorage.BackupLocationList[LocationsListView.SelectedIndex].Directory)],Sys.Settings.Default.Language["BackupDrop.DriveBackupWarning.Title"],MessageBoxButton.YesNo,MessageBoxImage.Asterisk) != MessageBoxResult.Yes)return;
            }

            for(int a = 0; a < FileList.Length; a++){
                if (FileList[a][FileList[a].Length-1] == '\\')FileList[a] += '\\';
            }

            Process newProcess = new Process();
            newProcess.StartInfo.Arguments = "-runshell -nohide -locid "+LocationsListView.SelectedIndex+" -src \""+string.Join("\" \"",FileList)+"\"";
            newProcess.StartInfo.FileName = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);
            newProcess.EnableRaisingEvents = true;
            newProcess.Start();

            if (CompatMode){
                MainWindow.Instance.Close();
                return;
            }

            newProcess.Exited += (sender2, args2) => {
                if (newProcess.ExitCode == 0){
                    Dispatcher.Invoke(new Action(() => {
                        MainWindow.Instance.ShowPage(PrevPageType,MainWindow.IgnoreShowData);
                    }));
                }

                Running = false;
            };

            Running = true;
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            MainWindow.Instance.ShowPage(PrevPageType,MainWindow.IgnoreShowData);
        }
    }
}
