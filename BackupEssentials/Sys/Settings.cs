using BackupEssentials.Sys.UI;
using BackupEssentials.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace BackupEssentials.Sys{
    public class Settings : INotifyPropertyChanged{
        private static readonly Settings defaultInstance = new Settings();
        public static Settings Default { get { return defaultInstance; } }

        private readonly ObservableDictionary<string,object> Data = new ObservableDictionary<string,object>();
        public event PropertyChangedEventHandler PropertyChanged;

        public Settings(){
            Data.PropertyChanged += (sender, args) => { if (PropertyChanged != null)PropertyChanged(sender,args); };
            SetToDefault();
        }

        public void SetToDefault(){
            Data.PauseObservation = true;

            ExplorerIntegration = true;
            DateFormat = SettingsData.DateFormatList[0];
            
            Data.PauseObservation = false;
        }

        public void Load(){
            // TODO implement
        }

        public void Save(){
            // TODO implement
        }

        // List of settings
        
        public IEnumerable<string> Properties {
            get { return Data.Keys; }
        }

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
