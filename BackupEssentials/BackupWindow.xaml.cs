using BackupEssentials.Backup;
using BackupEssentials.Backup.History;
using BackupEssentials.Sys;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class BackupWindow : Window{
        private BackupRunner Runner;
        private int ActionCount;
        private BackupReport Report;
        private BackupReportWindow ReportWindow;
        private DispatcherTimer CloseTimer;
        private BackgroundWorker HistoryGenWorker;

        public BackupWindow(BackupRunner runner){
            InitializeComponent();

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            ProgressBar.IsIndeterminate = true;
            LabelInfo.Content = "Preparing backup...";

            runner.EventProgressUpdate = WorkerProgressUpdate;
            runner.EventCompleted = WorkerCompleted;
            runner.Start();

            this.Runner = runner;

            Loaded += (sender, args) => {
                // workaround WindowStartupLocation and WindowState.Minimized conflicting
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Math.Round(Left+Width/2),(int)Math.Round(Top+Height/2)));
                Left = screen.WorkingArea.X+screen.WorkingArea.Width/2-Width/2;
                Top = screen.WorkingArea.Y+screen.WorkingArea.Height/2-Height/2;
            };

            Closing += (sender, args) => {
                if (HistoryGenWorker != null){
                    if (MessageBox.Show(App.Window,"The history entry has not been generated yet, do you want to close the window anyways?","History entry is generating",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.No){
                        args.Cancel = true;
                    }
                }
            };
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
            ButtonShowReport.IsEnabled = true;
            ButtonEnd.Content = "Close";
            Report = e.Result as BackupReport;

            Settings settings = Settings.Default;

            if ((settings.HistoryEntriesKept.Value > 0 || settings.HistoryEntriesKept.Value == -1) && (!(Report.TryFindValue(BackupReport.Constants.EntriesAdded,0) == 0 && Report.TryFindValue(BackupReport.Constants.EntriesUpdated,0) == 0 && Report.TryFindValue(BackupReport.Constants.EntriesDeleted,0) == 0) || settings.SaveHistoryWithNoEntries)){
                HistoryGenWorker = HistoryGenerator.FromReport(Runner.RunInfo,Report).GenerateAsync((sender2, historyArgs) => {
                    HistoryGenWorker = null;

                    if (historyArgs.Result == null){
                        App.LogException(historyArgs.Error == null ? new Exception("History generation failed (no stack trace)") : new Exception("History generation failed",historyArgs.Error));
                    }
                });
            }

            Runner = null;
            TaskbarItemInfo.ProgressValue = 100;

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
            
            int closeTime = Settings.Default.WindowCloseTime.Value;
            if (closeTime == -1)return;

            CloseTimer = new DispatcherTimer();
            CloseTimer.Interval = new TimeSpan(0,0,0,0,100);

            if (closeTime == 0){
                CloseTimer.Tick += (sender2, args2) => {
                    if (HistoryGenWorker == null)Close();
                };
            }
            else{
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;

                CloseTimer.Tick += (sender2, args2) => { 
                    if (TaskbarItemInfo.ProgressValue <= 0){
                        if (HistoryGenWorker == null)Close();
                    }
                    else TaskbarItemInfo.ProgressValue -= 0.00001D+(0.1D/Settings.Default.WindowCloseTime.Value);
                };
            }

            CloseTimer.Start();
        }

        private void ButtonEndClick(object sender, RoutedEventArgs e){
            if (Runner == null){
                if (CloseTimer != null)CloseTimer.Stop();
                Close();
            }
            else Runner.Cancel();
        }

        private void ButtonShowReportClick(object sender, RoutedEventArgs e){
            if (Runner == null){
                if (CloseTimer != null){
                    CloseTimer.Stop();
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                }

                if (ReportWindow != null)ReportWindow.Close();

                ReportWindow = new BackupReportWindow(Report);
                ReportWindow.Closed += (sender2, args2) => { ReportWindow = null; };
                ReportWindow.Show();
            }
        }
    }
}
