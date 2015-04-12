using BackupEssentials.Backup;
using BackupEssentials.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class App : Application{
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        /// <summary>
        /// List of arguments
        /// =================
        /// -runshell = switch to backup runner
        ///     [ required -src and either -dest or -locid ]
        ///     -src = backup source (folder or file, supports multiple entries)
        ///     -dest = backup destination (folder)
        ///     -locid = backup location id
        /// </summary>
        private void StartApp(object sender, StartupEventArgs args){
            ProgramArgsParser parser = new ProgramArgsParser(args.Args);
            
            if (parser.HasFlag("runshell")){
                int locid = -1;
                string dest = parser.GetValue("dest","");

                if (int.TryParse(parser.GetValue("locid","-1"),out locid) && locid >= 0){
                    DataStorage.Load(DataStorage.Type.Locations);

                    if (locid < DataStorage.BackupLocationList.Count){
                        dest = DataStorage.BackupLocationList[locid].Directory;
                    }
                }

                if (dest.Length > 0){
                    BackupRunInfo info = new BackupRunInfo(parser.GetMultiValue("src"),dest);
                    new BackupWindow(new BackupRunner(info)).Show();
                }
                else Application.Current.Shutdown();
            }
            else{
                Process[] running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

                if (running.Length > 1){
                    int myId = Process.GetCurrentProcess().Id;

                    foreach(Process process in running){
                        if (process.Id != myId && process.MainWindowHandle != IntPtr.Zero){
                            if (process.Responding){
                                SetForegroundWindow(process.MainWindowHandle);
                                Application.Current.Shutdown();
                                return;
                            }
                            else{
                                if (MessageBox.Show("The application is already running, but is not responding. Do you want to force close it?","Application is already running",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes){
                                    try{
                                        process.Kill();
                                    }catch(Exception e){
                                        MessageBox.Show("Could not close the application: "+e.Message);
                                    }
                                }
                            }
                        }
                    }
                }

                new MainWindow().Show();
            }
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e){
            using(FileStream fileStream = new FileStream("exceptions.log",FileMode.Append)){
                using(StreamWriter writer = new StreamWriter(fileStream)){
                    writer.WriteLine(e.Exception.ToString());
                    writer.WriteLine();
                }
            }

            // TODO
        }
    }
}
