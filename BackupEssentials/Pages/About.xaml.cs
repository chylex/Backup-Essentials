using BackupEssentials.Utils;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BackupEssentials.Pages{
    public partial class About : Page{
        public About(){
            InitializeComponent();

            AboutTextVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
            
            StringBuilder build = new StringBuilder();
            OperatingSystem os = Environment.OSVersion;

            build.Append(WindowsVersion.Get()).Append(' ').Append(Environment.Is64BitOperatingSystem ? "x64" : "x86").Append(' ').Append(os.ServicePack.Replace("Service Pack ","SP"));
            string version = build.ToString();

            build.Clear();
            build.Append(Sys.Settings.Default.Language["About.WinVersion.Using",version]).Append(' ');
            build.Append(Sys.Settings.Default.Language[WindowsVersion.IsFullySupported() ? "About.WinVersion.Supported" : "About.WinVersion.Unsupported"]);
            
            AboutTextOS.Text = build.ToString();
        }

        private void ClickLink(object sender, RequestNavigateEventArgs e){
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
