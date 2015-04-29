using BackupEssentials.Backup.IO;
using BackupEssentials.Sys;
using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BackupEssentials.Backup{
    public class BackupRunner{
        private static readonly bool DEBUG = false;

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
            BackgroundWorker worker = (BackgroundWorker)sender;
            BackupRunInfo data = (BackupRunInfo)e.Argument;

            string[] src = data.Source;
            string fullSrc = string.Join(", ",src);
            string destFolder = data.Destination;
            string srcParent = null;

            if (src[0].EndsWith(@":\")){
                if (src.Length == 1){
                    srcParent = src[0];

                    List<string> newSrc = new List<string>();
                    foreach(string dir in GetVisibleDirectories(src[0],"*",SearchOption.TopDirectoryOnly))newSrc.Add(Path.Combine(src[0],dir));
                    foreach(string file in Directory.GetFiles(src[0],"*.*",SearchOption.TopDirectoryOnly))newSrc.Add(Path.Combine(src[0],file));
                    src = newSrc.ToArray();
                }
            }
            else srcParent = Directory.GetParent(src[0]).FullName;

            // Verify source files
            if (srcParent == null){
                throw new Exception("Cannot backup multiple locations!");
            }

            for(int a = 1; a < src.Length; a++){
                if (!Directory.GetParent(src[a]).FullName.Equals(srcParent))throw new Exception("Cannot backup multiple locations!");
            }

            if (srcParent[srcParent.Length-1] == '\\')srcParent = srcParent.Substring(0,srcParent.Length-1);

            // Figure out the source file and directory lists
            HashSet<string> rootSrcEntries = new HashSet<string>();
            Dictionary<string,IOEntry> srcEntries = new Dictionary<string,IOEntry>(), dstEntries = new Dictionary<string,IOEntry>();
            string[] updatedSrc = new string[src.Length];

            for(int a = 0; a < src.Length; a++){
                string srcEntry = src[a];

                if (File.GetAttributes(srcEntry).HasFlag(FileAttributes.Directory)){
                    int srcLen = srcParent.Length+1;
                    string dname = srcEntry.Remove(0,srcLen);

                    rootSrcEntries.Add(dname);
                    srcEntries.Add(dname,new IOEntry(){ Type = IOType.Directory, AbsolutePath = srcEntry });

                    foreach(string dir in GetVisibleDirectories(srcEntry,"*",SearchOption.AllDirectories))srcEntries.Add(dir.Remove(0,srcLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
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

            // Figure out the destination info
            if (!Directory.Exists(destFolder))Directory.CreateDirectory(destFolder);
            
            int destFolderLen = destFolder.Length+1;

            foreach(string entry in Directory.GetFileSystemEntries(destFolder,"*",SearchOption.TopDirectoryOnly)){
                string entryName = entry.Remove(0,destFolderLen);

                if (rootSrcEntries.Remove(entryName)){
                    if (File.GetAttributes(entry).HasFlag(FileAttributes.Directory)){
                        dstEntries.Add(entryName,new IOEntry(){ Type = IOType.Directory, AbsolutePath = entry });
                        foreach(string dir in GetVisibleDirectories(entry,"*",SearchOption.AllDirectories))dstEntries.Add(dir.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
                        foreach(string file in Directory.GetFiles(entry,"*.*",SearchOption.AllDirectories))dstEntries.Add(file.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.File, AbsolutePath = file });
                    }
                    else dstEntries.Add(entryName,new IOEntry(){ Type = IOType.File, AbsolutePath = entry });
                }
            }

            // Generate the IO actions
            List<IOActionEntry> actions = new List<IOActionEntry>();
            KeyEqualityComparer<string,IOEntry> keyComparer = new KeyEqualityComparer<string,IOEntry>();
            
            IEnumerable<KeyValuePair<string,IOEntry>> ioDeleted = dstEntries.Except(srcEntries,keyComparer);
            IEnumerable<KeyValuePair<string,IOEntry>> ioAdded = srcEntries.Except(dstEntries,keyComparer);
            IEnumerable<string> ioIntersecting = srcEntries.Keys.Intersect(dstEntries.Keys);
            
            foreach(KeyValuePair<string,IOEntry> deleted in ioDeleted){
                if (deleted.Key.IndexOf(Path.DirectorySeparatorChar) == -1)continue; // ignore everything in root folder
                actions.Add(new IOActionEntry(){ Type = deleted.Value.Type, Action = IOAction.Delete, RelativePath = deleted.Key });
            }

            foreach(KeyValuePair<string,IOEntry> added in ioAdded){
                actions.Add(new IOActionEntry(){ Type = added.Value.Type, Action = IOAction.Create, RelativePath = added.Key });
            }

            foreach(string intersecting in ioIntersecting){
                IOEntry srcEntry = srcEntries[intersecting];
                if (srcEntry.Type == IOType.Directory)continue;

                FileInfo srcFileInfo = new FileInfo(srcEntry.AbsolutePath);
                FileInfo dstFileInfo = new FileInfo(dstEntries[intersecting].AbsolutePath);

                if (srcFileInfo.Length == dstFileInfo.Length && srcFileInfo.LastWriteTime == dstFileInfo.LastWriteTime)continue;
                actions.Add(new IOActionEntry(){ Type = IOType.File, Action = IOAction.Replace, RelativePath = intersecting });
            }

            actions.Sort((entry1, entry2) => {
                if (entry1.Type == IOType.Directory && entry2.Type == IOType.File)return -1;
                else if (entry2.Type == IOType.Directory && entry1.Type == IOType.File)return 1;
                else return 0;
            });

            // Report a state update
            worker.ReportProgress(0,actions.Count);

            // Start working
            List<int> indexesToRemove = new List<int>();
            int totalActions = actions.Count, attempts = 10;
            bool firstAttempt = true;
            string path;

            BackupReport.Builder reportBuilder = new BackupReport.Builder();

            if (DEBUG){
                reportBuilder.Add("= Caution =");
                reportBuilder.Add("Backup Essentials is running in debug mode, all actions will be logged into the report but will not actually modify any files or folders!");
                reportBuilder.Add("");
            }

            reportBuilder.Add("= Preparing backup =");
            reportBuilder.Add(BackupReport.Constants.Source,fullSrc);
            reportBuilder.Add(BackupReport.Constants.Destination,destFolder);
            reportBuilder.Add(BackupReport.Constants.Date,Settings.Default.DateFormat.ParseDate(DateTime.Now));
            reportBuilder.Add("");
            reportBuilder.Add("= Files and folders =");
            reportBuilder.Add(BackupReport.Constants.EntriesAdded,actions.Count((entry) => entry.Action == IOAction.Create).ToString());
            reportBuilder.Add(BackupReport.Constants.EntriesUpdated,actions.Count((entry) => entry.Action == IOAction.Replace).ToString());
            reportBuilder.Add(BackupReport.Constants.EntriesDeleted,actions.Count((entry) => entry.Action == IOAction.Delete).ToString());
            reportBuilder.Add("");
            reportBuilder.Add("= Starting backup =");

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
                                    if (DEBUG)reportBuilder.Add("[D] Deleting file: "+path);
                                    else File.Delete(path);
                                }
                                else ignoreEntry = true;
                            }
                            else if (entry.Type == IOType.Directory){
                                if (Directory.Exists(path)){
                                    if (DEBUG)reportBuilder.Add("[D] Deleting directory: "+path);
                                    else Directory.Delete(path,true);
                                }
                                else ignoreEntry = true;
                            }
                        }
                        else if (entry.Action == IOAction.Create){
                            path = Path.Combine(destFolder,entry.RelativePath);

                            if (entry.Type == IOType.File){
                                if (DEBUG)reportBuilder.Add("[D] Copying file: "+Path.Combine(srcParent,entry.RelativePath)+" --> "+path);
                                else File.Copy(Path.Combine(srcParent,entry.RelativePath),path,false);
                            }
                            else if (entry.Type == IOType.Directory){
                                if (!Directory.Exists(path)){
                                    if (DEBUG)reportBuilder.Add("[D] Creating directory: "+path);
                                    else Directory.CreateDirectory(path);
                                }
                            }
                        }
                        else if (entry.Action == IOAction.Replace){
                            if (DEBUG)reportBuilder.Add("[D] Replacing file: "+Path.Combine(srcParent,entry.RelativePath)+" --> "+Path.Combine(destFolder,entry.RelativePath));
                            else File.Copy(Path.Combine(srcParent,entry.RelativePath),Path.Combine(destFolder,entry.RelativePath),true);
                        }
                        
                        indexesToRemove.Add(index-indexesToRemove.Count); // goes from 0 to actions.Count, removing each index will move the structure
                        if (!ignoreEntry)reportBuilder.Add(entry.Action,entry.Type,entry.RelativePath);

                        worker.ReportProgress((int)Math.Ceiling(((totalActions-actions.Count+indexesToRemove.Count)*100D)/totalActions));
                        if (worker.CancellationPending)break;
                    }catch(Exception exception){ // if an action failed, it will not be removed
                        Debug.WriteLine("Failed: "+entry.ToString());
                        Debug.WriteLine(exception.Message);
                        // TODO handle special exceptions (security etc)
                    }

                    if (worker.CancellationPending){
                        reportBuilder.Add("= Backup canceled =");
                        e.Result = reportBuilder.Finish();
                        throw new Exception("Backup canceled.");
                    }
                }

                foreach(int index in indexesToRemove)actions.RemoveAt(index);
                indexesToRemove.Clear();
            }

            if (attempts == 0){
                reportBuilder.Add("= Backup failed (out of attempts) =");
                e.Result = reportBuilder.Finish();
                throw new Exception("Backup failed: ran out of attempts.");
            }

            reportBuilder.Add("= Backup finished =");
            e.Result = reportBuilder.Finish();
        }

        private static IEnumerable<string> GetVisibleDirectories(string path, string searchPattern, SearchOption searchOption){
            foreach(string dir in Directory.EnumerateDirectories(path,searchPattern,searchOption)){
                if (!File.GetAttributes(dir).HasFlag(FileAttributes.Hidden))yield return dir;
            }
        }

        private class IOEntry{
            public IOType Type;
            public string AbsolutePath;

            public override string ToString(){
                return "{ Type: "+Type.ToString()+", AbsolutePath: "+(AbsolutePath == null ? "<null>" : AbsolutePath.ToString())+" }";
            }
        }

        private class IOActionEntry{
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
