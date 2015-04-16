using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System;
using System.IO;
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
            HistoryEntry entry = HistoryListView.SelectedItem as HistoryEntry;

            if (entry != null && MessageBox.Show(App.Window,"Are you sure you want to delete the history entry? This action cannot be taken back.","Confirm deletion",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes){
                DataStorage.HistoryEntryList.Remove(entry);

                try{
                    File.Delete(Path.Combine(HistoryEntry.Directory,entry.Filename));
                }catch(Exception ex){
                    App.LogException(ex);
                    MessageBox.Show(App.Window,"Failed deleting the entry file: "+ex.Message,"Error deleting history entry",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }
    }
}
