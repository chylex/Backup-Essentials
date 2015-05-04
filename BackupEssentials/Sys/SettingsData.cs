using BackupEssentials.Data;
using BackupEssentials.Sys.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackupEssentials.Sys{
    public static class SettingsData{
        public static T FindObj<T>(this IEnumerable<T> array, Func<T,bool> checkFunc){
            return array.First(checkFunc);
        }

        public static T FindObj<T>(this IEnumerable<T> array, Func<T,bool> checkFunc, Func<T,bool> defaultFunc){
            T val = array.FirstOrDefault(checkFunc);
            return val == null && defaultFunc != null ? array.First(defaultFunc) : val; // cannot use ??
        }

        // Languages

        private static Language[] _languageList = new Language[]{
            new Language("en","English"),
            new Language("cs","Czech (Čeština)")
        };

        public static IEnumerable<Language> LanguageList { get { return _languageList; } }

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

        public static IEnumerable<DateFormat> DateFormatList { get { return _dateFormatList; } }

        // Backup window close time

        private static DisplaySetting<int>[] _windowCloseList = new DisplaySetting<int>[]{
            new DisplaySetting<int>(0,"Settings.Option.WindowClose.Immediately"),
            new DisplaySetting<int>(5,"Settings.Option.WindowClose.5"),
            new DisplaySetting<int>(10,"Settings.Option.WindowClose.10"),
            new DisplaySetting<int>(30,"Settings.Option.WindowClose.30"),
            new DisplaySetting<int>(-1,"Settings.Option.WindowClose.Never")
        };

        public static IEnumerable<DisplaySetting<int>> WindowCloseList { get { return _windowCloseList; } }

        // History entry count

        private static DisplaySetting<int>[] _historyKeptList = new DisplaySetting<int>[]{
            new DisplaySetting<int>(0,"Settings.Option.ReportsKept.None"),
            new DisplaySetting<int>(25,"25"),
            new DisplaySetting<int>(50,"50"),
            new DisplaySetting<int>(100,"100"),
            new DisplaySetting<int>(250,"250"),
            new DisplaySetting<int>(500,"500"),
            new DisplaySetting<int>(1000,"1000"),
            new DisplaySetting<int>(-1,"Settings.Option.ReportsKept.All")
        };

        public static IEnumerable<DisplaySetting<int>> HistoryKeptList { get { return _historyKeptList; } }
    }
}
