using BackupEssentials.Utils;
using System;

namespace BackupEssentials.Backup.History{
    public class HistoryEntry : StringDictionarySerializer.IObjectToDictionary{
        public string LocationName { get; set; }
        public DateTime BackupTime { get; set; }
        public int EntriesAdded { get; set; }
        public int EntriesUpdated { get; set; }
        public int EntriesRemoved { get; set; }
        public string Filename = "";

        public HistoryEntry(){
            LocationName = "";
            BackupTime = DateTime.MinValue;
            EntriesAdded = EntriesUpdated = EntriesRemoved = 0;
        }

        void StringDictionarySerializer.IObjectToDictionary.ToDictionary(SafeDictionary<string,string> dict){
            dict["Name"] = LocationName;
            dict["Time"] = Convert.ToBase64String(BitConverter.GetBytes(BackupTime.ToBinary()));
            dict["EnA"] = EntriesAdded.ToString();
            dict["EnU"] = EntriesUpdated.ToString();
            dict["EnR"] = EntriesRemoved.ToString();
            dict["File"] = Filename;
        }

        void StringDictionarySerializer.IObjectToDictionary.FromDictionary(SafeDictionary<string,string> dict){
            LocationName = dict["Name"] ?? "";
            BackupTime = DateTime.FromBinary(BitConverter.ToInt64(Convert.FromBase64String(dict["Time"] ?? ""),0));

            int enAdd = 0, enUpd = 0, enRem = 0;
            int.TryParse(dict["EnA"] ?? "0",out enAdd);
            int.TryParse(dict["EnU"] ?? "0",out enUpd);
            int.TryParse(dict["EnR"] ?? "0",out enRem);
            EntriesAdded = enAdd;
            EntriesUpdated = enUpd;
            EntriesRemoved = enRem;

            Filename = dict["File"] ?? "";
        }
    }
}
