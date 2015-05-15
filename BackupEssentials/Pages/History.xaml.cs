using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System;
using System.Collections;
using System.Collections.Generic;
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
            ButtonShowReport.IsEnabled = HistoryListView.SelectedItems.Count == 1;
            ButtonRemove.IsEnabled = HistoryListView.SelectedItems.Count >= 1;
        }

        private void ClickShowReport(object sender, RoutedEventArgs e){
            HistoryEntry entry = HistoryListView.SelectedItem as HistoryEntry;
            if (entry != null)new BackupReportWindow(entry).Show();
        }

        private void ClickRemove(object sender, RoutedEventArgs e){
            List<HistoryEntry> list = new List<HistoryEntry>(HistoryListView.SelectedItems.Count);
            foreach(HistoryEntry entry in HistoryListView.SelectedItems)list.Add(entry);

            if (list.Count > 0 && MessageBox.Show(App.Window,Sys.Settings.Default.Language["History.Deletion.Confirmation.",list.Count,list.Count.ToString()],Sys.Settings.Default.Language["History.Deletion.Confirmation.Title"],MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes){
                int index = HistoryListView.SelectedIndex;

                foreach(HistoryEntry entry in list){
                    try{
                        string path = Path.Combine(HistoryEntry.Directory,entry.Filename);
                        if (File.Exists(path))File.Delete(path);

                        DataStorage.HistoryEntryList.Remove(entry);
                    }catch(Exception ex){
                        App.LogException(ex);

                        MessageBoxResult res = MessageBox.Show(App.Window,Sys.Settings.Default.Language["History.Deletion.Failure.Line1",ex.Message]+Environment.NewLine+Sys.Settings.Default.Language["History.Deletion.Failure.Line2",ex.Message],Sys.Settings.Default.Language["History.Deletion.Failure.Title"],MessageBoxButton.YesNoCancel,MessageBoxImage.Error);
                        if (res == MessageBoxResult.Cancel)break;
                        else if (res == MessageBoxResult.Yes)DataStorage.HistoryEntryList.Remove(entry);
                    }
                }

                if (index > 0)HistoryListView.SelectedIndex = index-1;
                else if (HistoryListView.Items.Count > 0)HistoryListView.SelectedIndex = index;
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
