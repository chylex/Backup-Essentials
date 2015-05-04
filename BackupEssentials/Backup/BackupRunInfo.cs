namespace BackupEssentials.Backup{
    public struct BackupRunInfo{
        public string[] Source;
        public readonly string Name;
        public readonly string Destination;
        public readonly bool DisableHistory;

        public BackupRunInfo(string[] source, string name, string destination, bool disableHistory){
            this.Source = source;
            this.Name = name;
            this.Destination = destination;
            this.DisableHistory = disableHistory;
        }

        public override bool Equals(object obj){
            if (obj is BackupRunInfo){
                BackupRunInfo info = (BackupRunInfo)obj;
                return info.Name.Equals(Name) && info.Destination.Equals(Destination);
            }
            else return false;
        }

        public override int GetHashCode(){
            return Name.GetHashCode()*31+Destination.GetHashCode();
        }

        public static bool operator ==(BackupRunInfo obj1, BackupRunInfo obj2){
            return object.ReferenceEquals(obj1,null) ? object.ReferenceEquals(obj2,null) : obj1.Equals(obj2);
        }

        public static bool operator !=(BackupRunInfo obj1, BackupRunInfo obj2){
            return object.ReferenceEquals(obj1,null) ? !object.ReferenceEquals(obj2,null) : !obj1.Equals(obj2);
        }
    }
}
