﻿using BackupEssentials.Data;
using BackupEssentials.Sys.UI;
using BackupEssentials.Utils;
using BackupEssentials.Utils.Collections;
using BackupEssentials.Utils.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace BackupEssentials.Sys{
    public class Settings : INotifyPropertyChanged{
        private static readonly Settings defaultInstance = new Settings("Settings.dat");
        public static Settings Default { get { return defaultInstance; } }

        private readonly ObservableDictionary<string,object> Data = new ObservableDictionary<string,object>();
        private readonly string Filename;

        public event PropertyChangedEventHandler PropertyChanged;
        public IEnumerable<string> Properties { get { return Data.Keys; } }

        public Settings(string filename){
            this.Filename = filename;
            Data.PropertyChanged += (sender, args) => { if (PropertyChanged != null)PropertyChanged(sender,args); };
            SetToDefault();
        }

        // Default values

        public void SetToDefault(){
            Data.PauseObservation = true;

            Language = SettingsData.LanguageList.FindObj(var => true);
            ExplorerIntegration = true;
            ExplorerLabel = "Backup Essentials";
            DateFormat = SettingsData.DateFormatList.FindObj(var => true);
            WindowCloseTime = SettingsData.WindowCloseList.FindObj(var => var.Value == 10);
            HistoryEntriesKept = SettingsData.HistoryKeptList.FindObj(var => var.Value == 500);
            SaveHistoryWithNoEntries = false;
            
            Data.PauseObservation = false;
        }

        // Serialization

        public void Load(){
            Data.PauseObservation = true;

            FileUtils.ReadFile(Filename,FileMode.Open,(line) => {
                string key = line.Substring(0,2), data = line.Substring(2);

                switch(key){
                    case "LG": Language = SettingsData.LanguageList.FindObj(var => var.File.Equals(data)); break;
                    case "EX": ExplorerIntegration = data.Equals("1"); break;
                    case "EL": ExplorerLabel = data; break;
                    case "DF": DateFormat = SettingsData.DateFormatList.FindObj(var => var.Format.Equals(data),var => true); break;
                    case "CT": int valueCT = NumberSerialization.ReadInt(data); WindowCloseTime = SettingsData.WindowCloseList.FindObj(var => var.Value == valueCT,var => var.Value == 10); break;
                    case "HE": int valueHE = NumberSerialization.ReadInt(data); HistoryEntriesKept = SettingsData.HistoryKeptList.FindObj(var => var.Value == valueHE,var => var.Value == 500); break;
                    case "H0": SaveHistoryWithNoEntries = data.Equals("1"); break;
                }
            });
            
            Data.PauseObservation = false;
        }

        public void Save(){
            FileUtils.WriteFile(Filename,FileMode.Create,(writer) => {
                writer.Write("LG"); writer.WriteLine(Language.File);
                writer.Write("EX"); writer.WriteLine(ExplorerIntegration ? "1" : "0");
                writer.Write("EL"); writer.WriteLine(ExplorerLabel);
                writer.Write("DF"); writer.WriteLine(DateFormat.Format);
                writer.Write("CT"); writer.WriteLine(NumberSerialization.WriteInt(WindowCloseTime.Value));
                writer.Write("HE"); writer.WriteLine(NumberSerialization.WriteInt(HistoryEntriesKept.Value));
                writer.Write("H0"); writer.WriteLine(SaveHistoryWithNoEntries ? "1" : "0");
            });
        }

        // Utility methods

        public void Reload(){
            SetToDefault();
            Load();
        }

        // List of settings

        public Language Language {
            get { return (Language)Data["Language"]; }
            set { Data["Language"] = (Language)value; }
        }

        public bool ExplorerIntegration {
            get { return (bool)Data["ExplorerIntegration"]; }
            set { Data["ExplorerIntegration"] = (bool)value; }
        }

        public string ExplorerLabel {
            get { return (string)Data["ExplorerLabel"]; }
            set { Data["ExplorerLabel"] = (string)value; }
        }

        public DateFormat DateFormat {
            get { return (DateFormat)Data["DateFormat"]; }
            set { Data["DateFormat"] = (DateFormat)value; }
        }

        public DisplaySetting<int> WindowCloseTime {
            get { return (DisplaySetting<int>)Data["WindowCloseTime"]; }
            set { Data["WindowCloseTime"] = (DisplaySetting<int>)value; }
        }

        public DisplaySetting<int> HistoryEntriesKept {
            get { return (DisplaySetting<int>)Data["HistoryEntriesKept"]; }
            set { Data["HistoryEntriesKept"] = (DisplaySetting<int>)value; }
        }

        public bool SaveHistoryWithNoEntries {
            get { return (bool)Data["SaveHistoryWithNoEntries"]; }
            set { Data["SaveHistoryWithNoEntries"] = (bool)value; }
        }
    }
}
