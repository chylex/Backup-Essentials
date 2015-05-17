using BackupEssentials.Backup.IO;
using BackupEssentials.Sys;
using BackupEssentials.Utils;
using BackupEssentials.Utils.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace BackupEssentials.Backup{
    public class BackupRunner{
        private BackgroundWorker Worker;
        public readonly BackupRunInfo RunInfo;

        public Action<object,ProgressChangedEventArgs> EventProgressUpdate;
        public Action<object,RunWorkerCompletedEventArgs> EventCompleted;

        public BackupRunner(BackupRunInfo info){
            RunInfo = info;
        }

        public void Start(){
            if (Worker != null)return;

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            if (EventProgressUpdate != null)Worker.ProgressChanged += new ProgressChangedEventHandler(EventProgressUpdate);
            if (EventCompleted != null)Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EventCompleted);
            Worker.DoWork += new DoWorkEventHandler(WorkerDoWork);

            Worker.RunWorkerAsync(RunInfo);
        }

        public void Cancel(){
            if (Worker != null)Worker.CancelAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e){
            Profiler.Start("Runner");

            BackgroundWorker worker = (BackgroundWorker)sender;
            BackupRunInfo data = (BackupRunInfo)e.Argument;

            string[] src = data.Source;
            string fullSrc = string.Join(", ",src);
            string destFolder = data.Destination;
            string srcParent = null;

            bool ignoreRoot = false;

            if (src[0].EndsWith(@":\",StringComparison.Ordinal)){
                if (src.Length == 1){
                    srcParent = src[0];

                    List<string> newSrc = new List<string>();
                    foreach(string dir in GetNonSystemDirectories(src[0],"*",SearchOption.TopDirectoryOnly))newSrc.Add(Path.Combine(src[0],dir));
                    foreach(string file in Directory.GetFiles(src[0],"*.*",SearchOption.TopDirectoryOnly))newSrc.Add(Path.Combine(src[0],file));
                    src = newSrc.ToArray();

                    ignoreRoot = true;
                }
            }
            else srcParent = Directory.GetParent(src[0]).FullName;

            // Verify source files
            if (srcParent == null){
                throw new Exception(Settings.Default.Language["BackupWindow.Error.MultipleLocations"]);
            }

            for(int a = 1; a < src.Length; a++){
                if (!Directory.GetParent(src[a]).FullName.Equals(srcParent))throw new Exception(Settings.Default.Language["BackupWindow.Error.MultipleLocations"]);
            }

            if (srcParent[srcParent.Length-1] == '\\')srcParent = srcParent.Substring(0,srcParent.Length-1);

            // Figure out the source file and directory lists
            Profiler.Start("Runner - sources");

            HashSet<string> rootSrcEntries = new HashSet<string>();
            Dictionary<string,IOEntry> srcEntries = new Dictionary<string,IOEntry>(), dstEntries = new Dictionary<string,IOEntry>();
            string[] updatedSrc = new string[src.Length];

            for(int a = 0; a < src.Length; a++){
                string srcEntry = src[a];

                FileAttributes attributes = File.GetAttributes(srcEntry);
                if (attributes.HasFlag(FileAttributes.System))continue;

                if (attributes.HasFlag(FileAttributes.Directory)){
                    int srcLen = srcParent.Length+1;
                    string dname = srcEntry.Remove(0,srcLen);

                    rootSrcEntries.Add(dname);
                    srcEntries.Add(dname,new IOEntry(){ Type = IOType.Directory, AbsolutePath = srcEntry });

                    foreach(string dir in GetNonSystemDirectories(srcEntry,"*",SearchOption.AllDirectories))srcEntries.Add(dir.Remove(0,srcLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
                    foreach(string file in Directory.GetFiles(srcEntry,"*.*",SearchOption.AllDirectories))srcEntries.Add(file.Remove(0,srcLen),new IOEntry(){ Type = IOType.File, AbsolutePath = file });
                }
                else{
                    string fname = Path.GetFileName(srcEntry);

                    rootSrcEntries.Add(fname);
                    srcEntries.Add(fname,new IOEntry(){ Type = IOType.File, AbsolutePath = srcEntry });
                
                    updatedSrc[a] = Directory.GetParent(srcEntry).FullName;
                }
            }

            src = updatedSrc;
            Profiler.End("Runner - sources");

            // Figure out the destination info
            if (!Directory.Exists(destFolder))Directory.CreateDirectory(destFolder);
            
            Profiler.Start("Runner - destination");
            int destFolderLen = destFolder.Length+1;

            foreach(string entry in Directory.GetFileSystemEntries(destFolder,"*",SearchOption.TopDirectoryOnly)){
                string entryName = entry.Remove(0,destFolderLen);

                if (ignoreRoot || rootSrcEntries.Remove(entryName)){
                    FileAttributes attributes = File.GetAttributes(entry);
                    if (attributes.HasFlag(FileAttributes.System))continue;

                    if (attributes.HasFlag(FileAttributes.Directory)){
                        dstEntries.Add(entryName,new IOEntry(){ Type = IOType.Directory, AbsolutePath = entry });
                        foreach(string dir in GetNonSystemDirectories(entry,"*",SearchOption.AllDirectories))dstEntries.Add(dir.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
                        foreach(string file in Directory.GetFiles(entry,"*.*",SearchOption.AllDirectories))dstEntries.Add(file.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.File, AbsolutePath = file });
                    }
                    else dstEntries.Add(entryName,new IOEntry(){ Type = IOType.File, AbsolutePath = entry });
                }
            }

            Profiler.End("Runner - destination");

            // Generate the IO actions
            Profiler.Start("Runner - action gen");

            List<IOActionEntry> actions = new List<IOActionEntry>(srcEntries.Count);
            KeyEqualityComparer<string,IOEntry> keyComparer = new KeyEqualityComparer<string,IOEntry>();
            
            IEnumerable<KeyValuePair<string,IOEntry>> ioDeleted = dstEntries.Except(srcEntries,keyComparer);
            IEnumerable<KeyValuePair<string,IOEntry>> ioAdded = srcEntries.Except(dstEntries,keyComparer);
            IEnumerable<string> ioIntersecting = srcEntries.Keys.Intersect(dstEntries.Keys);
            
            Profiler.Start("Runner - action gen - deleted");

            foreach(KeyValuePair<string,IOEntry> deleted in ioDeleted){
                if (!ignoreRoot && deleted.Key.IndexOf(Path.DirectorySeparatorChar) == -1)continue; // ignore everything in root folder
                actions.Add(new IOActionEntry(){ Type = deleted.Value.Type, Action = IOAction.Delete, RelativePath = deleted.Key });
            }
            
            Profiler.End("Runner - action gen - deleted");
            Profiler.Start("Runner - action gen - added");

            foreach(KeyValuePair<string,IOEntry> added in ioAdded){
                actions.Add(new IOActionEntry(){ Type = added.Value.Type, Action = IOAction.Create, RelativePath = added.Key });
            }

            Profiler.End("Runner - action gen - added");
            Profiler.Start("Runner - action gen - intersecting");

            foreach(string intersecting in ioIntersecting){
                IOEntry srcEntry = srcEntries[intersecting];
                if (srcEntry.Type == IOType.Directory)continue;

                FileInfo srcFileInfo = new FileInfo(srcEntry.AbsolutePath);
                FileInfo dstFileInfo = new FileInfo(dstEntries[intersecting].AbsolutePath);

                if (srcFileInfo.Length == dstFileInfo.Length && srcFileInfo.LastWriteTime == dstFileInfo.LastWriteTime)continue;
                actions.Add(new IOActionEntry(){ Type = IOType.File, Action = IOAction.Replace, RelativePath = intersecting });
            }
            
            Profiler.End("Runner - action gen - intersecting");
            Profiler.End("Runner - action gen");
            Profiler.Start("Runner - action sort");

            actions.Sort((entry1, entry2) => {
                if (entry1.Type == IOType.Directory && entry2.Type == IOType.File)return -1;
                else if (entry2.Type == IOType.Directory && entry1.Type == IOType.File)return 1;
                else return 0;
            });

            Profiler.End("Runner - action sort");

            // Report a state update
            worker.ReportProgress(0,actions.Count);

            // Start working
            List<int> indexesToRemove = new List<int>();
            int totalActions = actions.Count, attempts = 10;
            bool firstAttempt = true;
            string path;

            BackupReport.Builder reportBuilder = new BackupReport.Builder();

            /*if (DEBUG){
                reportBuilder.Add("Report.Title.Debug");
                reportBuilder.Add("Report.Info.Debug");
                reportBuilder.AddLine();
            }*/

            reportBuilder.Add("Report.Title.PreparingBackup");
            reportBuilder.Add(BackupReport.Constants.Source,fullSrc);
            reportBuilder.Add(BackupReport.Constants.Destination,destFolder);
            reportBuilder.Add(BackupReport.Constants.Date,Settings.Default.DateFormat.ParseDate(DateTime.Now));
            reportBuilder.AddLine();
            reportBuilder.Add("Report.Title.FilesAndFolders");
            reportBuilder.Add(BackupReport.Constants.EntriesAdded,actions.Count((entry) => entry.Action == IOAction.Create).ToString(CultureInfo.InvariantCulture));
            reportBuilder.Add(BackupReport.Constants.EntriesUpdated,actions.Count((entry) => entry.Action == IOAction.Replace).ToString(CultureInfo.InvariantCulture));
            reportBuilder.Add(BackupReport.Constants.EntriesDeleted,actions.Count((entry) => entry.Action == IOAction.Delete).ToString(CultureInfo.InvariantCulture));
            reportBuilder.AddLine();
            reportBuilder.Add("Report.Title.StartingBackup");

            Profiler.Start("Runner - work");

            while(actions.Count > 0 && --attempts > 0){
                if (firstAttempt)firstAttempt = false;
                else Thread.Sleep(200);

                for(int index = 0; index < actions.Count; index++){
                    IOActionEntry entry = actions[index];

                    try{
                        bool ignoreEntry = false;

                        if (entry.Action == IOAction.Delete){
                            path = Path.Combine(destFolder,entry.RelativePath);

                            if (entry.Type == IOType.File){
                                if (File.Exists(path)){
                                    App.LogInfo("[D] Deleting file: "+path);
                                    File.Delete(path);
                                }
                                else ignoreEntry = true;
                            }
                            else if (entry.Type == IOType.Directory){
                                if (Directory.Exists(path)){
                                    App.LogInfo("[D] Deleting directory: "+path);
                                    Directory.Delete(path,true);
                                }
                                else ignoreEntry = true;
                            }
                        }
                        else if (entry.Action == IOAction.Create){
                            path = Path.Combine(destFolder,entry.RelativePath);

                            if (entry.Type == IOType.File){
                                App.LogInfo("[D] Copying file: "+Path.Combine(srcParent,entry.RelativePath)+" --> "+path);
                                File.Copy(Path.Combine(srcParent,entry.RelativePath),path,false);
                            }
                            else if (entry.Type == IOType.Directory){
                                if (!Directory.Exists(path)){
                                    App.LogInfo("[D] Creating directory: "+path);
                                    Directory.CreateDirectory(path);
                                }
                            }
                        }
                        else if (entry.Action == IOAction.Replace){
                            App.LogInfo("[D] Replacing file: "+Path.Combine(srcParent,entry.RelativePath)+" --> "+Path.Combine(destFolder,entry.RelativePath));
                            File.Copy(Path.Combine(srcParent,entry.RelativePath),Path.Combine(destFolder,entry.RelativePath),true);
                        }
                        
                        indexesToRemove.Add(index-indexesToRemove.Count); // goes from 0 to actions.Count, removing each index will move the structure
                        if (!ignoreEntry)reportBuilder.Add(entry.Action,entry.Type,entry.RelativePath);
                        
                        worker.ReportProgress((int)Math.Ceiling(((totalActions-actions.Count+indexesToRemove.Count)*100D)/totalActions));
                        if (worker.CancellationPending)break;
                    }catch(Exception exception){ // if an action failed, it will not be removed
                        App.LogException(exception);
                        Debug.WriteLine("Failed: "+entry.ToString());
                        Debug.WriteLine(exception.Message);
                        // TODO handle special exceptions (security etc)
                    }

                    if (worker.CancellationPending){
                        reportBuilder.Add("Report.Title.BackupCanceled");
                        e.Result = reportBuilder.Finish();
                        throw new Exception(Settings.Default.Language["Report.Error.Canceled"]);
                    }
                }

                foreach(int index in indexesToRemove)actions.RemoveAt(index);
                indexesToRemove.Clear();
            }

            if (attempts == 0){
                reportBuilder.Add("Report.Title.BackupFailed");
                e.Result = reportBuilder.Finish();
                throw new Exception(Settings.Default.Language["Report.Error.Failed"]);
            }

            reportBuilder.Add("Report.Title.BackupFinished");
            e.Result = reportBuilder.Finish();
            
            Profiler.End("Runner - work");
            Profiler.End("Runner");
        }

        private static IEnumerable<string> GetNonSystemDirectories(string path, string searchPattern, SearchOption searchOption){
            foreach(string dir in Directory.EnumerateDirectories(path,searchPattern,searchOption)){
                if (!File.GetAttributes(dir).HasFlag(FileAttributes.System))yield return dir;
            }
        }

        private struct IOEntry{
            public IOType Type;
            public string AbsolutePath;

            public override string ToString(){
                return "{ Type: "+Type.ToString()+", AbsolutePath: "+(AbsolutePath == null ? "<null>" : AbsolutePath.ToString())+" }";
            }
        }

        private struct IOActionEntry{
            private string _relativePath;

            public IOType Type;
            public IOAction Action;
            public string RelativePath { get { return _relativePath; } set { _relativePath = value; if (_relativePath.StartsWith("\\",StringComparison.OrdinalIgnoreCase))_relativePath = _relativePath.Substring(1); } }
            
            public override string ToString(){
                return "{ Type: "+Type.ToString()+", Action: "+Action.ToString()+", RelativePath: "+(RelativePath == null ? "<null>" : RelativePath.ToString())+" }";
            }
        }
    }
}
