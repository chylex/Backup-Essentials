using BackupEssentials.Backup.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class Settings : Page, IPageShowData, IPageSwitchHandler{
        private Sys.Settings AppSettings { get { return Sys.Settings.Default; } }

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
                else AppSettings.Load();
            }

            return false;
        }

        private void ClickSave(object sender, RoutedEventArgs e){
            SaveAndUpdate();
            Changed = false;
            UpdateButtons();
        }

        private void ClickCancel(object sender, RoutedEventArgs e){
            AppSettings.Load();
            Changed = false;
            UpdateButtons();
        }

        private void ClickReset(object sender, RoutedEventArgs e){
            if (MessageBox.Show(App.Window,"Are you sure? This action cannot be taken back!","Reset settings",MessageBoxButton.YesNo) == MessageBoxResult.Yes){
                AppSettings.SetToDefault();
                Changed = false;
                UpdateButtons();
            }
        }

        private void UpdateButtons(){
            ButtonSave.IsEnabled = ButtonCancel.IsEnabled = Changed;
        }

        private void SaveAndUpdate(){
            AppSettings.Save();

            if (PropertiesChanged["IntegrateWindowsExplorer"]){
                if (AppSettings.ExplorerIntegration)ExplorerIntegration.Refresh(true);
                else ExplorerIntegration.Remove();
            }

            foreach(string key in new List<string>(PropertiesChanged.Keys))PropertiesChanged[key] = false;
        }
    }
}
