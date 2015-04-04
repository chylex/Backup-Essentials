namespace BackupEssentials.Backup{
    public class BackupLocation{
        public string Name { get; set; }
        public string Directory { get; set; }

        public void Set(BackupLocation NewData){
            this.Name = NewData.Name;
            this.Directory = NewData.Directory;
        }

        public BackupLocation Clone(){
            return new BackupLocation(){ Name = this.Name, Directory = this.Directory };
        }
    }
}
