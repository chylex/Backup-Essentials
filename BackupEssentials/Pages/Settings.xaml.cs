using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class Settings : Page, IPageShowData, IPageSwitchHandler{
        private static Sys.Settings AppSettings { get { return Sys.Settings.Default; } }

        private bool Changed = false;
        private Dictionary<string,bool> PropertiesChanged = new Dictionary<string,bool>(); // use dictionary in order to get an exception if a key is wrong

        public Settings(){
            InitializeComponent();

            foreach(string prop in AppSettings.Properties){
                PropertiesChanged.Add(prop,false);
            }

            AppSettings.PropertyChanged += (sender, args) => {
                Changed = true;
                PropertiesChanged[args.PropertyName] = true;
                UpdateButtons();
            };
        }

        void IPageShowData.OnShow(object data){
            Changed = false;
            foreach(string key in new List<string>(PropertiesChanged.Keys))PropertiesChanged[key] = false;
        }

        bool IPageSwitchHandler.OnSwitch(){
            if (Changed){
                MessageBoxResult result = MessageBox.Show(App.Window,"You have changed the settings, do you want to save them?","Changed settings",MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Cancel)return true;
                else if (result == MessageBoxResult.Yes)SaveAndUpdate();
                else AppSettings.Reload();
            }

            return false;
        }

        private void ClickSave(object sender, RoutedEventArgs e){
            SaveAndUpdate();
            Changed = false;
            UpdateButtons();
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            AppSettings.Reload();
            Changed = false;
            UpdateButtons();
            UpdateUI();
        }

        private void ClickReset(object sender, RoutedEventArgs e){
            if (MessageBox.Show(App.Window,"Are you sure? This action cannot be taken back!","Reset settings",MessageBoxButton.YesNo) == MessageBoxResult.Yes){
                AppSettings.SetToDefault();
                SaveAndUpdate();
                Changed = false;
                UpdateButtons();
                UpdateUI();
            }
        }

        private void UpdateButtons(){
            ButtonSave.IsEnabled = ButtonCancel.IsEnabled = Changed;
        }

        private void UpdateUI(){
            object prevContext = GridContainer.DataContext;
            GridContainer.DataContext = null;
            GridContainer.DataContext = prevContext;
        }

        private void SaveAndUpdate(){
            AppSettings.Save();
            AppPageManager.ResetCache();

            if (PropertiesChanged["ExplorerIntegration"]){
                if (AppSettings.ExplorerIntegration)ExplorerIntegration.Refresh(true);
                else ExplorerIntegration.Remove();
            }

            if (PropertiesChanged["ExplorerLabel"]){
                ExplorerIntegration.Refresh(true);
            }

            if (PropertiesChanged["HistoryEntriesKept"]){
                HistoryUtils.TryRemoveOldEntries();
            }

            foreach(string key in new List<string>(PropertiesChanged.Keys))PropertiesChanged[key] = false;
        }

        private void HistoryEntriesKeptChanged(object sender, SelectionChangedEventArgs e){
            int kept = AppSettings.HistoryEntriesKept.Value, existing = DataStorage.HistoryEntryList.Count;
            if (kept != -1 && kept < existing)MessageBox.Show(App.Window,"There are currently "+existing+" history entries, saving the settings will delete last "+(existing-kept)+" entries.","Caution!",MessageBoxButton.OK,MessageBoxImage.Exclamation);
        }
    }
}
