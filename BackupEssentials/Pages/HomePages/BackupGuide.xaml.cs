using BackupEssentials.Utils;
using System.Windows.Controls;

namespace BackupEssentials.Pages.HomePages{
    public partial class BackupGuide : Page{
        public BackupGuide(){
            InitializeComponent();

            if (!WindowsVersion.IsFullySupported())TextBlockContextMenuWarning.Opacity = 1;
        }
    }
}
