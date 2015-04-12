namespace BackupEssentials.Backup{
    public class BackupRunInfo{
        public readonly string[] source;
        public readonly string destination;

        public BackupRunInfo(string[] source, string destination){
            this.source = source;
            this.destination = destination;
        }
    }
}
