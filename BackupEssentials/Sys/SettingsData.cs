using BackupEssentials.Sys.UI;

namespace BackupEssentials.Sys{
    public static class SettingsData{
        private static DateFormat[] _dateFormatList = new DateFormat[]{
            new DateFormat.Detect(),
            new DateFormat("d.M.yyyy HH:mm"),
            new DateFormat("dd.MM.yyyy HH:mm"),
            new DateFormat("yyyy-MM-dd HH:mm"),
            new DateFormat("yyyy-M-d HH:mm"),
            new DateFormat("yyyy-dd-MM HH:mm"),
            new DateFormat("yyyy-MM-dd HH:mm")
        };

        public static DateFormat[] DateFormatList { get { return _dateFormatList; } }
    }
}
