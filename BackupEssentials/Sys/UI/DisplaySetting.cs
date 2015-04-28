namespace BackupEssentials.Sys.UI{
    public class DisplaySetting<T>{
        public T Value { get; private set; }

        private readonly string _displayKey;
        public string Display { get { return Settings.Default.Language[_displayKey]; } }

        public DisplaySetting(T value, string display){
            this.Value = value;
            this._displayKey = display;
        }
    }
}
