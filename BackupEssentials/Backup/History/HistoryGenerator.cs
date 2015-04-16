using BackupEssentials.Backup.Data;
using BackupEssentials.Utils;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace BackupEssentials.Backup.History{
    class HistoryGenerator{
        public static HistoryGenerator FromReport(BackupReport report){
            return new HistoryGenerator(report);
        }

        private readonly BackupReport Report;

        private HistoryGenerator(BackupReport report){
            this.Report = report;
        }

        public BackgroundWorker GenerateAsync(Action<object,RunWorkerCompletedEventArgs> onComplete){
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = false;

            worker.DoWork += (sender, args) => {
                args.Result = null;
                BackupReport report = (BackupReport)args.Argument;

                DataStorage.SetupForSaving(false);
                DataStorage.Load(DataStorage.Type.History);

                HistoryEntry entry = new HistoryEntry(){
                    LocationName = "TestLocName", // TODO
                    BackupTime = DateTime.Now,
                    EntriesAdded = report.TryFindValue("Added",0),
                    EntriesUpdated = report.TryFindValue("Updated",0),
                    EntriesRemoved = report.TryFindValue("Removed",0)
                };

                DataStorage.HistoryEntryList.Insert(0,entry);

                if (!Directory.Exists(HistoryEntry.Directory))Directory.CreateDirectory(HistoryEntry.Directory);

                string filename = entry.LocationName+'_'+entry.BackupTime.ToString("yyyy-MM-dd_HH-mm-ss",CultureInfo.InvariantCulture)+".log";
                if (FileUtils.WriteFileCompressed(Path.Combine(HistoryEntry.Directory,filename),FileMode.Create,report.UnparsedReport))entry.Filename = filename;

                DataStorage.Save(true);
                args.Result = "";
            };
            
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(onComplete);
            worker.RunWorkerAsync(Report);
            return worker;
        }
    }
}
