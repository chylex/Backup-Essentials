using BackupEssentials.Backup.History;
using BackupEssentials.Sys;
using BackupEssentials.Utils;
using BackupEssentials.Utils.Collections;
using BackupEssentials.Utils.IO;
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

        private static ScheduledUpdate SaveTimer;
        private static bool IsSetupForSaving = false;

        private static readonly ReadOnlyCollection<Type> EmptyList = new ReadOnlyCollection<Type>(new List<Type>());

        private static Dictionary<Type,bool> LoadedData = new Dictionary<Type,bool>();

        static DataStorage(){
            foreach(Type type in Enum.GetValues(typeof(Type)))LoadedData[type] = false;
        }

        public static readonly ObservableCollection<BackupLocation> BackupLocationList = new ObservableCollection<BackupLocation>(new List<BackupLocation>(8));
        public static readonly ChangeTracker BackupLocationListTracker = new ChangeTracker();
        public static readonly ObservableCollection<HistoryEntry> HistoryEntryList = new ObservableCollection<HistoryEntry>(new LinkedList<HistoryEntry>());
        public static readonly ChangeTracker HistoryEntryListTracker = new ChangeTracker();

        public class ChangeTracker{
            public bool Changed;
        }

        static NotifyCollectionChangedEventHandler Tracker(ChangeTracker tracker, bool scheduled){
            return new NotifyCollectionChangedEventHandler((sender, args) => { tracker.Changed = true; if (scheduled)Save(); });
        }

        /// <summary>
        /// Run this to allow data saving (loading is available automatically).
        /// </summary>
        public static void SetupForSaving(bool scheduled){
            IsSetupForSaving = true;

            BackupLocationList.CollectionChanged += Tracker(BackupLocationListTracker,scheduled);
            HistoryEntryList.CollectionChanged += Tracker(HistoryEntryListTracker,scheduled);
            
            if (scheduled){
                SaveTimer = ScheduledUpdate.Forever(10,() => {
                    Save(true);
                });

                SaveTimer.Start();
            }
        }

        static bool ShouldLoad(Type[] types, Type type){
            return types.Length == 0 || types.Contains(type);
        }

        public static void Load(params Type[] types){
            if (ShouldLoad(types,Type.Locations)){
                LoadedData[Type.Locations] = true;
                BackupLocationList.Clear();

                FileUtils.ReadFile("DS.Locations.dat",FileMode.Open,(line) => {
                    if (line.Length == 0)return;
                    BackupLocation loc = new BackupLocation();
                    StringDictionarySerializer.FromString(loc,line);
                    BackupLocationList.Add(loc);
                });
            }

            if (ShouldLoad(types,Type.History)){
                LoadedData[Type.History] = true;
                HistoryEntryList.Clear();

                FileUtils.ReadFile("DS.History.dat",FileMode.Open,(line) => {
                    if (line.Length == 0)return;
                    HistoryEntry entry = new HistoryEntry();
                    StringDictionarySerializer.FromString(entry,line);
                    HistoryEntryList.Add(entry);
                });
            }
        }

        public static IList<Type> Save(){
            return Save(false);
        }

        public static IList<Type> Save(bool force){
            if (!IsSetupForSaving)throw new NotSupportedException(Settings.Default.Language["General.Storage.ErrorSaving"]);
            else if (SaveTimer == null && !force)throw new NotSupportedException(Settings.Default.Language["General.Storage.ErrorScheduledSaving"]);

            if (!force){
                SaveTimer.NeedsUpdate = true;
                return EmptyList;
            }

            List<Type> saved = new List<Type>();

            if (BackupLocationListTracker.Changed && LoadedData[Type.Locations]){
                if (FileUtils.WriteFile("DS.Locations.dat",FileMode.Create,(writer) => {
                    foreach(BackupLocation loc in BackupLocationList)writer.WriteLine(StringDictionarySerializer.ToString(loc));
                })){
                    BackupLocationListTracker.Changed = false;
                    saved.Add(Type.Locations);
                }
            }

            if (HistoryEntryListTracker.Changed && LoadedData[Type.History]){
                if (FileUtils.WriteFile("DS.History.dat",FileMode.Create,(writer) => {
                    foreach(HistoryEntry entry in HistoryEntryList)writer.WriteLine(StringDictionarySerializer.ToString(entry));
                })){
                    HistoryEntryListTracker.Changed = false;
                    saved.Add(Type.History);
                }
            }

            return saved;
        }
    }
}
