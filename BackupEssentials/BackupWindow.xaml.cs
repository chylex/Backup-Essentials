﻿using BackupEssentials.Backup;
using BackupEssentials.Backup.History;
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

            Closing += (sender, args) => {
                if (HistoryGenWorker != null){
                    if (MessageBox.Show(App.Window,"History entry is generating","The history entry has not been generated yet, do you want to close the window anyways?",MessageBoxButton.YesNo) == MessageBoxResult.No){
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
            Runner = null;
            ButtonShowReport.IsEnabled = true;
            ButtonEnd.Content = "Close";
            Report = e.Result as BackupReport;

            HistoryGenWorker = HistoryGenerator.FromReport(Report).GenerateAsync((sender2, historyArgs) => {
                HistoryGenWorker = null;

                if (historyArgs.Result == null){
                    App.LogException(historyArgs.Error == null ? new Exception("History generation failed (no stack trace)") : new Exception("History generation failed",historyArgs.Error));
                }
            });

            if (e.Error != null){
                LabelInfo.Content = e.Error.Message;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                Debug.WriteLine(e.Error.ToString());
                return;
            }
            
            ProgressBar.Value = 100;
            ProgressBar.Value = 99; // progress bar animation hack
            ProgressBar.Value = 100;
            TaskbarItemInfo.ProgressValue = 100;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
            LabelInfo.Content = "Finished! Updated "+ActionCount+" files and folders.";

            CloseTimer = new DispatcherTimer();
            CloseTimer.Interval = new TimeSpan(0,0,0,0,250);
            CloseTimer.Tick += (sender2, args2) => { 
                if (TaskbarItemInfo.ProgressValue <= 0){
                    if (HistoryGenWorker == null)Close();
                }
                else TaskbarItemInfo.ProgressValue -= 0.02001D;
            };

            CloseTimer.Start();
        }

        private void ButtonEndClick(object sender, RoutedEventArgs e){
            if (Runner == null){
                CloseTimer.Stop();
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
