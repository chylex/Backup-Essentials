namespace BackupEssentials.Sys.UI{
    public class DisplaySetting<T>{
        public T Value { get; private set; }
        public string Display { get; private set; }

        public DisplaySetting(T value, string display){
            this.Value = value;
            this.Display = display;
        }
    }
}
