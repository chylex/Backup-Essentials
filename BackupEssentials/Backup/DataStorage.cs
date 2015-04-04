using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BackupEssentials.Backup{
    class DataStorage{
        public static readonly ObservableCollection<BackupLocation> BackupLocationList = new ObservableCollection<BackupLocation>(new List<BackupLocation>(8));

        static DataStorage(){
            
        }
    }
}
