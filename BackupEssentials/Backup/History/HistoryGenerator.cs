using BackupEssentials.Backup.Data;
using BackupEssentials.Sys;
using BackupEssentials.Utils;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

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

                DataStorage.SetupForSaving(false);
                DataStorage.Load(DataStorage.Type.History);

                HistoryEntry entry = new HistoryEntry(){
                    LocationName = data.Item1.Name,
                    BackupTime = DateTime.Now,
                    EntriesAdded = data.Item2.TryFindValue(BackupReport.Constants.EntriesAdded,0),
                    EntriesUpdated = data.Item2.TryFindValue(BackupReport.Constants.EntriesUpdated,0),
                    EntriesDeleted = data.Item2.TryFindValue(BackupReport.Constants.EntriesDeleted,0)
                };

                DataStorage.HistoryEntryList.Insert(0,entry);

                HistoryUtils.TryRemoveOldEntries();

                if (!Directory.Exists(HistoryEntry.Directory))Directory.CreateDirectory(HistoryEntry.Directory);

                string filename = entry.LocationName+'_'+entry.BackupTime.ToString("yyyy-MM-dd_HH-mm-ss",CultureInfo.InvariantCulture)+".log";
                if (FileUtils.WriteFileCompressed(Path.Combine(HistoryEntry.Directory,filename),FileMode.Create,data.Item2.UnparsedReport))entry.Filename = filename;

                DataStorage.Save(true);
                args.Result = "";
            };
            
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(onComplete);
            worker.RunWorkerAsync(Tuple.Create(Info,Report));
            return worker;
        }
    }
}
