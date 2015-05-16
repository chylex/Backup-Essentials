using BackupEssentials.Backup.Data;
using BackupEssentials.Sys;
using BackupEssentials.Utils;
using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace BackupEssentials.Backup.History{
    class HistoryGenerator{
        public static HistoryGenerator FromReport(BackupRunInfo info, BackupReport report){
            return new HistoryGenerator(info,report);
        }

        private readonly BackupRunInfo Info;
        private readonly BackupReport Report;

        private HistoryGenerator(BackupRunInfo info, BackupReport report){
            this.Info = info;
            this.Report = report;
        }

        public BackgroundWorker GenerateAsync(Action<object,RunWorkerCompletedEventArgs> onComplete){
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = false;

            worker.DoWork += (sender, args) => {
                args.Result = null;
                Tuple<BackupRunInfo,BackupReport> data = (Tuple<BackupRunInfo,BackupReport>)args.Argument;

                HistoryEntry entry = new HistoryEntry(){
                    LocationName = data.Item1.Name,
                    BackupTime = DateTime.Now,
                    EntriesAdded = data.Item2.TryFindValue(BackupReport.Constants.EntriesAdded,0),
                    EntriesUpdated = data.Item2.TryFindValue(BackupReport.Constants.EntriesUpdated,0),
                    EntriesDeleted = data.Item2.TryFindValue(BackupReport.Constants.EntriesDeleted,0)
                };

                if (!Directory.Exists(HistoryEntry.Directory))Directory.CreateDirectory(HistoryEntry.Directory);

                string fileStart = WindowsFileUtils.ReplaceInvalidFileCharacters(entry.LocationName,'_')+'_'+entry.BackupTime.ToString("yyyy-MM-dd_HH-mm-ss",CultureInfo.InvariantCulture)+"_";
                int sub = 0;

                while(entry.Filename.Length == 0){
                    string filename = fileStart+sub+".log";

                    if (FileUtils.WriteFileCompressed(Path.Combine(HistoryEntry.Directory,filename),FileMode.CreateNew,data.Item2.UnparsedReport))entry.Filename = filename;
                    else ++sub;
                }

                FileLock flock = new FileLock("DS.History.Lock");

                while(true){
                    if (worker.CancellationPending)break;

                    if (!flock.IsLocked && !flock.TryLock()){
                        Thread.Sleep(50);
                        continue;
                    }

                    DataStorage.Load(DataStorage.Type.History);
                    DataStorage.SetupForSaving(false);
                    DataStorage.HistoryEntryList.Insert(0,entry);
                    List<HistoryEntry> oldEntries = HistoryUtils.RemoveOldEntriesFromList();

                    if (DataStorage.Save(true).Contains(DataStorage.Type.History) && flock.ReleaseLock()){
                        HistoryUtils.RemoveEntryFiles(oldEntries);
                        break;
                    }
                    
                    Thread.Sleep(475);
                }

                args.Result = "";
            };
            
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(onComplete);
            worker.RunWorkerAsync(Tuple.Create(Info,Report));
            return worker;
        }
    }
}
