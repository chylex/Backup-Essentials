using BackupEssentials.Backup;
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
        private bool Running;

        public BackupDrop(){
            InitializeComponent();

            LocationsListView.Items.Clear();
            LocationsListView.ItemsSource = DataStorage.BackupLocationList;
        }

        void IPageShowData.OnShow(object data){
            FileList = (string[])((Object[])data)[0];
            PrevPageType = (Type)((Object[])data)[1];
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e){
            ButtonBackup.IsEnabled = LocationsListView.SelectedItems.Count == 1;
        }

        private void ClickBackup(object sender, RoutedEventArgs e){
            if (FileList.Length == 0 || Running)return;

            Process newProcess = new Process();
            newProcess.StartInfo.Arguments = "-runshell -locid "+LocationsListView.SelectedIndex+" -src \""+string.Join("\" \"",FileList)+"\"";
            newProcess.StartInfo.FileName = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);
            newProcess.EnableRaisingEvents = true;
            newProcess.Start();

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
