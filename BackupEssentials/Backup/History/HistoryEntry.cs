using BackupEssentials.Utils;
using System;

namespace BackupEssentials.Backup.History{
    class HistoryEntry : StringDictionarySerializer.IObjectToDictionary{
        public string LocationName = "";
        public DateTime BackupTime = DateTime.MinValue;
        public int EntriesAdded = 0, EntriesUpdated = 0, EntriesRemoved = 0;
        public string Filename = "";

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
            int.TryParse(dict["EnA"] ?? "0",out EntriesAdded);
            int.TryParse(dict["EnU"] ?? "0",out EntriesUpdated);
            int.TryParse(dict["EnR"] ?? "0",out EntriesRemoved);
            Filename = dict["File"] ?? "";
        }
    }
}
