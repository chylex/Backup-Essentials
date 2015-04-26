namespace BackupEssentials.Backup{
    public struct BackupRunInfo{
        public readonly string[] Source;
        public readonly string Name;
        public readonly string Destination;
        public readonly bool DisableHistory;

        public BackupRunInfo(string[] source, string name, string destination, bool disableHistory){
            this.Source = source;
            this.Name = name;
            this.Destination = destination;
            this.DisableHistory = disableHistory;
        }
    }
}
