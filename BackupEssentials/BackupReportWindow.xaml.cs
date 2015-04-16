using BackupEssentials.Backup;
using BackupEssentials.Backup.History;
using BackupEssentials.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class BackupReportWindow : Window{
        public BackupReportWindow(BackupReport report){
            InitializeComponent();
            ReportTextBlock.Text = report == null || report.Report == null ? "Error fetching report." : report.Report;
        }

        public BackupReportWindow(HistoryEntry entry){
            InitializeComponent();
            ReportTextBlock.Text = "Fetching report...";

            Thread thread = new Thread(new ParameterizedThreadStart(LoadReportFileAsync));
            thread.Start(entry);
        }

        private void LoadReportFileAsync(object entry){
            BackupReport finalReport = null;
            string data = FileUtils.ReadFileCompressed(Path.Combine(HistoryEntry.Directory,((HistoryEntry)entry).Filename),FileMode.Open);
            
            if (data == null)data = "Failed fetching report.";
            else{
                finalReport = new BackupReport(data);
                string _unused = finalReport.Report; // init and cache
            }

            Dispatcher.BeginInvoke(new Action<object>((d) => {
                BackupReport report = d as BackupReport;
                ReportTextBlock.Text = report != null ? report.Report : d as string;
            }),DispatcherPriority.Background,finalReport == null ? (object)data : (object)finalReport);
        }

        private void TitleBarLeftButtonDown(object sender, MouseButtonEventArgs e){
            DragMove();
        }

        private void ButtonWindowCloseClick(object sender, RoutedEventArgs e){
            Close();
        }
    }
}
