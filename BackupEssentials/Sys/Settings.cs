﻿using BackupEssentials.Sys.UI;
using BackupEssentials.Utils;
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

            ExplorerIntegration = true;
            DateFormat = SettingsData.DateFormatList[0];
            WindowCloseTime = SettingsData.WindowCloseList[2];
            SaveHistoryWithNoEntries = false;
            
            Data.PauseObservation = false;
        }

        // Serialization

        public void Load(){
            Data.PauseObservation = true;

            FileUtils.ReadFile(Filename,FileMode.Open,(line) => {
                string key = line.Substring(0,2), data = line.Substring(2);

                switch(key){
                    case "EX": ExplorerIntegration = data.Equals("1"); break;
                    case "DF": DateFormat = SettingsData.FindDateFormat(data); break;
                    case "CT": WindowCloseTime = SettingsData.FindWindowCloseTime(NumberSerialization.ReadInt(data)); break;
                    case "H0": SaveHistoryWithNoEntries = data.Equals("1"); break;
                }
            });
            
            Data.PauseObservation = false;
        }

        public void Save(){
            FileUtils.WriteFile(Filename,FileMode.Create,(writer) => {
                writer.Write("EX"); writer.WriteLine(ExplorerIntegration ? "1" : "0");
                writer.Write("DF"); writer.WriteLine(DateFormat.Format);
                writer.Write("CT"); writer.WriteLine(NumberSerialization.WriteInt(WindowCloseTime.Value));
                writer.Write("H0"); writer.WriteLine(SaveHistoryWithNoEntries ? "1" : "0");
            });
        }

        // List of settings

        public bool ExplorerIntegration {
            get { return (bool)Data["ExplorerIntegration"]; }
            set { Data["ExplorerIntegration"] = (bool)value; }
        }

        public DateFormat DateFormat {
            get { return (DateFormat)Data["DateFormat"]; }
            set { Data["DateFormat"] = (DateFormat)value; }
        }

        public DisplaySetting<int> WindowCloseTime {
            get { return (DisplaySetting<int>)Data["WindowCloseTime"]; }
            set { Data["WindowCloseTime"] = (DisplaySetting<int>)value; }
        }

        public bool SaveHistoryWithNoEntries {
            get { return (bool)Data["SaveHistoryWithNoEntries"]; }
            set { Data["SaveHistoryWithNoEntries"] = (bool)value; }
        }
    }
}
