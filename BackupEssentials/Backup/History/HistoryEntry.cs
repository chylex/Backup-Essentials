using BackupEssentials.Utils;
using System;

namespace BackupEssentials.Backup.History{
    public class HistoryEntry : StringDictionarySerializer.IObjectToDictionary{
        public static readonly string Directory = "History";

        public string LocationName { get; set; }
        public DateTime BackupTime { get; set; }
        public int EntriesAdded { get; set; }
        public int EntriesUpdated { get; set; }
        public int EntriesDeleted { get; set; }
        public string Filename = "";

        public HistoryEntry(){
            LocationName = "";
            BackupTime = DateTime.MinValue;
            EntriesAdded = EntriesUpdated = EntriesDeleted = 0;
        }

        void StringDictionarySerializer.IObjectToDictionary.ToDictionary(SafeDictionary<string,string> dict){
            dict["Name"] = LocationName;
            dict["Time"] = NumberSerialization.WriteLong(BackupTime.ToBinary());
            dict["EnA"] = EntriesAdded.ToString();
            dict["EnU"] = EntriesUpdated.ToString();
            dict["EnR"] = EntriesDeleted.ToString();
            dict["File"] = Filename;
        }

        void StringDictionarySerializer.IObjectToDictionary.FromDictionary(SafeDictionary<string,string> dict){
            LocationName = dict["Name"] ?? "";
            BackupTime = DateTime.FromBinary(NumberSerialization.ReadLong(dict["Time"] ?? ""));

            int enAdd = 0, enUpd = 0, enRem = 0;
            int.TryParse(dict["EnA"] ?? "0",out enAdd);
            int.TryParse(dict["EnU"] ?? "0",out enUpd);
            int.TryParse(dict["EnR"] ?? "0",out enRem);
            EntriesAdded = enAdd;
            EntriesUpdated = enUpd;
            EntriesDeleted = enRem;

            Filename = dict["File"] ?? "";
        }
    }
}
