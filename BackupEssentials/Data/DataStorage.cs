﻿using BackupEssentials.Backup.History;
using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace BackupEssentials.Backup.Data{
    class DataStorage{
        public enum Type{
            Locations, History
        }

        private static readonly ScheduledUpdate SaveTimer = ScheduledUpdate.Forever(10,() => {
            Save(true);
        });

        private static Dictionary<Type,bool> LoadedData = new Dictionary<Type,bool>();

        public static readonly ObservableCollection<BackupLocation> BackupLocationList = new ObservableCollection<BackupLocation>(new List<BackupLocation>(8));
        public static readonly ChangeTracker BackupLocationListTracker = new ChangeTracker();
        public static readonly ObservableCollection<HistoryEntry> HistoryEntryList = new ObservableCollection<HistoryEntry>(new List<HistoryEntry>(32));
        public static readonly ChangeTracker HistoryEntryListTracker = new ChangeTracker();

        static DataStorage(){
            SaveTimer.Start();

            BackupLocationList.CollectionChanged += Tracker(BackupLocationListTracker);
            HistoryEntryList.CollectionChanged += Tracker(HistoryEntryListTracker);

            foreach(Type type in Enum.GetValues(typeof(Type))){
                LoadedData[type] = false;
            }
        }

        public class ChangeTracker{
            public bool Changed;
        }

        static NotifyCollectionChangedEventHandler Tracker(ChangeTracker tracker){
            return new NotifyCollectionChangedEventHandler((sender, args) => { tracker.Changed = true; Save(); });
        }

        static bool ShouldLoad(Type[] types, Type type){
            return types.Length == 0 || types.Contains(type);
        }

        public static void Load(params Type[] types){
            if (File.Exists("DS.Locations.dat") && ShouldLoad(types,Type.Locations)){
                LoadedData[Type.Locations] = true;

                FileUtils.ReadFile("DS.Locations.dat",FileMode.Open,(line) => {
                    if (line.Length == 0)return;
                    BackupLocation loc = new BackupLocation();
                    StringDictionarySerializer.FromString(loc,line);
                    BackupLocationList.Add(loc);
                });
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

            if (BackupLocationListTracker.Changed && LoadedData[Type.Locations]){
                BackupLocationListTracker.Changed = false;

                FileUtils.WriteFile("DS.Locations.dat",FileMode.Create,(writer) => {
                    foreach(BackupLocation loc in BackupLocationList)writer.WriteLine(StringDictionarySerializer.ToString(loc));
                });
            }
        }
    }
}