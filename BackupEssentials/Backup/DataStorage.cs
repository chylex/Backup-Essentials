using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace BackupEssentials.Backup{
    class DataStorage{
        private static readonly ScheduledUpdate SaveTimer = ScheduledUpdate.Forever(10,() => {
            Save(true);
        });

        public static readonly ObservableCollection<BackupLocation> BackupLocationList = new ObservableCollection<BackupLocation>(new List<BackupLocation>(8));
        private static bool BackupLocationListChanged = false;

        static DataStorage(){
            BackupLocationList.CollectionChanged += (sender, args) => {
                BackupLocationListChanged = true;
                Save();
            };
        }

        public static void Load(){
            if (File.Exists("DS.Locations.dat")){
                try{
                    using(StreamReader reader = new StreamReader(new FileStream("DS.Locations.dat",FileMode.Open))){
                        string data = reader.ReadToEnd();

                        foreach(string entry in data.Split((char)30)){
                            if (entry.Length == 0)break;

                            string[] record = entry.Split(new char[]{ (char)31 },2);
                            if (record.Length == 2)BackupLocationList.Add(new BackupLocation(){ Name = record[0], Directory = record[1] });
                        }
                    }
                }catch(Exception e){
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        public static void Save(){
            Save(false);
        }

        public static void Save(bool force){
            if (!force){
                SaveTimer.NeedsUpdate = true;
                return;
            }

            if (BackupLocationListChanged){
                BackupLocationListChanged = false;

                using(StreamWriter writer = new StreamWriter(new FileStream("DS.Locations.dat",FileMode.Create))){
                    foreach(BackupLocation loc in BackupLocationList){
                        writer.Write(loc.Name);
                        writer.Write((char)31);
                        writer.Write(loc.Directory);
                        writer.Write((char)30);
                    }
                }
            }
        }
    }
}
