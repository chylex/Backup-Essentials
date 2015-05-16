using System.Diagnostics;
using System.IO;

namespace BackupEssentials.Utils.IO{
    class FileLock{
        private static readonly int processID = Process.GetCurrentProcess().Id;

        private readonly string FileName;
        public bool IsLocked { get; private set; }

        public FileLock(string lockFileName){
            this.FileName = lockFileName;
        }

        public bool TryLock(){
            if (FileUtils.WriteFile(FileName,FileMode.CreateNew,(writer) => { writer.Write(processID); })){
                IsLocked = false;
                FileUtils.ReadFile(FileName,FileMode.Open,(line) => { IsLocked = line.Equals(processID.ToString()); });
                return IsLocked;
            }
            else return false;
        }

        public bool ReleaseLock(){
            if (IsLocked){
                try{
                    File.Delete(FileName);
                    return true;
                }catch(IOException){}
            }

            return false;
        }
    }
}
