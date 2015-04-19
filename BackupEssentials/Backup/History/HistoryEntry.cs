using BackupEssentials.Utils;
using System;
using System.Globalization;

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
            dict["EnA"] = EntriesAdded.ToString(CultureInfo.InvariantCulture);
            dict["EnU"] = EntriesUpdated.ToString(CultureInfo.InvariantCulture);
            dict["EnD"] = EntriesDeleted.ToString(CultureInfo.InvariantCulture);
            dict["File"] = Filename;
        }

        void StringDictionarySerializer.IObjectToDictionary.FromDictionary(SafeDictionary<string,string> dict){
            LocationName = dict["Name"] ?? "";
            BackupTime = DateTime.FromBinary(NumberSerialization.ReadLong(dict["Time"] ?? ""));

            int enAdd = 0, enUpd = 0, enDel = 0;
            if (!int.TryParse(dict["EnA"] ?? "0",out enAdd))enAdd = 0;
            if (!int.TryParse(dict["EnU"] ?? "0",out enUpd))enUpd = 0;
            if (!int.TryParse(dict["EnD"] ?? "0",out enDel))enDel = 0;
            EntriesAdded = enAdd;
            EntriesUpdated = enUpd;
            EntriesDeleted = enDel;

            Filename = dict["File"] ?? "";
        }
    }
}
