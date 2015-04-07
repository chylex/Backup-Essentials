using BackupEssentials.Backup;
using System.Windows;
using System.Windows.Input;

namespace BackupEssentials{
    public partial class BackupReportWindow : Window{
        public BackupReportWindow(BackupReport report){
            InitializeComponent();
            ReportTextBlock.Text = report == null || report.Report == null ? "Error fetching report." : report.Report;
        }

        private void TitleBarLeftButtonDown(object sender, MouseButtonEventArgs e){
            DragMove();
        }

        private void ButtonWindowCloseClick(object sender, RoutedEventArgs e){
            Close();
        }
    }
}
