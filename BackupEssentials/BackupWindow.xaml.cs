using BackupEssentials.Backup;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;

namespace BackupEssentials{
    public partial class BackupWindow : Window{
        private BackupRunner Runner;
        private int ActionCount;

        public BackupWindow(BackupRunner runner){
            InitializeComponent();

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            ProgressBar.IsIndeterminate = true;
            LabelInfo.Content = "Preparing backup...";

            runner.EventProgressUpdate = WorkerProgressUpdate;
            runner.EventCompleted = WorkerCompleted;
            runner.Start();

            this.Runner = runner;
        }

        private void WorkerProgressUpdate(object sender, ProgressChangedEventArgs e){
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = e.ProgressPercentage/100D;

            if (e.ProgressPercentage > 0){
                ProgressBar.Value = e.ProgressPercentage;
                ProgressBar.Value = e.ProgressPercentage-1; // progress bar animation hack
                ProgressBar.Value = e.ProgressPercentage;
            }

            LabelInfo.Content = "Processing the files and folders...";

            if (e.ProgressPercentage == 0 && e.UserState is int){
                ActionCount = (int)e.UserState;
                ProgressBar.IsIndeterminate = false;
            }
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e){
            Runner = null;
            ButtonEnd.Content = "Close";

            if (e.Error != null){
                LabelInfo.Content = e.Error.Message;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                Debug.WriteLine(e.Error.ToString());
                return;
            }
            
            ProgressBar.Value = 100;
            ProgressBar.Value = 99; // progress bar animation hack
            ProgressBar.Value = 100;
            LabelInfo.Content = "Finished! Updated "+ActionCount+" files and folders.";
        }

        private void ButtonEndClick(object sender, RoutedEventArgs e){
            if (Runner == null)Close();
            else Runner.Cancel();
        }
    }
}
