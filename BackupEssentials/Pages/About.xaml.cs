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

            build.Append("You are using ").Append(WindowsVersion.Get()).Append(' ');
            build.Append(Environment.Is64BitOperatingSystem ? "x64" : "x86").Append(' ');
            build.Append(os.ServicePack.Replace("Service Pack ","SP")).Append(' ');
            build.Append(os.Version.Major >= 6 ? "(supported)." : "(not supported).");
            
            AboutTextOS.Text = build.ToString();
        }

        private void ClickLink(object sender, RequestNavigateEventArgs e){
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
