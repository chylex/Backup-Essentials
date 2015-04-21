using BackupEssentials.Backup.Data;
using BackupEssentials.Sys;
using System;
using System.IO;

namespace BackupEssentials.Backup.History{
    static class HistoryUtils{
        public static bool TryRemoveOldEntries(){
            int entriesKept = Settings.Default.HistoryEntriesKept.Value, index;
            bool succeeded = true;

            if (entriesKept != -1){
                while(DataStorage.HistoryEntryList.Count > entriesKept){ // TODO test
                    string file = Path.Combine(HistoryEntry.Directory,DataStorage.HistoryEntryList[index = DataStorage.HistoryEntryList.Count-1].Filename);
                    DataStorage.HistoryEntryList.RemoveAt(index);

                    try{
                        if (File.Exists(file))File.Delete(file);
                    }catch(Exception e){
                        App.LogException(e);
                        succeeded = false;
                    }
                }
            }

            return succeeded;
        }
    }
}
