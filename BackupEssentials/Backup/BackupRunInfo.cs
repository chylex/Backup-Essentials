namespace BackupEssentials.Backup{
    public struct BackupRunInfo{
        public readonly string[] Source;
        public readonly string Name;
        public readonly string Destination;
        public bool DisableHistory { get; private set; }

        public BackupRunInfo(string[] source, string name, string destination){
            this.Source = source;
            this.Name = name;
            this.Destination = destination;
        }

        public void SetDisableHistory(){
            DisableHistory = true;
        }
    }
}
