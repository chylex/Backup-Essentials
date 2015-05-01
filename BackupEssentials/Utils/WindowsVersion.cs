using BackupEssentials.Sys;
using Microsoft.Win32;
using System;

namespace BackupEssentials.Utils{
    static class WindowsVersion{
        private static string CachedVersionName;

        public static string Get(){
            if (CachedVersionName != null)return CachedVersionName;

            try{
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (key != null)return CachedVersionName = (string)key.GetValue("ProductName");
            }catch{}

            return CachedVersionName = (GetFromEnvironment() ?? Settings.Default.Language["About.WinVersion.Unknown"]);
        }

        private static string GetFromEnvironment(){
            Version version = Environment.OSVersion.Version;

            switch(version.Major){
                case 10: return "Windows 10";

                case 6:
                    switch(version.Minor){
                        case 0: return "Windows Vista";
                        case 1: return "Windows 7";
                        case 2: return "Windows 8";
                        case 3: return "Windows 8.1";
                    }

                    break;

                case 5:
                    switch(version.Minor){
                        case 0: return "Windows 2000";
                        case 1: return "Windows XP";
                        case 2: return "Windows XP";
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Full support is only available on Windows 7 and newer. Returns true if the used OS version has full support.
        /// </summary>
        public static bool IsFullySupported(){
            Version version = Environment.OSVersion.Version;
            return version.Major > 6 || (version.Major == 6 && version.Minor >= 1);
        }
    }
}
