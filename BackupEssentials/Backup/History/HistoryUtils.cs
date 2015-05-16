using BackupEssentials.Backup.Data;
using BackupEssentials.Sys;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackupEssentials.Backup.History{
    static class HistoryUtils{ // TODO test
        public static List<HistoryEntry> RemoveOldEntriesFromList(){
            List<HistoryEntry> list = new List<HistoryEntry>();
            int entriesKept = Settings.Default.HistoryEntriesKept.Value, index;

            if (entriesKept != -1){
                App.LogInfo("== Trying to remove old entries... current "+DataStorage.HistoryEntryList.Count+", target "+entriesKept); // TODO

                while(DataStorage.HistoryEntryList.Count > entriesKept){
                    list.Add(DataStorage.HistoryEntryList[index = DataStorage.HistoryEntryList.Count-1]);
                    DataStorage.HistoryEntryList.RemoveAt(index);
                    App.LogInfo("Removing... current "+DataStorage.HistoryEntryList.Count); // TODO
                }
            }

            return list;
        }

        public static bool RemoveEntryFiles(List<HistoryEntry> entries){
            bool succeeded = true;

            foreach(HistoryEntry entry in entries){
                try{
                    string file = Path.Combine(HistoryEntry.Directory,entry.Filename);
                    App.LogInfo("Deleting entry file... "+entry.Filename); // TODO
                    if (File.Exists(file))File.Delete(file);
                }catch(Exception e){
                    App.LogException(e);
                    succeeded = false;
                }
            }

            return succeeded;
        }
    }
}
