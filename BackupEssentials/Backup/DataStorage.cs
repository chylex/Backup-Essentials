using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BackupEssentials.Backup{
    class DataStorage{
        public enum Type{
            Locations
        }

        private static readonly ScheduledUpdate SaveTimer = ScheduledUpdate.Forever(10,() => {
            Save(true);
        });

        private static Dictionary<Type,bool> LoadedData = new Dictionary<Type,bool>();

        public static readonly ObservableCollection<BackupLocation> BackupLocationList = new ObservableCollection<BackupLocation>(new List<BackupLocation>(8));
        public static bool BackupLocationListChanged = false;

        static DataStorage(){
            SaveTimer.Start();

            BackupLocationList.CollectionChanged += (sender, args) => {
                BackupLocationListChanged = true;
                Save();
            };

            foreach(Type type in Enum.GetValues(typeof(Type))){
                LoadedData[type] = false;
            }
        }

        static bool ShouldLoad(Type[] types, Type type){
            return types.Length == 0 || types.Contains(type);
        }

        public static void Load(params Type[] types){
            if (File.Exists("DS.Locations.dat") && ShouldLoad(types,Type.Locations)){
                LoadedData[Type.Locations] = true;

                try{
                    using(FileStream fileStream = new FileStream("DS.Locations.dat",FileMode.Open)){
                        using(StreamReader reader = new StreamReader(fileStream)){
                            string line;

                            while((line = reader.ReadLine()) != null){
                                if (line.Length == 0)continue;
                                BackupLocation loc = new BackupLocation();
                                StringDictionarySerializer.FromString(loc,line);
                                BackupLocationList.Add(loc);
                            }
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

            if (BackupLocationListChanged && LoadedData[Type.Locations]){
                BackupLocationListChanged = false;

                using(FileStream fileStream = new FileStream("DS.Locations.dat",FileMode.Create)){
                    using(StreamWriter writer = new StreamWriter(fileStream)){
                        foreach(BackupLocation loc in BackupLocationList)writer.WriteLine(StringDictionarySerializer.ToString(loc));
                    }
                }
            }
        }
    }
}
