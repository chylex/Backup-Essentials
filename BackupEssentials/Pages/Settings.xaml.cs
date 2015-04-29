using BackupEssentials.Backup.Data;
using BackupEssentials.Backup.History;
using System.Collections.Generic;
using System.ComponentModel;
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

            AppSettings.PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
        }

        void IPageShowData.OnShow(object data){
            Changed = false;
            foreach(string key in new List<string>(PropertiesChanged.Keys))PropertiesChanged[key] = false;
        }

        bool IPageSwitchHandler.OnSwitch(){
            if (Changed){
                MessageBoxResult result = MessageBox.Show(App.Window,AppSettings.Language["Page.Settings.Message.ChangedWarning"],AppSettings.Language["Page.Settings.Message.ChangedWarning.Title"],MessageBoxButton.YesNoCancel,MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)return true;
                else if (result == MessageBoxResult.Yes)SaveAndUpdate();
                else ClickCancel(this,new RoutedEventArgs());
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
            AppPageManager.ResetUI();
        }

        private void ClickReset(object sender, RoutedEventArgs e){
            if (MessageBox.Show(App.Window,AppSettings.Language["Page.Settings.Message.ResetWarning"],AppSettings.Language["Page.Settings.Message.ResetWarning.Title"],MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes){
                AppSettings.SetToDefault();
                SaveAndUpdate();
                Changed = false;
                UpdateButtons();
                AppPageManager.ResetUI();
            }
        }

        private void UpdateButtons(){
            ButtonSave.IsEnabled = ButtonCancel.IsEnabled = Changed;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args){
            Changed = true;
            PropertiesChanged[args.PropertyName] = true;
            UpdateButtons();

            if (PropertiesChanged["Language"]){
                AppPageManager.ResetUI();
            }
        }

        private void SaveAndUpdate(){
            AppSettings.Save();

            if (PropertiesChanged["ExplorerIntegration"]){
                if (AppSettings.ExplorerIntegration)ExplorerIntegration.Refresh(true);
                else ExplorerIntegration.Remove();
            }

            if (PropertiesChanged["ExplorerLabel"]){
                ExplorerIntegration.Refresh(true);
            }

            if (PropertiesChanged["Language"] || PropertiesChanged["DateFormat"]){
                AppPageManager.ResetUI();
            }

            if (PropertiesChanged["HistoryEntriesKept"]){
                HistoryUtils.TryRemoveOldEntries();
            }

            foreach(string key in new List<string>(PropertiesChanged.Keys))PropertiesChanged[key] = false;
        }

        private void HistoryEntriesKeptChanged(object sender, SelectionChangedEventArgs e){
            int kept = AppSettings.HistoryEntriesKept.Value, existing = DataStorage.HistoryEntryList.Count;

            if (kept != -1 && kept < existing){
                MessageBox.Show(App.Window,AppSettings.Language["Page.Settings.Message.HistoryWarning.PartOne.",existing,existing.ToString()]+AppSettings.Language["Page.Settings.Message.HistoryWarning.PartTwo.",existing-kept,(existing-kept).ToString()],AppSettings.Language["Page.Settings.Message.HistoryWarning.Title"],MessageBoxButton.OK,MessageBoxImage.Exclamation);
            }
        }
    }
}
