using System;
using System.IO;

namespace BackupEssentials.Backup{
    public class BackupLocation{
        public string Name { get; set; }
        public string Directory { get; set; }

        public DirectoryStatus GetDirectoryStatus(){
            if (Directory.Length == 0)return DirectoryStatus.Empty;

            try{
                Path.GetFullPath(Directory);
            }catch(ArgumentException){
                return DirectoryStatus.Invalid;
            }

            if (!Path.IsPathRooted(Directory))return DirectoryStatus.NotAbsolute;
            if (!System.IO.Directory.Exists(Directory))return DirectoryStatus.NotExists;

            return DirectoryStatus.Ok;
        }

        public bool ShouldRegister(){
            DirectoryStatus status = GetDirectoryStatus();
            return status == DirectoryStatus.Ok || status == DirectoryStatus.NotExists;
        }

        public void Set(BackupLocation NewData){
            this.Name = NewData.Name;
            this.Directory = NewData.Directory;
        }

        public BackupLocation Clone(){
            return new BackupLocation(){ Name = this.Name, Directory = this.Directory };
        }

        public enum DirectoryStatus{
            Ok, NotExists, NotAbsolute, Invalid, Empty
        }
    }
}
