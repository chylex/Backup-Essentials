using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Threading;

namespace BackupEssentials.Backup{
    static class ExplorerIntegration{
        private static bool NeedsUpdate = false;

        public static void InitializeRefreshTimer(){
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,10);

            timer.Tick += (sender, args) => {
                if (NeedsUpdate){
                    NeedsUpdate = false;
                    Refresh(true);
                }
            };

            timer.Start();
        }

        public static void Refresh(){
            Refresh(false);
        }

        public static bool Refresh(bool force){
            if (!force){
                NeedsUpdate = true;
                return true;
            }

            if (DataStorage.BackupLocationList.Count == 0)return true;

            try{
                List<string> commandNames = new List<string>();
                for(int a = 0; a < DataStorage.BackupLocationList.Count; a++)commandNames.Add("BackupEssentials"+a);
                string commands = String.Join(";",commandNames);

                foreach(string target in new string[]{ "*", "Directory" }){
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\"+target+@"\shell\BackupEssentials","MUIVerb","Backup Essentials");
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\"+target+@"\shell\BackupEssentials","SubCommands",commands);
                }

                string path = Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8); // remove file:///
                int cmd = 0;

                foreach(BackupLocation loc in DataStorage.BackupLocationList){
                    string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\BackupEssentials"+cmd;
                    ++cmd;
                    Registry.SetValue(key,null,loc.Name);
                    Registry.SetValue(key+@"\command",null,path+" \""+loc.Directory+"\" \"%1\"");
                }

                return true;
            }catch(Exception e){
                Debug.WriteLine(e.ToString());
                Remove();
                return false;
            }
        }

        public static void Remove(){
            Registry.ClassesRoot.DeleteSubKey(@"*\shell\BackupEssentials",false);
            Registry.ClassesRoot.DeleteSubKey(@"Directory\shell\BackupEssentials",false);

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
