using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Pages;
using BackupEssentials.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class App : Application{
        public static Window Window { get { return Application.Current.MainWindow; } }

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
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Sys.Settings.Default.Load();

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),new FrameworkPropertyMetadata(){ DefaultValue = 30 });

            ProgramArgsParser parser = new ProgramArgsParser(args.Args);
            
            if (parser.HasFlag("runshell"))RunShell(parser);
            else if (parser.HasFlag("runcompat"))RunCompatWindow(parser);
            else RunMainWindow(parser);
        }

        /// <summary>
        /// Runs backup executed from Windows Explorer, tasks or command line.
        /// </summary>
        private void RunShell(ProgramArgsParser parser){
            int locid = -1;
            string dest = parser.GetValue("dest",""), name = "(Manual)";

            if (int.TryParse(parser.GetValue("locid","-1"),out locid) && locid >= 0){
                DataStorage.Load(DataStorage.Type.Locations);

                if (locid < DataStorage.BackupLocationList.Count){
                    name = DataStorage.BackupLocationList[locid].Name;
                    dest = DataStorage.BackupLocationList[locid].Directory;
                }
            }

            if (dest.Length > 0){
                BackupRunInfo info = new BackupRunInfo(parser.GetMultiValue("src"),name,dest,parser.HasFlag("noreport"));

                BackupWindow window = new BackupWindow(new BackupRunner(info));
                if (parser.HasFlag("nohide"))window.WindowState = WindowState.Normal;
                window.Show();
            }
            else throw new ArgumentException(Sys.Settings.Default.Language["General.App.DestinationMissing",string.Join(" ",args.Args)]);
        }

        /// <summary>
        /// Runs a Windows XP/Vista compatibility window that do not have cascaded Explorer entries.
        /// </summary>
        private void RunCompatWindow(ProgramArgsParser parser){
            MainWindow window = new MainWindow();
            window.ShowPage(typeof(BackupDrop),new object[]{ parser.GetMultiValue("src"), null, true });
            window.Show();
        }

        /// <summary>
        /// Runs the program without any special settings. Checks for already running process of the program, if there is one it moves it to foreground, if not it shows a new window.
        /// </summary>
        private void RunMainWindow(ProgramArgsParser parser){
            Process[] running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

            if (running.Length > 1){
                int myId = Process.GetCurrentProcess().Id;

                foreach(Process process in running){
                    if (process.Id != myId && process.MainWindowHandle != IntPtr.Zero){
                        if (process.Responding){
                            NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                            Application.Current.Shutdown();
                            return;
                        }
                        else{
                            if (MessageBox.Show(MainWindow,Sys.Settings.Default.Language["General.App.AlreadyRunning"],Sys.Settings.Default.Language["General.App.AlreadyRunning.Title"],MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes){
                                try{
                                    process.Kill();
                                }catch(Exception e){
                                    MessageBox.Show(MainWindow,Sys.Settings.Default.Language["General.App.CannotClose",e.Message],Sys.Settings.Default.Language["General.App.CannotClose.Title"],MessageBoxButton.OK,MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }

            SplashScreen splash = new SplashScreen("Resources/SplashScreen.png");
            splash.Show(false,false);
            new MainWindow(splash).Show();
        }

        /// <summary>
        /// Logs thrown exceptions into exceptions.log file and displays the message to the user.
        /// </summary>
        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e){
            LogException(e.Exception);

            // TODO
        }

        public static void LogException(Exception e){
            using(FileStream fileStream = new FileStream("exceptions.log",FileMode.Append)){
                using(StreamWriter writer = new StreamWriter(fileStream)){
                    writer.WriteLine(e.ToString());
                    writer.WriteLine();
                }
            }
        }
    }
}
