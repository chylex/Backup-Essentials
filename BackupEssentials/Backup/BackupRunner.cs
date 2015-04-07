using BackupEssentials.Backup.IO;
using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BackupEssentials.Backup{
    public class BackupRunner{
        private BackgroundWorker Worker;
        private Tuple<string,string> WorkerData;

        public Action<object,ProgressChangedEventArgs> EventProgressUpdate;
        public Action<object,RunWorkerCompletedEventArgs> EventCompleted;

        public BackupRunner(string source, string destFolder){
            WorkerData = new Tuple<string,string>(source,destFolder);
        }

        public void Start(){
            if (Worker != null)return;

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            if (EventProgressUpdate != null)Worker.ProgressChanged += new ProgressChangedEventHandler(EventProgressUpdate);
            if (EventCompleted != null)Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EventCompleted);
            Worker.DoWork += new DoWorkEventHandler(WorkerDoWork);

            Worker.RunWorkerAsync(WorkerData);
        }

        public void Cancel(){
            if (Worker != null)Worker.CancelAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e){
            BackgroundWorker worker = (BackgroundWorker)sender;
            Tuple<string,string> data = (Tuple<string,string>)e.Argument;

            string src = data.Item1, fullSrc = src;
            string destFolder = data.Item2;

            // Figure out the file and directory lists
            Dictionary<string,IOEntry> srcEntries = new Dictionary<string,IOEntry>(), dstEntries = new Dictionary<string,IOEntry>();

            if (File.GetAttributes(src).HasFlag(FileAttributes.Directory)){
                int srcLen = src.Length;
                foreach(string dir in Directory.GetDirectories(src,"*",SearchOption.AllDirectories))srcEntries.Add(dir.Remove(0,srcLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
                foreach(string file in Directory.GetFiles(src,"*.*",SearchOption.AllDirectories))srcEntries.Add(file.Remove(0,srcLen),new IOEntry(){ Type = IOType.File, AbsolutePath = file });
                
                destFolder = Path.Combine(destFolder,Path.GetFileName(src.TrimEnd(Path.DirectorySeparatorChar)));
                if (!Directory.Exists(destFolder))Directory.CreateDirectory(destFolder);

                int destFolderLen = destFolder.Length;
                foreach(string dir in Directory.GetDirectories(destFolder,"*",SearchOption.AllDirectories))dstEntries.Add(dir.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.Directory, AbsolutePath = dir });
                foreach(string file in Directory.GetFiles(destFolder,"*.*",SearchOption.AllDirectories))dstEntries.Add(file.Remove(0,destFolderLen),new IOEntry(){ Type = IOType.File, AbsolutePath = file });
            }
            else{
                string fname = Path.GetFileName(src);
                srcEntries.Add(fname,new IOEntry(){ Type = IOType.File, AbsolutePath = src });
                
                src = Directory.GetParent(src).FullName;

                string dst = Path.Combine(destFolder,fname);
                if (File.Exists(dst))dstEntries.Add(fname,new IOEntry{ Type = IOType.File, AbsolutePath = dst });

                if (!Directory.Exists(destFolder))Directory.CreateDirectory(destFolder);
            }

            // Generate the IO actions
            List<IOActionEntry> actions = new List<IOActionEntry>();
            KeyEqualityComparer<string,IOEntry> keyComparer = new KeyEqualityComparer<string,IOEntry>();
            
            IEnumerable<KeyValuePair<string,IOEntry>> ioDeleted = dstEntries.Except(srcEntries,keyComparer);
            IEnumerable<KeyValuePair<string,IOEntry>> ioAdded = srcEntries.Except(dstEntries,keyComparer);
            IEnumerable<string> ioIntersecting = srcEntries.Keys.Intersect(dstEntries.Keys);
            
            foreach(KeyValuePair<string,IOEntry> deleted in ioDeleted){
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
            reportBuilder.Add("= Preparing backup =");
            reportBuilder.Add("Source: "+fullSrc);
            reportBuilder.Add("Destination: "+destFolder);
            reportBuilder.Add("Date: "+DateTime.Now.ToString("d")+" "+DateTime.Now.ToString("t"));
            reportBuilder.Add("");
            reportBuilder.Add("= Files and folders =");
            reportBuilder.Add("Added: "+actions.Count((entry) => entry.Action == IOAction.Create));
            reportBuilder.Add("Updated: "+actions.Count((entry) => entry.Action == IOAction.Replace));
            reportBuilder.Add("Deleted: "+actions.Count((entry) => entry.Action == IOAction.Delete));
            reportBuilder.Add("");
            reportBuilder.Add("= Starting backup =");

            while(actions.Count > 0 && --attempts > 0){
                if (firstAttempt)firstAttempt = false;
                else Thread.Sleep(200);

                for(int index = 0; index < actions.Count; index++){
                    IOActionEntry entry = actions[index];

                    try{
                        if (entry.Action == IOAction.Delete){
                            path = Path.Combine(destFolder,entry.RelativePath);

                            if (entry.Type == IOType.File){
                                if (File.Exists(path))File.Delete(path);
                            }
                            else if (entry.Type == IOType.Directory){
                                if (Directory.Exists(path))Directory.Delete(path,true);
                            }
                        }
                        else if (entry.Action == IOAction.Create){
                            path = Path.Combine(destFolder,entry.RelativePath);

                            if (entry.Type == IOType.File)File.Copy(Path.Combine(src,entry.RelativePath),path,false);
                            else if (entry.Type == IOType.Directory){
                                if (!Directory.Exists(path))Directory.CreateDirectory(path);
                            }
                        }
                        else if (entry.Action == IOAction.Replace){
                            File.Copy(Path.Combine(src,entry.RelativePath),Path.Combine(destFolder,entry.RelativePath),true);
                        }
                        
                        indexesToRemove.Add(index-indexesToRemove.Count); // goes from 0 to actions.Count, removing each index will move the structure
                        reportBuilder.Add(entry.Action,entry.Type,entry.RelativePath);

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
            public string RelativePath { get { return _relativePath; } set { _relativePath = value; if (_relativePath.StartsWith("\\"))_relativePath = _relativePath.Substring(1); } }
            
            public override string ToString(){
                return "{ Type: "+Type.ToString()+", Action: "+Action.ToString()+", RelativePath: "+(RelativePath == null ? "<null>" : RelativePath.ToString())+" }";
            }
        }
    }
}
