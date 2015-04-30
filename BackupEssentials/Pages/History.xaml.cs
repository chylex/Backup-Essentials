using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BackupEssentials.Pages{
    public partial class History : Page, IPageResetUI{
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
            if (entry != null)new BackupReportWindow(entry).Show();
        }

        private void ClickRemove(object sender, RoutedEventArgs e){
            HistoryEntry entry = HistoryListView.SelectedItem as HistoryEntry;

            if (entry != null && MessageBox.Show(App.Window,Sys.Settings.Default.Language["History.Deletion.Confirmation"],Sys.Settings.Default.Language["History.Deletion.Confirmation.Title"],MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes){
                DataStorage.HistoryEntryList.Remove(entry);

                try{
                    File.Delete(Path.Combine(HistoryEntry.Directory,entry.Filename));
                }catch(Exception ex){
                    App.LogException(ex);
                    MessageBox.Show(App.Window,Sys.Settings.Default.Language["History.Deletion.Failure",ex.Message],Sys.Settings.Default.Language["History.Deletion.Failure.Title"],MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }

        private void ClickHistoryEntry(object sender, MouseButtonEventArgs e){
            if (e.ClickCount == 2){
                HistoryEntry entry = HistoryListView.SelectedItem as HistoryEntry;
                if (entry != null)new BackupReportWindow(entry).Show();

                e.Handled = true; // required to not have the main window steal focus
            }
        }

        void IPageResetUI.OnReset(){
            HistoryListView.Items.Refresh();
        }
    }
}
