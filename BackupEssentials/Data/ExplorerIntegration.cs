using BackupEssentials.Properties;
using BackupEssentials.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BackupEssentials.Backup.Data{
    static class ExplorerIntegration{
        /*
        HKCR\*\shell\<appname>
            - MUIVerb = root command name
            - SubCommands = semicolon separated list of commands

        HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\<command>
            - (Default) = command name
            > command
              - (Default) = exe link
         */

        private static readonly ScheduledUpdate RefreshTimer = ScheduledUpdate.Forever(10,() => {
            Refresh(true);
        });

        public static void InitializeRefreshTimer(){
            RefreshTimer.Start();
        }

        public static void Load(){ // TODO add into settings or something
            DataStorage.BackupLocationList.Clear();
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell");
            int cmd = 0;

            while(true){
                string name = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\BackupEssentials"+cmd,null,null) as string;
                if (name == null)break;

                string fullpath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\BackupEssentials"+cmd+@"\command",null,"") as string;
                int end = fullpath.IndexOf(".EXE",StringComparison.OrdinalIgnoreCase)+6; // <...>.EXE "<path>" "%1"

                string path = fullpath.Substring(end,fullpath.IndexOf('"',end)-end);
                DataStorage.BackupLocationList.Add(new BackupLocation(){ Name = name, Directory = path });

                ++cmd;
            }
        }

        public static void Refresh(){
            Refresh(false);
        }

        public static bool Refresh(bool force){
            if (!force){
                RefreshTimer.NeedsUpdate = true;
                return true;
            }

            if (!Settings.Default.IntegrateWindowsExplorer)return true;

            IEnumerable<BackupLocation> valid = DataStorage.BackupLocationList.Where(loc => loc.ShouldRegister());
            if (valid.Count() == 0)return true;

            try{
                string path = Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8).Replace('/','\\'); // remove file:///
                int cmd = 0;

                List<string> commandNames = new List<string>();
                for(int a = 0; a < DataStorage.BackupLocationList.Count; a++)commandNames.Add("BackupEssentials"+a);
                string commands = String.Join(";",commandNames);

                foreach(string target in new string[]{ "*", "Directory" }){
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\"+target+@"\shell\BackupEssentials","MUIVerb","Backup Essentials");
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\"+target+@"\shell\BackupEssentials","SubCommands",commands);
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\"+target+@"\shell\BackupEssentials\command",null,path+" -runcompat -src \"%1\""); // TODO test on win xp
                }

                foreach(BackupLocation loc in valid){
                    string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\BackupEssentials"+cmd;
                    Registry.SetValue(key,null,loc.Name);
                    Registry.SetValue(key+@"\command",null,path+" -runshell -locid "+cmd+" -src \"%1\"");
                    ++cmd;
                }

                return true;
            }catch(Exception e){
                Debug.WriteLine(e.ToString());
                Remove();
                return false;
            }
        }

        public static void Remove(){
            try{
                Registry.ClassesRoot.DeleteSubKey(@"*\shell\BackupEssentials\command",false);
                Registry.ClassesRoot.DeleteSubKey(@"*\shell\BackupEssentials",false);
                Registry.ClassesRoot.DeleteSubKey(@"Directory\shell\BackupEssentials\command",false);
                Registry.ClassesRoot.DeleteSubKey(@"Directory\shell\BackupEssentials",false);
            }catch{}

            int cmd = 0;

            while(true){
                try{
                    Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\BackupEssentials"+cmd);
                    ++cmd;
                }catch{
                    break;
                }
            }
        }
    }
}
