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
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void StartApp(object sender, StartupEventArgs args){
            ProgramArgsParser parser = new ProgramArgsParser(args.Args);
            
            if (parser.HasFlag("runshell")){
                new BackupWindow(new BackupRunner(parser.GetValue("src",null),parser.GetValue("dest",null))).Show();
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
            // TODO
        }
    }
}
