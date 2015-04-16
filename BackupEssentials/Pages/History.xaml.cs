using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class History : Page{
        public History(){
            InitializeComponent();

            HistoryListView.Items.Clear();
            HistoryListView.ItemsSource = DataStorage.HistoryEntryList;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e){
            ButtonShowReport.IsEnabled = ButtonRemove.IsEnabled = HistoryListView.SelectedItems.Count == 1;
        }

        private void ClickShowReport(object sender, RoutedEventArgs e){
            HistoryEntry entry = HistoryListView.SelectedItem as HistoryEntry;
            if (entry == null)return;

            BackupReportWindow reportWindow = new BackupReportWindow(entry);
            reportWindow.Show();
        }

        private void ClickRemove(object sender, RoutedEventArgs e){

        }
    }
}
