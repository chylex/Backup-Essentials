using Microsoft.Win32;
using System;

namespace BackupEssentials.Utils{
    static class WindowsVersion{
        public static string Get(){
            try{
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (key != null)return (string)key.GetValue("ProductName");
            }catch{}

            return GetFromEnvironment() ?? "(unknown)";
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
    }
}
