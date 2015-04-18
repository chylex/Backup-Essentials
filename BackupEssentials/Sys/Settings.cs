using BackupEssentials.Sys.UI;
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
                }
            });
            
            Data.PauseObservation = false;
        }

        public void Save(){
            FileUtils.WriteFile(Filename,FileMode.Create,(writer) => {
                writer.Write("EX"); writer.WriteLine(ExplorerIntegration ? "1" : "0");
                writer.Write("DF"); writer.WriteLine(DateFormat.Format);
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
    }
}
