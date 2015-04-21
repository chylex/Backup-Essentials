using BackupEssentials.Sys.UI;
using System.Linq;

namespace BackupEssentials.Sys{
    public static class SettingsData{
        // Date format

        private static DateFormat[] _dateFormatList = new DateFormat[]{
            new DateFormat.Detect(),
            new DateFormat("d.M.yyyy HH:mm"),
            new DateFormat("d.M.yyyy hh:mm tt"),
            new DateFormat("dd.MM.yyyy HH:mm"),
            new DateFormat("dd.MM.yyyy hh:mm tt"),
            new DateFormat("d/M/yyyy HH:mm"),
            new DateFormat("d/M/yyyy hh:mm tt"),
            new DateFormat("M/d/yyyy HH:mm"),
            new DateFormat("M/d/yyyy hh:mm tt"),
            new DateFormat("yyyy-MM-dd HH:mm"),
            new DateFormat("yyyy-MM-dd hh:mm tt"),
            new DateFormat("yyyy-dd-MM HH:mm"),
            new DateFormat("yyyy-dd-MM hh:mm tt")
        };

        public static DateFormat[] DateFormatList { get { return _dateFormatList; } }

        public static DateFormat FindDateFormat(string format){
            return DateFormatList.FirstOrDefault(fmt => fmt.Format.Equals(format)) ?? DateFormatList[0];
        }

        // Backup window close time

        private static DisplaySetting<int>[] _windowCloseList = new DisplaySetting<int>[]{
            new DisplaySetting<int>(0,"Immediately"),
            new DisplaySetting<int>(5,"After 5 seconds"),
            new DisplaySetting<int>(10,"After 10 seconds"),
            new DisplaySetting<int>(30,"After 30 seconds"),
            new DisplaySetting<int>(-1,"Never")
        };

        public static DisplaySetting<int>[] WindowCloseList { get { return _windowCloseList; } }

        public static DisplaySetting<int> FindWindowCloseTime(int value){
            return WindowCloseList.FirstOrDefault(setting => setting.Value == value) ?? WindowCloseList[2];
        }

        // History entry count

        private static DisplaySetting<int>[] _historyKeptList = new DisplaySetting<int>[]{
            new DisplaySetting<int>(0,"None"),
            new DisplaySetting<int>(25,"25"),
            new DisplaySetting<int>(50,"50"),
            new DisplaySetting<int>(100,"100"),
            new DisplaySetting<int>(250,"250"),
            new DisplaySetting<int>(500,"500"),
            new DisplaySetting<int>(1000,"1000"),
            new DisplaySetting<int>(-1,"All")
        };

        public static DisplaySetting<int>[] HistoryKeptList { get { return _historyKeptList; } }

        public static DisplaySetting<int> FindHistoryEntryCount(int value){
            return HistoryKeptList.FirstOrDefault(setting => setting.Value == value) ?? HistoryKeptList[5];
        }
    }
}
